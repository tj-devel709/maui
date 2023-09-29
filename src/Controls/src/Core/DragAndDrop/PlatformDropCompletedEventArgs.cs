using System;
namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the DropCompletedEventArgs
/// </summary>
public class PlatformDropCompletedEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView? Sender { get; }

	/// <summary>
	/// Gets the interaction used for dragging items.
	/// </summary>
	/// /// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.UIDragInteraction? DragInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drag session.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.IUIDragSession? DragSession { get; }

	/// <summary>
	/// Gets the value representing the response to a drop.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the SessionWillEnd method.
	/// </remarks>
	public UIKit.UIDropOperation? DropOperation { get; }

	/// <summary>
	/// Gets the interaction used for dropping items.
	/// </summary>
	/// /// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the PerformDrop method.
	/// </remarks>
	public UIKit.UIDropInteraction? DropInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drop session.
	/// </summary>
	/// <remarks>
	/// This property is used when <see cref="PlatformDropCompletedEventArgs"/> is called from the PerformDrop method.
	/// </remarks>
	public UIKit.IUIDropSession? DropSession { get; }

	internal Func<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.UITargetedDragPreview, UIKit.UITargetedDragPreview?>? PreviewForCancellingItem { get; private set; }
	internal Action<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.IUIDragAnimating>? WillAnimateCancel { get; private set; }

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDragInteraction dragInteraction,
		UIKit.IUIDragSession dragSession, UIKit.UIDropOperation dropOperation)
	{
		Console.WriteLine("CREATING PlatformDropCompletedEventArgs");
		Sender = sender;
		DragInteraction = dragInteraction;
		DragSession = dragSession;
		DropOperation = dropOperation;
	}

	internal PlatformDropCompletedEventArgs(UIKit.UIView? sender, UIKit.UIDropInteraction dropInteraction,
		UIKit.IUIDropSession dropSession)
	{
		Sender = sender;
		DropInteraction = dropInteraction;
		DropSession = dropSession;
	}

	// TODO Needs docs
	public void SetPreviewForCancellingItem(Func<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.UITargetedDragPreview, UIKit.UITargetedDragPreview?>? previewForCancellingItem)
	{
		PreviewForCancellingItem = previewForCancellingItem;
	}

	// TODO Needs docs
	public void SetWillAnimateCancel(Action<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.IUIDragAnimating>? willAnimateCancel)
	{
		WillAnimateCancel = willAnimateCancel;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Android.Views.View Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public Android.Views.DragEvent DragEvent { get; }

	internal PlatformDropCompletedEventArgs(Android.Views.View sender, Android.Views.DragEvent dragEvent)
	{
		Sender = sender;
		DragEvent = dragEvent;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.UIElement Sender { get; }

	/// <summary>
	/// Gets data for the DropCompleted event.
	/// </summary>
	public Microsoft.UI.Xaml.DropCompletedEventArgs DropCompletedEventArgs { get; }

	internal PlatformDropCompletedEventArgs(Microsoft.UI.Xaml.UIElement sender,
		Microsoft.UI.Xaml.DropCompletedEventArgs dropCompletedEventArgs)
	{
		Sender = sender;
		DropCompletedEventArgs = dropCompletedEventArgs;
	}

#else
	internal PlatformDropCompletedEventArgs()
	{
	}
#endif
}
