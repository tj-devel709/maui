using System;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiTextField : UITextField
	{
#pragma warning disable RS0016 // Add public types and members to the declared API
		public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			//base.PressesBegan(presses, evt);

			var pressUsed = false;

			foreach (UIPress press in presses)
			{
				if (press.Key is UIKey key)
				{
					Console.WriteLine(key.KeyCode);
					var keyMod = key.CharactersIgnoringModifiers;
					//Console.WriteLine(keyMod);
					if (keyMod == "p")
						pressUsed = true;
				}
			}

			if (!pressUsed)
				this.NextResponder.PressesBegan(presses, evt);
		}

		[SupportedOSPlatform("ios13.0")]
		[Export("TestingStuff:")]
		internal void TestingStuff(UICommand uICommand)
		{
		}

		public override UIKeyCommand[] KeyCommands
		{
#pragma warning disable RS0016 // Add public types and members to the declared API
			get
#pragma warning restore RS0016 // Add public types and members to the declared API
			{
				return new[]{

					UIKeyCommand.Create(UIKeyCommand.LeftArrow, UIKeyModifierFlags.Command, new Selector($"TestingStuff:"))
				};
			}
		}
		public MauiTextField(CGRect frame)
			: base(frame)
		{
		}

		public MauiTextField()
		{
		}

		public override void WillMoveToWindow(UIWindow? window)
		{
			base.WillMoveToWindow(window);
			ResignFirstResponderTouchGestureRecognizer.Update(this, window);
		}

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public override NSAttributedString? AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public override UITextRange? SelectedTextRange
		{
			get => base.SelectedTextRange;
			set
			{
				var old = base.SelectedTextRange;

				base.SelectedTextRange = value;

				if (old?.Start != value?.Start || old?.End != value?.End)
					SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler? TextPropertySet;
		internal event EventHandler? SelectionChanged;
	}
}