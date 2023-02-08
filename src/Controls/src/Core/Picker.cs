#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="Type[@FullName='Microsoft.Maui.Controls.Picker']/Docs/*" />
	public partial class Picker : View, IFontElement, ITextElement, ITextAlignmentElement, IElementConfiguration<Picker>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TitleProperty']/Docs/*" />
		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(Picker), default(string));

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TitleColorProperty']/Docs/*" />
		public static readonly BindableProperty TitleColorProperty =
			BindableProperty.Create(nameof(TitleColor), typeof(Color), typeof(Picker), default(Color));

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='SelectedIndexProperty']/Docs/*" />
		public static readonly BindableProperty SelectedIndexProperty =
			BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Picker), -1, BindingMode.TwoWay,
									propertyChanged: OnSelectedIndexChanged, coerceValue: CoerceSelectedIndex);

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='ItemsSourceProperty']/Docs/*" />
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Picker), default(IList),
									propertyChanged: OnItemsSourceChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='SelectedItemProperty']/Docs/*" />
		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Picker), null, BindingMode.TwoWay,
									propertyChanged: OnSelectedItemChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='HorizontalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='VerticalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		readonly Lazy<PlatformConfigurationRegistry<Picker>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Picker()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Picker>>(() => new PlatformConfigurationRegistry<Picker>(this));
		}
		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
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

		TextTransform ITextElement.TextTransform
		{
			get => TextTransform.Default;
			set { }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='Items']/Docs/*" />
		public IList<string> Items { get; } = new LockableObservableListWrapper();

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='ItemsSource']/Docs/*" />
		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='SelectedIndex']/Docs/*" />
		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='SelectedItem']/Docs/*" />
		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TitleColor']/Docs/*" />
		public Color TitleColor
		{
			get { return (Color)GetValue(TitleColorProperty); }
			set { SetValue(TitleColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		BindingBase _itemDisplayBinding;
		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='ItemDisplayBinding']/Docs/*" />
		public BindingBase ItemDisplayBinding
		{
			get { return _itemDisplayBinding; }
			set
			{
				if (_itemDisplayBinding == value)
					return;

				OnPropertyChanging();
				var oldValue = value;
				_itemDisplayBinding = value;
				OnItemDisplayBindingChanged(oldValue, _itemDisplayBinding);
				OnPropertyChanged();
			}
		}

		public event EventHandler SelectedIndexChanged;

		static readonly BindableProperty s_displayProperty =
			BindableProperty.Create("Display", typeof(string), typeof(Picker), default(string));

		string GetDisplayMember(object item)
		{
			if (ItemDisplayBinding == null)
				return item == null ? string.Empty : item.ToString();

			ItemDisplayBinding.Apply(item, this, s_displayProperty);
			ItemDisplayBinding.Unapply();
			return (string)GetValue(s_displayProperty);
		}

		static object CoerceSelectedIndex(BindableObject bindable, object value)
		{
			var picker = (Picker)bindable;
			return picker.Items == null ? -1 : ((int)value).Clamp(-1, picker.Items.Count - 1);
		}

		void OnItemDisplayBindingChanged(BindingBase oldValue, BindingBase newValue)
		{
			ResetItems();
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var oldIndex = SelectedIndex;
			var newIndex = SelectedIndex = SelectedIndex.Clamp(-1, Items.Count - 1);
			// If the index has not changed, still need to change the selected item
			if (newIndex == oldIndex)
				UpdateSelectedItem(newIndex);

			Handler?.UpdateValue(nameof(IPicker.Items));
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Picker)bindable).OnItemsSourceChanged((IList)oldValue, (IList)newValue);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{
			var oldObservable = oldValue as INotifyCollectionChanged;
			if (oldObservable != null)
				oldObservable.CollectionChanged -= CollectionChanged;

			var newObservable = newValue as INotifyCollectionChanged;
			if (newObservable != null)
			{
				newObservable.CollectionChanged += CollectionChanged;
			}

			if (newValue != null)
			{
				((LockableObservableListWrapper)Items).IsLocked = true;
				ResetItems();
			}
			else
			{
				((LockableObservableListWrapper)Items).InternalClear();
				((LockableObservableListWrapper)Items).IsLocked = false;
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e);
					break;
				default: //Move, Replace, Reset
					ResetItems();
					break;
			}

			Handler?.UpdateValue(nameof(IPicker.Items));
		}

		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			foreach (object newItem in e.NewItems)
				((LockableObservableListWrapper)Items).InternalInsert(index++, GetDisplayMember(newItem));
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
			foreach (object _ in e.OldItems)
				((LockableObservableListWrapper)Items).InternalRemoveAt(index--);
		}

		void ResetItems()
		{
			if (ItemsSource == null)
				return;
			((LockableObservableListWrapper)Items).InternalClear();
			foreach (object item in ItemsSource)
				((LockableObservableListWrapper)Items).InternalAdd(GetDisplayMember(item));
			Handler?.UpdateValue(nameof(IPicker.Items));
			UpdateSelectedItem(SelectedIndex);
		}

		static void OnSelectedIndexChanged(object bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedItem(picker.SelectedIndex);
			picker.SelectedIndexChanged?.Invoke(bindable, EventArgs.Empty);
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedIndex(newValue);
		}

		void UpdateSelectedIndex(object selectedItem)
		{
			if (ItemsSource != null)
			{
				SelectedIndex = ItemsSource.IndexOf(selectedItem);
				return;
			}
			SelectedIndex = Items.IndexOf(selectedItem);
		}

		void UpdateSelectedItem(int index)
		{
			if (index == -1)
			{
				SelectedItem = null;
				return;
			}

			if (ItemsSource != null)
			{
				SelectedItem = ItemsSource[index];
				return;
			}

			SelectedItem = Items[index];
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Picker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{

		}

	}
}
