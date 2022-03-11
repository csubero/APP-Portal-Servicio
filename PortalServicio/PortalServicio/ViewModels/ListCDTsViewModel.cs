using Plugin.Connectivity;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class ListCDTsViewModel : BaseViewModel
    {
        #region Properties
        private CDTViewModel _SelectedCDT;
        private ObservableCollection<CDTViewModel> _CDTsObtained;
        private ObservableCollection<CDTViewModel> _CDTsFiltered;
        private string _SearchText;
        private bool _IsBusy;
        private bool _IsLoading;
        private readonly IPageService _pageService;

        public CDTViewModel SelectedCDT
        {
            get { return _SelectedCDT; }
            set { SetValue(ref _SelectedCDT, value); }
        }
        public ObservableCollection<CDTViewModel> CDTsObtained
        {
            get { return _CDTsObtained; }
            private set { SetValue(ref _CDTsObtained, value); }
        }
        public ObservableCollection<CDTViewModel> CDTsFiltered
        {
            get { return _CDTsFiltered; }
            private set { SetValue(ref _CDTsFiltered, value); }
        }
        public string SearchText
        {
            get { return _SearchText; }
            set { SetValue(ref _SearchText, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsLoading
        {
            get { return _IsLoading; }
            set { SetValue(ref _IsLoading, value); }
        }

        public ICommand OpenCDTCommand { get; private set; }
        public ICommand SearchCDTsCommand { get; private set; }
        public ICommand FilterCDTsCommand { get; private set; }
        public ICommand LoadCDTsCommand { get; private set; }
        #endregion

        #region Constructors
        public ListCDTsViewModel(IPageService pageService)
        {
            _pageService = pageService;
            CDTsObtained = new ObservableCollection<CDTViewModel>();
            OpenCDTCommand = new Command(async () => await ClickCDT());
            SearchCDTsCommand = new Command(async () => await SearchCDTs());
            FilterCDTsCommand = new Command(FilterCDTs);
            LoadCDTsCommand = new Command(async () => await LoadCDTs());
            LoadCDTs().ConfigureAwait(false);
        }
        #endregion

        #region Events
        private async Task LoadCDTs()
        {
            IsLoading = true;
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), "Cargando CDTs", "Obteniendo datos sobre CDTs", 0, 2);
            await _pageService.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    vm.ProgressUp("Cargando CDTs desde información local");
                    CDTsObtained = new ObservableCollection<CDTViewModel>(await CRMConnector.GetLocalCDTs());
                }
                else
                {
                    vm.ProgressUp("Obteniendo CDTs del CRM");
                    CDTsObtained = new ObservableCollection<CDTViewModel>(await CRMConnector.GetCDTsViewModel());
                }
                vm.ProgressUp("Carga finalizada");
            }
            catch (HttpRequestException)
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    IsBusy = false;
                    await _pageService.DisplayAlert("Has perdido la conexión", "Se perdió la conexión mientras se realizaba la operación", "Ok");
                    return;
                }
                else
                    CDTsObtained = new ObservableCollection<CDTViewModel>((await CRMConnector.GetCDTsViewModel()).OrderByDescending(cdt => cdt.Number));
            }
            IsLoading = false;
            FilterCDTs();
            vm.IsLoading = false;
        }

        private async Task ClickCDT()
        {
            if (SelectedCDT == null || IsBusy)
                return;
            IsBusy = true;
            CDT cdt = null;
            //try
            //{
                if (CrossConnectivity.Current.IsConnected)
                    cdt = await CRMConnector.GetCDT(SelectedCDT.InternalId);
                else
                {
                    NotificationService.DisplayMessage("Sin internet", "Accesando información local.");
                    cdt = await CRMConnector.GetLocalCDT(SelectedCDT.InternalId);
                }
            //}
            //catch (HttpRequestException)
            //{
            //    IsBusy = false;
            //    if (!CrossConnectivity.Current.IsConnected)
            //        await _pageService.DisplayAlert("Sin conexión", "Se ha detectado cambios en la red o falta de conexión a la misma. Reintente la operación", "Ok");
            //    SelectedCDT = null;
            //    return;
            //}
            //catch (Exception ex)
            //{
            //    await _pageService.DisplayAlert("Error", ex.Message, "Ok");
            //}
            IsBusy = false;
            if (cdt == null)
                NotificationService.DisplayMessage("No hay información", "No hay información local para cargar este caso. ");
            else
                await _pageService.PushAsync(new CDTSummaryPage(new CDTViewModel(cdt)));
            SelectedCDT = null;
        }

        private async Task SearchCDTs()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                IsBusy = true;
                try
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar búsquedas de CDTs.", "Ok");
                        IsBusy = false;
                        return;
                    }
                    CDTsObtained = new ObservableCollection<CDTViewModel>((await CRMConnector.FindCDTsViewModel(SearchText)).OrderByDescending(cdt => cdt.Number));
                }
                catch (HttpRequestException)
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar búsquedas de casos.", "Ok");
                        IsBusy = false;
                        return;
                    }
                    CDTsObtained = new ObservableCollection<CDTViewModel>((await CRMConnector.FindCDTsViewModel(SearchText)).OrderByDescending(cdt => cdt.Number));
                }
                CDTsFiltered = CDTsObtained;
                IsBusy = false;
            }
        }

        private void FilterCDTs() =>
                CDTsFiltered = (string.IsNullOrEmpty(SearchText)) ? new ObservableCollection<CDTViewModel>(CDTsObtained.OrderByDescending(inc => inc.CreatedOn.Ticks)) : new ObservableCollection<CDTViewModel>(CDTsObtained.Where(
                    inc => (
                        inc.Number.ToUpper().Contains(SearchText.ToUpper()) ||
                        inc.Client.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                        inc.Client.Alias.ToUpper().Contains(SearchText.ToUpper())
                        )
                    ).OrderByDescending(inc => inc.CreatedOn.Ticks)
                );
        #endregion
    }
}
