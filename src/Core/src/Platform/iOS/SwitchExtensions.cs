using System;
using System.Linq;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			UIView uIView;
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				uIView = uiSwitch.Subviews[0].Subviews[0];
			else
				uIView = uiSwitch.Subviews[0].Subviews[0].Subviews[0];

			if (!view.IsOn)
				uIView.BackgroundColor = uiSwitch.GetOffTrackColor ();

			else if (view.TrackColor is not null) {
				uiSwitch.OnTintColor = view.TrackColor.ToPlatform ();
				uIView.BackgroundColor = uiSwitch.OnTintColor;
			}
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			Graphics.Color thumbColor = view.ThumbColor;
			if (thumbColor != null)
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
		}

		internal static UIView GetTrackSubview(this UISwitch uISwitch)
		{
			UIView uIView;
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				uIView = uISwitch.Subviews[0].Subviews[0];
			else
				uIView = uISwitch.Subviews[0].Subviews[0].Subviews[0];

			return uIView;
		}

		internal static UIColor GetOffTrackColor(this UISwitch uISwitch) => UIColor.FromRGB (230, 230, 232);
	}
}
