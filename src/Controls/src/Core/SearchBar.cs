#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchBar']/Docs/*" />
	public partial class SearchBar : InputView, IFontElement, ITextAlignmentElement, ISearchBarController, IElementConfiguration<SearchBar>, ICommandElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommandProperty']/Docs/*" />
		public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(
			"SearchCommand", typeof(ICommand), typeof(SearchBar), null,
			propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommandParameterProperty']/Docs/*" />
		public static readonly BindableProperty SearchCommandParameterProperty = BindableProperty.Create(
			"SearchCommandParameter", typeof(object), typeof(SearchBar), null,
			propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CancelButtonColorProperty']/Docs/*" />
		public static readonly BindableProperty CancelButtonColorProperty = BindableProperty.Create("CancelButtonColor", typeof(Color), typeof(SearchBar), default(Color));

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs/*" />
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(SearchBar), true, BindingMode.Default);

		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(SearchBar), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(SearchBar), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='HorizontalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='VerticalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		readonly Lazy<PlatformConfigurationRegistry<SearchBar>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='CancelButtonColor']/Docs/*" />
		public Color CancelButtonColor
		{
			get { return (Color)GetValue(CancelButtonColorProperty); }
			set { SetValue(CancelButtonColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommand']/Docs/*" />
		public ICommand SearchCommand
		{
			get { return (ICommand)GetValue(SearchCommandProperty); }
			set { SetValue(SearchCommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='SearchCommandParameter']/Docs/*" />
		public object SearchCommandParameter
		{
			get { return GetValue(SearchCommandParameterProperty); }
			set { SetValue(SearchCommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
		}

		public int CursorPosition
		{
			get { return (int)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		public int SelectionLength
		{
			get { return (int)GetValue(SelectionLengthProperty); }
			set { SetValue(SelectionLengthProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public event EventHandler SearchButtonPressed;

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SearchBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SearchBar>>(() => new PlatformConfigurationRegistry<SearchBar>(this));
		}

		ICommand ICommandElement.Command => SearchCommand;

		object ICommandElement.CommandParameter => SearchCommandParameter;

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this);

		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		/// <include file="../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="//Member[@MemberName='OnSearchButtonPressed']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void OnSearchButtonPressed()
		{
			ICommand cmd = SearchCommand;

			if (cmd != null && !cmd.CanExecute(SearchCommandParameter))
				return;

			cmd?.Execute(SearchCommandParameter);
			SearchButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, SearchBar> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}
