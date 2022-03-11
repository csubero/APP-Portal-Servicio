using PortalServicio.Services;
using PortalServicio.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class MyLegalizationsViewModel : BaseViewModel
    {
        #region Properties
        private LegalizationViewModel _SelectedLegalization;
        private ObservableCollection<LegalizationViewModel> _LegalizationsObtained;
        private ObservableCollection<LegalizationViewModel> _LegalizationsFiltered;
        private string _SearchText;
        private bool _IsBusy;
        private bool _IsLoading;
        private readonly IPageService _pageService;

        public LegalizationViewModel SelectedLegalization
        {
            get { return _SelectedLegalization; }
            set { SetValue(ref _SelectedLegalization, value); }
        }
        public ref LegalizationViewModel SelectedLegalizationRef
        {
            get { return ref _SelectedLegalization; }
        }
        public ObservableCollection<LegalizationViewModel> LegalizationsObtained
        {
            get { return _LegalizationsObtained; }
            private set { SetValue(ref _LegalizationsObtained, value); }
        }
        public ObservableCollection<LegalizationViewModel> LegalizationsFiltered
        {
            get { return _LegalizationsFiltered; }
            set { SetValue(ref _LegalizationsFiltered, value); }
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
        #endregion

        #region Commands
        public ICommand OpenLegalizationCommand { get; private set; }  //Network Operations 
        public ICommand LoadLegalizationsCommand { get; private set; } //Network Operations
        public ICommand CreateNewLegalizationCommand { get; private set; }
        public ICommand FilterLegalizationsCommand { get; private set; }
        #endregion

        public MyLegalizationsViewModel(IPageService pageService)
        {
            _pageService = pageService;
            LoadLegalizationsCommand = new Command(async () => await LoadLegalizations());
            FilterLegalizationsCommand = new Command(FilterLegalizations);
            OpenLegalizationCommand = new Command(async () => await OpenLegalization());
            CreateNewLegalizationCommand = new Command(async ()=> await CreateNewLegalization());
            LegalizationsObtained = new ObservableCollection<LegalizationViewModel>();
            LoadLegalizations().ConfigureAwait(false);
        }

        #region Actions (implementation of Commands)
        /// <summary>
        /// Loads PARTIAL legalizations of the logged in user from CRM
        /// </summary>
        /// <returns>Void</returns>
        private async Task LoadLegalizations()
        {
            if (IsLoading)
                return;
            IsLoading = true;
            try
            {
                LegalizationsObtained = new ObservableCollection<LegalizationViewModel>();
                LegalizationsObtained = await CRMConnector.GetMoneyLegalizationsDTOViewModel();
                FilterLegalizations();
            }catch(Exception ex)
            {
                await _pageService.DisplayAlert("Error", ex.Message, "Ok");
            }          
            IsLoading = false;
        }

        /// <summary>
        /// Loads a FULL version of an specific legalization to view its data.
        /// </summary>
        /// <returns>Void</returns>
        private async Task OpenLegalization()
        {
            if (SelectedLegalization == null)
                return;
            try
            {
                await _pageService.PushAsync(new MyLegalizationSummary(await CRMConnector.GetMoneyLegalizationViewModel(SelectedLegalization.InternalId)));
            }
            catch(Exception ex)
            {
                await _pageService.DisplayAlert("Error", ex.Message, "Ok");
            }
            SelectedLegalization = null;
        }

        /// <summary>
        /// Opens a new page to fill the form required to create a new legalization.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreateNewLegalization()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            Page p = new CreateNewLegalizationPage
            {
                BindingContext = new CreateNewLegalizationViewModel(new PageService())
            };
            await _pageService.PushAsync(p);
            IsBusy = false;
        }

        /// <summary>
        /// Filters the legalization obtained from CRM by the text written in the search bar.
        /// </summary>
        private void FilterLegalizations() =>
            LegalizationsFiltered = string.IsNullOrEmpty(SearchText)? new ObservableCollection<LegalizationViewModel>(LegalizationsObtained.OrderByDescending(l=>l.LegalizationNumber)) : new ObservableCollection<LegalizationViewModel>(LegalizationsObtained.Where(l => l.LegalizationNumber.Contains(SearchText)));
        #endregion
    }
}
