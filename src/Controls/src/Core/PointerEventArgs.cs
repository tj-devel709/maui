using System;
using Microsoft.Maui.Graphics;
#if MACCATALYST
using UIKit;
#endif
namespace Microsoft.Maui.Controls
{
	/// <summary>
	///	Arguments for PointerGestureRecognizer events.
	/// </summary>
	public class PointerEventArgs : EventArgs
	{

		Func<IElement?, Point?>? _getPosition;
#pragma warning disable RS0016 // Add public types and members to the declared API
		public object? ModifierFlags;
#pragma warning restore RS0016 // Add public types and members to the declared API

		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition)
		{
			_getPosition = getPosition;
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition, object? modifierFlags)
		{
			_getPosition = getPosition;
#if MACCATALYST
			if (modifierFlags is UIKeyModifierFlags modifiers)
				ModifierFlags = modifiers;
#endif
		}

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}