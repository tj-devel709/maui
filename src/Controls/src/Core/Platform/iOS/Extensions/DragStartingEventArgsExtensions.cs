using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

#pragma warning disable RS0016
public static class DragStartingEventArgsExtensions
{
	public static DragGestureRecognizer? ToPlatform(this DragStartingEventArgs args)
	{
		//return args._gestureRecognizer as UIDragGestureRecognizer;
		return args._gestureRecognizer as DragGestureRecognizer;
		//return args._gestureRecognizer;
	}
}