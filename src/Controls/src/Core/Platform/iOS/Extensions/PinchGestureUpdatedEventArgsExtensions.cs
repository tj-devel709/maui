using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class PinchGestureUpdatedEventArgsExtensions
{
	public static UIPinchGestureRecognizer? ToPlatform(this PinchGestureUpdatedEventArgs args)
	{
		return args._gestureRecognizer as UIPinchGestureRecognizer;
	}
}