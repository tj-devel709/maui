using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class PointerEventArgsExtensions
{
	public static UIHoverGestureRecognizer? ToPlatform(this PointerEventArgs args)
	{
		return args._gestureRecognizer as UIHoverGestureRecognizer;
	}
}
