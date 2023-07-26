using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		void PointerGestureRecognizer_PointerEntered(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			// sender is Label control
			var t = PointerEventArgsExtensions.ToPlatform(e);
		}

		void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
		{
#if MACCATALYST
			var t = TappedEventArgsExtensions.ToPlatform(e);
#endif
		}

		void SwipeGestureRecognizer_Swiped(System.Object sender, Microsoft.Maui.Controls.SwipedEventArgs e)
		{
#if MACCATALYST
			var t = SwipedEventArgsExtensions.ToPlatform(e);
#endif
		}

		void PinchGestureRecognizer_PinchUpdated(System.Object sender, Microsoft.Maui.Controls.PinchGestureUpdatedEventArgs e)
		{
#if MACCATALYST
			var t = PinchGestureUpdatedEventArgsExtensions.ToPlatform(e);
#endif
		}

		void PanGestureRecognizer_PanUpdated(System.Object sender, Microsoft.Maui.Controls.PanUpdatedEventArgs e)
		{
#if MACCATALYST
			var t = PanUpdatedEventArgsExtensions.ToPlatform(e);
#endif
		}

		// TODO Look more at the UIDragInteraction / UIDropInteraction
		// since those are not UIGestureRecognizers

		void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
		{
#if MACCATALYST
			var t = DragStartingEventArgsExtensions.ToPlatform(e);
#endif
		}

		void DropGestureRecognizer_Drop(System.Object sender, Microsoft.Maui.Controls.DropEventArgs e)
		{
#if MACCATALYST
			var t = DropEventArgsExtensions.ToPlatform(e);
#endif
		}

		void DropGestureRecognizer_DragLeave(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
#if MACCATALYST
			var t = DragEventArgsExtensions.ToPlatform(e);
#endif
		}

		void DropGestureRecognizer_DragOver(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
#if MACCATALYST
			var t = DragEventArgsExtensions.ToPlatform(e);
#endif
		}
	}
}