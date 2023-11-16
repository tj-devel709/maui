using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public abstract class MauiView : UIView, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IUIViewLifeCycleEvents
	{
		static bool? _respondsToSafeArea;
		//static bool _isTopLevelGrid;

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

		WeakReference<IView>? _reference;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool RespondsToSafeArea()
		{
			if (_respondsToSafeArea.HasValue)
				return _respondsToSafeArea.Value;
			return (bool)(_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets")));
		}

		CGRect originalGridBound;

		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			// only do things if we have a grid
			if (View is IGridLayout gridLayout)
			{
				// see if the grid has a star row
				bool hasStarRow = false;
				foreach (var row in gridLayout.RowDefinitions)
				{
					if (row.Height.GridUnitType == GridUnitType.Star)
					{
						hasStarRow = true;
						break;
					}
				}

				if (hasStarRow)
				{
					// if we are not currently scrolling (KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment finds just the
					// scrolling outside of scrollview scrolling so more precise version of the IsKeyboardAutoScrollHandling for this context),
					// then we save the current bounds of the grid
					if (!KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment && View?.ToPlatform() is UIView uiView)
					{
						var bottomSafeAreaHeight = uiView.SafeAreaInsets.Bottom;
						originalGridBound = new CGRect(bounds.X, bounds.Y, bounds.Width, bounds.Height - bottomSafeAreaHeight);
					}

					// apply the previous saved bounds or just the normal bounds
					return originalGridBound == CGRect.Empty ? bounds : originalGridBound;
				}

			}


			// issue, the safearea is applied to the bottom of the grid on set up, but when the grid
			// moves up, there is no longer a safe area on the bottom affecting things causing the grid
			// to size up


			// if we have a top level grid with a row of star height,
			// expand the row to not consider safe area so it doesn't resize
			// when the keyboard scrolls - https://github.com/dotnet/maui/issues/18354
			//if (View is IGridLayout gridLayout && gridLayout.Parent is IContentView contentView
			//	&& contentView.)
			//{
			//	Console.WriteLine($"Grid: {View} - Grid bounds: {bounds}");

			//	if (originalGridBound == CGRect.Empty && View.ToPlatform() is UIView uiView)
			//	{
			//		var bottomSafeAreaHeight = uiView.SafeAreaInsets.Bottom;
			//		originalGridBound = new CGRect(bounds.X, bounds.Y, bounds.Width, bounds.Height - bottomSafeAreaHeight);
			//	}

			//	return originalGridBound == CGRect.Empty ? bounds : originalGridBound;
			//}













			//// check the ContentPage to see if a grid with star sized row is top level
			//if (View is IContentView contentView && contentView.Content is IGridLayout gridLayout)
			//{
			//	var plat = contentView.ToPlatform();
			//	var p = contentView.Parent?.ToPlatform();
			//	foreach (var row in gridLayout.RowDefinitions)
			//	{
			//		if (row.Height.GridUnitType == GridUnitType.Star)
			//		{
			//			//_isTopLevelGrid = true;
			//			break;
			//		}
			//	}
			//}

			//if (KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling) // && ShouldIgnoreSafeAreaAdjustment)
			//{
			//	//Console.WriteLine($"View: {View} - safeArea");
			//	//return SafeAreaInsets.InsetRect(bounds);
			//	Console.WriteLine($"IsKeyboardAuto.. View: {View} - bounds");
			//	return bounds;
			//}

			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea || !RespondsToSafeArea())
			{
				Console.WriteLine($"Regular View: {View} - bounds: {bounds}");
				return bounds;
			}

#pragma warning disable CA1416 // TODO 'UIView.SafeAreaInsets' is only supported on: 'ios' 11.0 and later, 'maccatalyst' 11.0 and later, 'tvos' 11.0 and later.
			Console.WriteLine($"End View: {View} - safeArea {SafeAreaInsets.InsetRect(bounds)}");
			return SafeAreaInsets.InsetRect(bounds);
#pragma warning restore CA1416
		}

		protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		protected void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
		}

		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		public override void SafeAreaInsetsDidChange()
		{
			base.SafeAreaInsetsDidChange();

			if (View is ISafeAreaView2 isav2)
				isav2.SafeAreaInsets = this.SafeAreaInsets.ToThickness();
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = size.Width;
			var heightConstraint = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return crossPlatformSize.ToCGSize();
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_crossPlatformLayoutReference == null)
			{
				return;
			}

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();
			//var bounds = Bounds.ToRectangle();

			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
			// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
			// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.

			if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
			{
				CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			CrossPlatformArrange(bounds);
		}

		public override void SetNeedsLayout()
		{
			InvalidateConstraintsCache();
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		IVisualTreeElement? IVisualTreeElementProvidable.GetElement()
		{

			if (View is IVisualTreeElement viewElement &&
				viewElement.IsThisMyPlatformView(this))
			{
				return viewElement;
			}

			if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
				layoutElement.IsThisMyPlatformView(this))
			{
				return layoutElement;
			}

			return null;
		}

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}
