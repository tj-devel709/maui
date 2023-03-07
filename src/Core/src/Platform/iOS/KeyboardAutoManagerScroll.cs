﻿/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their MIT License can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/blob/7399efb730eea084571b45a1a9b36a3a3c54c44f/LICENSE.md
 */

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManagerScroll
{
	static UIScrollView? LastScrollView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;
	static CGRect KeyboardFrame = CGRect.Empty;
	static CGPoint TopViewBeginOrigin = new(nfloat.MaxValue, nfloat.MaxValue);
	static readonly CGPoint InvalidPoint = new(nfloat.MaxValue, nfloat.MaxValue);
	static double AnimationDuration = 0.25;
	static UIView? View = null;
	static UIView? RootController = null;
	static CGRect? CursorRect = null;
	static bool IsKeyboardShowing = false;
	static int TextViewTopDistance = 20;
	static int DebounceCount = 0;
	static NSObject? WillShowToken = null;
	static NSObject? DidHideToken = null;
	static NSObject? TextFieldToken = null;
	static NSObject? TextViewToken = null;

	internal static void Init()
	{
		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), DidUITextBeginEditing);

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), DidUITextBeginEditing);

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), WillKeyboardShow);

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), WillHideKeyboard);
	}

	static async void DidUITextBeginEditing(NSNotification notification)
	{
		if (notification.Object is not null)
		{
			View = (UIView)notification.Object;
			await SetUpTextEdit();
		}
	}

	static async void WillKeyboardShow(NSNotification notification)
	{
		var userInfo = notification.UserInfo;

		if (userInfo is not null)
		{
			if (userInfo.TryGetValue(new NSString("UIKeyboardFrameEndUserInfoKey"), out var frameSize))
			{
				var frameSizeRect = DescriptionToCGRect(frameSize?.Description);
				if (frameSizeRect is not null)
					KeyboardFrame = (CGRect)frameSizeRect;
			}

			if (userInfo.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out var curveSize))
			{
				var num = (NSNumber)NSObject.FromObject(curveSize);
				AnimationDuration = (double)num;
			}
		}

		if (TopViewBeginOrigin == InvalidPoint && RootController is not null)
			TopViewBeginOrigin = new CGPoint(RootController.Frame.X, RootController.Frame.Y);

		if (!IsKeyboardShowing)
		{
			await AdjustPositionDebounce();
			IsKeyboardShowing = true;
		}
	}

	static void WillHideKeyboard(NSNotification notification)
	{
		NSObject? curveSize = null;

		var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
		if (foundAnimationDuration == true && curveSize is not null)
		{
			var num = (NSNumber)NSObject.FromObject(curveSize);
			AnimationDuration = (double)num;
		}

		if (LastScrollView is not null)
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateHidingKeyboard, () => { });

		if (IsKeyboardShowing)
			RestorePosition();

		IsKeyboardShowing = false;
		View = null;
		LastScrollView = null;
		KeyboardFrame = CGRect.Empty;
		StartingContentInsets = new UIEdgeInsets();
		StartingScrollIndicatorInsets = new UIEdgeInsets();
		StartingContentInsets = new UIEdgeInsets();
	}

	static void AnimateHidingKeyboard()
	{
		if (LastScrollView is not null && LastScrollView.ContentInset != StartingContentInsets)
		{
			LastScrollView.ContentInset = StartingContentInsets;
			LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
		}

		var superScrollView = LastScrollView;
		while (superScrollView is not null)
		{
			var contentSize = new CGSize(Math.Max(superScrollView.ContentSize.Width, superScrollView.Frame.Width),
				Math.Max(superScrollView.ContentSize.Height, superScrollView.Frame.Height));

			var minY = contentSize.Height - superScrollView.Frame.Height;
			if (minY < superScrollView.ContentOffset.Y)
			{
				var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, minY);
				if (!superScrollView.ContentOffset.Equals(newContentOffset))
				{
					if (View?.Superview is UIStackView)
						superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
					else
						superScrollView.ContentOffset = newContentOffset;
				}
			}
			superScrollView = superScrollView.FindResponder<UIScrollView>();
		}
	}

	internal static void Destroy()
	{
		if (WillShowToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(WillShowToken);
		if (DidHideToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(DidHideToken);
		if (TextFieldToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextFieldToken);
		if (TextViewToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextViewToken);
	}

	// Used to get the numeric values from the UserInfo dictionary's NSObject value to CGRect.
	// Doing manually since CGRectFromString is not yet bound
	static CGRect? DescriptionToCGRect(string? description)
	{
		// example of passed in description: "NSRect: {{0, 586}, {430, 346}}"

		if (description is null)
			return null;

		// remove everything except for numbers and commas
		var temp = Regex.Replace(description, @"[^0-9,]", "");
		var dimensions = temp.Split(',');

		if (int.TryParse(dimensions[0], out var x) && int.TryParse(dimensions[1], out var y)
			&& int.TryParse(dimensions[2], out var width) && int.TryParse(dimensions[3], out var height))
		{
			return new CGRect(x, y, width, height);
		}

		return null;
	}

	static async Task SetUpTextEdit()
	{
		if (View is null)
			return;

		CursorRect = null;

		RootController = View.SetContainerView();

		// the cursor needs a small amount of time to update the position
		await Task.Delay(5);

		CGRect? localCursor = null;

		if (View is IUITextInput textInput)
		{
			var selectedTextRange = textInput.SelectedTextRange;
			if (selectedTextRange is UITextRange selectedRange)
			{
				localCursor = textInput.GetCaretRectForPosition(selectedRange.Start);
				if (localCursor is CGRect local)
					CursorRect = View.ConvertRectToView(local, null);
			}
		}

		TextViewTopDistance = localCursor is CGRect cGRect ? 20 + (int)cGRect.Height : 20;

		await AdjustPositionDebounce();
	}

	// Used to debounce calls from different oberservers so we can be sure
	// all the fields are updated before calling AdjustPostition()
	internal static async Task AdjustPositionDebounce()
	{
		Interlocked.CompareExchange(ref DebounceCount, 0, 100);
		Interlocked.Increment(ref DebounceCount);

		var entranceCount = DebounceCount;

		await Task.Delay(10);

		if (entranceCount == DebounceCount)
			AdjustPosition();
	}

	// main method to calculate and animate the scrolling
	internal static void AdjustPosition()
	{
		if (View is not UITextField field && View is not UITextView)
			return;

		if (RootController is null)
			return;

		var rootViewOrigin = new CGPoint(RootController.Frame.GetMinX(), RootController.Frame.GetMinY());
		var window = RootController.Window;

		var kbSize = KeyboardFrame.Size;
		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		kbSize = intersectRect == CGRect.Empty ? new CGSize(KeyboardFrame.Width, 0) : intersectRect.Size;

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (RootController.GetNavigationController() is UINavigationController navigationController)
		{
			navigationBarAreaHeight = navigationController.NavigationBar.Frame.GetMaxY();
		}
		else
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13, 0))
				statusBarHeight = window.WindowScene?.StatusBarManager?.StatusBarFrame.Height ?? 0;
			else
				statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;

			navigationBarAreaHeight = statusBarHeight;
		}

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, RootController.LayoutMargins.Top) + 5;

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewTopDistance;

		var viewRectInWindow = View.ConvertRectToView(View.Bounds, window);

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (RootController.Superview is UIView v)
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		CGRect cursorRect;

		if (CursorRect is CGRect cRect)
			cursorRect = cRect;
		else
			return;

		nfloat cursorNotInViewScroll = 0;
		nfloat move = 0;
		bool cursorTooHigh = false;
		bool cursorTooLow = false;

		if (cursorRect.Y >= viewRectInWindow.GetMaxY())
		{
			cursorNotInViewScroll = viewRectInWindow.GetMaxY() - cursorRect.GetMaxY();
			move = cursorRect.Y - keyboardYPosition + cursorNotInViewScroll;
			cursorTooLow = true;
		}

		else if (cursorRect.Y < viewRectInWindow.GetMinY())
		{
			cursorNotInViewScroll = viewRectInWindow.GetMinY() - cursorRect.Y;
			move = cursorRect.Y - keyboardYPosition + cursorNotInViewScroll;
			cursorTooHigh = true;

			// no need to move the screen down if we can already see the view
			if (move < 0)
				move = 0;
		}

		else if (cursorRect.Y >= topLayoutGuide && cursorRect.Y < keyboardYPosition)
			return;

		else if (cursorRect.Y > keyboardYPosition)
			move = cursorRect.Y - keyboardYPosition - cursorNotInViewScroll;

		else if (cursorRect.Y <= topLayoutGuide)
			move = cursorRect.Y - (nfloat)topLayoutGuide - cursorNotInViewScroll;

		// Find the next parent ScrollView that is scrollable
		var superView = View.FindResponder<UIScrollView>();
		var superScrollView = FindParentScroll(superView);

		// This is the case when the keyboard is already showing and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
					UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateStartingLastScrollView, () => { });

				if (!LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (View.FindResponder<UIStackView>() is UIStackView)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				StartingContentInsets = new UIEdgeInsets();
				StartingScrollIndicatorInsets = new UIEdgeInsets();
				StartingContentOffset = new CGPoint(0, 0);
				LastScrollView = null;
			}

			// if we have different LastScrollView and superScrollViews, set the LastScrollView to the original frame
			// and set the LastScrollView as the superScrolView
			else if (superScrollView != LastScrollView)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
					UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateStartingLastScrollView, () => { });

				if (!LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (View.FindResponder<UIStackView>() is not null)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				LastScrollView = superScrollView;
				if (superScrollView is not null)
				{
					StartingContentInsets = superScrollView.ContentInset;
					StartingContentOffset = superScrollView.ContentOffset;

					if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
						StartingScrollIndicatorInsets = superScrollView.VerticalScrollIndicatorInsets;
					else
						StartingScrollIndicatorInsets = superScrollView.ScrollIndicatorInsets;
				}
			}
		}

		// If there was no LastScrollView, but there is a superScrollView,
		// set the LastScrollView to be the superScrollView
		else if (superScrollView is not null)
		{
			LastScrollView = superScrollView;
			StartingContentInsets = superScrollView.ContentInset;
			StartingContentOffset = superScrollView.ContentOffset;

			StartingScrollIndicatorInsets = OperatingSystem.IsIOSVersionAtLeast(11, 1) ?
				superScrollView.VerticalScrollIndicatorInsets : superScrollView.ScrollIndicatorInsets;
		}

		// Calculate the move for the ScrollViews
		if (LastScrollView is not null)
		{
			var lastView = View;
			superScrollView = LastScrollView;
			nfloat innerScrollValue = 0;

			while (superScrollView is not null)
			{
				var shouldContinue = false;

				if (move > 0)
					shouldContinue = move > -superScrollView.ContentOffset.Y - superScrollView.ContentInset.Top;

				else if (superScrollView.FindResponder<UITableView>() is UITableView tableView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View.FindResponder<UITableViewCell>() is UITableViewCell tableCell
						&& tableView.IndexPathForCell(tableCell) is NSIndexPath indexPath
						&& tableView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath)
					{
						var previousCellRect = tableView.RectForRowAtIndexPath(previousIndexPath);
						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, RootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				else if (superScrollView.FindResponder<UICollectionView>() is UICollectionView collectionView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View.FindResponder<UICollectionViewCell>() is UICollectionViewCell collectionCell
						&& collectionView.IndexPathForCell(collectionCell) is NSIndexPath indexPath
						&& collectionView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath
						&& collectionView.GetLayoutAttributesForItem(previousIndexPath) is UICollectionViewLayoutAttributes attributes)
					{
						var previousCellRect = attributes.Frame;

						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, RootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				else
				{
					shouldContinue = !(innerScrollValue == 0
						&& cursorRect.Y + cursorNotInViewScroll >= topLayoutGuide
						&& cursorRect.Y + cursorNotInViewScroll <= keyboardYPosition);

					if (cursorRect.Y - innerScrollValue < topLayoutGuide && !cursorTooHigh)
						move = cursorRect.Y - innerScrollValue - (nfloat)topLayoutGuide;
					else if (cursorRect.Y - innerScrollValue > keyboardYPosition && !cursorTooLow)
						move = cursorRect.Y - innerScrollValue - keyboardYPosition;
				}

				// Go up the hierarchy and look for other scrollViews until we reach the UIWindow
				if (shouldContinue)
				{
					var tempScrollView = superScrollView.FindResponder<UIScrollView>();
					var nextScrollView = FindParentScroll(tempScrollView);

					var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);

					// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
					move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

					var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

					if (!superScrollView.ContentOffset.Equals(newContentOffset) || innerScrollValue != 0)
					{
						if (nextScrollView is null)
						{
							UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
							{
								newContentOffset.Y += innerScrollValue;
								innerScrollValue = 0;

								if (View.FindResponder<UIStackView>() is not null)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}, () => { });
						}

						else
						{
							// add the amount we would have moved to the next scroll value
							innerScrollValue += newContentOffset.Y;
						}
					}

					lastView = superScrollView;
					superScrollView = nextScrollView;
				}

				else
				{
					// if we did not get to scroll all the way, add the value to move
					move += innerScrollValue;
					break;
				}
			}
		}

		if (move >= 0)
		{
			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height - TextViewTopDistance));

			if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
			{
				var rect = RootController.Frame;
				rect.X = rootViewOrigin.X;
				rect.Y = rootViewOrigin.Y;

				UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
			}
		}

		else
		{
			rootViewOrigin.Y -= move;

			if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
			{
				var rect = RootController.Frame;
				rect.X = rootViewOrigin.X;
				rect.Y = rootViewOrigin.Y;

				UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
			}
		}
	}

	static void AnimateStartingLastScrollView()
	{
		if (LastScrollView is not null)
		{
			LastScrollView.ContentInset = StartingContentInsets;
			LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
		}
	}

	static void AnimateRootView(CGRect rect)
	{
		if (RootController is not null)
			RootController.Frame = rect;
	}

	static UIScrollView? FindParentScroll(UIScrollView? view)
	{
		while (view is not null)
		{
			if (view.ScrollEnabled)
				return view;

			view = view.FindResponder<UIScrollView>();
		}

		return null;
	}

	static void RestorePosition()
	{
		if (RootController is not null && (RootController.Frame.X != TopViewBeginOrigin.X || RootController.Frame.Y != TopViewBeginOrigin.Y))
		{
			var rect = RootController.Frame;
			rect.X = TopViewBeginOrigin.X;
			rect.Y = TopViewBeginOrigin.Y;

			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
		}
		View = null;
		RootController = null;
		TopViewBeginOrigin = InvalidPoint;
		CursorRect = null;
	}

	static NSIndexPath? GetPreviousIndexPath(this UIScrollView scrollView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0 && scrollView is UICollectionView collectionView)
				previousRow = (int)(collectionView.NumberOfItemsInSection(previousSection) - 1);
			else if (previousSection >= 0 && scrollView is UITableView tableView)
				previousRow = (int)(tableView.NumberOfRowsInSection(previousSection) - 1);
			else
				return null;
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}
}
