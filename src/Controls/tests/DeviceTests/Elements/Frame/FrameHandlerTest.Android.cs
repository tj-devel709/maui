﻿using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameHandlerTest
	{
		public override Task ContainerViewInitializesCorrectly()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}

		public override Task ContainerViewAddsAndRemoves()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}

		public override Task ContainerViewRemainsIfShadowMapperRunsAgain()
		{
			// https://github.com/dotnet/maui/pull/12218
			return Task.CompletedTask;
		}

		public override async Task ReturnsNonEmptyNativeBoundingBox(int size)
		{
			// Frames have a legacy hard-coded minimum size of 20x20
			var expectedSize = Math.Max(20, size);
			var expectedBounds = new Graphics.Rect(0, 0, expectedSize, expectedSize);

			var view = new Frame()
			{
				HeightRequest = size,
				WidthRequest = size
			};

			var nativeBoundingBox = await GetValueAsync(view, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, Graphics.Rect.Zero);

			AssertWithinTolerance(expectedBounds.Size, nativeBoundingBox.Size);
		}
	}
}
