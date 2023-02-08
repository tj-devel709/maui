﻿using System;
using System.ComponentModel.Design;
using System.Drawing;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchBar : UISearchBar
	{
		public MauiSearchBar() : this(RectangleF.Empty)
		{
		}

		public MauiSearchBar(NSCoder coder) : base(coder)
		{
		}

		public MauiSearchBar(CGRect frame) : base(frame)
		{
		}

		protected MauiSearchBar(NSObjectFlag t) : base(t)
		{
		}

		protected internal MauiSearchBar(NativeHandle handle) : base(handle)
		{
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		public event EventHandler<UISearchBarTextChangedEventArgs>? TextSetOrChanged;

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					TextSetOrChanged?.Invoke(this, new UISearchBarTextChangedEventArgs(value ?? String.Empty));
				}
			}
		}

		internal event EventHandler? OnMovedToWindow;
		internal event EventHandler? EditingChanged;

		public override void WillMoveToWindow(UIWindow? window)
		{
			var editor = this.GetSearchTextField();

			base.WillMoveToWindow(window);

			if (editor != null)
				ResignFirstResponderTouchGestureRecognizer.Update(editor, window);

			if (editor != null)
			{
				editor.EditingChanged -= OnEditingChanged;
				if (window != null)
					editor.EditingChanged += OnEditingChanged;
			}

			if (window != null)
				OnMovedToWindow?.Invoke(this, EventArgs.Empty);
		}

		void OnEditingChanged(object? sender, EventArgs e)
		{
			EditingChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}