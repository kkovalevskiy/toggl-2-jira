using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Toggl2Jira.UI.Utils;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Toggl2Jira.UI.Views
{
    [TemplatePart(Name = TextBoxTemplateName, Type = typeof(TextBox))]
    [TemplatePart(Name = AutocompleteDataSelectorTemplateName, Type = typeof(ListBox))]
    [TemplateVisualState(GroupName = "BusyStatus", Name = LoadingStatusName)]
    [TemplateVisualState(GroupName = "BusyStatus", Name = ReadyStatusName)]
    [TemplateVisualState(GroupName = "AutocompleteDataStatus", Name = ShowAutocompleteDataStatusName)]
    [TemplateVisualState(GroupName = "AutocompleteDataStatus", Name = HideAutocompleteDataStatusName)]
    public class Autocomplete : Control
    {
        public const string TextBoxTemplateName = "PART_TextBox";
        public const string AutocompleteDataSelectorTemplateName = "PART_AutocompleteDataSelector";
        
        public const string LoadingStatusName = "Loading";
        public const string ReadyStatusName = "Ready";
        public const string ShowAutocompleteDataStatusName = "Show";
        public const string HideAutocompleteDataStatusName = "Hide";

        private bool _isAutocompleteDataVisible = false;
        private bool _isLoading = false;

        private TextBox _textBox;
        private ListBox _autocompleteDataSelector;
        
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        
        static Autocomplete()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Autocomplete),new FrameworkPropertyMetadata(typeof(Autocomplete)));
        }

        public Autocomplete()
        {
            Loaded += (sender, args) =>
                VisualTreeHelperEx.FindAncestorByType<Window>(this).LocationChanged +=
                    (o, eventArgs) => IsAutocompleteDataVisible = false;            
        }

        private bool IsLoading
        {
            set
            {
                if (_isLoading == value)
                {
                    return;
                }

                _isLoading = value;
                VisualStateManager.GoToState(this, value ? LoadingStatusName : ReadyStatusName, true);
            }
        }

        private bool IsAutocompleteDataVisible
        {
            set
            {
                if (_isAutocompleteDataVisible == value)
                {
                    return;
                }

                _isAutocompleteDataVisible = value;
                
                VisualStateManager.GoToState(this,
                    value ? ShowAutocompleteDataStatusName : HideAutocompleteDataStatusName, true);
                _autocompleteDataSelector.SelectedIndex = -1;
                if (AutocompleteData != null)
                {
                    _autocompleteDataSelector.ScrollIntoView(AutocompleteData.OfType<object>().First());
                }
            }
        }

        private IEnumerable AutocompleteData
        {
            set => _autocompleteDataSelector.ItemsSource = value;
            get => _autocompleteDataSelector.ItemsSource;
        }

        public override void OnApplyTemplate()
        {            
            base.OnApplyTemplate();
            _subscriptions.ForEach(s => s.Dispose());
            _subscriptions.Clear();
            
            _textBox = (TextBox) GetTemplateChild(TextBoxTemplateName);
            _autocompleteDataSelector = (ListBox) GetTemplateChild(AutocompleteDataSelectorTemplateName);
            
            AttachListenersToControl();            
        }

        private void AttachListenersToControl()
        {
            if (_textBox == null)
            {
                return;
            }

            var scheduler = new DispatcherScheduler(Dispatcher);
            
            var textChanged = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                d => d.Invoke,                 
                h => _textBox.KeyUp += h ,
                h => _textBox.KeyUp -= h ,
                scheduler);


            var textSuggestions = textChanged
                .ObserveOnDispatcher()
                .Where(e => e.EventArgs.Key.In(Key.Escape, Key.Down, Key.Up, Key.Enter) == false)
                .Select(t => _textBox.Text)
                .DistinctUntilChanged() 
                .Do(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AutocompleteData = null;
                        IsAutocompleteDataVisible = false;                        
                    });
                })
                .Throttle(TimeSpan.FromMilliseconds(500), scheduler)
                .Do(_ => Dispatcher.Invoke(() => IsLoading = true))
                .Select(t => Observable.FromAsync(() => AutocompleteDataSource?.GetAutocompleteData(t)))                
                .Switch();
            
            var subscription = textSuggestions.Subscribe(s =>
            {
                Dispatcher.Invoke(() =>
                {
                    AutocompleteData = s.Any() == false ? null : s;
                    IsAutocompleteDataVisible = true;
                    IsLoading = false;
                });
            });
            
            _subscriptions.Add(subscription);
            _textBox.LostKeyboardFocus += (sender, args) => IsAutocompleteDataVisible = false;            
            _textBox.PreviewKeyDown += ArrowsHandler;
            _textBox.PreviewKeyDown += EscHandler;
            _textBox.PreviewKeyDown += EnterHandler;
            _textBox.TextChanged += (sender, args) => Text = _textBox.Text;
        }

        private void EnterHandler(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key != Key.Enter) return;
            if (_autocompleteDataSelector.SelectedIndex == -1) return;
            var text = AutocompleteDataSource.GetTextFromAutocompleteData(_autocompleteDataSelector.SelectedItem);
            Text = text;
            IsAutocompleteDataVisible = false;
        }

        private void EscHandler(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                IsAutocompleteDataVisible = false;
            }
        }

        private void ArrowsHandler(object sender, KeyEventArgs keyEventArgs)
        {
            int? selectedItemIncrement = null;
            if (keyEventArgs.Key == Key.Down)
            {
                selectedItemIncrement = 1;
            }

            if (keyEventArgs.Key == Key.Up)
            {
                selectedItemIncrement = -1;
            }

            if (selectedItemIncrement.HasValue == false || AutocompleteData == null)
            {
                return;
            }

            IsAutocompleteDataVisible = true;
            _autocompleteDataSelector.SelectedIndex = Math.Max(selectedItemIncrement.Value + _autocompleteDataSelector.SelectedIndex, -1);
            _autocompleteDataSelector.ScrollIntoView(_autocompleteDataSelector.SelectedItem);
        }

        public static readonly DependencyProperty AutocompleteDataSourceProperty = DependencyProperty.Register(
            nameof(AutocompleteDataSource), typeof(IAutocompleteDataSource), typeof(Autocomplete), new PropertyMetadata(default(IAutocompleteDataSource)));

        public IAutocompleteDataSource AutocompleteDataSource
        {
            get => (IAutocompleteDataSource) GetValue(AutocompleteDataSourceProperty);
            set => SetValue(AutocompleteDataSourceProperty, value);
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (Autocomplete), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, TextPropertyChangedCallback, null, true, UpdateSourceTrigger.PropertyChanged));

        private static void TextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var autocomplete = (Autocomplete) dependencyObject;
            autocomplete._textBox.Text = (string) dependencyPropertyChangedEventArgs.NewValue;
        }
    }
}