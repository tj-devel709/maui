﻿using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ActivityIndicator)]
	public partial class ActivityIndicatorHandlerTests : CoreHandlerTestBase<ActivityIndicatorHandler, ActivityIndicatorStub>
	{
		[Theory(DisplayName = "IsRunning Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsRunningInitializesCorrectly(bool isRunning)
		{
			var activityIndicator = new ActivityIndicatorStub()
			{
				IsRunning = isRunning
			};

			await ValidatePropertyInitValue(activityIndicator, () => activityIndicator.IsRunning, GetNativeIsRunning, activityIndicator.IsRunning);
		}

		[Fact(DisplayName = "Background Updates Correctly",
			Skip = "This test is currently invalid https://github.com/dotnet/maui/issues/11948")]
		public async Task BackgroundUpdatesCorrectly()
		{
			var activityIndicator = new ActivityIndicatorStub()
			{
				IsRunning = true
			};

			await ValidateHasColor(activityIndicator, Colors.Yellow, () => activityIndicator.Background = new SolidPaintStub(Colors.Yellow));
		}
	}
}