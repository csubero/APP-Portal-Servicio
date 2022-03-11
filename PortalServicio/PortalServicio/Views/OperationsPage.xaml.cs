using PortalServicio.Services;
using PortalServicio.ViewModels;
using PortalServicio.Views;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OperationsPage : ContentPage
    {
        #region Constructors
        public OperationsPage(string username)
        {
            InitializeComponent();
            LblWelcome.Text += username;
            Image imgLogout = this.FindByName<Image>("BtnLogout");
            imgLogout.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    ClickEffectButton(imgLogout);
                    CRMConnector.Logout();
                    DependencyService.Get<IClear>().ClearCache();
                    await Navigation.PopAsync();
                })
            });
            CreateMenuButton(OnBtnCasesTapped, "toolicon.png", "Casos", 0, 0); //0
            CreateMenuButton(OnBtnProgrammingTapped, "scheduleicon.png", "Programaciones", 1, 0); //1
            CreateMenuButton(OnBtnTicketTapped, "ticketicon.png", "Crear Ticket", 0, 1);  //2
            CreateMenuButton(OnBtnCDTsTapped, "employeeicon.png", "CDTs", 1, 1);  //3
            CreateMenuButton(OnBtnLegalizationTapped, "moneyicon.png", "Legalizaciones", 0, 2);  //3
            //CreateMenuButton(OnBtnContractsTapped, "employeeicon.png", "Contratistas", 1, 2); //3
        }
        #endregion

        #region Private Methods
        private void OnBtnCDTsTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Navigation.PushAsync(new ListCDTsPage());
        }

        private void OnBtnContractsTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Navigation.PushAsync(new ListContractsPage());
        }

        private void OnBtnProgrammingTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Navigation.PushAsync(new ListProgramCasesPage());
        }

        private void OnBtnLegalizationTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Page p = new MyLegalizationsPage
            {
                BindingContext = new MyLegalizationsViewModel(new PageService())
            };
            Navigation.PushAsync(p);
        }

        private void OnOptionsTapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }

        private void OnBtnCasesTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Navigation.PushAsync(new ListCasesPage());
        }

        private void OnBtnTicketTapped(object sender, EventArgs e)
        {
            ClickEffect(sender);
            Navigation.PushAsync(new CreateTicketPage());
        }

        private void CreateMenuButton(EventHandler callback,string iconName, string description, int column, int row)
        {
            Grid MenuContainer = this.FindByName<Grid>("MenuContainer");
            Frame buttonFrame = new Frame
            {
                VerticalOptions = LayoutOptions.Center,
                CornerRadius = 10,
                HasShadow = false,
                Style = (Style) App.Current.Resources["mainButtonColor"],
                Padding = new Thickness(10)
            };
            TapGestureRecognizer tapHandler = new TapGestureRecognizer();
            tapHandler.Tapped += callback;
            buttonFrame.GestureRecognizers.Add(tapHandler);
            StackLayout ContentContainer = new StackLayout();
            Image icon = new Image()
            {
                Source = ImageSource.FromResource(iconName),
                HeightRequest = 75,
                Aspect = Aspect.AspectFit
            };
            Label label = new Label
            {
                Text = description,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            ContentContainer.Children.Add(icon);
            ContentContainer.Children.Add(label);
            buttonFrame.Content = ContentContainer;
            MenuContainer.Children.Add(buttonFrame,column,row);
        }

        private void ClickEffect(Object item)
        {
            (item as Frame).Opacity = 0.5;
            Task.Run(async () => { Thread.Sleep(300); (item as Frame).Opacity = 1; });
        }

        void ClickEffectButton(Image item)
        {
            item.Opacity = 0.5;
            Task.Run(async () => { Thread.Sleep(300); item.Opacity = 1; });
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        #endregion
    }
}