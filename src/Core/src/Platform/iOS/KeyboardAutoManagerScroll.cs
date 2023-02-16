using System;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using UIKit;
using ObjCRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManagerScroll
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0052
#pragma warning disable IDE0051
#pragma warning disable CS0649
#pragma warning disable CS0169
#pragma warning disable CS0414

	static nfloat MovedDistance;
	static UIScrollView? LastScrollView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;
	static UIEdgeInsets StartingTextViewContentInsets;
	static UIEdgeInsets StartingTextViewScrollIndicatorInsets;
	static bool IsTextViewContentInsetChanged;
	static bool HasPendingAdjustRequest;
	static bool ShouldIgnoreScrollingAdjustment;
	static bool ShouldRestoreScrollViewContentOffset;
	static bool ShouldIgnoreContentInsetAdjustment;

	static nfloat KeyboardDistanceFromTextField = 10.0f;
	static nfloat SearchBarKeyboardDistanceFromTextField = 15.0f;
	static CGRect KeyboardFrame = CGRect.Empty;
	static double AnimationDuration = 0.25;
	static UIViewAnimationOptions AnimationCurve = UIViewAnimationOptions.CurveEaseOut;
	static UIEdgeInsets uIEdgeInsets = new UIEdgeInsets();
	static bool LayoutifNeededOnUpdate = false;
	static CGPoint TopViewBeginOrigin = new CGPoint(nfloat.MaxValue, nfloat.MaxValue);
	static CGPoint InvalidPoint = new CGPoint(nfloat.MaxValue, nfloat.MaxValue);

	static NSObject? WillShowToken = null;
	static NSObject? DidHideToken = null;
	static NSObject? TextFieldToken = null;
	static NSObject? TextViewToken = null;
	static bool IsKeyboardShowing = false;

	static UIView? view = null;
	static UIView? rootController = null;
	static CGRect? TextViewRect = null;
	static CGRect? TextViewRectLocal = null;
	static CGRect? TextFieldRect = null;
	static CGRect? TextFieldRectLocal = null;
	static int TextViewTopDistance = 20;


	// Set up the observers for the keyboard and the UITextField/UITextView
	internal static void Init()
	{
		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), async (notification) =>
		{
			if (notification.Object is not null)
			{
				view = (UIView)notification.Object;
				rootController = view.FindResponder<ContainerViewController>()?.View;
			}

			RestoreRects();

			await Task.Delay(5);

			var v = view as UITextField;
			if (v is UITextField vf)
			{

				var selectedTextRange = vf.SelectedTextRange;
				if (selectedTextRange is UITextRange selectedRange)
				{
					TextFieldRectLocal = vf.GetCaretRectForPosition(selectedRange.Start);
					if (TextFieldRectLocal is CGRect local)
						TextFieldRect = vf.ConvertRectToView(local, null);
				}

				TextViewTopDistance = TextFieldRectLocal is CGRect cGRect ? 20 + (int)cGRect.Height : 20;

				if (vf.InputAccessoryView is UIView accessory)
				{
					TextViewTopDistance += (int)accessory.Frame.Height;
				}
			}

			if (IsKeyboardShowing)
				AdjustPosition();
		});

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), async (notification) =>
		{
			if (notification.Object is not null)
			{
				RestoreRects();

				view = (UIView)notification.Object;

				rootController = view.FindResponder<ContainerViewController>()?.View;

				await Task.Delay(5);

				var v = view as UITextView;

				if (v is UITextView vt)
				{

					var selectedTextRange = vt.SelectedTextRange;
					if (selectedTextRange is UITextRange selectedRange)
					{
						TextViewRectLocal = vt.GetCaretRectForPosition(selectedRange.Start);
						if (TextViewRectLocal is CGRect local)
						{
							TextViewRect = vt.ConvertRectToView(local, null);
						}

					}


					TextViewTopDistance = TextViewRectLocal is CGRect cGRect ? 20 + (int)cGRect.Height : 20;

					//if (vt.InputAccessoryView is UIView accessory)
					//{
					//	TextViewTopDistance += (int)accessory.Frame.Height;
					//}
				}

				if (IsKeyboardShowing)
					AdjustPosition();
			}
		});

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), async (notification) =>
		{
			NSObject? frameSize = null;
			NSObject? curveSize = null;

			var foundFrameSize = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardFrameEndUserInfoKey"), out frameSize);
			if (foundFrameSize == true && frameSize is not null)
			{
				var frameSizeRect = DescriptionToCGRect(frameSize.Description);
				if (frameSizeRect is not null)
					KeyboardFrame = (CGRect)frameSizeRect;
			}

			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSObject.FromObject(curveSize);
				AnimationDuration = (double)num;
			}

			if (TopViewBeginOrigin == InvalidPoint && rootController is not null)
				TopViewBeginOrigin = new CGPoint(rootController.Frame.X, rootController.Frame.Y);

			if (!IsKeyboardShowing)
			{
				await Task.Delay(1);
				AdjustPosition();
				IsKeyboardShowing = true;
			}
		});

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), (notification) =>
		{
			NSObject? curveSize = null;

			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSObject.FromObject(curveSize);
				AnimationDuration = (double)num;
			}

			if (LastScrollView is not null)
			{
				UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
				{
					if (LastScrollView.ContentInset != StartingContentInsets)
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}

					var superScrollView = LastScrollView as UIScrollView;
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
								if (view?.Superview is UIStackView)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}
						}

						superScrollView = superScrollView.Superview as UIScrollView;
					}
				}, () => { });
			}

			if (IsKeyboardShowing)
				RestorePosition();

			LastScrollView = null;
			KeyboardFrame = CGRect.Empty;
			StartingContentInsets = new UIEdgeInsets();
			StartingScrollIndicatorInsets = new UIEdgeInsets();
			StartingContentInsets = new UIEdgeInsets();

			IsKeyboardShowing = false;
		});
	}

	// used to get the numeric values from the UserInfo dictionary's NSObject value to CGRect
	static CGRect? DescriptionToCGRect(string description)
	{
		if (description is null)
			return null;

		string one, two, three, four;
		one = two = three = four = string.Empty;

		var sb = new StringBuilder();
		var isInNumber = false;
		foreach (var c in description)
		{
			if (char.IsDigit(c))
			{
				sb.Append(c);
				isInNumber = true;
			}

			else if (isInNumber && !char.IsDigit(c))
			{
				if (string.IsNullOrEmpty(one))
					one = sb.ToString();
				else if (string.IsNullOrEmpty(two))
					two = sb.ToString();
				else if (string.IsNullOrEmpty(three))
					three = sb.ToString();
				else if (string.IsNullOrEmpty(four))
					four = sb.ToString();
				else
					break;

				isInNumber = false;
				sb.Clear();
			}
		}

		if (int.TryParse(one, out var oneNum) && int.TryParse(two, out var twoNum)
			&& int.TryParse(three, out var threeNum) && int.TryParse(four, out var fourNum))
		{
			return new CGRect(oneNum, twoNum, threeNum, fourNum);
		}

		return null;
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

	internal static void AdjustPosition()
	{
		if (view is not UITextField field && view is not UITextView)
			return;

		if (rootController is null)
			return;
		var rootFrame = rootController.Frame;
		var rootViewOrigin = new CGPoint(rootFrame.GetMinX(), rootFrame.GetMinY());
		var window = rootController.Window;

		var specialKeyboardDistanceFromTextField = KeyboardDistanceFromTextField;
		//var specialKeyboardDistanceFromTextField = view.GetTextFieldSearchBar() is null ?
		//	KeyboardDistanceFromTextField : SearchBarKeyboardDistanceFromTextField;

		var kbSize = KeyboardFrame.Size;
		var kbFrame = KeyboardFrame;
		//kbFrame.Y -= specialKeyboardDistanceFromTextField;
		//kbFrame.Height += specialKeyboardDistanceFromTextField;
		var intersectRect = CGRect.Intersect(kbFrame, window.Frame);
		if (intersectRect == CGRect.Empty)
			kbSize = new CGSize(kbFrame.Width, 0);
		else
			kbSize = intersectRect.Size;


		// Set the StatusBarHeight and NavigationBarAreaHeight

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (rootController.GetNavigationController() is UINavigationController navigationController)
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

		// Do we have a UITextView or a UITextField

		var layoutAreaHeight = rootController.LayoutMargins.Bottom;
		var isTextView = false;
		var isNonScrollableTextView = false;

		if (view is UIScrollView scrollView)
		{
			isTextView = true;
			isNonScrollableTextView = !scrollView.ScrollEnabled;
		}

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, layoutAreaHeight) + 5;
		var bottomLayoutGuide = (isTextView && !isNonScrollableTextView) ? 0 : rootController.LayoutMargins.Bottom;

		// the height that will be above the keyboard
		var visibleHeight = window.Frame.Height - kbSize.Height;
		//var textViewHeight = Math.Min(textView.Frame.Height, rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping - specialKeyboardDistanceFromTextField);

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewTopDistance;
		//var keyboardYPosition = window.Frame.Height - kbSize.Height + specialKeyboardDistanceFromTextField;

		var viewRect = TextViewRect ?? TextFieldRect;
		var localViewRect = TextViewRectLocal ?? TextFieldRectLocal;

		//if (viewRect is CGRect vRect && vRect.Y >= topLayoutGuide && vRect.Y < keyboardYPosition - TextViewTopDistance)
		if (viewRect is CGRect vRect && vRect.Y >= topLayoutGuide && vRect.Y < keyboardYPosition)
			return;

		//if (TextViewRect is CGRect viewRect1 && viewRect1.Y < keyboardYPosition)
		//	return;


		var viewRectInWindowMaxY = view.Superview.ConvertRectToView(view.Frame, window).GetMaxY();
		var viewRectInRootSuperviewMinY = view.Superview.ConvertRectToView(view.Frame, rootController.Superview).GetMinY();

		// if move is positive, the textField is hidden behind the keyboard
		// if move is negative, the textField is not blocked by the keyboard
		nfloat move = 0;

		// Debugging
		//var m1 = viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide;
		//var m2 = (nfloat)Math.Min(viewRectInRootSuperviewMinY - topLayoutGuide, viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide);

		// if the TextView is not scrollable, scroll to the bottom of the part of the view
		//if (isNonScrollableTextView)
		//	move = viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide;
		//else
		//	move = (nfloat)Math.Min(viewRectInRootSuperviewMinY - topLayoutGuide, viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide);









		// Try figuring out a better move here:

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (rootController.Superview is UIView v)
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		var keyboardOverlapping = rootSuperViewFrameInWindow.GetMaxY() - keyboardYPosition;

		var availableSpace = rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping; // - specialKeyboardDistanceFromTextField;

		// how much of the textView can fit on the screen with the keyboard up
		var textViewHeight = Math.Min(view.Frame.Height, availableSpace);

		Console.WriteLine($"keyboardYPosition: {keyboardYPosition}");
		Console.WriteLine($"rootSuperViewFrameInWindow: {rootSuperViewFrameInWindow}");
		Console.WriteLine($"keyboardOverlapping: {keyboardOverlapping}");
		Console.WriteLine($"textViewHeight: {textViewHeight}");
		Console.WriteLine($"textView.Frame.Height: {view.Frame.Height}");

		// if the entire textview cannot fit on the screen
		//if (view.Frame.Size.Height > textViewHeight)
		//{
		//var viewRect = TextViewRect ?? TextFieldRect;
		// if the text is going to still be below the keyboard, add to the newContentInset.Bottom
		if (viewRect is CGRect rect)
		{
			if (rect.Y > keyboardYPosition)
			{
				move = rect.Y - (nfloat)keyboardYPosition;
				//move = rect.Y - (nfloat)keyboardYPosition + TextViewTopDistance;
			}

			else if (rect.Y <= topLayoutGuide)
			{
				move = rect.Y - (nfloat)topLayoutGuide;
			}
		}

		Console.WriteLine($"new move: {move}");
		//}
















		Console.WriteLine($"\n\n\n\n\n!!Starting!!");
		Console.WriteLine($"Initial move calculated: {move}");

		// Find the next highest scroll view

		UIScrollView? superScrollView = null;
		var superView = view.FindResponder<UIScrollView>();
		while (superView is not null)
		{
			if (superView.ScrollEnabled && !ShouldIgnoreScrollingAdjustment)
			{
				superScrollView = superView;
				break;
			}

			superView = superView.FindResponder<UIScrollView>();
		}

		// This is the case when the keyboard is already up and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					Console.WriteLine($"!!!ANIMATING SOMETHING #1!!!");
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}, () => { });
				}

				if (ShouldRestoreScrollViewContentOffset && !LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (view.FindResponder<UIStackView>() is UIStackView)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				StartingContentInsets = uIEdgeInsets;
				StartingScrollIndicatorInsets = uIEdgeInsets;
				StartingContentOffset = new CGPoint(0, 0);
				LastScrollView = null;
			}

			// if we have different LastScrollView and superScrollViews, set the LastScrollView to the original frame
			// and set the superScrolView as the LastScrollView
			else if (superScrollView != LastScrollView)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						Console.WriteLine($"!!!ANIMATING SOMETHING #2!!!");
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}, () => { });
				}

				if (ShouldRestoreScrollViewContentOffset && !LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (view.FindResponder<UIStackView>() is UIStackView)
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

			if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
				StartingScrollIndicatorInsets = superScrollView.VerticalScrollIndicatorInsets;
			else
				StartingScrollIndicatorInsets = superScrollView.ScrollIndicatorInsets;
		}

		// if we found a LastScrollView above, then set the contentOffset to see textField
		if (LastScrollView is not null)
		{
			var lastView = view;
			superScrollView = LastScrollView;
			nfloat innerScrollValue = 0;

			var debuggingIteration = 1;

			while (superScrollView is not null)
			{
				Console.WriteLine($"Starting Iteration #{debuggingIteration}");
				Console.WriteLine($"superScrollView: {superScrollView}");
				Console.WriteLine($"LastScrollView: {LastScrollView}");

				var shouldContinue = false;

				Console.WriteLine($"move: {move}");

				if (move > 0)
				{
					shouldContinue = move > -superScrollView.ContentOffset.Y - superScrollView.ContentInset.Top;
					Console.WriteLine($"shouldContinue: {shouldContinue}");
				}

				else if (superScrollView.FindResponder<UITableView>() is UITableView tableView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && view.FindResponder<UITableViewCell>() is UITableViewCell tableCell
						&& tableView.IndexPathForCell(tableCell) is NSIndexPath indexPath
						&& tableView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath)
					{
						var previousCellRect = tableView.RectForRowAtIndexPath(previousIndexPath);
						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, rootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						} 
					}
					Console.WriteLine($"Entered is TableView and move == 0");
					Console.WriteLine($"shouldContinue: {shouldContinue}");
				}

				else if (superScrollView.FindResponder<UICollectionView>() is UICollectionView collectionView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && view.FindResponder<UICollectionViewCell>() is UICollectionViewCell collectionCell
						&& collectionView.IndexPathForCell(collectionCell) is NSIndexPath indexPath
						&& collectionView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath
						&& collectionView.GetLayoutAttributesForItem(previousIndexPath) is UICollectionViewLayoutAttributes attributes)
					{
						var previousCellRect = attributes.Frame;

						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, rootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}

					Console.WriteLine($"Entered is CollectionView and move == 0");
					Console.WriteLine($"shouldContinue: {shouldContinue}");
				}

				else
				{
					if (isNonScrollableTextView)
					{
						shouldContinue = viewRectInWindowMaxY < visibleHeight + bottomLayoutGuide;

						if (shouldContinue)
							move = (nfloat)Math.Min(0, viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide);

						Console.WriteLine($"Entered is isNonScrollableTextView and new move is: {move}");
						Console.WriteLine($"shouldContinue: {shouldContinue}");
					}
					else
					{
						//shouldContinue = viewRectInRootSuperviewMinY < topLayoutGuide;



						//shouldContinue = viewRectInRootSuperviewMinY < topLayoutGuide;

						if (viewRect is CGRect rect1)
						{
							if (rect1.Y - innerScrollValue >= topLayoutGuide && rect1.Y - innerScrollValue <= (nfloat)keyboardYPosition)
								shouldContinue = false;
							else
								shouldContinue = true;

							//move = rect1.Y - (nfloat)keyboardYPosition + TextViewTopDistance;
							//shouldContinue = rect1.Y < topLayoutGuide;

							//if (shouldContinue)
							//	move = rect1.Y - (nfloat)keyboardYPosition + TextViewTopDistance;

							if (rect1.Y - innerScrollValue < topLayoutGuide)
								move = rect1.Y - innerScrollValue - (nfloat)topLayoutGuide;
							else if (rect1.Y - innerScrollValue > (nfloat)keyboardYPosition)
								move = rect1.Y - innerScrollValue - (nfloat)keyboardYPosition;
						}

						//if (shouldContinue)
						//	move = (nfloat)Math.Min(0, viewRectInRootSuperviewMinY - topLayoutGuide);

						Console.WriteLine($"Entered is NOT isNonScrollableTextView and new move is: {move}");
						Console.WriteLine($"shouldContinue: {shouldContinue}");
					}
				}

				// Go up the hierarchy and look for other scrollViews until we reach the UIWindow
				if (shouldContinue)
				{
					var tempScrollView = superScrollView.FindResponder<MauiScrollView>();
					UIScrollView? nextScrollView = null;

					Console.WriteLine($"tempScrollView: {tempScrollView}");

					// set tempScrollView to next scrollable superview of superScrollView
					while (tempScrollView is not null)
					{
						if (tempScrollView.ScrollEnabled && !ShouldIgnoreScrollingAdjustment)
						{
							nextScrollView = tempScrollView;
							break;
						}
						tempScrollView = tempScrollView.FindResponder<MauiScrollView>();
					}

					Console.WriteLine($"nextScrollView: {nextScrollView}");

					// Get the lastViewRect
					//if (lastView.Superview.ConvertRectToView(lastView.Frame, superScrollView) is CGRect lastViewRect)
					if (localViewRect is CGRect rect3 && superScrollView.ConvertRectFromView(rect3, superScrollView) is CGRect cursorRect)
					{
						var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);
						Console.WriteLine($"shouldOffsetY Before: {shouldOffsetY}");

						//if (viewRect is CGRect rect2)
							//shouldOffsetY = Math.Min(shouldOffsetY, cursorRect.Y);
						//else if (isNonScrollableTextView)
						//	shouldOffsetY = Math.Min(shouldOffsetY, lastViewRect.GetMaxY() - visibleHeight + bottomLayoutGuide);
						//else
						//	shouldOffsetY = Math.Min(shouldOffsetY, lastViewRect.GetMinY());

						Console.WriteLine($"shouldOffsetY After: {shouldOffsetY}");

						if (isTextView && !isNonScrollableTextView && nextScrollView is null && shouldOffsetY >= 0)
						{
							//// currentTextFieldViewRect is rectangle in regards to window bounds
							//if (view.Superview.ConvertRectToView(view.Frame, window) is CGRect currentTextFieldViewRect)
							//{
							//	// figure out if we need to consider the navigation bar in our offset
							//	var expectedFixDistance = currentTextFieldViewRect.GetMinY() - topLayoutGuide;

							//	shouldOffsetY = Math.Min(shouldOffsetY, superScrollView.ContentOffset.Y + expectedFixDistance);

							//	// no need to move now, contentOffset will handle the moving logic
							//	move = 0;

							//	Console.WriteLine($"isTextView, move is now == 0, shouldOffetY: {shouldOffsetY}");
							//}
							//else
							//{
								// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
								move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

								Console.WriteLine($"isTextView #2, shouldOffetY: {shouldOffsetY}");
								Console.WriteLine($"move : {move}");
							//}
						}

						else
						{
							// the superScrollView will use shouldOffsetY in it's offset, so we can remove it from the move distance
							move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

							Console.WriteLine($"not TextView with other conditions met, move is now: {move}");
							Console.WriteLine($"superScrollView.ContentOffset.Y: {superScrollView.ContentOffset.Y}");
							Console.WriteLine($"shouldOffsetY: {shouldOffsetY}");
						}

						var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

						Console.WriteLine($"newContentOffset: {newContentOffset}");
						Console.WriteLine($"superScrollView.ContentOffset: {superScrollView.ContentOffset}");

						if (!superScrollView.ContentOffset.Equals(newContentOffset))
						{
							if (nextScrollView is null)
							{
								UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
								{
									Console.WriteLine($"!!!ANIMATING SOMETHING #3!!!");
									newContentOffset.Y += innerScrollValue;
									innerScrollValue = 0;

									Console.WriteLine($"nextScrollView is null");
									Console.WriteLine($"superScrollView offset changing by: {superScrollView.ContentOffset.Y - newContentOffset.Y}");
									Console.WriteLine($"innerScrollValue: {innerScrollValue}");

									if (view.FindResponder<UIStackView>() is UIStackView)
										superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
									else
										superScrollView.ContentOffset = newContentOffset;
								}, () =>
								{
									if (superScrollView is UITableView || superScrollView is UICollectionView)
									{
										AddToolbarIfRequired();
									}
								});
							}

							else
							{

								//move += newContentOffset.Y;
								innerScrollValue += newContentOffset.Y;
								Console.WriteLine($"nextScrollView is not null and innerScrollValue: {innerScrollValue}");

							}
						}
					}
					lastView = superScrollView;
					superScrollView = nextScrollView;

					Console.WriteLine($"lastView: {lastView}");
					Console.WriteLine($"superScrollView: {superScrollView}");
					debuggingIteration++;
					Console.WriteLine("\n\n");
				}

				else
				{
					//move = 0;
					move += innerScrollValue;
					Console.WriteLine($"move: {move}");
					break;
				}
			}

			// What exactly is this block below doing?
			// No noticable negative difference without applying the animation here

			// maybe this is handling scrolling the LastScrollView if we don't need to scroll parents? 
			if (LastScrollView.Superview.ConvertRectToView(LastScrollView.Frame, window) is CGRect lastScrollViewRect
				&& !ShouldIgnoreContentInsetAdjustment)
			{
				var bottomInset = kbSize.Height - window.Frame.Height + lastScrollViewRect.GetMaxY();
				var bottomScrollIndicatorInset = bottomInset - specialKeyboardDistanceFromTextField;

				// update insets in case the offset is near the bottom on the scroll view
				bottomInset = (nfloat)Math.Max(StartingContentInsets.Bottom, bottomInset);
				bottomScrollIndicatorInset = (nfloat)Math.Max(StartingScrollIndicatorInsets.Bottom, bottomScrollIndicatorInset);

				if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
				{
					bottomInset -= LastScrollView.SafeAreaInsets.Bottom;
					bottomScrollIndicatorInset -= LastScrollView.SafeAreaInsets.Bottom;
				}

				var movedInsets = LastScrollView.ContentInset;
				movedInsets.Bottom = bottomInset;

				Console.WriteLine($"LastScrollView.ContentInset: {LastScrollView.ContentInset}");
				Console.WriteLine($"movedInsets.Bottom: {movedInsets.Bottom}");
				Console.WriteLine($"Would have animated the LastScrollView.ContentInset.Bottom by {LastScrollView.ContentInset.Bottom - movedInsets.Bottom}");


				if (LastScrollView.ContentInset != movedInsets)
				{
					//UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					//{
					//	Console.WriteLine($"!!!ANIMATING SOMETHING #6!!!");
					//	Console.WriteLine($"Before - LastScrollView.ContentInset: {LastScrollView.ContentInset}");
					//	LastScrollView.ContentInset = movedInsets;
					//	Console.WriteLine($"After - LastScrollView.ContentInset: {LastScrollView.ContentInset}");

					//	UIEdgeInsets newScrollIndicatorInsets;

					//	if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
					//		newScrollIndicatorInsets = LastScrollView.VerticalScrollIndicatorInsets;
					//	else
					//		newScrollIndicatorInsets = LastScrollView.ScrollIndicatorInsets;

					//	newScrollIndicatorInsets.Bottom = bottomScrollIndicatorInset;
					//	Console.WriteLine($"newScrollIndicatorInsets: {newScrollIndicatorInsets}");

					//	LastScrollView.ScrollIndicatorInsets = newScrollIndicatorInsets;

					//}, () => { });
				}
			}
		}

		//// readjust contentInset when the textView height is too large for the screen
		//if (view is UIScrollView textView && textView.ScrollEnabled && view is UITextView tview && tview.Editable)
		//{
		//	var rootSuperViewFrameInWindow = window.Frame;
		//	if (rootController.Superview is UIView v)
		//		rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		//	var keyboardOverlapping = rootSuperViewFrameInWindow.GetMaxY() - keyboardYPosition;

		//	// how much of the textView can fit on the screen with the keyboard up
		//	var textViewHeight = Math.Min(textView.Frame.Height, rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping - specialKeyboardDistanceFromTextField);

		//	Console.WriteLine($"keyboardYPosition: {keyboardYPosition}");
		//	Console.WriteLine($"rootSuperViewFrameInWindow: {rootSuperViewFrameInWindow}");
		//	Console.WriteLine($"keyboardOverlapping: {keyboardOverlapping}");
		//	Console.WriteLine($"textViewHeight: {textViewHeight}");
		//	Console.WriteLine($"textView.Frame.Height: {textView.Frame.Height}");

		//	// if the entire textview cannot fit on the screen
		//	if (textView.Frame.Size.Height - textView.ContentInset.Bottom > textViewHeight)
		//	{
		//		if (!IsTextViewContentInsetChanged)
		//		{
		//			StartingTextViewContentInsets = textView.ContentInset;
		//			if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
		//				StartingTextViewScrollIndicatorInsets = textView.VerticalScrollIndicatorInsets;
		//			else
		//				StartingTextViewScrollIndicatorInsets = textView.ScrollIndicatorInsets;
		//		}

		//		IsTextViewContentInsetChanged = true;

		//		var newContentInset = textView.ContentInset;

		//		Console.WriteLine($"Initial newContentInset: {newContentInset}");

		//		// set the bottom of the newContentInset to either the bottom of the textview
		//		// or the lowest point that the clicked portion will still fit on the screen

		//		// TJ - this sets the bottom of the textview to the top of the keyboard
		//		//newContentInset.Bottom = (nfloat)(textView.Frame.Size.Height - textViewHeight);

		//		// if the text is going to still be below the keyboard, add to the newContentInset.Bottom
		//		if (TextViewRectLocal is CGRect localViewRect && localViewRect.Y > textViewHeight)
		//		{
		//			//if (TextViewRect is CGRect viewRect && textView.Frame.Size.Height > textViewHeight)
		//			//{
		//			//	newContentInset.Bottom = 2 * localViewRect.Y - (nfloat)textViewHeight - viewRect.Y + TextViewTopDistance;
		//			//}

		//			//else
		//			//{
		//			//	newContentInset.Bottom = localViewRect.Y - (nfloat)textViewHeight + TextViewTopDistance;
		//			//}

		//			if (TextViewRect is CGRect viewRect)
		//			{
		//				Console.WriteLine($"viewRect.Y: {viewRect.Y}");
		//			}

		//			Console.WriteLine($"localViewRect.Y: {localViewRect.Y}");

		//			newContentInset.Bottom = localViewRect.Y - (nfloat)textViewHeight + TextViewTopDistance;

		//			//newContentInset.Bottom = localViewRect.Y - (nfloat)textViewHeight + TextViewTopDistance;
		//			//newContentInset.Bottom = localViewRect.Y - (nfloat)textViewHeight + TextViewTopDistance;
		//			//var t = TextViewRect;
					
		//			Console.WriteLine($"first newContentInset.Bottom: {newContentInset.Bottom}");
		//		}

		//		//if (TextViewRect is CGRect viewRect && viewRect.Y > textViewHeight)
		//		//{
		//		//	newContentInset.Bottom = viewRect.Y - (nfloat)textViewHeight + TextViewTopDistance;
		//		//	Console.WriteLine($"viewRect.Y: {viewRect.Y}");
		//		//	Console.WriteLine($"first newContentInset.Bottom: {newContentInset.Bottom}");
		//		//}


		//		// TJ - when I thought the bottom was constant
		//		//if (TextViewRectLocal is CGRect localViewRect && textViewHeight < textView.Frame.Size.Height - localViewRect.Y)
		//		//	newContentInset.Bottom = -(textView.Frame.Height - (nfloat)textViewHeight - localViewRect.Y); // (nfloat)viewRect.Y - TextViewTopDistance;

		//		if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
		//		{
		//			newContentInset.Bottom -= textView.SafeAreaInsets.Bottom;
		//			Console.WriteLine($"second newContentInset.Bottom: {newContentInset.Bottom}");
		//		}

		//		// Until this issue (https://github.com/dotnet/maui/issues/12485) is fixed, just move the entire parent scrollview
		//		move += newContentInset.Bottom;

		//		Console.WriteLine($"third newContentInset.Bottom: {newContentInset.Bottom}");
		//		Console.WriteLine($"move: {move}");

		//		//if (textView.ContentInset != newContentInset)
		//		//{
		//		//UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
		//		//{
		//		//	textView.ContentInset = newContentInset;
		//		//	textView.ScrollIndicatorInsets = newContentInset;
		//		//}, () => { });
		//		//}
		//	}
		//}

		//// readjust contentInset when the textView height is too large for the screen
		//if (view is UIScrollView textView && textView.ScrollEnabled && view is UITextView tview && tview.Editable)
		//{
		//	var keyboardYPosition = window.Frame.Height - kbSize.Height + specialKeyboardDistanceFromTextField;
		//	var rootSuperViewFrameInWindow = window.Frame;
		//	if (rootController.Superview is UIView v)
		//		rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		//	var keyboardOverlapping = rootSuperViewFrameInWindow.GetMaxY() - keyboardYPosition;

		//	// how much of the textView can fit on the screen with the keyboard up
		//	var textViewHeight = Math.Min(textView.Frame.Height, rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping);

		//	if (textView.Frame.Size.Height - textView.ContentInset.Bottom > textViewHeight)
		//	{
		//		if (!IsTextViewContentInsetChanged)
		//		{
		//			StartingTextViewContentInsets = textView.ContentInset;
		//			if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
		//				StartingTextViewScrollIndicatorInsets = textView.VerticalScrollIndicatorInsets;
		//			else
		//				StartingTextViewScrollIndicatorInsets = textView.ScrollIndicatorInsets;
		//		}

		//		IsTextViewContentInsetChanged = true;

		//		var newContentInset = textView.ContentInset;
		//		newContentInset.Bottom = (nfloat)(textView.Frame.Size.Height - textViewHeight);

		//		if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
		//			newContentInset.Bottom -= textView.SafeAreaInsets.Bottom;

		//		if (textView.ContentInset != newContentInset)
		//		{
		//			// Until this issue (https://github.com/dotnet/maui/issues/12485) is fixed, just move the entire parent scrollview

		//			//UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
		//			//{
		//			//	textView.ContentInset = newContentInset;
		//			//	textView.ScrollIndicatorInsets = newContentInset;
		//			//}, () => { });

		//			move += newContentInset.Bottom;
		//		}
		//	}
		//}

		if (move >= 0)
		{
			Console.WriteLine($"\n\n Last Section - positive or zero move");
			Console.WriteLine($"old rootViewOrigin.Y: {rootViewOrigin.Y}");

			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height + TextViewTopDistance));
			Console.WriteLine($"move: {move}");
			Console.WriteLine($"new rootViewOrigin.Y: {rootViewOrigin.Y}");


			// TJ - This is the difference that will be restored on exiting the keyboard focus
			if (rootController.Frame.X != rootViewOrigin.X || rootController.Frame.Y != rootViewOrigin.Y)
			{
				Console.WriteLine($"!!!ANIMATING SOMETHING #4!!!");
				UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
				{
					var rect = rootController.Frame;
					rect.X = rootViewOrigin.X;
					rect.Y = rootViewOrigin.Y;

					rootController.Frame = rect;

					if (LayoutifNeededOnUpdate)
					{
						rootController.SetNeedsLayout();
						rootController.LayoutIfNeeded();

					}
				}, () => { });
			}

			MovedDistance = (TopViewBeginOrigin.Y - rootViewOrigin.Y);
		} 

		// move is negative
		else
		{
			// if distburbDistance is negative - frame is disturbed
			// if distburbDistance is positive - frame is not disturbed
			var disturbDistance = rootViewOrigin.Y - TopViewBeginOrigin.Y;

			Console.WriteLine($"\n\n Last Section - positive or zero move");
			Console.WriteLine($"rootViewOrigin.Y: {rootViewOrigin.Y}");
			Console.WriteLine($"TopViewBeginOrigin.Y: {TopViewBeginOrigin.Y}");
			Console.WriteLine($"disturbDistance: {disturbDistance}");
			Console.WriteLine($"move: {move}");

			if (disturbDistance <= 0)
			{
				rootViewOrigin.Y -= (nfloat)Math.Max(move, disturbDistance);

				Console.WriteLine($"rootViewOrigin.Y: {rootViewOrigin.Y}");

				if (rootController.Frame.X != rootViewOrigin.X || rootController.Frame.Y != rootViewOrigin.Y)
				{
					Console.WriteLine($"!!!ANIMATING SOMETHING #5!!!");
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						var rect = rootController.Frame;
						rect.X = rootViewOrigin.X;
						rect.Y = rootViewOrigin.Y;

						rootController.Frame = rect;

						if (LayoutifNeededOnUpdate)
						{
							rootController.SetNeedsLayout();
							rootController.LayoutIfNeeded();

						}
					}, () => { });
				}
				MovedDistance = TopViewBeginOrigin.Y - rootViewOrigin.Y;
			}
		}
	}

	static void RestorePosition()
	{
		HasPendingAdjustRequest = false;

		if (rootController is not null && (rootController.Frame.X != TopViewBeginOrigin.X || rootController.Frame.Y != TopViewBeginOrigin.Y))
		{
			UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
			{
				var rect = rootController.Frame;
				rect.X = TopViewBeginOrigin.X;
				rect.Y = TopViewBeginOrigin.Y;

				rootController.Frame = rect;

				if (LayoutifNeededOnUpdate)
				{
					rootController.SetNeedsLayout();
					rootController.LayoutIfNeeded();

				}
			}, () => { });
		}

		MovedDistance = 0;

		rootController = null;
		TopViewBeginOrigin = InvalidPoint;
		RestoreRects();
	}

	static void RestoreRects()
	{
		TextViewRect = null;
		TextViewRectLocal = null;
		TextFieldRect = null;
		TextFieldRectLocal = null;
	}


	static NSIndexPath? GetPreviousIndexPath(this UITableView tableView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0)
				previousRow = (int)(tableView.NumberOfRowsInSection(previousSection) - 1);
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}

	static NSIndexPath? GetPreviousIndexPath(this UICollectionView collectionView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0)
				previousRow = (int)(collectionView.NumberOfItemsInSection(previousSection) - 1);
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}

	static void AddToolbarIfRequired ()
	{

	}


	// Add toolbar if it is required to add on textFields and it's siblings.
	//internal func addToolbarIfRequired()
	//{

	//	//Either there is no inputAccessoryView or if accessoryView is not appropriate for current situation(There is Previous/Next/Done toolbar).
	//	guard let siblings = responderViews(), !siblings.isEmpty,
	//             let textField = textFieldView, textField.responds(to: #selector(setter: UITextField.inputAccessoryView)),
	//             (textField.inputAccessoryView == nil ||
	//			textField.inputAccessoryView?.tag == IQKeyboardManager.kIQPreviousNextButtonToolbarTag ||
	//			textField.inputAccessoryView?.tag == IQKeyboardManager.kIQDoneButtonToolbarTag) else
	//	{
	//		return

	//	}

	//	let startTime = CACurrentMediaTime()

	//	showLog(">>>>> \(#function) started >>>>>", indentation: 1)


	//	showLog("Found \(siblings.count) responder sibling(s)")


	//	let rightConfiguration: IQBarButtonItemConfiguration


	//	if let doneBarButtonItemImage = toolbarDoneBarButtonItemImage {
	//		rightConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.doneAction(_:)))
	//       } else if let doneBarButtonItemText = toolbarDoneBarButtonItemText {
	//		rightConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.doneAction(_:)))
	//       } else
	//	{
	//		rightConfiguration = IQBarButtonItemConfiguration(barButtonSystemItem: .done, action: #selector(self.doneAction(_:)))
	//       }
	//	rightConfiguration.accessibilityLabel = toolbarDoneBarButtonItemAccessibilityLabel ?? "Done"

	//	//    If only one object is found, then adding only Done button.
	//	if (siblings.count <= 1 && previousNextDisplayMode == .default) || previousNextDisplayMode == .alwaysHide {

	//		textField.addKeyboardToolbarWithTarget(target: self, titleText: (shouldShowToolbarPlaceholder ? textField.drawingToolbarPlaceholder : nil), rightBarButtonConfiguration: rightConfiguration, previousBarButtonConfiguration: nil, nextBarButtonConfiguration: nil)


	//		textField.inputAccessoryView?.tag = IQKeyboardManager.kIQDoneButtonToolbarTag //  (Bug ID: #78)


	//	} else if previousNextDisplayMode == .default || previousNextDisplayMode == .alwaysShow {

	//		let prevConfiguration: IQBarButtonItemConfiguration


	//		if let doneBarButtonItemImage = toolbarPreviousBarButtonItemImage {
	//			prevConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.previousAction(_:)))
	//           } else if let doneBarButtonItemText = toolbarPreviousBarButtonItemText {
	//			prevConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.previousAction(_:)))
	//           } else
	//		{
	//			prevConfiguration = IQBarButtonItemConfiguration(image: (UIImage.keyboardPreviousImage() ?? UIImage()), action: #selector(self.previousAction(_:)))
	//           }
	//		prevConfiguration.accessibilityLabel = toolbarPreviousBarButtonItemAccessibilityLabel ?? "Previous"


	//		let nextConfiguration: IQBarButtonItemConfiguration


	//		if let doneBarButtonItemImage = toolbarNextBarButtonItemImage {
	//			nextConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.nextAction(_:)))
	//           } else if let doneBarButtonItemText = toolbarNextBarButtonItemText {
	//			nextConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.nextAction(_:)))
	//           } else
	//		{
	//			nextConfiguration = IQBarButtonItemConfiguration(image: (UIImage.keyboardNextImage() ?? UIImage()), action: #selector(self.nextAction(_:)))
	//           }
	//		nextConfiguration.accessibilityLabel = toolbarNextBarButtonItemAccessibilityLabel ?? "Next"


	//		textField.addKeyboardToolbarWithTarget(target: self, titleText: (shouldShowToolbarPlaceholder ? textField.drawingToolbarPlaceholder : nil), rightBarButtonConfiguration: rightConfiguration, previousBarButtonConfiguration: prevConfiguration, nextBarButtonConfiguration: nextConfiguration)


	//		textField.inputAccessoryView?.tag = IQKeyboardManager.kIQPreviousNextButtonToolbarTag //  (Bug ID: #78)

	//	}

	//	let toolbar = textField.keyboardToolbar

	//	//Setting toolbar tintColor //  (Enhancement ID: #30)
	//	toolbar.tintColor = shouldToolbarUsesTextFieldTintColor ? textField.tintColor : toolbarTintColor

	//	//  Setting toolbar to keyboard.
	//	if let textFieldView = textField as? UITextInput {

	//		//Bar style according to keyboard appearance
	//		switch textFieldView.keyboardAppearance {

	//			case .dark ?:
	//			toolbar.barStyle = .black

	//			toolbar.barTintColor = nil

	//		default:
	//				toolbar.barStyle = .default

	//			toolbar.barTintColor = toolbarBarTintColor

	//		}
	//	}

	//	//Setting toolbar title font.   //  (Enhancement ID: #30)
	//	if shouldShowToolbarPlaceholder, !textField.shouldHideToolbarPlaceholder {

	//		//Updating placeholder font to toolbar.     //(Bug ID: #148, #272)
	//		if toolbar.titleBarButton.title == nil ||
	//			toolbar.titleBarButton.title != textField.drawingToolbarPlaceholder {
	//			toolbar.titleBarButton.title = textField.drawingToolbarPlaceholder

	//		}

	//		//Setting toolbar title font.   //  (Enhancement ID: #30)
	//		toolbar.titleBarButton.titleFont = placeholderFont

	//		//Setting toolbar title color.   //  (Enhancement ID: #880)
	//		toolbar.titleBarButton.titleColor = placeholderColor

	//		//Setting toolbar button title color.   //  (Enhancement ID: #880)
	//		toolbar.titleBarButton.selectableTitleColor = placeholderButtonColor


	//	} else
	//	{
	//		toolbar.titleBarButton.title = nil

	//	}

	//	//In case of UITableView (Special), the next/previous buttons has to be refreshed everytime.    (Bug ID: #56)

	//	textField.keyboardToolbar.previousBarButton.isEnabled = (siblings.first != textField)   //    If firstTextField, then previous should not be enabled.

	//	textField.keyboardToolbar.nextBarButton.isEnabled = (siblings.last != textField)        //    If lastTextField then next should not be enaled.


	//	let elapsedTime = CACurrentMediaTime() - startTime

	//	showLog("<<<<< \(#function) ended: \(elapsedTime) seconds <<<<<", indentation: -1)

	//}
}


