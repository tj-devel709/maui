#if __MOBILE__
using System;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	[SupportedOSPlatform("ios11.0")]
	class DragAndDropDelegate : NSObject, IUIDragInteractionDelegate, IUIDropInteractionDelegate
	{
		IPlatformViewHandler? _viewHandler;
		PlatformDragStartingEventArgs? _platformDragStartingEventArgs;

		public DragAndDropDelegate(IPlatformViewHandler viewHandler)
		{
			_viewHandler = viewHandler;
		}

		public void Disconnect()
		{
			_viewHandler = null;
		}

		[Export("dragInteraction:prefersFullSizePreviewsForSession:")]
		[Preserve(Conditional = true)]
		public bool PrefersFullSizePreviews(UIDragInteraction interaction, IUIDragSession session)
		{
			return _platformDragStartingEventArgs?.PrefersFullSizePreviews?.Invoke(interaction, session) ?? false;
		}

		[Export("dropInteraction:sessionDidEnd:")]
		[Preserve(Conditional = true)]
		public void SessionDidEnd(UIDropInteraction interaction, IUIDropSession session)
		{
			_platformDragStartingEventArgs = null;
		}

		[Export("dragInteraction:session:didEndWithOperation:")]
		public void SessionDidEnd(UIDragInteraction interaction, IUIDragSession session, UIDropOperation operation)
		{
			_platformDragStartingEventArgs = null;
		}

		[Export("dragInteraction:session:willEndWithOperation:")]
		[Preserve(Conditional = true)]
		public void SessionWillEnd(UIDragInteraction interaction, IUIDragSession session, UIDropOperation operation)
		{
			if ((operation == UIDropOperation.Cancel || operation == UIDropOperation.Forbidden) &&
				session.Items.Length > 0 &&
				session.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				this.HandleDropCompleted(cdi.View, new PlatformDropCompletedEventArgs(cdi?.View?.Handler?.PlatformView as UIView, interaction, session, operation));
			}
		}

		[Preserve(Conditional = true)]
		public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
		{
			return HandleDragStarting(_viewHandler?.VirtualView as View, _viewHandler, session, new PlatformDragStartingEventArgs(_viewHandler?.PlatformView, interaction, session));
		}

		[Export("dropInteraction:canHandleSession:")]
		[Preserve(Conditional = true)]
		public bool CanHandleSession(UIDropInteraction interaction, IUIDropSession session) => true;

		[Export("dropInteraction:sessionDidExit:")]
		[Preserve(Conditional = true)]
		public void SessionDidExit(UIDropInteraction interaction, IUIDropSession session)
		{
			DataPackage? package = null;

			if (session.LocalDragSession?.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				package = cdi.DataPackage;
			}


			HandleDragLeave(_viewHandler?.VirtualView as View, package, session.LocalDragSession, new PlatformDragEventArgs(_viewHandler?.PlatformView, interaction, session));
		}

		[Export("dropInteraction:sessionDidUpdate:")]
		[Preserve(Conditional = true)]
		public UIDropProposal SessionDidUpdate(UIDropInteraction interaction, IUIDropSession session)
		{
			UIDropOperation operation = UIDropOperation.Cancel;

			if (session.LocalDragSession == null)
				return new UIDropProposal(operation);

			DataPackage? package = null;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				package = cdi.DataPackage;
			}

			var platformArgs = new PlatformDragEventArgs(_viewHandler?.PlatformView, interaction, session);

			if (HandleDragOver(_viewHandler?.VirtualView as View, package, session.LocalDragSession, platformArgs))
			{
				if (platformArgs.DropProposal is not null)
					return platformArgs.DropProposal;

				operation = UIDropOperation.Copy;
			}

			return new UIDropProposal(operation);
		}

		[Export("dropInteraction:performDrop:")]
		[Preserve(Conditional = true)]
		public void PerformDrop(UIDropInteraction interaction, IUIDropSession session)
		{
			if (session.LocalDragSession?.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi &&
				_viewHandler?.VirtualView is View view)
			{
				HandleDrop(view, cdi.DataPackage, session, new PlatformDropEventArgs(cdi.View?.Handler?.PlatformView as UIView, interaction, session));
				HandleDropCompleted(cdi.View, new PlatformDropCompletedEventArgs(cdi.View?.Handler?.PlatformView as UIView, interaction, session));
			}
			else if (_viewHandler?.VirtualView is View v)
			{
				// if the developer added their own LocalObject, pass in null and still allow the HandleDrop to fire
				HandleDrop(v, null, session, new PlatformDropEventArgs(null, interaction, session));
			}
		}

		// PlatformDragStarting
		[Export("dropInteraction:sessionDidEnter:")]
		[Preserve(Conditional = true)]
		public void SessionDidEnter(UIDropInteraction interaction, IUIDropSession session)
		{
			Console.WriteLine("Delegate SessionDidEnter");
			//_platformDragStartingEventArgs?.SessionDidEnter?.Invoke(interaction, session);
		}

		// PlatformDropEvent
		[Export("dropInteraction:concludeDrop:")]
		[Preserve(Conditional = true)]
		public void ConcludeDrop(UIDropInteraction interaction, IUIDropSession session)
		{
			Console.WriteLine("Delegate ConcludeDrop");
			//_platformDragStartingEventArgs?.ConcludeDrop?.Invoke(interaction, session);
		}

		// PlatformDropEvent
		[Export("dropInteraction:previewForDroppingItem:withDefault:")]
		[Preserve(Conditional = true)]
		public UITargetedDragPreview? GetPreviewForDroppingItem(UIDropInteraction interaction, UIDragItem item, UITargetedDragPreview defaultPreview)
		{
			Console.WriteLine("Delegate GetPreviewForDroppingItem");
			return null;
			//return _platformDragStartingEventArgs?.GetPreviewForDroppingItem?.Invoke(interaction, item, defaultPreview) ?? null;
		}

		// PlatformDropEvent
		[Export("dropInteraction:item:willAnimateDropWithAnimator:")]
		[Preserve(Conditional = true)]
		public void WillAnimateDrop(UIDropInteraction interaction, UIDragItem item, IUIDragAnimating animator)
		{
			Console.WriteLine("Delegate WillAnimateDrop");
			//_platformDragStartingEventArgs?.WillAnimateDrop?.Invoke(interaction, item, animator);
		}

		// If we have this method, it will use the preview provided here,
		// and if not, it will use the drag interactions preview.
		// TODO how can we use the items.PreviewProvider (UIDragItem) as the default when
		// the type here is UITargetedDragPreview

		// PlatformDragStarting
		[Export("dragInteraction:previewForLiftingItem:session:")]
		[Preserve(Conditional = true)]
		public UITargetedDragPreview? GetPreviewForLiftingItem(UIDragInteraction interaction, UIDragItem item, IUIDragSession session)
		{
			Console.WriteLine("Delegate GetPreviewForLiftingItem");

			//if (item.PreviewProvider?.Invoke()?.View is UIView view)
			//{
			//	var parameters = new UIDragPreviewParameters();
			//	try
			//	{
					//var container = new UIView(view.Bounds);

			//		container.AddSubview(view);
			//		return new UITargetedDragPreview(container);
			//	}
			//	catch(Exception e)
			//	{
			//		Console.Write(e.ToString());
			//		return null;
			//	}
			//}
			//else
			//{
			//	var parameters = new UIDragPreviewParameters();

			//	return new UITargetedDragPreview(interaction.View, parameters);
			//}

			//var parameters = new UIDragPreviewParameters() { BackgroundColor = UIColor.Green};

			return new UITargetedDragPreview(interaction.View);

			//return null;
			//CGRect frame = new CGRect(0, 0, 100, 100);
			//return new UITargetedDragPreview(new UIView(frame)
			//{
			//	BackgroundColor = UIColor.Green
			//}, parameters);



			//return _platformDragStartingEventArgs?.GetPreviewForLiftingItem?.Invoke(interaction, item, session) ?? null;
		}

		// PlatformDragStarting
		[Export("dragInteraction:willAnimateLiftWithAnimator:session:")]
		[Preserve(Conditional = true)]
		public void WillAnimateLift(UIDragInteraction interaction, IUIDragAnimating animator, IUIDragSession session)
		{
			Console.WriteLine("Delegate WillAnimateLift");
			//_platformDragStartingEventArgs?.WillAnimateLift?.Invoke(interaction, animator, session);
		}

		// PlatformDragStarting
		[Export("dragInteraction:sessionWillBegin:")]
		[Preserve(Conditional = true)]
		public void SessionWillBegin(UIDragInteraction interaction, IUIDragSession session)
		{
			Console.WriteLine("Delegate SessionWillBegin");
			//_platformDragStartingEventArgs?.SessionWillBegin?.Invoke(interaction, session);
		}

		// PlatformDragStarting
		[Export("dragInteraction:sessionAllowsMoveOperation:")]
		[Preserve(Conditional = true)]
		public bool SessionAllowsMoveOperation(UIDragInteraction interaction, IUIDragSession session)
		{
			Console.WriteLine("Delegate SessionAllowsMoveOperation");
			return true;
			//return _platformDragStartingEventArgs?.SessionAllowsMoveOperation?.Invoke(interaction, session) ?? true;
		}

		// PlatformDragStarting
		[Export("dragInteraction:sessionIsRestrictedToDraggingApplication:")]
		[Preserve(Conditional = true)]
		public bool SessionIsRestrictedToDraggingApplication(UIDragInteraction interaction, IUIDragSession session)
		{
			Console.WriteLine("Delegate SessionIsRestrictedToDraggingApplication");
			return false;
			//return _platformDragStartingEventArgs?.SessionIsRestrictedToDraggingApplication?.Invoke(interaction, session) ?? false;
		}

		bool IsCounted = false;

		//PlatformDragStarting
		[Export("dragInteraction:sessionDidMove:")]
		[Preserve(Conditional = true)]
		public void SessionDidMove(UIDragInteraction interaction, IUIDragSession session)
		{
			if (!IsCounted)
				Console.WriteLine("Delegate SessionDidMove");
			IsCounted = true;
			//Console.WriteLine("Delegate SessionDidMove");
			//_platformDragStartingEventArgs?.SessionDidMove?.Invoke(interaction, session);
		}

		// PlatformDragEvent
		[Export("dragInteraction:sessionDidTransferItems:")]
		[Preserve(Conditional = true)]
		public void SessionDidTransferItems(UIDragInteraction interaction, IUIDragSession session)
		{
			Console.WriteLine("Delegate SessionDidTransferItems");
			//_platformDragStartingEventArgs?.SessionDidTransferItems?.Invoke(interaction, session);
		}


		// PlatformDragStartingEventArgs
		[Export("dragInteraction:itemsForAddingToSession:withTouchAtPoint:")]
		[Preserve(Conditional = true)]
		public UIDragItem[] GetItemsForAddingToSession(UIDragInteraction interaction, IUIDragSession session, CGPoint point)
		{
			Console.WriteLine("Delegate GetItemsForAddingToSession");

			//return new UIDragItem[]{
			//	new UIDragItem(new NSItemProvider(interaction.View, "view"))
			//};

			return Array.Empty<UIDragItem>();
			//return _platformDragStartingEventArgs?.GetItemsForAddingToSession?.Invoke(interaction, session, point) ?? Array.Empty<UIDragItem>();
		}

		// PlatformDragStartingEvent
		[Export("dragInteraction:sessionForAddingItems:withTouchAtPoint:")]
		[Preserve(Conditional = true)]
		public IUIDragSession? GetSessionForAddingItems(UIDragInteraction interaction, IUIDragSession[] sessions, CGPoint point)
		{
			Console.WriteLine("Delegate GetSessionForAddingItems");
			return null;
			//return _platformDragStartingEventArgs?.GetSessionForAddingItems?.Invoke(interaction, sessions, point) ?? null;
		}

		// PlatformDragStartEvent
		[Export("dragInteraction:session:willAddItems:forInteraction:")]
		[Preserve(Conditional = true)]
		public void WillAddItems(UIDragInteraction interaction, IUIDragSession session, UIDragItem[] items, UIDragInteraction addingInteraction)
		{
			Console.WriteLine("Delegate WillAddItems");
			//_platformDragStartingEventArgs?.WillAddItems?.Invoke(interaction, session, items, addingInteraction);
		}

		// PlatformDropCompleted
		[Export("dragInteraction:previewForCancellingItem:withDefault:")]
		[Preserve(Conditional = true)]
		public UITargetedDragPreview? GetPreviewForCancellingItem(UIDragInteraction interaction, UIDragItem item, UITargetedDragPreview defaultPreview)
		{
			Console.WriteLine("Delegate GetPreviewForCancellingItem");
			return null;
			//return _platformDragStartingEventArgs?.GetPreviewForCancellingItem?.Invoke(interaction, item, defaultPreview) ?? null;
		}

		// PlatformDropCompleted
		[Export("dragInteraction:item:willAnimateCancelWithAnimator:")]
		[Preserve(Conditional = true)]
		public void WillAnimateCancel(UIDragInteraction interaction, UIDragItem item, IUIDragAnimating animator)
		{
			Console.WriteLine("Delegate WillAnimateCancel");
			//_platformDragStartingEventArgs?.WillAnimateCancel?.Invoke(interaction, item, animator);
		}

		void SendEventArgs<TRecognizer>(Action<TRecognizer> func, View? view)
				where TRecognizer : class
		{
			var gestures =
				view?.GestureRecognizers;

			if (gestures == null)
				return;

			foreach (var gesture in gestures)
			{
				if (gesture is TRecognizer recognizer)
					func(recognizer);
			}
		}


		public UIDragItem[] HandleDragStarting(View? element, IPlatformViewHandler? handler, IUIDragSession session, PlatformDragStartingEventArgs platformArgs)
		{
			UIDragItem[]? returnValue = null;
			// if we touch and hold an item but do not move it, the _platformDragStartingEventArgs could be assigned. Reset it here in that case.
			_platformDragStartingEventArgs = null;
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
					return;

				var viewHandlerRef = new WeakReference(handler);
				var sessionRef = new WeakReference(session);

				var args = rec.SendDragStarting(element, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef), platformArgs);

				if (args.Cancel)
					return;

