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

		void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
		{
		}

		void DragGestureRecognizer_DropCompleted(System.Object sender, Microsoft.Maui.Controls.DropCompletedEventArgs e)
		{
		}

		void DropGestureRecognizer_Drop(System.Object sender, Microsoft.Maui.Controls.DropEventArgs e)
		{
		}

		void DropGestureRecognizer_DragLeave(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
		}

		void DropGestureRecognizer_DragOver(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
		}
	}
}