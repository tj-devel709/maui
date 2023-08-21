#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.DragGestureRecognizer']/Docs/*" />
	public class DragGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="CanDrag"/>.</summary>
		public static readonly BindableProperty CanDragProperty = BindableProperty.Create(nameof(CanDrag), typeof(bool), typeof(DragGestureRecognizer), true);

		/// <summary>Bindable property for <see cref="DropCompletedCommand"/>.</summary>
		public static readonly BindableProperty DropCompletedCommandProperty = BindableProperty.Create(nameof(DropCompletedCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DropCompletedCommandParameter"/>.</summary>
		public static readonly BindableProperty DropCompletedCommandParameterProperty = BindableProperty.Create(nameof(DropCompletedCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragStartingCommand"/>.</summary>
		public static readonly BindableProperty DragStartingCommandProperty = BindableProperty.Create(nameof(DragStartingCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragStartingCommandParameter"/>.</summary>
		public static readonly BindableProperty DragStartingCommandParameterProperty = BindableProperty.Create(nameof(DragStartingCommandParameter), typeof(object), typeof(DragGestureRecognizer), null);

		bool _isDragActive;

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DragGestureRecognizer()
		{
		}

		public event EventHandler<DropCompletedEventArgs> DropCompleted;
		public event EventHandler<DragStartingEventArgs> DragStarting;

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='CanDrag']/Docs/*" />
		public bool CanDrag
		{
			get { return (bool)GetValue(CanDragProperty); }
			set { SetValue(CanDragProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='DropCompletedCommand']/Docs/*" />
		public ICommand DropCompletedCommand
		{
			get { return (ICommand)GetValue(DropCompletedCommandProperty); }
			set { SetValue(DropCompletedCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='DropCompletedCommandParameter']/Docs/*" />
		public object DropCompletedCommandParameter
		{
			get { return (object)GetValue(DropCompletedCommandParameterProperty); }
			set { SetValue(DropCompletedCommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='DragStartingCommand']/Docs/*" />
		public ICommand DragStartingCommand
		{
			get { return (ICommand)GetValue(DragStartingCommandProperty); }
			set { SetValue(DragStartingCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragGestureRecognizer.xml" path="//Member[@MemberName='DragStartingCommandParameter']/Docs/*" />
		public object DragStartingCommandParameter
		{
			get { return (object)GetValue(DragStartingCommandParameterProperty); }
			set { SetValue(DragStartingCommandParameterProperty, value); }
		}

		internal void SendDropCompleted(DropCompletedEventArgs args)
		{
			if (!_isDragActive)
			{
				// this is mainly relevant for Android
				// Android fires an Ended action on every single view that has a drop handler
				// but we only need one of those DropCompleted actions to make it through
				return;
			}

			_isDragActive = false;
			_ = args ?? throw new ArgumentNullException(nameof(args));

			DropCompletedCommand?.Execute(DropCompletedCommandParameter);
			DropCompleted?.Invoke(this, args);
		}

		internal DragStartingEventArgs SendDragStarting(IView element, PlatformDragStartingEventArgs platformArgs = null)
		{
			var args = new DragStartingEventArgs();
			args.PlatformArgs = platformArgs;

			DragStartingCommand?.Execute(DragStartingCommandParameter);
			DragStarting?.Invoke(this, args);

			if (!args.Handled)
			{
				args.Data.PropertiesInternal.Add("DragSource", element);
			}

			if (args.Cancel || args.Handled)
				return args;

			_isDragActive = true;

			if (args.Data.Image == null && element is IImageElement ie)
			{
				args.Data.Image = ie.Source;
			}

			if (String.IsNullOrWhiteSpace(args.Data.Text))
				args.Data.Text = element.GetStringValue();

			return args;
		}
	}
}
