using System;
namespace Microsoft.Maui.Controls;

#pragma warning disable RS0016 // Add public types and members to the declared API
public class PlatformDropCompletedEventArgs
{
#if IOS || MACCATALYST
	public UIKit.UIView? Sender { get; set; }
	// If coming from SessionWillEnd(UIDragInteraction interaction, IUIDragSession session, UIDropOperation operation)
	public UIKit.UIDragInteraction? DragInteraction { get; set; }
	public UIKit.IUIDragSession? DragSession { get; set; }
	public UIKit.UIDropOperation? DropOperation { get; set; }

	// If coming from PerformDrop(UIDropInteraction interaction, IUIDropSession session)
	public UIKit.UIDropInteraction? DropInteraction { get; set; }
	public UIKit.IUIDropSession? DropSession { get; set; }

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDragInteraction dragInteraction,
		UIKit.IUIDragSession dragSession, UIKit.UIDropOperation dropOperation)
	{
		Sender = sender;
		DragInteraction = dragInteraction;
		DragSession = dragSession;
		DropOperation = dropOperation;
	}

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDropInteraction dropInteraction,
		UIKit.IUIDropSession? dropSession)
	{
		Sender = sender;
		DropInteraction = dropInteraction;
		DropSession = dropSession;
	}

#elif ANDROID
	public Android.Views.View Sender { get; set; }
	public Android.Views.DragEvent DragEvent { get; set; }

	internal PlatformDropCompletedEventArgs(Android.Views.View sender, Android.Views.DragEvent dragEvent)
	{
		Sender = sender;
		DragEvent = dragEvent;
	}

#elif WINDOWS

	// double check the class
	public Microsoft.UI.Xaml.UIElement Sender { get; set; }
	public Microsoft.UI.Xaml.DropCompletedEventArgs DropCompletedEventArgs { get; set; }

	internal PlatformDropCompletedEventArgs(Microsoft.UI.Xaml.UIElement sender,
		Microsoft.UI.Xaml.DropCompletedEventArgs dropCompletedEventArgs)
	{
		Sender = sender;
		DropCompletedEventArgs = dropCompletedEventArgs;
	}
#endif
}
