﻿using System;
using Microsoft.VisualBasic;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Platform-specific arguments associated with the DragStartingEventArgs.
/// </summary>
public class PlatformDragStartingEventArgs
{
#if IOS || MACCATALYST
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public UIKit.UIView? Sender { get; }

	/// <summary>
	/// Gets the interaction used for dragging items.
	/// </summary>
	public UIKit.UIDragInteraction DragInteraction { get; }

	/// <summary>
	/// Gets the associated information from the drag session.
	/// </summary>
	public UIKit.IUIDragSession DragSession { get; }

	internal Foundation.NSItemProvider? ItemProvider { get; private set; }
	internal Func<UIKit.UIDragPreview?>? PreviewProvider { get; private set; }
	internal UIKit.UIDragItem[]? DragItems { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? PrefersFullSizePreviews { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.IUIDragSession, UIKit.UITargetedDragPreview?>? PreviewForLiftingItem { get; private set; }
	internal Action<UIKit.UIDragInteraction, UIKit.IUIDragSession>? SessionWillBegin { get; private set; }
	internal Action<UIKit.UIDragInteraction, UIKit.IUIDragAnimating, UIKit.IUIDragSession>? WillAnimateLift { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? SessionAllowsMoveOperation { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? SessionIsRestrictedToDraggingApplication { get; private set; }
	internal Action<UIKit.UIDragInteraction, UIKit.IUIDragSession>? SessionDidMove { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, CoreGraphics.CGPoint, UIKit.UIDragItem[]>? ItemsForAddingToSession { get; private set; }
	internal Func<UIKit.UIDragInteraction, UIKit.IUIDragSession[], CoreGraphics.CGPoint, UIKit.IUIDragSession?>? SessionForAddingItems { get; private set; }
	internal Action<UIKit.UIDragInteraction, UIKit.IUIDragSession, UIKit.UIDragItem[], UIKit.UIDragInteraction>? WillAddItems { get; private set; }

	internal PlatformDragStartingEventArgs(UIKit.UIView? sender, UIKit.UIDragInteraction dragInteraction,
		UIKit.IUIDragSession dragSession)
	{
		Console.WriteLine("CREATING PlatformDragStargingEventArgs");
		Sender = sender;
		DragInteraction = dragInteraction;
		DragSession = dragSession;
	}

	/// <summary>
	/// Sets the item provider when dragging begins.
	/// </summary>
	/// <param name="itemProvider">The custom item provider to use.</param>
	/// <remarks>
	/// This itemProvider will be applied to the MAUI generated dragItem.
	/// </remarks>
	public void SetItemProvider (Foundation.NSItemProvider itemProvider)
	{
		ItemProvider = itemProvider;
	}

	/// <summary>
	/// Sets the preview provider when dragging begins.
	/// </summary>
	/// <param name="previewProvider">The custom preview provider to use.</param>
	/// <remarks>
	/// This previewProvider will be applied to the MAUI generated dragItem.
	/// </remarks>
	public void SetPreviewProvider(Func<UIKit.UIDragPreview?> previewProvider)
	{
		PreviewProvider = previewProvider;
	}

	/// <summary>
	/// Sets the drag items when dragging begins.
	/// </summary>
	/// <param name="dragItems">The custom drag items to use.</param>
	/// <remarks>
	/// These dragItems will be used in place of the MAUI generated dragItem.
	/// </remarks>
	public void SetDragItems(UIKit.UIDragItem[] dragItems)
	{
		DragItems = dragItems;
	}

	/// <summary>
	/// Sets the func that requests to keep drag previews full-sized when dragging begins.
	/// </summary>
	/// <param name="prefersFullSizePreviews">Func that returns whether to request full size previews.</param>
	/// <remarks>
	/// The default behavior on iOS is to reduce the size of the drag shadow if not requested here.
	/// Even if requested, it is up to the system whether or not to fulfill the request.
	/// This method exists inside <see cref="PlatformDragStartingEventArgs"/> since the preview must
	/// have this value set when dragging begins.
	/// </remarks>
	public void SetPrefersFullSizePreviews(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? prefersFullSizePreviews)
	{
		PrefersFullSizePreviews = prefersFullSizePreviews;
	}

	// TODO Needs docs
	public void SetPreviewForLiftingItem(Func<UIKit.UIDragInteraction, UIKit.UIDragItem, UIKit.IUIDragSession, UIKit.UITargetedDragPreview?>? previewForLiftingItem)
	{
		PreviewForLiftingItem = previewForLiftingItem;
	}

	// TODO Needs docs
	public void SetWillAnimateLift(Action<UIKit.UIDragInteraction, UIKit.IUIDragAnimating, UIKit.IUIDragSession>? willAnimateLift)
	{
		WillAnimateLift = willAnimateLift;
	}

	// TODO Needs docs
	public void SetSessionWillBegin(Action<UIKit.UIDragInteraction, UIKit.IUIDragSession>? sessionWillBegin)
	{
		SessionWillBegin = sessionWillBegin;
	}

	// TODO Needs docs
	public void SetSessionAllowsMoveOperation(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? sessionAllowsMoveOperation)
	{
		SessionAllowsMoveOperation = sessionAllowsMoveOperation;
	}

	// TODO Needs docs
	public void SetSessionIsRestrictedToDraggingApplication(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, bool>? sessionIsRestrictedToDraggingApplication)
	{
		SessionIsRestrictedToDraggingApplication = sessionIsRestrictedToDraggingApplication;
	}

	// TODO Needs docs
	public void SetSessionDidMove(Action<UIKit.UIDragInteraction, UIKit.IUIDragSession>? sessionDidMove)
	{
		SessionDidMove = sessionDidMove;
	}

	// TODO Needs docs
	public void SetItemsForAddingToSession(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession, CoreGraphics.CGPoint, UIKit.UIDragItem[]>? itemsForAddingToSession)
	{
		ItemsForAddingToSession = itemsForAddingToSession;
	}

	// TODO Needs docs
	public void SetSessionForAddingItems(Func<UIKit.UIDragInteraction, UIKit.IUIDragSession[], CoreGraphics.CGPoint, UIKit.IUIDragSession?>? sessionForAddingItems)
	{
		SessionForAddingItems = sessionForAddingItems;
	}

	// TODO Needs docs
	public void SetWillAddItems(Action<UIKit.UIDragInteraction, UIKit.IUIDragSession, UIKit.UIDragItem[], UIKit.UIDragInteraction>? willAddItems)
	{
		WillAddItems = willAddItems;
	}

#elif ANDROID
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Android.Views.View Sender { get; }

	/// <summary>
	/// Gets the event containing information for drag and drop status.
	/// </summary>
	public Android.Views.MotionEvent MotionEvent { get; }

	internal Android.Views.View.DragShadowBuilder? DragShadowBuilder { get; private set; }
	internal Android.Content.ClipData? ClipData { get; private set; }
	internal Java.Lang.Object? LocalData { get; private set; }
	internal Android.Views.DragFlags? DragFlags { get; private set; }

	internal PlatformDragStartingEventArgs(Android.Views.View sender, Android.Views.MotionEvent motionEvent)
	{
		Sender = sender;
		MotionEvent = motionEvent;
	}

	/// <summary>
	/// Sets the drag shadow when dragging begins.
	/// </summary>
	/// <param name="dragShadowBuilder">The custom drag shadow builder to use.</param>
	public void SetDragShadowBuilder(Android.Views.View.DragShadowBuilder dragShadowBuilder)
	{
		DragShadowBuilder = dragShadowBuilder;
	}

	/// <summary>
	/// Sets the clip data when dragging begins.
	/// </summary>
	/// <param name="clipData">The custom clip data to use.</param>
	public void SetClipData(Android.Content.ClipData clipData)
	{
		ClipData = clipData;
	}

	/// <summary>
	/// Sets the local data when dragging begins.
	/// </summary>
	/// <param name="localData">The custom local data to use.</param>
	public void SetLocalData(Java.Lang.Object localData)
	{
		LocalData = localData;
	}

	/// <summary>
	/// Sets the drag flags when dragging begins.
	/// </summary>
	/// <param name="dragFlags">The custom drag flags to use.</param>
	public void SetDragFlags(Android.Views.DragFlags dragFlags)
	{
		DragFlags = dragFlags;
	}

#elif WINDOWS
	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public Microsoft.UI.Xaml.UIElement Sender { get; }

	/// <summary>
	/// Gets data for the DragStarting event.
	/// </summary>
	public Microsoft.UI.Xaml.DragStartingEventArgs DragStartingEventArgs { get; }

	/// <summary>
	/// Gets or sets a value that indicates whether the DragStartingEventArgs are changed.
	/// </summary>
	/// <remarks>
	/// Set this property's value to true when changing the DragStartingEventArgs so the system does not override the changes.
	/// </remarks>
	public bool Handled { get; set; }

	internal PlatformDragStartingEventArgs(Microsoft.UI.Xaml.UIElement sender,
		Microsoft.UI.Xaml.DragStartingEventArgs dragStartingEventArgs)
	{
		Sender = sender;
		DragStartingEventArgs = dragStartingEventArgs;
	}

#else
	internal PlatformDragStartingEventArgs()
	{
	}
#endif
}
