using System;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	public class PageViewController : ContainerViewController
	{
#pragma warning disable RS0016 // Add public types and members to the declared API
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			var actionSelector = new Selector("NewFileAccelerator:");
			var accelerator = UIKeyCommand.Create((NSString)"N", UIKeyModifierFlags.Command, actionSelector);

			var actionSelector1 = new Selector("NewFileAccelerator1:");
			var accelerator1 = UIKeyCommand.Create((NSString)"N", UIKeyModifierFlags.Command, actionSelector1);

			var actionSelector2 = new Selector("NewFileAccelerator2:");
			var accelerator2 = UIKeyCommand.Create((NSString)"L", UIKeyModifierFlags.Command, actionSelector2);

			var actionSelector3 = new Selector("NewFileAccelerator3:");
			var accelerator3 = UIKeyCommand.Create((NSString)"L", UIKeyModifierFlags.Command, actionSelector2);


			AddKeyCommand(accelerator);
			AddKeyCommand(accelerator1);
			AddKeyCommand(accelerator2);
		}

		[Export("NewFileAccelerator:")]
		void NewFileAccelerator(UIKeyCommand cmd)
		{
			if (View?.BackgroundColor is not null)
			{
				if (View.BackgroundColor == UIColor.Yellow)
					View.BackgroundColor = UIColor.Purple;
				else
					View.BackgroundColor = UIColor.Yellow;
			}
		}

		[Export("NewFileAccelerator1:")]
		void NewFileAccelerator1(UIKeyCommand cmd)
		{
			if (View?.BackgroundColor is not null)
				View.BackgroundColor = UIColor.Green;
		}

		[Export("NewFileAccelerator2:")]
		void NewFileAccelerator2(UIKeyCommand cmd)
		{
			if (View?.BackgroundColor is not null)
				View.BackgroundColor = UIColor.Gray;
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			//base.PressesBegan(presses, evt);


			var pressUsed = false;

			foreach (UIPress press in presses)
			{
				if (press.Key is UIKey key)
				{
					//Console.WriteLine(key.KeyCode);
					var keyMod = key.CharactersIgnoringModifiers;
					//Console.WriteLine(keyMod);
					if (keyMod == "p")
						pressUsed = true;
				}
			}

			if (!pressUsed)
				this.NextResponder.PressesBegan(presses, evt);
		}

		public PageViewController(IView page, IMauiContext mauiContext)
		{
			CurrentView = page;
			Context = mauiContext;

			LoadFirstView(page);
		}

		protected override UIView CreatePlatformView(IElement view)
		{
			return new ContentView
			{
				CrossPlatformLayout = ((IContentView)view)
			};
		}

		public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
		{
			if (CurrentView?.Handler is ElementHandler handler)
			{
				var application = handler.GetRequiredService<IApplication>();

				application?.UpdateUserInterfaceStyle();
				application?.ThemeChanged();
			}

			base.TraitCollectionDidChange(previousTraitCollection);
		}
	}
}