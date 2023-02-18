using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManagerScroll
{
	static UIScrollView? LastScrollView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;

	static CGRect KeyboardFrame = CGRect.Empty;
	static CGPoint TopViewBeginOrigin = new CGPoint(nfloat.MaxValue, nfloat.MaxValue);
	static readonly CGPoint InvalidPoint = new CGPoint(nfloat.MaxValue, nfloat.MaxValue);
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

	// Set up the observers for the keyboard and the UITextField/UITextView
	internal static void Init()
	{
		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), async (notification) =>
		{
			Console.WriteLine($"entry observer started");
			if (notification.Object is not null)
			{
				View = (UIView)notification.Object;
				RootController = View.FindResponder<ContainerViewController>()?.View;
			}

			CursorRect = null;

			await Task.Delay(5);

			var v = View as UITextField;
			if (v is UITextField vf)
			{
				CGRect? localCursor = null;
				Console.WriteLine("Cursor started being updated");
				var selectedTextRange = vf.SelectedTextRange;
				if (selectedTextRange is UITextRange selectedRange)
				{
					localCursor = vf.GetCaretRectForPosition(selectedRange.Start);
					if (localCursor is CGRect local)
						CursorRect = vf.ConvertRectToView(local, null);
				}

				Console.WriteLine($"localCursor: {localCursor}");
				Console.WriteLine($"TextFieldRect: {CursorRect}");

				TextViewTopDistance = localCursor is CGRect cGRect ? 20 + (int)cGRect.Height : 20;

				Console.WriteLine($"TextViewTopDistance: {TextViewTopDistance}");
				Console.WriteLine("Cursor finished being updated");
			}

			Console.WriteLine($"entry calling AdjustPositionDebounce");
			AdjustPositionDebounce();
			Console.WriteLine($"entry finishing AdjustPositionDebounce");
		});

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), async (notification) =>
		{
			Console.WriteLine("Editor started observer");

			if (notification.Object is not null)
			{
				CursorRect = null;

				View = (UIView)notification.Object;

				RootController = View.FindResponder<ContainerViewController>()?.View;

				await Task.Delay(5);
				var v = View as UITextView;

				if (v is UITextView vt)
				{
					CGRect? localCursor = null;
					Console.WriteLine("Cursor started being updated");
					var selectedTextRange = vt.SelectedTextRange;
					if (selectedTextRange is UITextRange selectedRange)
					{
						localCursor = vt.GetCaretRectForPosition(selectedRange.Start);
						if (localCursor is CGRect local)
						{
							CursorRect = vt.ConvertRectToView(local, null);
						}

						Console.WriteLine($"TextViewRectLocal: {localCursor}");
						Console.WriteLine($"TextViewRect: {CursorRect}");

					}

					TextViewTopDistance = localCursor is CGRect cGRect ? 20 + (int)cGRect.Height : 20;
					Console.WriteLine($"TextViewTopDistance: {TextViewTopDistance}");
					Console.WriteLine("Cursor finished being updated");
				}

				Console.WriteLine($"editor calling AdjustPositionDebounce");
				AdjustPositionDebounce();
				Console.WriteLine($"editor finishing AdjustPositionDebounce");
			}

			Console.WriteLine($"Editor finished observer\n\n\n\n\n\n\n");
		});

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), (notification) =>
		{
			Console.WriteLine("Keyboard started observer");
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

			if (TopViewBeginOrigin == InvalidPoint && RootController is not null)
				TopViewBeginOrigin = new CGPoint(RootController.Frame.X, RootController.Frame.Y);

			Console.WriteLine($"Keyboard calling AdjustPositionDebounce");
			AdjustPositionDebounce();
			Console.WriteLine($"keyboard finishing AdjustPositionDebounce");

			if (!IsKeyboardShowing)
			{
				Console.WriteLine($"Keyboard setting IsKeyboardShowing - true");
				IsKeyboardShowing = true;
			}

			Console.WriteLine($"Keyboard finished observer\n\n\n\n\n\n\n");
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
				AnimateScroll(() =>
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
								if (View?.Superview is UIStackView)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}
						}
						superScrollView = superScrollView.Superview as UIScrollView;
					}
				});
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

	internal static async void AdjustPositionDebounce()
	{
		DebounceCount++;
		var entranceCount = DebounceCount;

		await Task.Delay(10);

		if (entranceCount == DebounceCount)
		{
			Console.WriteLine($"Calling AdjustPosition in Debounce - entranceCount-{entranceCount} | DebounceCount-{DebounceCount}");
			AdjustPosition();
			DebounceCount = 0;
		}
		else
		{
			Console.WriteLine($"Did not adjust Position in Debounce - entranceCount-{entranceCount} | DebounceCount-{DebounceCount}");
		}
	}

	internal static void AdjustPosition()
	{
		Console.WriteLine("AdjustPosition Started");
		if (View is not UITextField field && View is not UITextView)
			return;

		if (RootController is null)
			return;

		var rootFrame = RootController.Frame;
		var rootViewOrigin = new CGPoint(rootFrame.GetMinX(), rootFrame.GetMinY());
		var window = RootController.Window;

		var kbSize = KeyboardFrame.Size;
		var kbFrame = KeyboardFrame;
		var intersectRect = CGRect.Intersect(kbFrame, window.Frame);
		if (intersectRect == CGRect.Empty)
			kbSize = new CGSize(kbFrame.Width, 0);
		else
			kbSize = intersectRect.Size;

		// Set the StatusBarHeight and NavigationBarAreaHeight

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

		var layoutAreaHeight = RootController.LayoutMargins.Bottom;
		var isTextView = false;

		if (View is UIScrollView scrollView)
			isTextView = true;

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, layoutAreaHeight) + 5;
		var bottomLayoutGuide = isTextView ? 0 : RootController.LayoutMargins.Bottom;

		// the height that will be above the keyboard
		var visibleHeight = window.Frame.Height - kbSize.Height;

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewTopDistance;

		var cursorRectTemp = CursorRect;
		CGRect cursorRect;

		if (cursorRectTemp is CGRect cRect)
			cursorRect = cRect;
		else
			return;


		if (cursorRect.Y >= topLayoutGuide && cursorRect.Y < keyboardYPosition)
		{
			Console.WriteLine("returning the adjustposition since we are in visible area");
			return;
		}

		var viewRectInWindowMaxY = View.Superview.ConvertRectToView(View.Frame, window).GetMaxY();
		var viewRectInRootSuperviewMinY = View.Superview.ConvertRectToView(View.Frame, RootController.Superview).GetMinY();

		nfloat move = 0;

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (RootController.Superview is UIView v)
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		var keyboardOverlapping = rootSuperViewFrameInWindow.GetMaxY() - keyboardYPosition;

		var availableSpace = rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping;

		// how much of the textView can fit on the screen with the keyboard up
		var textViewHeight = Math.Min(View.Frame.Height, availableSpace);

		Console.WriteLine($"keyboardYPosition: {keyboardYPosition}");
		Console.WriteLine($"rootSuperViewFrameInWindow: {rootSuperViewFrameInWindow}");
		Console.WriteLine($"keyboardOverlapping: {keyboardOverlapping}");
		Console.WriteLine($"textViewHeight: {textViewHeight}");
		Console.WriteLine($"textView.Frame.Height: {View.Frame.Height}");

		if (cursorRect.Y > keyboardYPosition)
			move = cursorRect.Y - keyboardYPosition;

		else if (cursorRect.Y <= topLayoutGuide)
			move = cursorRect.Y - (nfloat)topLayoutGuide;

		Console.WriteLine($"new move: {move}");

		Console.WriteLine($"\n\n\n\n\n!!Starting!!");
		Console.WriteLine($"Initial move calculated: {move}");

		// Find the next highest scroll View

		UIScrollView? superScrollView = null;
		var superView = View.FindResponder<UIScrollView>();
		while (superView is not null)
		{
			if (superView.ScrollEnabled)
			{
				superScrollView = superView;
				break;
			}

			superView = superView.FindResponder<UIScrollView>();
		}

		// This is the case when the keyboard is already showing and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					Console.WriteLine($"!!!ANIMATING SOMETHING #1!!!");
					AnimateScroll(() =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					});
				}

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
			// and set the superScrolView as the LastScrollView
			else if (superScrollView != LastScrollView)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					AnimateScroll(() =>
					{
						Console.WriteLine($"!!!ANIMATING SOMETHING #2!!!");
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					});
				}

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

			if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
				StartingScrollIndicatorInsets = superScrollView.VerticalScrollIndicatorInsets;
			else
				StartingScrollIndicatorInsets = superScrollView.ScrollIndicatorInsets;
		}

		// if we found a LastScrollView above, then set the contentOffset to see textField
		if (LastScrollView is not null)
		{
			var lastView = View;
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
					Console.WriteLine($"Entered is TableView and move == 0");
					Console.WriteLine($"shouldContinue: {shouldContinue}");
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

					Console.WriteLine($"Entered is CollectionView and move == 0");
					Console.WriteLine($"shouldContinue: {shouldContinue}");
				}

				else
				{
					if (cursorRect.Y - innerScrollValue >= topLayoutGuide && cursorRect.Y - innerScrollValue <= keyboardYPosition)
						shouldContinue = false;
					else
						shouldContinue = true;

					if (cursorRect.Y - innerScrollValue < topLayoutGuide)
						move = cursorRect.Y - innerScrollValue - (nfloat)topLayoutGuide;
					else if (cursorRect.Y - innerScrollValue > keyboardYPosition)
						move = cursorRect.Y - innerScrollValue - keyboardYPosition;

					Console.WriteLine($"Entered is NOT isNonScrollableTextView and new move is: {move}");
					Console.WriteLine($"shouldContinue: {shouldContinue}");
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
						if (tempScrollView.ScrollEnabled)
						{
							nextScrollView = tempScrollView;
							break;
						}
						tempScrollView = tempScrollView.FindResponder<MauiScrollView>();
					}

					Console.WriteLine($"nextScrollView: {nextScrollView}");

					// Get the lastViewRect
					var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);
					Console.WriteLine($"shouldOffsetY Before: {shouldOffsetY}");

					Console.WriteLine($"shouldOffsetY After: {shouldOffsetY}");

					// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
					move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

					Console.WriteLine($"isTextView #2, shouldOffetY: {shouldOffsetY}");
					Console.WriteLine($"move : {move}");

					Console.WriteLine($"superScrollView.ContentOffset.Y: {superScrollView.ContentOffset.Y}");
					Console.WriteLine($"shouldOffsetY: {shouldOffsetY}");

					var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

					Console.WriteLine($"newContentOffset: {newContentOffset}");
					Console.WriteLine($"superScrollView.ContentOffset: {superScrollView.ContentOffset}");

					if (!superScrollView.ContentOffset.Equals(newContentOffset))
					{
						if (nextScrollView is null)
						{
							AnimateScroll(() =>
							{
								Console.WriteLine($"!!!ANIMATING SOMETHING #3!!!");
								newContentOffset.Y += innerScrollValue;
								innerScrollValue = 0;

								Console.WriteLine($"nextScrollView is null");
								Console.WriteLine($"superScrollView offset changing by: {superScrollView.ContentOffset.Y - newContentOffset.Y}");
								Console.WriteLine($"innerScrollValue: {innerScrollValue}");

								if (View.FindResponder<UIStackView>() is UIStackView)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							});
						}

						else
						{
							innerScrollValue += newContentOffset.Y;
							Console.WriteLine($"nextScrollView is not null and innerScrollValue: {innerScrollValue}");
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
					move += innerScrollValue;
					Console.WriteLine($"move: {move}");
					break;
				}
			}
		}

		if (move >= 0)
		{
			Console.WriteLine($"SuperScrollView at the end: {superScrollView}");
			Console.WriteLine($"LastScrollView at the end: {LastScrollView}");

			Console.WriteLine($"\n\n Last Section - positive or zero move");
			Console.WriteLine($"old rootViewOrigin.Y: {rootViewOrigin.Y}");

			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height - TextViewTopDistance));
			Console.WriteLine($"move: {move}");
			Console.WriteLine($"new rootViewOrigin.Y: {rootViewOrigin.Y}");

			if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
			{
				Console.WriteLine($"!!!ANIMATING SOMETHING #4!!!");
				AnimateScroll(() =>
				{
					var rect = RootController.Frame;
					rect.X = rootViewOrigin.X;
					rect.Y = rootViewOrigin.Y;

					RootController.Frame = rect;
				});
			}
		}

		// move is negative
		else
		{
			Console.WriteLine($"SuperScrollView at the end: {superScrollView}");
			Console.WriteLine($"LastScrollView at the end: {LastScrollView}");

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

				if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
				{
					Console.WriteLine($"!!!ANIMATING SOMETHING #5!!!");
					AnimateScroll(() =>
					{
						var rect = RootController.Frame;
						rect.X = rootViewOrigin.X;
						rect.Y = rootViewOrigin.Y;

						RootController.Frame = rect;
					});
				}
			}
		}
	}

	static void AnimateScroll(Action? action)
	{
		UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
		{
			action?.Invoke();
		}, () => { });

	}

	static void RestorePosition()
	{
		if (RootController is not null && (RootController.Frame.X != TopViewBeginOrigin.X || RootController.Frame.Y != TopViewBeginOrigin.Y))
		{
			AnimateScroll(() =>
			{
				var rect = RootController.Frame;
				rect.X = TopViewBeginOrigin.X;
				rect.Y = TopViewBeginOrigin.Y;

				RootController.Frame = rect;
			});
		}
		RootController = null;
		TopViewBeginOrigin = InvalidPoint;
		CursorRect = null;
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
}
