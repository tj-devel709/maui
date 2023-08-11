using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MauiModalPage_FullScreen : ContentPage
	{
		public MauiModalPage_FullScreen()
		{
			InitializeComponent();
		}

		async void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}

