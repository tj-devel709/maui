using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Views;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			//button2.Focus();

		}

		public void DisplayPopup()
		{
			var popup = new SimplePopup();

			this.ShowPopup(popup);
		}

		void Button_Pressed(System.Object sender, System.EventArgs e)
		{
			DisplayPopup();
		}

		async void Button_Pressed21(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new MauiModalPage_FullScreen());
		}

		async void Button_Pressed_11(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new MauiModalPage_PageSheet());
		}

		async void Button_Pressed_21(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new MauiModalPage_FormSheet());
		}

		async void Button_Pressed_31(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new MauiModalPage_OverFullScreen());
		}

		async void Button_Pressed_41(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new MauiModalPage_Automatic());
		}
	}
}
