using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MauiModalPage_OverFullScreen : ContentPage
	{
		public MauiModalPage_OverFullScreen()
		{
			InitializeComponent();
		}

		async void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}

