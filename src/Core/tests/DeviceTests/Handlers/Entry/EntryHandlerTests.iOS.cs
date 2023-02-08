﻿using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var entry = new EntryStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new EntryStub()
			{
				Text = "Test",
				ReturnType = xplatReturnType
			};

			UIReturnKeyType expectedValue = UIReturnKeyType.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					PlatformViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Some Test Text";
			var xplatCharacterSpacing = 4;

			var entry = new EntryStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		[Fact]
		public async Task NextMovesToNextEntry()
		{
			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry2.IsFocused);
			}, entry1, entry2);
		}

		[Fact]
		public async Task NextMovesPastNotEnabledEntry()
		{
			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var entry3 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry3.IsFocused);
			}, entry1, entry2, entry3);
		}

		[Fact]
		public async Task NextMovesToEditor()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var editor = new EditorStub
			{
				Text = "Editor"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				Assert.True(editor.IsFocused);
			}, entry, editor);
		}

		[Fact]
		public async Task NextMovesPastNotEnabledEditor()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var editor1 = new EditorStub
			{
				Text = "Editor1",
				IsEnabled = false
			};

			var editor2 = new EditorStub
			{
				Text = "Editor2"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				Assert.True(editor2.IsFocused);
			}, entry, editor1, editor2);
		}

		[Fact]
		public async Task NextMovesToSearchBar()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var searchBar = new SearchBarStub
			{
				Text = "Search Bar"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				var uISearchBar = searchBar.Handler.PlatformView as UISearchBar;
				Assert.True(uISearchBar.GetSearchTextField().IsFirstResponder);
			}, entry, searchBar);
		}

		[Fact]
		public async Task NextMovesRightToLeftEntry()
		{
			var hsl = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			hsl.Add(entry1);
			hsl.Add(entry2);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: hsl.ToPlatform().Superview);
				var entry1Rect = entry1.ToPlatform().ConvertRectToView(entry1.ToPlatform().Bounds, hsl.ToPlatform());
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl.ToPlatform());
				Assert.True(entry1Rect.Right > entry2Rect.Right);
				Assert.True(entry2.IsFocused);
			}, hsl);
		}

		[Fact]
		public async Task NextMovesRightToLeftMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right < entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesLtrToRtlMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.LeftToRight
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right < entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesRtlToLtrMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.LeftToRight
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right > entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesBackToTop()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry1.IsFocused);
			}, entry1, entry2);
		}

		[Fact]
		public async Task NextMovesBackToTopIgnoringNotEnabled()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var editor = new EntryStub
			{
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry3.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry2.IsFocused);
			}, entry1, editor, entry2, entry3);
		}

		async Task NextMovesHelper(Action action = null, params StubBase[] views)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<HorizontalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<EntryStub, EntryHandler>();
					handler.AddHandler<EditorStub, EditorHandler>();
					handler.AddHandler<SearchBarStub, SearchBarHandler>();
				});
			});

			var layout = new VerticalStackLayoutStub();

			foreach (var view in views)
			{
				layout.Add(view);
			}

			layout.Width = 300;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(() =>
				{
					action?.Invoke();
				});
			});
		}

		double GetNativeCharacterSpacing(EntryHandler entryHandler)
		{
			var entry = GetNativeEntry(entryHandler);
			return entry.AttributedText.GetCharacterSpacing();
		}

		static UITextField GetNativeEntry(EntryHandler entryHandler) =>
			(UITextField)entryHandler.PlatformView;

		static string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		internal static void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		internal static int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetNativeEntry(entryHandler);
			return (int)control.GetOffsetFromPosition(control.BeginningOfDocument, control.SelectedTextRange.Start);
		}

		internal static void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetNativeEntry(entryHandler);
			var endPosition = control.GetPosition(control.BeginningOfDocument, position);
			control.SelectedTextRange = control.GetTextRange(endPosition, endPosition);
		}

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextColor.ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SecureTextEntry;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Placeholder;

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).AutocorrectionType == UITextAutocorrectionType.Yes;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).UserInteractionEnabled;

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.DecimalPad;

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.EmailAddress;

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.PhonePad;

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.Url;

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEntry.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEntry.SpellCheckingType == UITextSpellCheckingType.No;
		}

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ClearButtonMode == UITextFieldViewMode.WhileEditing;

		UITextAlignment GetNativeHorizontalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).VerticalAlignment;

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToPlatformVertical();

		UIReturnKeyType GetNativeReturnType(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ReturnKeyType;

		int GetNativeCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetNativeEntry(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetNativeSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetNativeEntry(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}
	}
}