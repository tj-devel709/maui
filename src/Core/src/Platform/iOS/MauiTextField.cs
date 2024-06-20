using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{

	public class MauiTextField : UITextField, IUIViewLifeCycleEvents
	{

		public MauiTextField(CGRect frame)
			: base(frame)
		{
			var f = CanBecomeFocused;
		}

		public MauiTextField()
		{
			var f = CanBecomeFocused;
		}

		public override void WillMoveToWindow(UIWindow? window)
		{
			base.WillMoveToWindow(window);
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

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler? TextPropertySet;
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		internal event EventHandler? SelectionChanged;

#pragma warning disable RS0016
		// public override UIKeyCommand[] KeyCommands
		// {
		// 	get
		// 	{
		// 		// var keyCommand = UIKeyCommand.Create((NSString)"\t", 0, new Selector("handleTabKey:"));
		// 		var tabKeyCommand = UIKeyCommand.Create((NSString)"\t", 0, new ObjCRuntime.Selector("tabKeyPressed:"));
		// 		var gKeyCommand = UIKeyCommand.Create((NSString)"g", 0, new ObjCRuntime.Selector("gKeyPressed:"));

		// 		// return new UIKeyCommand[] { tabKeyCommand, gKeyCommand };
		// 		return new UIKeyCommand[] { gKeyCommand };
		// 	}
		// }

		[UnconditionalSuppressMessage("Memory", "MA0002")]
		UIKeyCommand[] tabCommands = {
			// UIKeyCommand.Create ((Foundation.NSString)"\t", 0, new ObjCRuntime.Selector ("tabForward:")),
			// UIKeyCommand.Create ((Foundation.NSString)"\t", UIKeyModifierFlags.Shift, new ObjCRuntime.Selector ("tabBackward:")),
			UIKeyCommand.Create((NSString)"\t", UIKeyModifierFlags.Command, new ObjCRuntime.Selector("tabKeyPressed:")),
			UIKeyCommand.Create((NSString)"g", UIKeyModifierFlags.Command, new ObjCRuntime.Selector("gKeyPressed:")),
			// UIKeyCommand.Create(UIKeyCommand.InputUpArrow, 0, new ObjCRuntime.Selector("upArrowKeyPressed:"))
		};

		// public override UIKeyCommand[] KeyCommands => tabCommands;

		public override UIKeyCommand[] KeyCommands 
		{
			get
			{
				var t = UIKeyCommand.Create((NSString)"\t", UIKeyModifierFlags.Command, new ObjCRuntime.Selector("tabKeyPressed:"));
				var g = UIKeyCommand.Create((NSString)"g", UIKeyModifierFlags.Command, new ObjCRuntime.Selector("gKeyPressed:"));

				g.WantsPriorityOverSystemBehavior = true;
				t.WantsPriorityOverSystemBehavior = true;

				return [t, g];
			}
		}
 

		// public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
		// {
		// 	foreach (UIPress press in presses)
		// 	{
		// 		if (press.Type == UIPressType.UpArrow)
		// 		{
		// 			// Handle Up Arrow key press
		// 			Console.WriteLine("HIT UP ARROW");
		// 		}
		// 		else if (press.Key?.Characters == "g")
		// 		{
		// 			// Handle "g" key press
		// 			Console.WriteLine("HIT G");
		// 		}
		// 		else if (press.Key?.Characters == "\t")
		// 		{
		// 			// Handle Tab key press
		// 			Console.WriteLine("HIT TAB");
		// 		}
		// 		else
		// 		{
		// 			// If the key press is not handled, pass it to the super class
		// 			base.PressesBegan(presses, evt);
		// 		}
		// 	}
		// }




		[Export("tabKeyPressed:")]
		static private void HandleTabKey(UIKeyCommand cmd)
		{
			// Do something when the Tab key is pressed
			Console.WriteLine ("HIT TAB");
		}
		
		[Export("upArrowKeyPressed:")]
		static private void HandleUpArrowKey(UIKeyCommand cmd)
		{
			// Do something when the Tab key is pressed
			Console.WriteLine ("HIT UP ARROW");
		}
		
		[Export("gKeyPressed:")]
		static private void HandleGKey(UIKeyCommand cmd)
		{
			Console.WriteLine ("HIT G");
			// Do something when the Tab key is pressed
		}

		public override bool CanBecomeFocused => true;

		// public override void KeyDown(NSEvent theEvent)
		// {
		// 	if (theEvent.KeyCode == (ushort)NSKey.Tab)
		// 	{
		// 		// bool shift = (theEvent.ModifierFlags & NSEventModifierMask.ShiftKeyMask) == NSEventModifierMask.ShiftKeyMask;
		// 		// var nextControl = FocusSearch(forwardDirection: !shift);
		// 		// if (nextControl != null)
		// 		// {
		// 		// 	Window?.MakeFirstResponder(nextControl);
		// 		// 	return;
		// 		// }
		// 	}
		// 	base.KeyUp(theEvent);
		// }
#pragma warning restore RS0016

	}
}