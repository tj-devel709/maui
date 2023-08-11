using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MauiModalPage_PageSheet : ContentPage
	{
		public MauiModalPage_PageSheet()
		{
			InitializeComponent();
		}

		async void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}

