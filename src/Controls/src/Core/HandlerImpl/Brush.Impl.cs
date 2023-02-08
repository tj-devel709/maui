﻿#nullable disable
using Microsoft.Maui.Graphics;
using GraphicsGradientStop = Microsoft.Maui.Graphics.PaintGradientStop;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Brush.xml" path="Type[@FullName='Microsoft.Maui.Controls.Brush']/Docs/*" />
	public partial class Brush
	{
		public static implicit operator Brush(Paint paint)
		{
			if (paint is SolidPaint solidPaint)
				return new SolidColorBrush { Color = solidPaint.Color };

			if (paint is GradientPaint gradientPaint)
			{
				var gradientStopCollection = gradientPaint.GradientStops;

				GradientStopCollection gradientStops = new GradientStopCollection();

				for (int i = 0; i < gradientStopCollection.Length; i++)
				{
					var gs = gradientStopCollection[i];
					gradientStops.Insert(i, new GradientStop(gs.Color, gs.Offset));
				}

				if (gradientPaint is LinearGradientPaint linearGradientPaint)
				{
					var startPoint = linearGradientPaint.StartPoint;
					var endPoint = linearGradientPaint.EndPoint;

					return new LinearGradientBrush { GradientStops = gradientStops, StartPoint = startPoint, EndPoint = endPoint };
				}

				if (gradientPaint is RadialGradientPaint radialGradientPaint)
				{
					var center = radialGradientPaint.Center;
					var radius = radialGradientPaint.Radius;

					return new RadialGradientBrush { GradientStops = gradientStops, Center = center, Radius = radius };
				}
			}

			if (paint is ImageSourcePaint imageSourcePaint && imageSourcePaint.ImageSource is ImageSource imageSource)
				return new ImageBrush { ImageSource = imageSource };

			return null;
		}

		public static implicit operator Paint(Brush brush)
		{
			if (brush is SolidColorBrush solidColorBrush)
				return new SolidPaint { Color = solidColorBrush.Color };

			if (brush is GradientBrush gradientBrush)
			{
				var gradientStopCollection = gradientBrush.GradientStops;

				GraphicsGradientStop[] gradientStops = new GraphicsGradientStop[gradientStopCollection.Count];

				for (int i = 0; i < gradientStopCollection.Count; i++)
				{
					var gs = gradientStopCollection[i];
					gradientStops[i] = new GraphicsGradientStop(gs.Offset, gs.Color);
				}

				if (gradientBrush is LinearGradientBrush linearGradientBrush)
				{
					var startPoint = linearGradientBrush.StartPoint;
					var endPoint = linearGradientBrush.EndPoint;

					return new LinearGradientPaint { GradientStops = gradientStops, StartPoint = startPoint, EndPoint = endPoint };
				}

				if (gradientBrush is RadialGradientBrush radialGradientBrush)
				{
					var center = radialGradientBrush.Center;
					var radius = radialGradientBrush.Radius;

					return new RadialGradientPaint { GradientStops = gradientStops, Center = center, Radius = radius };
				}
			}

			if (brush is ImageBrush imageBrush)
				return new ImageSourcePaint { ImageSource = imageBrush.ImageSource };

			return null;
		}
	}
}
