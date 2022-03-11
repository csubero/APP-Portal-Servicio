using PortalAPI.Contracts;
using PortalAPI.DTO;
using PortalServicio.Services;
using PortalServicio.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class CreateNewLegalizationViewModel : BaseViewModel
    {
        #region Properties
        private LegalizationViewModel _ToAdd;
        private bool _IsBusy;
        private readonly IPageService _pageService;
        private Dictionary<string, Types.SPCLEGALIZATION_TYPE> _LegalizationTypeDictionary;
        private ObservableCollection<CompanyViewModel> _AvailableCompanies;
        private ObservableCollection<CurrencyViewModel> _AvailableCurrencies;
        private bool _IsLoadingCompanies;
        private bool _IsLoadingCurrencies;
        private bool _IsSearchingRelation;
        private bool _IsRelatedCDTChosen;
        private bool _IsRelatedIncidentChosen;
        private bool _IsCurrenciesError;
        private bool _IsCompaniesError;
        private int _SelectedRelation;
        private string _CDTSearchText;
        private string _IncidentSearchText;

        public LegalizationViewModel ToAdd
        {
            get { return _ToAdd; }
            set
            {
                SetValue(ref _ToAdd, value);
            }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsLoadingCompanies
        {
            get { return _IsLoadingCompanies; }
            private set { SetValue(ref _IsLoadingCompanies, value); }
        }
        public bool IsLoadingCurrencies
        {
            get { return _IsLoadingCurrencies; }
            private set { SetValue(ref _IsLoadingCurrencies, value); }
        }
        public bool IsErrorCompanies
        {
            get { return _IsCompaniesError; }
            set { SetValue(ref _IsCompaniesError, value); }
        }
        public bool IsCurrenciesError
        {
            get { return _IsCurrenciesError; }
            set { SetValue(ref _IsCurrenciesError, value); }
        }
        public bool IsSearchingRelation
        {
            get { return _IsSearchingRelation; }
            set { SetValue(ref _IsSearchingRelation, value); }
        }
        public bool IsRelatedCDTChosen
        {
            get { return _IsRelatedCDTChosen; }
            set { SetValue(ref _IsRelatedCDTChosen, value); }
        }
        public bool IsRelatedIncidentChosen
        {
            get { return _IsRelatedIncidentChosen; }
            set { SetValue(ref _IsRelatedIncidentChosen, value); }
        }
        public int SelectedRelation
        {
            get { return _SelectedRelation; }
            set { SetValue(ref _SelectedRelation, value); }
        }
        public ObservableCollection<CompanyViewModel> AvailableCompanies
        {
            get { return _AvailableCompanies; }
            set { SetValue(ref _AvailableCompanies, value); }
        }
        public ObservableCollection<CurrencyViewModel> AvailableCurrencies
        {
            get { return _AvailableCurrencies; }
            set { SetValue(ref _AvailableCurrencies, value); }
        }
        public Dictionary<string, Types.SPCLEGALIZATION_TYPE> LegalizationTypeDictionary
        {
            get { return _LegalizationTypeDictionary; }
            private set { SetValue(ref _LegalizationTypeDictionary, value); }
        }
        public List<KeyValuePair<string, Types.SPCLEGALIZATION_TYPE>> LegalizationTypeList
        {
            get { return _LegalizationTypeDictionary.ToList(); }
        }
        public string CDTSearchText
        {
            get { return _CDTSearchText; }
            set { SetValue(ref _CDTSearchText, value); }
        }
        public string IncidentSearchText
        {
            get { return _IncidentSearchText; }
            set { SetValue(ref _IncidentSearchText, value); }
        }

        public ICommand CreateLegalizationCommand { get; private set; }  //network operation
        public ICommand LoadCompaniesCommand { get; private set; }  //network operation
        public ICommand LoadCurrenciesCommand { get; private set; } //network operation
        public ICommand SearchCDTRelationCommand { get; private set; } //network operation
        public ICommand SearchIncidentRelationCommand { get; private set; } //network operation
        public ICommand ChangeCDTTextCommand { get; private set; }
        public ICommand ChangeIncidentTextCommand { get; private set; }
        public string IncidentSearchText1 { get => _IncidentSearchText; set => _IncidentSearchText = value; }
        #endregion

        #region Constructors
        public CreateNewLegalizationViewModel(IPageService pageService, CDTViewModel cdt = null, IncidentViewModel incident = null)
        {
            _pageService = pageService;
            ToAdd = new LegalizationViewModel(new Models.Legalization())
            {
                RelatedCDT = cdt,
                RelatedCDTId = cdt?.SQLiteRecordId ?? 0,
                RelatedIncident = incident,
                RelatedIncidentId = incident?.SQLiteRecordId ?? 0
            };
            IsRelatedCDTChosen = cdt != null;
            SelectedRelation = cdt != null ? 2 : incident != null ? 1 : 0;
            CreateLegalizationCommand = new Command(async () => await CreateLegalization());
            LoadCompaniesCommand = new Command(async () => await LoadCompanies());
            LoadCurrenciesCommand = new Command(async () => await LoadCurrencies());
            SearchCDTRelationCommand = new Command(async () => await SearchCDTRelation());
            SearchIncidentRelationCommand = new Command(async () => await SearchIncidentRelation());
            ChangeCDTTextCommand = new Command(ChangeCDTText);
            ChangeIncidentTextCommand = new Command(ChangeIncidentText);
            LegalizationTypeDictionary = new Dictionary<string, Types.SPCLEGALIZATION_TYPE>
            {
                { "Caja Chica", Types.SPCLEGALIZATION_TYPE.CajaChica },
                { "Gastos de Viaje", Types.SPCLEGALIZATION_TYPE.GastosViaje },
                { "Transferencia", Types.SPCLEGALIZATION_TYPE.Transferencia },
                { "Tarjeta Empresarial", Types.SPCLEGALIZATION_TYPE.TarjetaEmpresarial }
            };
            LoadCompaniesCommand.Execute(null);
            LoadCurrenciesCommand.Execute(null);
        }
        #endregion

        private void ChangeCDTText()
        {
            ToAdd.RelatedCDT = null;
            ToAdd.RelatedCDTId = 0;
            IsRelatedCDTChosen = false;
        }

        private void ChangeIncidentText()
        {
            ToAdd.RelatedIncident = null;
            ToAdd.RelatedIncidentId = 0;
            IsRelatedIncidentChosen = false;
        }


        private async Task LoadCompanies()
        {
            IsLoadingCompanies = true;
            try
            {
                AvailableCompanies = await CRMConnector.GetCompaniesViewModel();
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error",ex.Message,"Ok");
            }
            IsLoadingCompanies = false;
        }

        private async Task LoadCurrencies()
        {
            IsLoadingCurrencies = true;
            AvailableCurrencies = await CRMConnector.GetCurrenciesViewModel();
            IsLoadingCurrencies = false;
        }

        private async Task SearchCDTRelation()
        {
            IsSearchingRelation = true;
            ToAdd.RelatedCDT = null;
            ToAdd.RelatedCDT = (await CRMConnector.FindCDTsViewModel("*"+CDTSearchText)).FirstOrDefault();
            if (ToAdd.RelatedCDT != null)
                ToAdd.RelatedCDT = new CDTViewModel(await CRMConnector.GetCDT(ToAdd.RelatedCDT.InternalId));
            IsRelatedCDTChosen = ToAdd.RelatedCDT != null;
            ToAdd.RelatedCDTId = IsRelatedCDTChosen ? ToAdd.RelatedCDT.SQLiteRecordId : 0;
            IsSearchingRelation = false;
        }

        private async Task SearchIncidentRelation()
        {
            IsSearchingRelation = true;
            ToAdd.RelatedIncident = null;
            DTO_IncidentLookUp partialIncident = (await CRMConnector.FindIncidents("*" + IncidentSearchText)).FirstOrDefault();
            if (partialIncident != null)
                ToAdd.RelatedIncident = new IncidentViewModel(await CRMConnector.GetIncident(partialIncident.InternalId));
            IsRelatedIncidentChosen = ToAdd.RelatedIncident != null;
            ToAdd.RelatedIncidentId = IsRelatedIncidentChosen ? ToAdd.RelatedIncident.SQLiteRecordId : 0;
            IsSearchingRelation = false;
        }

        /// <summary>
        /// Creates a new Legalization record on the CRM.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreateLegalization()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                await CRMConnector.CreateLegalization(ToAdd.ToModel(), SelectedRelation);
                await _pageService.PopAsync();
                NotificationService.DisplayMessage("Creado exitosamente", "Recargue listado para ver el cambio");
            }
            catch(Exception ex)
            {
                await _pageService.DisplayAlert("Ocurrió un problema", ex.Message, "Ok");
            }
            IsBusy = false;
        }
    }
}
