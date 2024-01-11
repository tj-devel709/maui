using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Foundation;


#if IOS || MACCATALYST
using UIKit;
#endif 

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void PushModal(object sender, EventArgs e)
		{
#if IOS	
			var p = pushButton.Handler;
			var button = p?.PlatformView as UIView;
			var thisVC = button?.FindResponder<UIViewController>();

			var vc = new MyViewController ();
			vc.PreferredContentSize = new CoreGraphics.CGSize(200, 500);
			vc.ModalPresentationStyle = UIModalPresentationStyle.Popover;
			if (vc.PopoverPresentationController is UIPopoverPresentationController presentationController && (sender as View)?.Handler?.PlatformView is UIView view)
			{
				presentationController.Delegate = new PopoverDelegate();
				presentationController.SourceView = view;
				presentationController.SourceRect = view.Bounds;
			}
			
			thisVC?.PresentViewController(vc, true, null);
#endif
		}

		sealed class PopoverDelegate : UIPopoverPresentationControllerDelegate
		{
			public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController forPresentationController) =>
				UIModalPresentationStyle.None;
			readonly WeakEventManager popoverDismissedEventManager = new();

			public event EventHandler<UIPresentationController> PopoverDismissedEvent
			{
				add => popoverDismissedEventManager.AddEventHandler(value);
				remove => popoverDismissedEventManager.RemoveEventHandler(value);
			}
			public override void DidDismiss(UIPresentationController presentationController) =>
				popoverDismissedEventManager.HandleEvent(this, presentationController, nameof(PopoverDismissedEvent));
		}
	}

	public class MyViewController : UIViewController
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View!.BackgroundColor = UIColor.Green;
			View.Frame = new CoreGraphics.CGRect(0, 0, 200, 500);

			// Create the first UITextField and set its properties
			var textField1 = new UITextField
			{
				Frame = new CoreGraphics.CGRect(20, 100, View!.Bounds.Width - 40, 40),
				BorderStyle = UITextBorderStyle.RoundedRect,
				Placeholder = "Top TextField",
			};

			// Create the second UITextField and set its properties
			var textField2 = new UITextField
			{
				Frame = new CoreGraphics.CGRect(20, View.Bounds.Height - 100, View.Bounds.Width - 40, 40),
				BorderStyle = UITextBorderStyle.RoundedRect,
				Placeholder = "Bottom TextField",
			};

			// Add the UITextFields to the view
			View.AddSubviews(textField1, textField2);
		}
	}
}