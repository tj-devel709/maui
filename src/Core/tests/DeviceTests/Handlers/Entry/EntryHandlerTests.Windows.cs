﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Xunit;

using NativeTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using NativeVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Theory(DisplayName = "MaxLength Works Correctly")]
		[InlineData("123")]
		[InlineData("Hello")]
		[InlineData("Goodbye")]
		public async Task MaxLengthWorksCorrectly(string text)
		{
			const int maxLength = 4;

			var entry = new EntryStub
			{
				Text = text,
				MaxLength = maxLength
			};

			var expectedText = text.Length > maxLength ? text.Substring(0, maxLength) : text;
			var platformText = await GetValueAsync(entry, GetNativeText);

			Assert.Equal(expectedText, platformText);
		}

		static TextBox GetNativeEntry(EntryHandler entryHandler) =>
			entryHandler.PlatformView;

		double GetNativeCharacterSpacing(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).CharacterSpacing;

		static string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		internal static void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		internal static int GetCursorStartPosition(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).GetCursorPosition();

		internal static void UpdateCursorStartPosition(EntryHandler entryHandler, int position) =>
			GetNativeEntry(entryHandler).SelectionStart = position;

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Resources.TryGet<SolidColorBrush>("TextControlForeground").ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			((MauiPasswordTextBox)GetNativeEntry(entryHandler)).IsPassword;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).PlaceholderText;

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).IsTextPredictionEnabled;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).IsReadOnly;

		bool IsInputScopeEquals(InputScope inputScope, InputScopeNameValue nameValue)
		{
			if (inputScope == null || inputScope.Names.Count == 0)
				return false;

			return inputScope.Names[0].NameValue == nameValue;
		}

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.Number);

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.EmailSmtpAddress);

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.TelephoneNumber);

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.Url);

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.Default);

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler) =>
			IsInputScopeEquals(GetNativeEntry(entryHandler).InputScope, InputScopeNameValue.Chat);

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).GetClearButtonVisibility();

		NativeTextAlignment GetNativeHorizontalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		NativeVerticalAlignment GetNativeVerticalTextAlignment(EntryHandler entryHandler)
		{
			var textBox = GetNativeEntry(entryHandler);

			var sv = textBox.GetDescendantByName<ScrollViewer>("ContentElement");
			var placeholder = textBox.GetDescendantByName<TextBlock>("PlaceholderTextContentPresenter");

			Assert.Equal(sv.VerticalAlignment, placeholder.VerticalAlignment);

			return sv.VerticalAlignment;
		}

		NativeVerticalAlignment GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToPlatformVerticalAlignment();

		int GetNativeCursorPosition(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).GetCursorPosition();

		int GetNativeSelectionLength(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SelectionLength;
	}
}