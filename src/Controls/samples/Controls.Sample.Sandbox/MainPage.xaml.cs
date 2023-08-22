using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

#if IOS || MACCATALYST
	void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
	{
		var action = () =>
		{
			var previewParameters = new UIKit.UIDragPreviewParameters();
			var funkyPath = new UIKit.UIBezierPath();
			funkyPath.MoveTo(new CoreGraphics.CGPoint(0, 0));
			funkyPath.AddLineTo(new CoreGraphics.CGPoint(100, 0));
			funkyPath.AddLineTo(new CoreGraphics.CGPoint(75, 50));
			funkyPath.AddLineTo(new CoreGraphics.CGPoint(100, 100));
			funkyPath.AddLineTo(new CoreGraphics.CGPoint(0, 100));
			funkyPath.ClosePath();
			previewParameters.VisiblePath = funkyPath;

			return new UIKit.UIDragPreview(e.PlatformArgs.Sender, previewParameters);
		};
		e.PlatformArgs.SetPreviewProvider(action);
	}
#elif ANDROID
	void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
	{
		e.PlatformArgs.SetDragShadowBuilder(new FunkyShadow(e.PlatformArgs.Sender));
	}

	public class FunkyShadow : Android.Views.View.DragShadowBuilder
	{
		public FunkyShadow(Android.Views.View view) : base(view)
		{
		}

		public override void OnProvideShadowMetrics(Android.Graphics.Point shadowSize, Android.Graphics.Point shadowTouchPoint)
		{
			shadowSize.Set(200, 200); // Set the size of the shadow
			shadowTouchPoint.Set(100, 100); // Set the touch point within the shadow
		}
		
		public override void OnDrawShadow(Android.Graphics.Canvas canvas)
		{
			var funkyPath = new Android.Graphics.Path();
			funkyPath.MoveTo(0, 0);
			funkyPath.LineTo(200, 0);
			funkyPath.LineTo(150, 100);
			funkyPath.LineTo(200, 200);
			funkyPath.LineTo(0, 200);
			funkyPath.Close();

			var paint = new Android.Graphics.Paint();
			paint.Color = Android.Graphics.Color.Green;
			paint.SetStyle(Android.Graphics.Paint.Style.Fill);
			canvas.DrawPath(funkyPath, paint);
		}
	}
#else
	void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
	{ }
#endif

	void DragGestureRecognizer_DropCompleted(System.Object sender, Microsoft.Maui.Controls.DropCompletedEventArgs e)
	{
	}

	void DropGestureRecognizer_Drop(System.Object sender, Microsoft.Maui.Controls.DropEventArgs e)
	{
	}

	void DropGestureRecognizer_DragLeave(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
	{
	}

	void DropGestureRecognizer_DragOver(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
	{
#if IOS || MACCATALYST
		e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Forbidden));
#endif
	}
}