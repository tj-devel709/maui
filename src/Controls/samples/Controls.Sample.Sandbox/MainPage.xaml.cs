using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

using Microsoft.Maui.Graphics;
#if __IOS__

using UIKit;
#endif

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			//DoStuff();
		}

		void DoStuff()
		{
#if __IOS__

			var sas = On<iOS>()?.SafeAreaInsets();

			var sas2 = On<iOS>();
			var sas3 = sas2?.SafeAreaInsets();

			On<iOS>().SetUseSafeArea(false);
			var s = On<iOS>().UsingSafeArea();

			On<iOS>().SetSafeAreaInsets(new Thickness(200, 200, 200, 0));

			var sas4 = On<iOS>()?.SafeAreaInsets();

			// (UIApplication) GetSafeAreaInsetsForWindow



			var scene = UIKit.UIApplication.SharedApplication.ConnectedScenes.ToArray().FirstOrDefault();
			var windowScene = (UIKit.UIWindowScene)scene;
			var safeArea2 = windowScene.Windows.FirstOrDefault()?.SafeAreaInsets;
			safeArea2 = new UIEdgeInsets(201, 201, 201, 0);

			var sas5 = On<iOS>()?.SafeAreaInsets();
			var scene1 = UIKit.UIApplication.SharedApplication.ConnectedScenes.ToArray().FirstOrDefault();
			var windowScene1 = (UIKit.UIWindowScene)scene1;
			var safeArea21 = windowScene1.Windows.FirstOrDefault()?.SafeAreaInsets;
#endif

#if ANDROID
			//var sa1 = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()?.Element;
			//var sa2 = sa1?.Handler;
			//var sa3 = sa2?.PlatformView;
			//var sa4 = sa3 as UIView;
			//var safeArea1 = sa4?.SafeAreaInsets;

			//var sa3 = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
			//var sa4 = sa3.Handler.PlatformView as UIView;
			//var safeArea3 = sa2?.SafeAreaInsets;

			//var safeArea = Page.SafeAreaInsets();

			var sas = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
			//var sas = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()?.SafeAreaInsets();

			//var sas2 = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
			//var sas3 = sas2?.SafeAreaInsets();

			//On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetSafeAreaInsets(new Thickness(100, 100, 100, 0));

			//var sas4 = On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()?.SafeAreaInsets();

			// (UIApplication) GetSafeAreaInsetsForWindow



			//var scene = UIKit.UIApplication.SharedApplication.ConnectedScenes.ToArray().FirstOrDefault();
			//var windowScene = (UIKit.UIWindowScene)scene;
			//var safeArea2 = windowScene.Windows.FirstOrDefault()?.SafeAreaInsets;
#endif
		}

		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			DoStuff();
		}
		//		internal static UIEdgeInsets SafeAreaInsetsForWindow
		//		{
		//			get
		//			{
		//				UIEdgeInsets safeAreaInsets;

		//				if (!Forms.IsiOS11OrNewer)
		//					safeAreaInsets = new UIEdgeInsets(UIApplication.SharedApplication.StatusBarFrame.Size.Height, 0, 0, 0);
		//				else if (UIApplication.SharedApplication.GetKeyWindow() != null)
		//					safeAreaInsets = UIApplication.SharedApplication.GetKeyWindow().SafeAreaInsets;
		//#pragma warning disable CA1416, CA1422  // TODO: UIApplication.Windows is unsupported on: 'ios' 15.0 and later
		//				else if (UIApplication.SharedApplication.Windows.Length > 0)
		//					safeAreaInsets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
		//#pragma warning restore CA1416, CA1422
		//				else
		//					safeAreaInsets = UIEdgeInsets.Zero;

		//				return safeAreaInsets;
		//			}
		//		}
	}
}