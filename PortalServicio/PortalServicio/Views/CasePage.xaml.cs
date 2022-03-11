using Microsoft.Xrm.Sdk.Samples;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.Services;
using PortalServicio.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CasePage : ContentPage
    {
        #region Constructors
        public CasePage(IncidentViewModel incident)
        {
            BindingContext = new CaseViewModel(new PageService(), incident);
            InitializeComponent();
        }
        #endregion

        private void OpenTicket(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as CaseViewModel).OpenServiceTicketCommand.Execute(e.SelectedItem);
        }
    }
}