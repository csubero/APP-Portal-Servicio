using System.Collections;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.Controls
{
    public class SearchList : ContentView
    {
        #region Backing Fields
        /// <summary>
        /// Command to execute when Search Button on Search Bar is triggered.
        /// </summary>
        public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SearchList), null, propertyChanged: SearchCommandPropertyChanged);
        /// <summary>
        /// Command to execute when an Item of the List is selected.
        /// </summary>
        public static readonly BindableProperty OnSelectCommandProperty = BindableProperty.Create(nameof(OnSelectCommand), typeof(ICommand), typeof(SearchList), null, propertyChanged: OnSelectCommandPropertyChanged);
        /// <summary>
        /// Command to execute everytime SearchBar's text is changed
        /// </summary>
        public static readonly BindableProperty OnSearchTextChangeCommandProperty = BindableProperty.Create(nameof(OnSearchTextChangeCommand), typeof(ICommand), typeof(SearchList), null, propertyChanged: OnSearchTextChangeCommandPropertyChanged);
        /// <summary>
        /// Data Template to display for every Item in the ItemSource.
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(SearchList), null, propertyChanged: ItemTemplatePropertyChanged);
        /// <summary>
        /// Source of data for the internal List. Should be an ObservableCollection in order to update automatically.
        /// </summary>
        public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(nameof(ItemSource), typeof(IEnumerable), typeof(SearchList), null, defaultBindingMode: BindingMode.TwoWay, propertyChanged: ItemSourcePropertyChanged);
        /// <summary>
        /// Color for the placeholder text
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(SearchList), Color.Gray, propertyChanged: PlaceholderColorPropertyChanged);
        /// <summary>
        /// Text to assign to the placeholder.
        /// </summary>
        public static readonly BindableProperty PlaceholderTextProperty = BindableProperty.Create(nameof(PlaceholderText), typeof(string), typeof(SearchList), string.Empty, propertyChanged: PlaceholderTextPropertyChanged);
        /// <summary>
        /// SearchBar text input.
        /// </summary>
        public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(SearchList), string.Empty, defaultBindingMode: BindingMode.TwoWay, propertyChanged: SearchTextPropertyChanged);
        /// <summary>
        /// Currently selected Item of the list.
        /// </summary>
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SearchList), null, defaultBindingMode: BindingMode.TwoWay, propertyChanged: SelectedItemPropertyChanged);
        /// <summary>
        /// Is Refresh gesture enabled.
        /// </summary>
        public static readonly BindableProperty IsPullToRefreshEnabledProperty = BindableProperty.Create(nameof(IsPullToRefreshEnabled), typeof(bool), typeof(SearchList), false, defaultBindingMode: BindingMode.TwoWay, propertyChanged: IsPullToRefreshEnabledPropertyChanged);
        /// <summary>
        /// Is currently refreshing its content.
        /// </summary>
        public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(SearchList), false, defaultBindingMode: BindingMode.TwoWay, propertyChanged: IsRefreshingPropertyChanged);
        /// <summary>
        /// Action to execute when refresh is triggered.
        /// </summary>
        public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(SearchList), null, propertyChanged: RefreshCommandPropertyChanged);
        #endregion

        #region Properties
        private SearchBar _SearchBar;
        private ListView _List;
        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public ICommand SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }
        public ICommand OnSelectCommand
        {
            get { return (ICommand)GetValue(OnSelectCommandProperty); }
            set { SetValue(OnSelectCommandProperty, value); }
        }
        public ICommand OnSearchTextChangeCommand
        {
            get { return (ICommand)GetValue(OnSearchTextChangeCommandProperty); }
            set { SetValue(OnSearchTextChangeCommandProperty, value); }
        }
        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }
        public bool IsRefreshing
        {
            get { return (bool)GetValue(IsRefreshingProperty); }
            set { SetValue(IsRefreshingProperty, value); }
        }
        public bool IsPullToRefreshEnabled
        {
            get { return (bool)GetValue(IsPullToRefreshEnabledProperty); }
            set { SetValue(IsPullToRefreshEnabledProperty, value); }
        }
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        #endregion

        #region Constructor
        public SearchList()
        {
            StackLayout container = new StackLayout();
            _SearchBar = new SearchBar()
            {
                HeightRequest = 50
            };
            #region Establish DataBindings for SearchBar
            _SearchBar.SetBinding(SearchBar.SearchCommandProperty, new Binding(nameof(SearchCommand), BindingMode.OneWay, source: this));
            _SearchBar.SetBinding(SearchBar.TextProperty, new Binding(nameof(SearchText), BindingMode.TwoWay, source: this));
            _SearchBar.SetBinding(SearchBar.PlaceholderProperty, new Binding(nameof(PlaceholderText), BindingMode.OneWay, source: this));
            _SearchBar.SetBinding(SearchBar.PlaceholderColorProperty, new Binding(nameof(PlaceholderColor), BindingMode.OneWay, source: this));
            #endregion
            _SearchBar.TextChanged += _SearchBar_TextChanged;
            _List = new ListView(ListViewCachingStrategy.RecycleElementAndDataTemplate)
            {
                HasUnevenRows = true,
                SeparatorVisibility = SeparatorVisibility.None,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            #region Establish DataBindings for ListView
            _List.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ItemSource), BindingMode.TwoWay, source: this));
            _List.SetBinding(ListView.ItemTemplateProperty, new Binding(nameof(ItemTemplate), BindingMode.OneWay, source: this));
            _List.SetBinding(ListView.SelectedItemProperty, new Binding(nameof(SelectedItem), BindingMode.TwoWay, source: this));
            _List.SetBinding(ListView.IsRefreshingProperty, new Binding(nameof(IsRefreshing), BindingMode.TwoWay, source: this));
            _List.SetBinding(ListView.IsPullToRefreshEnabledProperty, new Binding(nameof(IsPullToRefreshEnabled), BindingMode.OneWay, source: this));
            _List.SetBinding(ListView.RefreshCommandProperty, new Binding(nameof(RefreshCommand), BindingMode.OneWay, source: this));
            #endregion
            _List.ItemSelected += _List_ItemSelected;
            container.Children.Add(_SearchBar);
            container.Children.Add(_List);
            Content = container;
        }
        #endregion

        #region Events
        private void _List_ItemSelected(object sender, SelectedItemChangedEventArgs e) =>
            OnSelectCommand?.Execute(null);

        private void _SearchBar_TextChanged(object sender, TextChangedEventArgs e) =>
            OnSearchTextChangeCommand?.Execute(null);

        private static void PlaceholderTextPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).PlaceholderText = (string)newValue;

        private static void PlaceholderColorPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).PlaceholderColor = (Color)newValue;

        private static void SearchCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).SearchCommand = (ICommand)newValue;

        private static void OnSelectCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).OnSelectCommand = (ICommand)newValue;

        private static void OnSearchTextChangeCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).OnSearchTextChangeCommand = (ICommand)newValue;

        private static void RefreshCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
           ((SearchList)bindable).RefreshCommand = (ICommand)newValue;

        private static void IsRefreshingPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
           ((SearchList)bindable).IsRefreshing = (bool)newValue;

        private static void IsPullToRefreshEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
           ((SearchList)bindable).IsPullToRefreshEnabled = (bool)newValue;

        private static void SelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).SelectedItem = newValue;

        private static void SearchTextPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).SearchText = (string)newValue;

        private static void ItemTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).ItemTemplate = (DataTemplate)newValue;

        private static void ItemSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((SearchList)bindable).ItemSource = (IEnumerable)newValue;
        #endregion
    }
}
