using System;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Layouts
{
	public abstract class LayoutManager : ILayoutManager
	{
		public LayoutManager(ILayout layout)
		{
			Layout = layout;
		}

		public ILayout Layout { get; }

		public abstract Size Measure(double widthConstraint, double heightConstraint);
		public abstract Size ArrangeChildren(Rect bounds);

		public static double ResolveConstraints(double externalConstraint, double explicitLength, double measuredLength, double min = Minimum, double max = Maximum)
		{
			var length = IsExplicitSet(explicitLength) ? explicitLength : measuredLength; // good 200 - TODO changed here! 
			// Looks like the explicitLength is set in the good example. See if it is set in the bad example?
			// wait the length is also 200 in the bad example...

			if (max < length)
			{
				length = max;
			}

			if (min > length)
			{
				length = min;
			}

			return Math.Min(length, externalConstraint);
		}
	}
}
