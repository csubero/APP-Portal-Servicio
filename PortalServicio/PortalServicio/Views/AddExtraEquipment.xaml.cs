using PortalServicio.Services;
using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddExtraEquipment : PopupPage
	{
		public AddExtraEquipment (ref CDTViewModel cdt)
		{
			InitializeComponent ();
            BindingContext = new AddExtraEquipmentViewModel(new PageService(),ref cdt);
		}

        private void SelectProduct(object sender, SelectedItemChangedEventArgs e) =>
            (BindingContext as AddExtraEquipmentViewModel).SelectProductCommand?.Execute(null);

        private void CheckProcessType(object sender, EventArgs e) =>
            (BindingContext as AddExtraEquipmentViewModel).ValidateProcessTypeCommand?.Execute(null);
    }
}