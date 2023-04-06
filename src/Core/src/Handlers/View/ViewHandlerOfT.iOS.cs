using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public UIViewController? ViewController { get; set; }

		// on the good label - this rect is already 200, how about the bad one?
		// on bad one rect = X=-92.66666666666669, Y=0, Width=292.6666666666667, Height=17
		public override void PlatformArrange(Rect rect)
		{
			this.PlatformArrangeHandler(rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var debug = this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
			return debug;
		}

		protected override void SetupContainer()
		{
			if (PlatformView == null || ContainerView != null)
				return;

			var oldParent = (UIView?)PlatformView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(PlatformView);
			PlatformView.RemoveFromSuperview();

			ContainerView ??= new WrapperView(PlatformView.Bounds);
			ContainerView.AddSubview(PlatformView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(ContainerView, idx);
			else
				oldParent?.AddSubview(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (PlatformView == null || ContainerView == null || PlatformView.Superview != ContainerView)
			{
				CleanupContainerView(ContainerView);
				ContainerView = null;
				return;
			}

			var oldParent = (UIView?)ContainerView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(ContainerView);
			CleanupContainerView(ContainerView);
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(PlatformView, idx);
			else
				oldParent?.AddSubview(PlatformView);

			void CleanupContainerView(UIView? containerView)
			{
				if (containerView is WrapperView wrapperView)
				{
					wrapperView.RemoveFromSuperview();
					wrapperView.Dispose();
				}
			}
		}
	}
}