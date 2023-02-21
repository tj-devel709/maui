﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WindowSoftInputModeAdjust = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(WindowSoftInputModeAdjust.Resize)]
		[InlineData(WindowSoftInputModeAdjust.Pan)]
		public async Task ModalPageMarginCorrectAfterKeyboardOpens(WindowSoftInputModeAdjust panSize)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage);
			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					try
					{
						window.UpdateWindowSoftInputModeAdjust(panSize.ToPlatform());
						VerticalStackLayout layout = new VerticalStackLayout();
						List<Entry> entries = new List<Entry>();
						ContentPage modalPage = new ContentPage()
						{
							Content = layout
						};

						// Add enough entries into the stack layout so that we can
						// guarantee we'll have entries that would be covered by the keyboard
						for (int i = 0; i < 30; i++)
						{
							var entry = new Entry();
							entries.Add(entry);
							layout.Add(entry);
						}

						await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
						await OnNavigatedToAsync(modalPage);

						// Locate the lowest visible entry
						var pageBoundingBox = modalPage.GetBoundingBox();
						Entry testEntry = entries[0];
						foreach (var entry in entries)
						{
							var entryBox = entry.GetBoundingBox();

							// Locate the lowest visible entry
							if ((entryBox.Y + (entryBox.Height * 2)) > pageBoundingBox.Height)
								break;

							testEntry = entry;
						}

						// Ensure that the keyboard is closed before we start
						await AssertionExtensions.HideKeyboardForView(testEntry);

						// determine the screen dimensions with no keyboard open
						var rootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
						var modalOffsetY = modalPage.GetLocationOnScreen().Value.Y;
						var originalModalPageSize = modalPage.GetBoundingBox();

						await AssertionExtensions.ShowKeyboardForView(testEntry);

						// Type text into the entries
						testEntry.Text = "Typing";

						// Wait for the size of the screen to settle after the keyboard has opened
						bool offsetMatchesWhenKeyboardOpened = await AssertionExtensions.Wait(() =>
						{
							var keyboardOpenRootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
							var keyboardOpenModalOffsetY = modalPage.GetLocationOnScreen().Value.Y;

							var originalDiff = Math.Abs(rootPageOffsetY - modalOffsetY);
							var openDiff = Math.Abs(keyboardOpenRootPageOffsetY - keyboardOpenModalOffsetY);

							return Math.Abs(originalDiff - openDiff) <= 0.2;
						});

						Assert.True(offsetMatchesWhenKeyboardOpened, "Modal page has an invalid offset when open");

						await AssertionExtensions.HideKeyboardForView(testEntry);

						// Wait for the size of the screen to settle after the keyboard has closed
						bool offsetMatchesWhenKeyboardClosed = await AssertionExtensions.Wait(() =>
						{
							var keyboardClosedRootPageOffsetY = navPage.CurrentPage.GetLocationOnScreen().Value.Y;
							var keyboardClosedModalOffsetY = modalPage.GetLocationOnScreen().Value.Y;

							return rootPageOffsetY == keyboardClosedRootPageOffsetY &&
									modalOffsetY == keyboardClosedModalOffsetY;
						});

						Assert.True(offsetMatchesWhenKeyboardClosed, "Modal page failed to return to expected offset");

						// Make sure that everything has returned to the initial size once the keyboard has closed
						var finalModalPageSize = modalPage.GetBoundingBox();
						Assert.Equal(originalModalPageSize, finalModalPageSize);
					}
					finally
					{
						window.UpdateWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize.ToPlatform());
					}
				});
		}
	}
}
