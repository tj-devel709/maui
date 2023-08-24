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
	}

	void DropGestureRecognizer_DragOver2(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
	{
#if IOS || MACCATALYST
		e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Move));
#endif
	}

	void DropGestureRecognizer_DragOver3(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
	{
#if IOS || MACCATALYST
		e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Copy));
#endif
	}
}