#pragma warning disable CS0618 // Type or member is obsolete
				if (!args.Handled)
#pragma warning restore CS0618 // Type or member is obsolete
				{
					_platformDragStartingEventArgs = args.PlatformArgs;

					if (args.PlatformArgs?.DragItems is UIDragItem[] dragItems)
					{
						foreach (var item in dragItems)
						{
							if (item.LocalObject is null)
								SetLocalObject(item, handler, args.Data);
						}
						returnValue = dragItems;
						return;
					}

					UIImage? uIImage = null;
					string clipDescription = String.Empty;
					NSItemProvider? itemProvider = null;

					if (handler?.PlatformView is UIImageView iv)
						uIImage = iv.Image;

					if (handler?.PlatformView is UIButton b && b.ImageView != null)
						uIImage = b.ImageView.Image;

					if (uIImage != null)
					{
						if (uIImage != null)
							itemProvider = new NSItemProvider(uIImage);
						else
							itemProvider = new NSItemProvider(new NSString(""));

						if (args.Data.Image == null && handler?.VirtualView is IImageElement imageElement)
							args.Data.Image = imageElement.Source;
					}
					else
					{
						string text = args.Data.Text ?? clipDescription;

						if (String.IsNullOrWhiteSpace(text) && handler?.PlatformView?.ConvertToImage() is UIImage image)
						{
							itemProvider = new NSItemProvider(image);
						}
						else
						{
							itemProvider = new NSItemProvider(new NSString(text));
						}
					}

					var dragItem = new UIDragItem(args.PlatformArgs?.ItemProvider ?? itemProvider);

					SetLocalObject(dragItem, handler, args.Data);

					if (args.PlatformArgs?.PreviewProvider is not null)
						dragItem.PreviewProvider = args.PlatformArgs.PreviewProvider!;

					returnValue = new UIDragItem[] { dragItem };
				}
			},
			element);

			return returnValue ?? new UIDragItem[0];
		}

		void SetLocalObject(UIDragItem dragItem, IPlatformViewHandler? handler, DataPackage data)
		{
			dragItem.LocalObject = new CustomLocalStateData()
			{
				Handler = handler,
				View = handler?.VirtualView as View,
				DataPackage = data
			};
		}

		void HandleDropCompleted(View? element, PlatformDropCompletedEventArgs platformArgs)
		{
			var args = new DropCompletedEventArgs(platformArgs);
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args), element);
		}

		bool HandleDragLeave(View? element, DataPackage? dataPackage, IUIDragSession? session, PlatformDragEventArgs platformArgs)
		{
			var viewHandlerRef = new WeakReference(_viewHandler);
			var sessionRef = session is null ? null : new WeakReference(session);

			var dragEventArgs = new DragEventArgs(dataPackage, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef), platformArgs);

			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragLeave(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
			}, element);

			return validTarget;
		}

		bool HandleDragOver(View? element, DataPackage? dataPackage, IUIDragSession session, PlatformDragEventArgs platformArgs)
		{
			var viewHandlerRef = new WeakReference(_viewHandler);
			var sessionRef = new WeakReference(session);

			var dragEventArgs = new DragEventArgs(dataPackage, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef), platformArgs);

			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragOver(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
			}, element);

			return validTarget;
		}

		void HandleDrop(View element, DataPackage? datapackage, IUIDropSession session, PlatformDropEventArgs platformArgs)
		{
			var viewHandlerRef = new WeakReference(_viewHandler);
			var sessionRef = session is null ? null : new WeakReference(session);

			var args = new DropEventArgs(datapackage?.View, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef), platformArgs);
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
					return;

				try
				{
					await rec.SendDrop(args);
				}
				catch (Exception dropExc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<DropGestureRecognizer>()?.LogWarning(dropExc, "Error sending drop event");
				}
			}, (View)element);
		}

		static internal Point? CalculatePosition(IElement? relativeTo, WeakReference viewHandlerRef, WeakReference? sessionRef)
		{
			if (sessionRef is null)
				return null;

			var viewHandler = viewHandlerRef.Target as IPlatformViewHandler;
			var session = sessionRef.Target as IUIDragDropSession;

			var virtualView = viewHandler?.VirtualView;
			var platformView = viewHandler?.PlatformView;
			var relativeView = relativeTo?.Handler?.PlatformView as UIView;

			CGPoint dragLocation;

			if (virtualView is null || session is null)
				return null;

			// If relativeTo is null we get the location on the screen
			if (relativeTo is null)
			{
				var screenLocation = virtualView.GetLocationOnScreen();
				dragLocation = session.LocationInView(platformView);

				if (!screenLocation.HasValue)
					return null;

				double x = dragLocation.X + screenLocation.Value.X;
				double y = dragLocation.Y + screenLocation.Value.Y;

				return new Point(x, y);
			}

			// If relativeTo is the same as the view sending the event, we get the position relative to itself
			if (relativeTo == virtualView)
			{
				dragLocation = session.LocationInView(platformView);
				return new Point(dragLocation.X, dragLocation.Y);
			}
			else if (relativeView is not null)
			{
				dragLocation = session.LocationInView(relativeView);
				return new Point(dragLocation.X, dragLocation.Y);
			}

			return null;
		}

		class CustomLocalStateData : NSObject
		{
			public View? View { get; set; }
			public IViewHandler? Handler { get; set; }
			public DataPackage? DataPackage { get; set; }
		}

		//internal interface IPreviewForLiftingItemProvider : IUIDragInteractionDelegate
		//{
		//	[Export("dragInteraction:previewForLiftingItem:session:")]
		//	[Preserve(Conditional = true)]
		//	public UITargetedDragPreview? GetPreviewForLiftingItem(UIDragInteraction interaction, UIDragItem item, IUIDragSession session)
		//	{
		//		Console.WriteLine("Delegate GetPreviewForLiftingItem");
		//		var previewProvider = item.PreviewProvider?.Invoke();
		//		return null;
		//		//return _platformDragStartingEventArgs?.GetPreviewForLiftingItem?.Invoke(interaction, item, session) ?? null;
		//	}
		//}
	}
}
#endif
