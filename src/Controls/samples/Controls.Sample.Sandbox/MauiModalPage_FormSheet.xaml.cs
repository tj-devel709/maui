using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MauiModalPage_FormSheet : ContentPage
	{
		public MauiModalPage_FormSheet()
		{
			InitializeComponent();
		}

		async void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}

