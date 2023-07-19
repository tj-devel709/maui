using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			button2.Focus();
			//button2.Handler.PlatformView
			

			
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var hover = new UIHoverGestureRecognizer(GREntered);
			hover.AddTarget(() => GREntered(hover));
			var handler = GRLabel?.Handler;
			var plat = handler?.PlatformView;
			var uiLabel = GRLabel?.Handler?.PlatformView as UILabel;
			uiLabel?.AddGestureRecognizer(hover);
		}

		void button_Pressed(System.Object sender, System.EventArgs e)
		{
			button2.Focus();
		}

		void button_Pressed_1(System.Object sender, System.EventArgs e)
						{
			label.Text = label.Text + "1";
		}

		void button2_Focused(System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
		}

		void GREntered(UIHoverGestureRecognizer recognizer)
		{
			Console.WriteLine("Entered GR");
		}

		//ICommand PGREnterCommand(object r)
		//{
		//	Console.WriteLine("Entered GR");
		//}

		void PointerGestureRecognizer_PointerEntered(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{

			Console.WriteLine("Enter: " + e.ModifierFlags.ToString());
		}

		void PointerGestureRecognizer_PointerExited(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			Console.WriteLine("Exit: " + e.ModifierFlags.ToString());
			//Console.WriteLine("Left GR");
		}
	}
}