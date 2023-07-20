using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class PanUpdatedEventArgsExtensions
{
	public static UIPanGestureRecognizer? ToPlatform(this PanUpdatedEventArgs args)
	{
		return args._gestureRecognizer as UIPanGestureRecognizer;
	}
}
