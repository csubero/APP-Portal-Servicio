using PortalServicio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTMaterialOrdersPage : ContentPage
	{
		public CDTMaterialOrdersPage () =>
			InitializeComponent ();

        private void TapProjecMaterial(object sender, SelectedItemChangedEventArgs e) =>
            (BindingContext as CDTSummaryViewModel).TapProjectElementCommand.Execute(null);

        private void ToggleCollapseMaterial(object sender, SelectedItemChangedEventArgs e) =>
           (BindingContext as CDTSummaryViewModel).ToggleCollapseMaterialCommand.Execute(null);
    }
}