using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class SwipedEventArgsExtensions
{
	public static UISwipeGestureRecognizer? ToPlatform(this SwipedEventArgs args)
	{
		return args._gestureRecognizer as UISwipeGestureRecognizer;
	}
}