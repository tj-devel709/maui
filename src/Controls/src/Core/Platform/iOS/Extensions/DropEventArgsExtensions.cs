using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class DropEventArgsExtensions
{
	public static Object? ToPlatform(this DropEventArgs args)
	{
		//return args._gestureRecognizer as UIDropGestureRecognizer;
		return args._gestureRecognizer;
	}
}
