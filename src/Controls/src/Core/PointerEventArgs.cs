using System;
using Microsoft.Maui.Graphics;

#if IOS
using RecognizerType = UIKit.UIHoverGestureRecognizer;
#elif ANDROID
using RecognizerType = Android.Views.MotionEvent;
#else
using RecognizerType = System.Object;
#endif

namespace Microsoft.Maui.Controls
{
	/// <summary>
	///	Arguments for PointerGestureRecognizer events.
	/// </summary>
	public class PointerEventArgs : EventArgs
	{

		Func<IElement?, Point?>? _getPosition;
		//internal object? _gestureRecognizer;

#pragma warning disable RS0016 // Add public types and members to the declared API
		public RecognizerType? Recognizer { get; set; }
#pragma warning restore RS0016 // Add public types and members to the declared API

		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition, RecognizerType? recognizer = null)
		{
			_getPosition = getPosition;
			Recognizer = recognizer;
		}

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}