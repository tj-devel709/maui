﻿using System;
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
			button2.Focus();
			
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
	}
}