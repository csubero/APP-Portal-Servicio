using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class AddTechnicianRegistryViewModel : BaseViewModel
    {
        #region Properties
        private CDTViewModel _CDT;
        private TechnicianRegistryViewModel _ToAdd;
        private CDTTicketViewModel _SelectedCDTTicket;
        private int _SelectedPersonal;
        private bool _IsBusy;
        private bool _IsInformationCorrect;
        private readonly IPageService _pageService;
        private ObservableCollection<TechnicianViewModel> _AvailableTechnicians;

        public int SelectedPersonal
        {
            get { return _SelectedPersonal; }
            set { SetValue(ref _SelectedPersonal, value); }
        }
        public CDTViewModel CDT
        {
            get { return _CDT; }
            set { SetValue(ref _CDT, value); }
        }
        public TechnicianRegistryViewModel ToAdd
        {
            get { return _ToAdd; }
            set { SetValue(ref _ToAdd, value); }
        }
        public CDTTicketViewModel SelectedCDTTicket
        {
            get { return _SelectedCDTTicket; }
            set { SetValue(ref _SelectedCDTTicket, value); }
        }
        public ref CDTTicketViewModel RefSelectedCDTTicket
        {
            get { return ref _SelectedCDTTicket; }
        }
        public ObservableCollection<TechnicianViewModel> AvailableTechnicians
        {
            get { return _AvailableTechnicians; }
            set { SetValue(ref _AvailableTechnicians, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsInformationCorrect
        {
            get { return _IsInformationCorrect; }
            set { SetValue(ref _IsInformationCorrect, value); }
        }

        public ICommand AddRegistryCommand { get; private set; }
        public ICommand ValidateInformationCommand { get; private set; }
        #endregion

        #region Constructors
        public AddTechnicianRegistryViewModel(IPageService pageService, ref CDTViewModel cdt, int selectedST)
        {
            _pageService = pageService;
            CDT = cdt;
            SelectedCDTTicket = CDT.CDTTickets[selectedST];
            LoadTechnicians().ConfigureAwait(true);
            ToAdd = new TechnicianRegistryViewModel(new Models.TechnicianRegistry());
            AddRegistryCommand = new Command(async () => await AddRegistry());
            ValidateInformationCommand = new Command(ValidateInformation);
        }
        #endregion

        private async Task LoadTechnicians() =>
            AvailableTechnicians = await CRMConnector.GetTechniciansViewModel();

        private async Task AddRegistry()
        {
            IsBusy = true;
            var reg = await CRMConnector.CreateNewTechnicianRegistry(ToAdd.ToModel(), CDT.ToModel(), SelectedCDTTicket.ToModel());
            RefSelectedCDTTicket.TechniciansRegistered.Add(new TechnicianRegistryViewModel(reg));
            IsBusy = false;
            await _pageService.PopUpPopAsync();
        }

        private void ValidateInformation() =>
            IsInformationCorrect = (ToAdd.Technician != null);
    }
}
