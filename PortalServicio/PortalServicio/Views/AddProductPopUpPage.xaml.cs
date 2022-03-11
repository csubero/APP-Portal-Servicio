using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PortalServicio.ViewModels;
using PortalServicio.Services;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddProductPopUpPage : PopupPage
    {
        public AddProductPopUpPage(ref IncidentViewModel vm, int selectedServiceTicket)
        {
            BindingContext = new AddProductViewModel(new PageService(), ref vm, selectedServiceTicket);
            InitializeComponent();
        }

        private void SelectProduct(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as AddProductViewModel).SelectProductCommand?.Execute(null);
        }

        private void SelectionMade(object sender, EventArgs e)
        {
            (BindingContext as AddProductViewModel).MarkTreatmentCommand?.Execute(null);              
        }

        private void CheckDestination(object sender, EventArgs e)
        {
            (BindingContext as AddProductViewModel).ValidateDestinationCommand?.Execute(null);
        }

        // Method for animation child in PopupPage
        // Invoced after custom animation end
        protected override Task OnAppearingAnimationEnd()
        {
            return Content.FadeTo(1);
        }

        // Method for animation child in PopupPage
        // Invoked before custom animation begin
        protected override Task OnDisappearingAnimationBegin()
        {
            return Content.FadeTo(0);
        }

        // Invoced when background is clicked
        protected override bool OnBackgroundClicked()
        {
            bool res = !loadingIndicator.IsVisible;
            if (res)
                PopupNavigation.Instance.PopAsync();
            return res;
        }
    }
}