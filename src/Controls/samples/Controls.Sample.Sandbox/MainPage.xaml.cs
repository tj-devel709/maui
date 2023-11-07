using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		void Button_Pressed(System.Object sender, System.EventArgs e)
		{
			Console.WriteLine($"StarRow Height: {ColView.Height}");
			Console.WriteLine($"ContentPage Height: {ContentP.Height}");
		}


	}
}