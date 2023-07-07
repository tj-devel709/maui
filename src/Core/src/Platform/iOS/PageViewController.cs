using System;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class PageViewController : ContainerViewController
	{
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
					Console.WriteLine(key.KeyCode);
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