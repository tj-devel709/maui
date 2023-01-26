﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		internal const string BackgroundLayerName = "MauiBackgroundLayer";

		public static void UpdateIsEnabled(this UIView platformView, IView view)
		{
			if (platformView is not UIControl uiControl)
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void Focus(this UIView platformView, FocusRequest request)
		{
			request.IsFocused = platformView.BecomeFirstResponder();
		}

		public static void Unfocus(this UIView platformView, IView view)
		{
			platformView.ResignFirstResponder();
		}

		public static void UpdateVisibility(this UIView platformView, IView view) =>
			ViewExtensions.UpdateVisibility(platformView, view.Visibility);

		public static void UpdateVisibility(this UIView platformView, Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Visible:
					platformView.Inflate();
					platformView.Hidden = false;
					break;
				case Visibility.Hidden:
					platformView.Inflate();
					platformView.Hidden = true;
					break;
				case Visibility.Collapsed:
					platformView.Hidden = true;
					platformView.Collapse();
					break;
			}
		}

		public static void UpdateBackground(this ContentView platformView, IBorderStroke border)
		{
			bool hasShape = border.Shape != null;

			if (hasShape)
			{
				platformView.UpdateMauiCALayer(border);
			}
		}

		public static void UpdateBackground(this UIView platformView, IView view)
		{
			platformView.UpdateBackground(view.Background, view as IButtonStroke);
		}

		public static void UpdateBackground(this UIView platformView, Paint? paint, IButtonStroke? stroke = null)
		{
			// Remove previous background gradient layer if any
			platformView.RemoveBackgroundLayer();

			if (paint.IsNullOrEmpty())
			{
				if (platformView is LayoutView)
					platformView.BackgroundColor = null;
				else
					return;
			}


			if (paint is SolidPaint solidPaint)
			{
				Color backgroundColor = solidPaint.Color;

				if (backgroundColor == null)
					platformView.BackgroundColor = ColorExtensions.BackgroundColor;
				else
					platformView.BackgroundColor = backgroundColor.ToPlatform();

				return;
			}
			else if (paint is GradientPaint gradientPaint)
			{
				var backgroundLayer = gradientPaint?.ToCALayer(platformView.Bounds);

				if (backgroundLayer != null)
				{
					backgroundLayer.Name = BackgroundLayerName;
					platformView.BackgroundColor = UIColor.Clear;

					backgroundLayer.UpdateLayerBorder(stroke);

					platformView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}
		}

		public static void UpdateFlowDirection(this UIView platformView, IView view)
		{
			UISemanticContentAttribute updateValue = platformView.SemanticContentAttribute;

			switch (view.FlowDirection)
			{
				case FlowDirection.MatchParent:
					updateValue = GetParentMatchingSemanticContentAttribute(view);
					break;
				case FlowDirection.LeftToRight:
					updateValue = UISemanticContentAttribute.ForceLeftToRight;
					break;
				case FlowDirection.RightToLeft:
					updateValue = UISemanticContentAttribute.ForceRightToLeft;
					break;
			}

			if (updateValue != platformView.SemanticContentAttribute)
			{
				platformView.SemanticContentAttribute = updateValue;

				if (view is ITextAlignment)
				{
					// A change in flow direction may mean a change in text alignment
					view.Handler?.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment));
				}

				PropagateFlowDirection(updateValue, view);
			}
		}

		static UISemanticContentAttribute GetParentMatchingSemanticContentAttribute(IView view)
		{
			var parent = view.Parent?.Handler?.PlatformView as UIView;

			if (parent == null)
			{
				// No parent, no direction we need to match
				return UISemanticContentAttribute.Unspecified;
			}

			var parentSemanticContentAttribute = parent.SemanticContentAttribute;

			if (parentSemanticContentAttribute == UISemanticContentAttribute.ForceLeftToRight
				|| parentSemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				return parentSemanticContentAttribute;
			}

			// The parent view isn't using an explicit direction, so there's nothing for us to match
			return UISemanticContentAttribute.Unspecified;
		}

		static void PropagateFlowDirection(UISemanticContentAttribute semanticContentAttribute, IView view)
		{
			if (semanticContentAttribute != UISemanticContentAttribute.ForceLeftToRight
				&& semanticContentAttribute != UISemanticContentAttribute.ForceRightToLeft)
			{
				// If the current view isn't using an explicit LTR/RTL value, there's nothing to propagate
				return;
			}

			// If this view has any child/content views, we'll need to call UpdateFlowDirection on them
			// because they _may_ need to update their FlowDirection to match this view

			if (view is IContainer container)
			{
				foreach (var child in container)
				{
					if (child.Handler?.PlatformView is UIView uiView)
					{
						uiView.UpdateFlowDirection(child);
					}
				}
			}
			else if (view is IContentView contentView
				&& contentView.PresentedContent is IView child)
			{
				if (child.Handler?.PlatformView is UIView uiView)
				{
					uiView.UpdateFlowDirection(child);
				}
			}
		}

		public static void UpdateOpacity(this UIView platformView, IView view)
		{
			platformView.Alpha = (float)view.Opacity;
		}

		public static void UpdateAutomationId(this UIView platformView, IView view) =>
			platformView.AccessibilityIdentifier = view.AutomationId;

		public static void UpdateClip(this UIView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this UIView platformView, IView view)
		{
			var shadow = view.Shadow;
			var clip = view.Clip;

			// If there is a clip shape, then the shadow should be applied to the clip layer, not the view layer
			if (clip == null)
			{
				if (shadow == null)
					platformView.ClearShadow();
				else
					platformView.SetShadow(shadow);
			}
			else
			{
				if (platformView is WrapperView wrapperView)
					wrapperView.Shadow = view.Shadow;
			}
		}
		public static void UpdateBorder(this UIView platformView, IView view)
		{
			var border = (view as IBorder)?.Border;
			if (platformView is WrapperView wrapperView)
				wrapperView.Border = border;
		}

		public static T? FindDescendantView<T>(this UIView view) where T : UIView
		{
			var queue = new Queue<UIView>();
			queue.Enqueue(view);

			while (queue.Count > 0)
			{
				var descendantView = queue.Dequeue();

				if (descendantView is T result)
					return result;

				for (var i = 0; i < descendantView.Subviews?.Length; i++)
					queue.Enqueue(descendantView.Subviews[i]);
			}

			return null;
		}

		public static void UpdateBackgroundLayerFrame(this UIView view)
		{
			if (view == null || view.Frame.IsEmpty)
				return;

			var layer = view.Layer;

			if (layer == null || layer.Sublayers == null || layer.Sublayers.Length == 0)
				return;

			foreach (var sublayer in layer.Sublayers)
			{
				if (sublayer.Name == BackgroundLayerName && sublayer.Frame != view.Bounds)
				{
					sublayer.Frame = view.Bounds;
					break;
				}
			}
		}

		public static void InvalidateMeasure(this UIView platformView, IView view)
		{
			platformView.SetNeedsLayout();
			platformView.Superview?.SetNeedsLayout();
		}

		public static void UpdateWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMinimumHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMaximumHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMinimumWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMaximumWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateFrame(UIView platformView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}

			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default width/height will be taken care of by GetDesiredSize
			var currentFrame = platformView.Frame;
			platformView.Frame = new CoreGraphics.CGRect(currentFrame.X, currentFrame.Y, view.Width, view.Height);
		}

		public static async Task UpdateBackgroundImageSourceAsync(this UIView platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			if (imageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetImageAsync(imageSource);
				var backgroundImage = result?.Value;

				if (backgroundImage == null)
					return;

				platformView.BackgroundColor = UIColor.FromPatternImage(backgroundImage);
			}
		}

		public static int IndexOfSubview(this UIView platformView, UIView subview)
		{
			if (platformView.Subviews.Length == 0)
				return -1;

			return Array.IndexOf(platformView.Subviews, subview);
		}

		public static UIImage? ConvertToImage(this UIView view)
		{
			var imageRenderer = new UIGraphicsImageRenderer(view.Bounds.Size);

			return imageRenderer.CreateImage((a) =>
			{
				view.Layer.RenderInContext(a.CGContext);
			});
		}

		public static UINavigationController? GetNavigationController(this UIView view)
		{
			var rootController = view.Window?.RootViewController;
			if (rootController is UINavigationController nc)
				return nc;

			return rootController?.NavigationController;
		}

		internal static void Collapse(this UIView view)
		{
			// See if this view already has a collapse constraint we can use
			foreach (var constraint in view.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					// Active the collapse constraint; that will squish the view down to zero height
					collapseConstraint.Active = true;
					return;
				}
			}

			// Set up a collapse constraint and turn it on
			var collapse = new CollapseConstraint();
			view.AddConstraint(collapse);
			collapse.Active = true;
		}

		internal static bool Inflate(this UIView view)
		{
			// Find and deactivate the collapse constraint, if any; the view will go back to its normal height
			foreach (var constraint in view.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					collapseConstraint.Active = false;
					return true;
				}
			}

			return false;
		}

		public static void ClearSubviews(this UIView view)
		{
			for (int n = view.Subviews.Length - 1; n >= 0; n--)
			{
				view.Subviews[n].RemoveFromSuperview();
			}
		}

		internal static Rect GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
			{
				return new Rect();
			}

			return platformView.GetPlatformViewBounds();
		}

		internal static Rect GetPlatformViewBounds(this UIView platformView)
		{
			if (platformView == null)
				return new Rect();

			var superview = platformView;
			while (superview.Superview is not null)
			{
				superview = superview.Superview;
			}

			var convertPoint = platformView.ConvertRectToView(platformView.Bounds, superview);

			var X = convertPoint.X;
			var Y = convertPoint.Y;
			var Width = convertPoint.Width;
			var Height = convertPoint.Height;

			return new Rect(X, Y, Width, Height);
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return platformView.Layer.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this UIView view)
			=> view.Layer.GetViewTransform();

		internal static Point GetLocationOnScreen(this UIView view) =>
			view.GetPlatformViewBounds().Location;

		internal static Point? GetLocationOnScreen(this IElement element)
		{
			if (element.Handler?.MauiContext == null)
				return null;

			return (element.ToPlatform())?.GetLocationOnScreen();
		}

		internal static Graphics.Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetBoundingBox(this UIView? platformView)
		{
			if (platformView == null)
				return new Rect();
			var nvb = platformView.GetPlatformViewBounds();
			var transform = platformView.GetViewTransform();
			var radians = transform.ExtractAngleInRadians();
			var rotation = CoreGraphics.CGAffineTransform.MakeRotation((nfloat)radians);
			CGAffineTransform.CGRectApplyAffineTransform(nvb, rotation);
			return new Rect(nvb.X, nvb.Y, nvb.Width, nvb.Height);
		}

		internal static UIView? GetParent(this UIView? view)
		{
			return view?.Superview;
		}

		internal static Size LayoutToMeasuredSize(this IView view, double width, double height)
		{
			var size = view.Measure(width, height);
			var platformFrame = new CGRect(0, 0, size.Width, size.Height);

			if (view.Handler is IPlatformViewHandler viewHandler && viewHandler.PlatformView != null)
				viewHandler.PlatformView.Frame = platformFrame;

			view.Arrange(platformFrame.ToRectangle());
			return size;
		}

		public static void UpdateInputTransparent(this UIView platformView, IViewHandler handler, IView view)
		{
			if (view is ITextInput textInput)
			{
				platformView.UpdateInputTransparent(textInput.IsReadOnly, view.InputTransparent);
				return;
			}

			platformView.UserInteractionEnabled = !view.InputTransparent;
		}

		public static void UpdateInputTransparent(this UIView platformView, bool isReadOnly, bool inputTransparent)
		{
			platformView.UserInteractionEnabled = !(isReadOnly || inputTransparent);
		}


		internal static UIToolTipInteraction? GetToolTipInteraction(this UIView platformView)
		{
			UIToolTipInteraction? interaction = default;

			if (OperatingSystem.IsMacCatalystVersionAtLeast(15)
				|| OperatingSystem.IsIOSVersionAtLeast(15))
			{
				if (platformView is UIControl control)
				{
					interaction = control.ToolTipInteraction;
				}
				else
				{
					if (platformView.Interactions is not null)
					{
						foreach (var ia in platformView.Interactions)
						{
							if (ia is UIToolTipInteraction toolTipInteraction)
							{
								interaction = toolTipInteraction;
								break;
							}
						}
					}
				}
			}

			return interaction;
		}

		public static void UpdateToolTip(this UIView platformView, ToolTip? tooltip)
		{
			// UpdateToolTips were added in 15.0 for both iOS and MacCatalyst
			if (OperatingSystem.IsMacCatalystVersionAtLeast(15)
				|| OperatingSystem.IsIOSVersionAtLeast(15))
			{
				string? text = tooltip?.Content?.ToString();
				var interaction = platformView.GetToolTipInteraction();

				if (interaction is null)
				{
					if (!string.IsNullOrEmpty(text))
					{
						interaction = new UIToolTipInteraction(text);
						platformView.AddInteraction(interaction);
					}
				}
				else
				{
					interaction.DefaultToolTip = text;
				}
			}
		}

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as UIView);

		internal static IWindow? GetHostedWindow(this UIView? view)
			=> GetHostedWindow(view?.Window);

		internal static bool IsLoaded(this UIView uiView)
		{
			if (uiView == null)
				return false;

			return uiView.Window != null;
		}

		internal static IDisposable OnLoaded(this UIView uiView, Action action)
		{
			if (uiView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			Dictionary<NSString, NSObject> observers = new Dictionary<NSString, NSObject>();
			ActionDisposable? disposable = null;
			disposable = new ActionDisposable(() =>
			{
				disposable = null;
				foreach (var observer in observers)
				{
					uiView.Layer.RemoveObserver(observer.Value, observer.Key);
					observers.Remove(observer.Key);
				}
			});

			// Ideally we could wire into UIView.MovedToWindow but there's no way to do that without just inheriting from every single
			// UIView. So we just make our best attempt by observering some properties that are going to fire once UIView is attached to a window.			
			observers.Add(new NSString("bounds"), (NSObject)uiView.Layer.AddObserver("bounds", Foundation.NSKeyValueObservingOptions.OldNew, (oc) => OnLoadedCheck(oc)));
			observers.Add(new NSString("frame"), (NSObject)uiView.Layer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, (oc) => OnLoadedCheck(oc)));

			// OnLoaded is called at the point in time where the xplat view knows it's going to be attached to the window.
			// So this just serves as a way to queue a call on the UI Thread to see if that's enough time for the window
			// to get attached.
			uiView.BeginInvokeOnMainThread(() => OnLoadedCheck(null));

			void OnLoadedCheck(NSObservedChange? nSObservedChange = null)
			{
				if (disposable != null)
				{
					if (uiView.IsLoaded())
					{
						disposable.Dispose();
						disposable = null;
						action();
					}
					else if (nSObservedChange != null)
					{
						// In some cases (FlyoutPage) the arrange and measure all take place before
						// the view is added to the screen so this queues up a second check that
						// hopefully will fire loaded once the view is added to the window.
						// None of this code is great but I haven't found a better way
						// for an outside observer to know when a subview is added to a window
						uiView.BeginInvokeOnMainThread(() => OnLoadedCheck(null));
					}
				}
			};

			return disposable;
		}

		internal static IDisposable OnUnloaded(this UIView uiView, Action action)
		{

			if (!uiView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			Dictionary<NSString, NSObject> observers = new Dictionary<NSString, NSObject>();
			ActionDisposable? disposable = null;
			disposable = new ActionDisposable(() =>
			{
				disposable = null;
				foreach (var observer in observers)
				{
					uiView.Layer.RemoveObserver(observer.Value, observer.Key);
					observers.Remove(observer.Key);
				}
			});

			// Ideally we could wire into UIView.MovedToWindow but there's no way to do that without just inheriting from every single
			// UIView. So we just make our best attempt by observering some properties that are going to fire once UIView is attached to a window.	
			observers.Add(new NSString("bounds"), (NSObject)uiView.Layer.AddObserver("bounds", Foundation.NSKeyValueObservingOptions.OldNew, (_) => UnLoadedCheck()));
			observers.Add(new NSString("frame"), (NSObject)uiView.Layer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, (_) => UnLoadedCheck()));

			// OnUnloaded is called at the point in time where the xplat view knows it's going to be detached from the window.
			// So this just serves as a way to queue a call on the UI Thread to see if that's enough time for the window
			// to get detached.
			uiView.BeginInvokeOnMainThread(UnLoadedCheck);

			void UnLoadedCheck()
			{
				if (!uiView.IsLoaded() && disposable != null)
				{
					disposable.Dispose();
					disposable = null;
					action();
				}
			};

			return disposable;
		}

		internal static void UpdateLayerBorder(this CoreAnimation.CALayer layer, IButtonStroke? stroke)
		{
			if (stroke == null)
				return;

			if (stroke.StrokeColor != null)
				layer.BorderColor = stroke.StrokeColor.ToCGColor();

			if (stroke.StrokeThickness >= 0)
				layer.BorderWidth = (float)stroke.StrokeThickness;

			if (stroke.CornerRadius >= 0)
				layer.CornerRadius = stroke.CornerRadius;
		}

		internal static T? FindResponder<T>(this UIView view) where T : UIResponder
		{
			var nextResponder = view as UIResponder;
			while (nextResponder is not null)
			{
				nextResponder = nextResponder.NextResponder;

				if (nextResponder is T responder)
					return responder;
			}
			return null;
		}

		//internal static UIView? FindNextView(this UIView view, UIView superView, Func<UIView, bool> isValidType)
		//{
		//	// calculate the original CGRect parameters once here instead of multiple times later
		//	var originalRect = view.ConvertRectToView(view.Bounds, null);

		//	var isRtl = false;
		//	var foundBestSibling = false;
		//	var nextField = superView.FindNextView(originalRect, null, ref isRtl, isValidType, ref foundBestSibling);

		//	// wrap around to the top if we are at the end to mirror Xamarin.Forms behavior
		//	foundBestSibling = false;
		//	nextField ??= superView.FindNextView(new CGRect(float.MinValue, float.MinValue, 0, 0), null, ref isRtl, isValidType, ref foundBestSibling);

		//	return nextField;
		//}

		//static UIView? FindNextView(this UIView view, CGRect originalRect, UIView? currentBest, bool isRtl, Func<UIView, bool> isValidType, ref bool foundBestSibling)
		//{
		//	if (foundBestSibling)
		//		return currentBest;

		//	isRtl |= view.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft;

		//	foreach (var child in view.Subviews)
		//	{
		//		if (isValidType(child) && child.CanBecomeFirstResponder())
		//		{
		//			if (TryFindNewBestView(originalRect, currentBest, child, isRtl, out var newBest))
		//			{
		//				currentBest = newBest;
		//			}
		//		}

		//		else if (child.Subviews.Length > 0 && !child.Hidden && child.Alpha > 0f)
		//		{
		//			var newBestChild = child.FindNextView(originalRect, currentBest, isRtl, isValidType, false);
		//			if (newBestChild is not null && TryFindNewBestView(originalRect, currentBest, newBestChild, isRtl, out var newBest))
		//				currentBest = newBest;
		//		}
		//	}

		//	return currentBest;
		//}

		//static UIView? FindNextView(this UIView view, CGRect originalRect, UIView? currentBest, ref bool isRtl, Func<UIView, bool> isValidType, ref bool foundBestSibling)
		//{
		//	if (foundBestSibling)
		//		return currentBest;

		//	isRtl = view.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft;

		//	foreach (var child in view.Subviews)
		//	{
		//		if (isValidType(child) && child.CanBecomeFirstResponder())
		//		{
		//			if (TryFindNewBestView(originalRect, currentBest, child, ref isRtl, out var newBest))
		//			{
		//				currentBest = newBest;
		//			}
		//		}

		//		else if (child.Subviews.Length > 0 && !child.Hidden && child.Alpha > 0f)
		//		{
		//			var newBestChild = child.FindNextView(originalRect, currentBest, ref isRtl, isValidType, ref foundBestSibling);

		//			////if (newBestChild is not null &&
		//			////	child.SemanticContentAttribute != view.SemanticContentAttribute)
		//			//if (newBestChild is not null)
		//			//	//((isRtl && child.SemanticContentAttribute != UISemanticContentAttribute.ForceRightToLeft) ||
		//			//	//(!isRtl && child.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)))
		//			//{
		//			//	foundBestSibling = true;
		//			//	return newBestChild;
		//			//}

		//			if (newBestChild is not null && TryFindNewBestView(originalRect, currentBest, newBestChild, ref isRtl, out var newBest))
		//				currentBest = newBest;
		//		}
		//	}

		//	// we probably should be doing the following steps
		//	// recursing down and finding the best match inside the main elements
		//	// flow direction.
		//	// If we cannot find it inside the same flow direction, then pop out
		//	// and look inside the next elements that are in different flow directions?

		//	return currentBest;
		//}

		internal static UIView? FindNextView(this UIView view, UIView superView, Func<UIView, bool> isValidType)
		{
			// calculate the original CGRect parameters once here instead of multiple times later
			//var originalRect = view.ConvertRectToView(view.Bounds, null);

			var nextField = superView.FindNextView(view, null, isValidType);

			// wrap around to the top if we are at the end to mirror Xamarin.Forms behavior

			//nextField ??= superView.FindNextView(new UIView (new CGRect (double.MinValue, double.MinValue, 0, 0)), null, isValidType, out var _);

			//nextField ??= superView.FindNextView(null, null, isValidType);

			return nextField;
		}

		// first find the starting child and then decide on next based on parent and flow
		static UIView? FindNextView(this UIView view, UIView? origView, UIView? currentBest, Func<UIView, bool> isValidType)
		{
			var isRtl = view.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft;

			foreach (var child in view.Subviews)
			{
				if (isValidType(child) && child.CanBecomeFirstResponder())
				{
					if (isRtl && view.Subviews.Length > 1)
					{
						//var rtlRect = CreateRtlRect(child, view.Subviews);

						//if (TryFindNewBestView(origView, currentBest, rtlRect, isRtl, out var _))
						//	currentBest = child;
					}

					else
					{
						if (TryFindNewBestView(origView, currentBest, child, isRtl, out var newBest))
							currentBest = newBest;
					}
				}

				else if (child.Subviews.Length > 0 && !child.Hidden && child.Alpha > 0f)
				{
					var newBestChild = child.FindNextView(origView, currentBest, isValidType);

					if (newBestChild is not null && TryFindNewBestView(origView, currentBest, newBestChild, isRtl, out var newBest))
					{
						Console.WriteLine("!The above is the propogating up now!");
						currentBest = newBest;
					}
				}
			}

			return currentBest;
		}

		// TODO this seems expensive to do for every recursive call on an RTL element
		// I guess each rtl element would do this once and if we get
		// all the elements in a list, each individual rtl child would still need to be
		// changed similar to this so maybe this is okay?

		// return the element to the right of the target on the same line
		// out var is the element we should use to compare in the TryFindNewBestView
		static UIView? CreateRtlRect(UIView target, UIView[] siblings, out UIView? tempOrigView)
		{
			tempOrigView = null;

			// find the children that match the top dimension of child
			var similarHeightSiblings = new List<UIView>();

			var targetRect = target.ConvertRectToView(target.Bounds, null);

			foreach (var sibling in siblings)
			{
				var siblingRect = sibling.ConvertRectToView(sibling.Bounds, null);
				if (siblingRect.Top == targetRect.Top)
					similarHeightSiblings.Add(sibling);
			}

			// sort the new array/list of elements
			similarHeightSiblings.Sort(new ResponderSorter());

			// return the a_reverse[child] of the a[child]
			var index = similarHeightSiblings.FindIndex(t => t == target);

			//return similarHeightSiblings[^(index+1)];
			return similarHeightSiblings[index + 1];
		}

		class ResponderSorter : Comparer<UIView>
		{
			public override int Compare(UIView? view1, UIView? view2)
			{
				if (view1 is null || view2 is null)
					return 1;

				var bound1 = view1.ConvertRectToView(view1.Bounds, null);
				var bound2 = view2.ConvertRectToView(view2.Bounds, null);

				return bound1.Left > bound2.Left ? -1 : 1;
			}
		}

		static bool TryFindNewBestView(UIView? origView, UIView? currentBest, UIView newView, bool isRtl, out UIView newBest)
		{
			var originalRect = origView?.ConvertRectToView(origView.Bounds, null);
			var currentBestRect = currentBest?.ConvertRectToView(currentBest.Bounds, null);
			var newViewRect = newView.ConvertRectToView(newView.Bounds, null);

			var cbrValue = currentBestRect.GetValueOrDefault();
			var originalValue = originalRect.GetValueOrDefault();
			newBest = newView;

			if (isRtl && newViewRect == originalValue)
				return true;

			if (currentBestRect is not null && newViewRect.Top > cbrValue.Top)
			{
				return false;
			}

			else if ((originalRect is null || originalValue.Top < newViewRect.Top) &&
				(currentBestRect is null || newViewRect.Top < cbrValue.Top))
			{
				return true;
			}

			else if ((originalRect is null || (originalValue.Top == newViewRect.Top &&
					 originalValue.Left < newViewRect.Left)) &&
					 (currentBestRect is null || newViewRect.Left < cbrValue.Left))
			{
				return true;
			}

			return false;
		}

		//static bool TryFindNewBestView(UIView? origView, UIView? currentBest, UIView newView, bool isRtl, out UIView newBest)
		//{
		//	if (origView is null)
		//	{

		//	}

		//	var originalRect = origView?.ConvertRectToView(origView.Bounds, null);
		//	var currentBestRect = currentBest?.ConvertRectToView(currentBest.Bounds, null);
		//	var newViewRect = newView.ConvertRectToView(newView.Bounds, null);

		//	var cbrValue = currentBestRect.GetValueOrDefault();
		//	var originalValue = originalRect.GetValueOrDefault();
		//	newBest = newView;

		//	// Debugging bit here
		//	var cb = currentBest as UITextField;
		//	var nv = newView as UITextField;
		//	var cbText = cb?.Text;
		//	var nvText = nv?.Text;

		//	if (newView is UITextField t && currentBest is UITextField c && t.Text == "5" && c.Text != "5")
		//	{

		//	}

		//	if (newView is UITextField t2 && currentBest is null && t2.Text == "5")
		//	{

		//	}

		//	if (newView is UITextField t1 && t1.Text == "3")
		//	{

		//	}


		//	if (currentBestRect is not null && newViewRect.Top > cbrValue.Top)
		//	{
		//		if (origView is null)
		//		{
		//			Console.WriteLine($"NewView: {nvText ?? "null"} Not REPLACED BY CurrentBest: {cbText ?? "null"} at 1 - isRTL: {isRtl}");
		//		}
		//		return false;
		//	}



		//	else if ((originalRect is null || originalValue.Top < newViewRect.Top) &&
		//		(currentBestRect is null || newViewRect.Top < cbrValue.Top))
		//	{
		//		if (origView is null)
		//		{
		//			Console.WriteLine($"NewView: {nvText ?? "null"} -> CurrentBest: {cbText ?? "null"} at 2 - isRTL: {isRtl}");
		//		}

		//		return true;
		//	}

		//	else if (isRtl)
		//	{

		//		if ((originalRect is null || (originalValue.Top == newViewRect.Top &&
		//			 originalValue.Right > newViewRect.Right)) &&
		//			 (currentBestRect is null || newViewRect.Right > cbrValue.Right))
		//		{
		//			if (origView is null)
		//			{
		//				Console.WriteLine($"NewView: {nvText ?? "null"} -> CurrentBest: {cbText ?? "null"} at 3 - isRTL: {isRtl}");
		//			}
		//			return true;
		//		}
		//	}

		//	else if ((originalRect is null || (originalValue.Top == newViewRect.Top &&
		//			 originalValue.Left < newViewRect.Left)) &&
		//			 (currentBestRect is null || newViewRect.Left < cbrValue.Left))
		//	{
		//		if (origView is null)
		//		{
		//			Console.WriteLine($"NewView: {nvText ?? "null"} -> CurrentBest: {cbText ?? "null"} at 4 - isRTL: {isRtl}");
		//		}
		//		return true;
		//	}

		//	if (origView is null)
		//	{
		//		Console.WriteLine($"NewView: {nvText ?? "null"} Not REPLACED BY CurrentBest: {cbText ?? "null"} at 5 - isRTL: {isRtl}");
		//	}
		//	return false;
		//}

		internal static void ChangeFocusedView(this UIView view, UIView? newView)
		{
			if (newView is null)
				view.ResignFirstResponder();

			else
				newView.BecomeFirstResponder();
		}

		static bool CanBecomeFirstResponder(this UIView view)
		{
			var isFirstResponder = false;

			switch (view)
			{
				case UITextView tview:
					isFirstResponder = tview.Editable;
					break;
				case UITextField field:
					isFirstResponder = field.Enabled;
					break;
				// add in other control enabled properties here as necessary
				default:
					break;
			}

			return isFirstResponder && !view.Hidden && view.Alpha != 0f;
		}
	}
}
