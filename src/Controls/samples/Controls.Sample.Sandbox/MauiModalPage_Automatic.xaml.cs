using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MauiModalPage_Automatic : ContentPage
	{
		public MauiModalPage_Automatic()
		{
			InitializeComponent();
		}

		async void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}

