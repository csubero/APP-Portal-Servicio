using PortalAPI.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class CreateNewLegalizationItemViewModel : BaseViewModel
    {
        #region Properties
        private readonly IPageService _PageService;
        private Dictionary<string, Types.SPCLEGALIZATIONITEM_TYPE> _LegalizationItemTypeDictionary;
        //private ObservableCollection<CurrencyViewModel> _AvailableCurrencies;
        //private bool _IsLoadingCurrencies;
        private bool _IsBusy;
        //private bool _IsCurrenciesError;
        private LegalizationItemViewModel _ToAdd;
        private LegalizationViewModel _legalization;

        public Dictionary<string, Types.SPCLEGALIZATIONITEM_TYPE> LegalizationItemTypeDictionary
        {
            get { return _LegalizationItemTypeDictionary; }
            private set { SetValue(ref _LegalizationItemTypeDictionary, value); }
        }
        public List<KeyValuePair<string, Types.SPCLEGALIZATIONITEM_TYPE>> LegalizationItemTypeList
        {
            get { return _LegalizationItemTypeDictionary.ToList(); }
        }
        //public ObservableCollection<CurrencyViewModel> AvailableCurrencies
        //{
        //    get { return _AvailableCurrencies; }
        //    set { SetValue(ref _AvailableCurrencies, value); }
        //}
        //public bool IsLoadingCurrencies
        //{
        //    get { return _IsLoadingCurrencies; }
        //    private set { SetValue(ref _IsLoadingCurrencies, value); }
        //}
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        //public bool IsCurrenciesError
        //{
        //    get { return _IsCurrenciesError; }
        //    private set { SetValue(ref _IsCurrenciesError, value); }
        //}
        public LegalizationItemViewModel ToAdd
        {
            get { return _ToAdd; }
            set { SetValue(ref _ToAdd, value); }
        }
        #endregion

        #region Commands
        public ICommand CreateLegalizationItemCommand { get; private set; } //network operation
        //public ICommand LoadCurrenciesCommand { get; private set; } //network operation
        #endregion

        public CreateNewLegalizationItemViewModel(IPageService pageService,ref LegalizationViewModel legalization)
        {
            _legalization = legalization;
            _PageService = pageService;
            //Commands set to instructions to execute
            //LoadCurrenciesCommand = new Command(async () => await LoadCurrencies());
            CreateLegalizationItemCommand = new Command(async () => await CreateLegalizationItem());
            //Dummy object to be filled by user interaction
            ToAdd = new LegalizationItemViewModel(new Models.LegalizationItem
            {
                LegalizationId = legalization.SQLiteRecordId,
                SpentOn = DateTime.Now
            })
            {
                Currency = legalization.MoneyCurrency,
                CurrencyId = legalization.MoneyCurrencyId
            };
            //Expense Type picker population. May use translating service for several languages.
            LegalizationItemTypeDictionary = new Dictionary<string, Types.SPCLEGALIZATIONITEM_TYPE>
            {
                { "Teléfono, Fax, Beeper", Types.SPCLEGALIZATIONITEM_TYPE.PhoneFaxBeeper },
                { "Internet y Cable", Types.SPCLEGALIZATIONITEM_TYPE.InternetCable },
                { "Alquiler Vehículos", Types.SPCLEGALIZATIONITEM_TYPE.CarRent },
                { "Fletes y Acarreos", Types.SPCLEGALIZATIONITEM_TYPE.Shipping },
                { "Combustible y Lubricantes", Types.SPCLEGALIZATIONITEM_TYPE.FuelOil },
                { "Pases, Parqueos y Peajes", Types.SPCLEGALIZATIONITEM_TYPE.TicketsParkingToll},
                { "Alimentación y Hospedaje", Types.SPCLEGALIZATIONITEM_TYPE.FoodHost },
                { "Mant. Mobiliario y Eq.", Types.SPCLEGALIZATIONITEM_TYPE.ManteinanceEquipment },
                { "Mant. Vehículos", Types.SPCLEGALIZATIONITEM_TYPE.CarManteinance },
                { "Atención a Funcionarios", Types.SPCLEGALIZATIONITEM_TYPE.EmployeeAttending },
                { "Atención a Clientes", Types.SPCLEGALIZATIONITEM_TYPE.ClientAttending },
                { "Correos y Courier", Types.SPCLEGALIZATIONITEM_TYPE.Courier },
                { "Papelería y Útiles", Types.SPCLEGALIZATIONITEM_TYPE.PaperAndOfficeTools },
                { "Vigilancia", Types.SPCLEGALIZATIONITEM_TYPE.Surveillance },
                { "Aseo y Limpieza", Types.SPCLEGALIZATIONITEM_TYPE.Cleaning },
                { "Transporte y Encomiendas", Types.SPCLEGALIZATIONITEM_TYPE.Transport },
                { "Publicidad", Types.SPCLEGALIZATIONITEM_TYPE.Advertising },
                { "Licitación y Proyectos", Types.SPCLEGALIZATIONITEM_TYPE.TenderProject },
                { "Servicios Profesionales", Types.SPCLEGALIZATIONITEM_TYPE.ProfessionalServices },
                { "Visas e Impuestos de Salida", Types.SPCLEGALIZATIONITEM_TYPE.VisaOrTravelTaxes },
                { "Materiales", Types.SPCLEGALIZATIONITEM_TYPE.Materials },
                { "Herramientas Menores", Types.SPCLEGALIZATIONITEM_TYPE.MinorTools },
                { "Capacitación a Personal", Types.SPCLEGALIZATIONITEM_TYPE.PersonalTraining },
                { "Gastos No Deducibles", Types.SPCLEGALIZATIONITEM_TYPE.NoDeductibleExpenses },
                { "Impuesto de Rentas a Empleados", Types.SPCLEGALIZATIONITEM_TYPE.EmployeeTaxes },
                { "Reparación de Equip. En Garantía", Types.SPCLEGALIZATIONITEM_TYPE.WarrantyEquipmentRepairing },
                { "Kilometraje", Types.SPCLEGALIZATIONITEM_TYPE.CarTraveling }
            };
            //Execute loadCurrencies asynchronously.
            //LoadCurrenciesCommand.Execute(null);
        }

        #region Actions (Implementation of Commands)
        /// <summary>
        /// Loads the currency symbols from CRM to fill the picker options
        /// </summary>
        /// <returns>Void</returns>
        //private async Task LoadCurrencies()
        //{
        //    IsLoadingCurrencies = true;
        //    IsCurrenciesError = false;
        //    try
        //    {
        //        AvailableCurrencies = await CRMConnector.GetCurrenciesViewModel();
        //        //Legalization Currency is automatically set to this picker by default, so legalization items use same currency as legalization record.        
        //        ToAdd.Currency = AvailableCurrencies.Where(c => c.SQLiteRecordId == _legalization.MoneyCurrency.SQLiteRecordId).FirstOrDefault();           
        //    }
        //    catch (Exception ex)
        //    {
        //        IsCurrenciesError = true;
        //        await _PageService.DisplayAlert("Error", ex.Message, "Ok");
        //    }         
        //    IsLoadingCurrencies = false;
        //}

        /// <summary>
        /// Creates a new Legalization Item in the CRM.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreateLegalizationItem()
        {
            IsBusy = true;
            try
            {
                _legalization.LegalizationItems.Add(new LegalizationItemViewModel(await CRMConnector.CreateLegalizationItem(_legalization.ToModel(), ToAdd.ToModel())));
                await _PageService.PopAsync();
            }
            catch (Exception ex)
            {
                await _PageService.DisplayAlert("Ha ocurrido un error",ex.Message,"Ok");
            }
            IsBusy = false;
        }
        #endregion
    }
}
