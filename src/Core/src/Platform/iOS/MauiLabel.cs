#nullable disable

using System;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Platform
{
	public class MauiLabel : UILabel
	{
		UIControlContentVerticalAlignment _verticalAlignment = UIControlContentVerticalAlignment.Center;

		public UIEdgeInsets TextInsets { get; set; }
		internal UIControlContentVerticalAlignment VerticalAlignment
		{
			get => _verticalAlignment;
			set
			{
				_verticalAlignment = value;
				SetNeedsDisplay();
			}
		}

		// TJ TODO Where is this frame coming from?
		// Why do we only check the vertical and not the horizontal?
		public MauiLabel(RectangleF frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		// TODO TJ when we get here, the rect is already 292 -> larger than the parent of 200
		// need to figure out when this rect/Frame is being set to this value
		public override void DrawText(RectangleF rect)
		{
			rect = TextInsets.InsetRect(rect);

			if (_verticalAlignment != UIControlContentVerticalAlignment.Center
				&& _verticalAlignment != UIControlContentVerticalAlignment.Fill)
			{
				rect = AlignVertical(rect);
			}

			base.DrawText(rect);
		}

		RectangleF AlignVertical(RectangleF rect)
		{
			var frameSize = Frame.Size;
			var height = Lines == 1 ? Font.LineHeight : SizeThatFits(frameSize).Height;

			if (height < frameSize.Height)
			{
				if (_verticalAlignment == UIControlContentVerticalAlignment.Top)
				{
					rect.Height = height;
				}
				else if (_verticalAlignment == UIControlContentVerticalAlignment.Bottom)
				{
					rect = new RectangleF(rect.X, rect.Bottom - height, rect.Width, height);
				}
			}

			return rect;
		}

		public override void InvalidateIntrinsicContentSize()
		{
			base.InvalidateIntrinsicContentSize();

			if (Frame.Width == 0 && Frame.Height == 0)
			{
				// The Label hasn't actually been laid out on screen yet; no reason to request a layout
				return;
			}

			if (!Frame.Size.IsCloseTo(AddInsets(IntrinsicContentSize), (nfloat)0.001))
			{
				// The text or its attributes have changed enough that the size no longer matches the set Frame. It's possible
				// that the Label needs to be laid out again at a different size, so we request that the parent do so. 
				Superview?.SetNeedsLayout();
			}
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			var requestedSize = base.SizeThatFits(size);
			// the size of the container may be smaller than the requestedSize
			size.Width = nfloat.Min(requestedSize.Width, size.Width);
			size.Height = requestedSize.Height;
			return AddInsets(size);
		}

		//public override SizeF SizeThatFits(SizeF size) => AddInsets(base.SizeThatFits(size));

		SizeF AddInsets(SizeF size) => new SizeF(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}