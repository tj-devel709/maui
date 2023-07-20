#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PanGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.PanGestureRecognizer']/Docs/*" />
	public class PanGestureRecognizer : GestureRecognizer, IPanGestureController
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static AutoId CurrentId { get; } = new();

		/// <summary>Bindable property for <see cref="TouchPoints"/>.</summary>
		public static readonly BindableProperty TouchPointsProperty = BindableProperty.Create("TouchPoints", typeof(int), typeof(PanGestureRecognizer), 1);

		/// <include file="../../docs/Microsoft.Maui.Controls/PanGestureRecognizer.xml" path="//Member[@MemberName='TouchPoints']/Docs/*" />
		public int TouchPoints
		{
			get { return (int)GetValue(TouchPointsProperty); }
			set { SetValue(TouchPointsProperty, value); }
		}

		void IPanGestureController.SendPan(Element sender, double totalX, double totalY, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Running, gestureId, totalX, totalY));
		}

		internal void SendPan(Element sender, double totalX, double totalY, int gestureId, object recognizer)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Running, gestureId, totalX, totalY, recognizer));
		}

		void IPanGestureController.SendPanCanceled(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Canceled, gestureId));
		}

		internal void SendPanCanceled(Element sender, int gestureId, object recognizer)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Canceled, gestureId, recognizer));
		}

		void IPanGestureController.SendPanCompleted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Completed, gestureId));
		}

		internal void SendPanCompleted(Element sender, int gestureId, object recognizer)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Completed, gestureId));
		}

		void IPanGestureController.SendPanStarted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Started, gestureId));
		}

		internal void SendPanStarted(Element sender, int gestureId, object recognizer)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Started, gestureId, recognizer));
		}

		public event EventHandler<PanUpdatedEventArgs> PanUpdated;
	}
}
