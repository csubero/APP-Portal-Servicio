using PortalServicio.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class CDTSummaryViewModel : BaseViewModel
    {
        #region Properties
        private CDTViewModel _CDT;
        private ProjectEquipmentViewModel _SelectedProjectEquipment;
        private ProjectMaterialViewModel _SelectedProjectMaterial;
        private MaterialRequestOrderViewModel _SelectedMaterialOrder;
        private EquipmentRequestOrderViewModel _SelectedEquipmentOrder;
        private ExtraEquipmentRequestViewModel _SelectedExtraEquipment;
        private CDTTicketViewModel _SelectedTicket;
        private bool _IsBusy;
        private bool _IsMainContactSet;
        private bool _IsSecondaryContactSet;
        private bool _IsMonitorAccountSet;
        private int _ProjectEquipmentHeight;
        private readonly IPageService _pageService;

        public CDTViewModel CDT
        {
            get { return _CDT; }
            set
            {
                SetValue(ref _CDT, value);
                ProjectEquipmentHeight = _CDT.ProjectEquipment.Count * 125;
            }
        }
        public ref CDTViewModel RefCDT
        {
            get { return ref _CDT; }
        }
        public ProjectEquipmentViewModel SelectedProjectEquipment
        {
            get { return _SelectedProjectEquipment; }
            set { SetValue(ref _SelectedProjectEquipment, value); }
        }
        public ProjectMaterialViewModel SelectedProjectMaterial
        {
            get { return _SelectedProjectMaterial; }
            set { SetValue(ref _SelectedProjectMaterial, value); }
        }
        public MaterialRequestOrderViewModel SelectedMaterialOrder
        {
            get { return _SelectedMaterialOrder; }
            set { SetValue(ref _SelectedMaterialOrder, value); }
        }
        public EquipmentRequestOrderViewModel SelectedEquipmentOrder
        {
            get { return _SelectedEquipmentOrder; }
            set { SetValue(ref _SelectedEquipmentOrder, value); }
        }
        public ExtraEquipmentRequestViewModel SelectedExtraEquipment
        {
            get { return _SelectedExtraEquipment; }
            set { SetValue(ref _SelectedExtraEquipment, value); }
        }
        public CDTTicketViewModel SelectedTicket
        {
            get { return _SelectedTicket; }
            set { SetValue(ref _SelectedTicket, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsMainContactSet
        {
            get { return _IsMainContactSet; }
            private set { SetValue(ref _IsMainContactSet, value); }
        }
        public bool IsSecondaryContactSet
        {
            get { return _IsSecondaryContactSet; }
            private set { SetValue(ref _IsSecondaryContactSet, value); }
        }
        public bool IsMonitorAccountSet
        {
            get { return _IsMonitorAccountSet; }
            private set { SetValue(ref _IsMonitorAccountSet, value); }
        }
        public int ProjectEquipmentHeight
        {
            get { return _ProjectEquipmentHeight; }
            set { SetValue(ref _ProjectEquipmentHeight, value); }
        }

        public ICommand TapProjectElementCommand { get; private set; }
        public ICommand ReloadCDTCommand { get; private set; }
        public ICommand SaveChangesCommand { get; private set; }
        public ICommand OrderEquipmentCommand { get; private set; }
        public ICommand OrderMaterialsCommand { get; private set; }
        public ICommand ToggleCollapseMaterialCommand { get; private set; }
        public ICommand ToggleCollapseEquipmentCommand { get; private set; }
        public ICommand AddCDTTicketCommand { get; private set; }
        public ICommand AddExtraEquipmentCommand { get; private set; }
        public ICommand OpenTicketCommand { get; private set; }
        public ICommand ApproveByAdministrationCommand { get; private set; }
        public ICommand ApproveByComercialCommand { get; private set; }
        public ICommand ApproveByInstallationsCommand { get; private set; }
        public ICommand ApproveByPlanningCommand { get; private set; }
        public ICommand ApproveByFinancialCommand { get; private set; }
        public ICommand ApproveByCustomerServiceCommand { get; private set; }
        public ICommand ApproveByOperationsCommand { get; private set; }
        public ICommand SelectExtraEquipmentCommand { get; private set; }
        #endregion

        #region Constructors
        public CDTSummaryViewModel(IPageService pageService, CDTViewModel cdt)
        {
            _pageService = pageService;
            CDT = cdt;          
            ReloadCDTCommand = new Command(async () => await ReloadCDT());
            TapProjectElementCommand = new Command(Ignore);
            SaveChangesCommand = new Command(async () => await SaveChanges());
            OrderEquipmentCommand = new Command(async() => await OrderEquipment());
            OrderMaterialsCommand = new Command(async () => await OrderMaterials());
            ToggleCollapseMaterialCommand = new Command(ToggleCollapseMaterial);
            ToggleCollapseEquipmentCommand = new Command(ToggleCollapseEquipment);
            ApproveByAdministrationCommand = new Command(async () => await ApproveByAdministration());
            ApproveByComercialCommand = new Command(async () => await ApproveByComercial() );
            ApproveByCustomerServiceCommand = new Command(async () => await ApproveByCustomerService() );
            ApproveByFinancialCommand = new Command(async () => await ApproveByFinancial() );
            ApproveByInstallationsCommand = new Command(async () => await ApproveByInstallations());
            ApproveByPlanningCommand = new Command(async () => await ApproveByPlanning());
            ApproveByOperationsCommand = new Command(async () => await ApproveByOperations());
            OpenTicketCommand = new Command(async () => await OpenTicket());
            AddCDTTicketCommand = new Command(async () => await AddNewCDTTicket());
            AddExtraEquipmentCommand = new Command(async () => await AddExtraEquipment());
            SelectExtraEquipmentCommand = new Command(SelectExtraEquipment);
            UpdateUI();
        }
        #endregion

        #region Events
        private void SelectExtraEquipment() =>
           SelectedExtraEquipment = null;

        private async Task AddNewCDTTicket()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            CDTTicketViewModel toAdd = new CDTTicketViewModel(await CRMConnector.CreateNewCDTTicket(CDT.ToModel()));
            CDT.CDTTickets.Add(toAdd);
            IsBusy = false;
        }

        private async Task AddExtraEquipment() =>
            await _pageService.PopUpPushAsync(new AddExtraEquipment(ref RefCDT));

        private async Task OpenTicket()
        {
            if (SelectedTicket == null)
                return;
            await _pageService.PushAsync(new CDTTicketSummaryPage(ref RefCDT, CDT.CDTTickets.IndexOf(SelectedTicket)));
            SelectedTicket = null;
        }

        private void UpdateUI()
        {
            IsMainContactSet = !string.IsNullOrEmpty(CDT.MainContact);
            IsSecondaryContactSet = !string.IsNullOrEmpty(CDT.SecondaryContact);
            IsMonitorAccountSet = !string.IsNullOrEmpty(CDT.MonitoringAccountName);
        }

        /// <summary>
        /// Crea una nueva boleta de servicio, ya sea con internet o sin él.
        /// </summary>
        /// <returns>Void</returns>
        private async Task ReloadCDT()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            CDT = new CDTViewModel(await CRMConnector.GetCDT(CDT.InternalId));
            IsBusy = false;
        }

        private async Task ApproveByAdministration()
        {
            IsBusy = true;
            if(await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.","Firmar","Cancelar"))
            {
                if(await CRMConnector.ApproveAdministrationForCDT(CDT))
                {
                    CDT.ApproverAdministration = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverAdministrationId = CDT.ApproverAdministration.SQLiteRecordId;
                    CDT.IsApprovedAdministration = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByPlanning()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApprovePlanningForCDT(CDT))
                {
                    CDT.ApproverPlanning = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverPlanningId = CDT.ApproverPlanning.SQLiteRecordId;
                    CDT.IsApprovedPlanning = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByComercial()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApproveComercialForCDT(CDT))
                {
                    CDT.ApproverComercial = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverComercialId = CDT.ApproverComercial.SQLiteRecordId;
                    CDT.IsApprovedComercial = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByFinancial()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApproveFinancialForCDT(CDT))
                {
                    CDT.ApproverFinancial = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverFinancialId = CDT.ApproverFinancial.SQLiteRecordId;
                    CDT.IsApprovedFinancial = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByCustomerService()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApproveCustomerServiceForCDT(CDT))
                {
                    CDT.ApproverCustomerService = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverCustomerServiceId = CDT.ApproverCustomerService.SQLiteRecordId;
                    CDT.IsApprovedCustomerService = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByInstallations()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApproveInstallationsForCDT(CDT))
                {
                    CDT.ApproverInstallation = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverInstallationId = CDT.ApproverInstallation.SQLiteRecordId;
                    CDT.IsApprovedInstallation = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task ApproveByOperations()
        {
            IsBusy = true;
            if (await _pageService.DisplayAlert("Estás seguro de realizar esta acción.", "Debes tener permisos para realizar esta acción.", "Firmar", "Cancelar"))
            {
                if (await CRMConnector.ApproveOperationsForCDT(CDT))
                {
                    CDT.ApproverOperations = new SystemUserViewModel(await CRMConnector.GetLoginUserOffline());
                    CDT.ApproverOperationsId = CDT.ApproverOperations.SQLiteRecordId;
                    CDT.IsApprovedOperations = true;
                }
                else
                    await _pageService.DisplayAlert("Error al realizar operación", "No cuenta con permisos necesarios para realizar esta acción", "Cerrar");
            }
            IsBusy = false;
        }

        private async Task SaveChanges()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            await CRMConnector.SaveProjectChanges(CDT);
            IsBusy = false;
        }

        private async Task OrderEquipment()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                if (await CRMConnector.SaveProjectChanges(CDT))
                {
                    await CRMConnector.OrderEquipment(CDT);
                    await _pageService.DisplayAlert("Exito", "Finalizado con éxito", "Ok");
                }
                else
                    await _pageService.DisplayAlert("Error", "No se ha podido guardar las cantidades requeridas, por lo que no se realizó la solicitud", "Ok");
            }catch(Exception ex)
            {
                await _pageService.DisplayAlert(ex.Message,ex.StackTrace,"Cerrar");
            }
            IsBusy = false;
            
        }

        private void ToggleCollapseMaterial()
        {
            if (SelectedMaterialOrder != null)
            {
                SelectedMaterialOrder.IsCollapsed = !SelectedMaterialOrder.IsCollapsed;
                SelectedMaterialOrder = null;
            }
        }

        private void ToggleCollapseEquipment()
        {
            if (SelectedEquipmentOrder != null)
            {
                SelectedEquipmentOrder.IsCollapsed = !SelectedEquipmentOrder.IsCollapsed;
                SelectedEquipmentOrder = null;
            }
        }

        private async Task OrderMaterials()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            if (await CRMConnector.SaveProjectChanges(CDT))
            {
                await CRMConnector.OrderMaterials(CDT);
                await _pageService.DisplayAlert("Exito", "Finalizado con éxito", "Ok");
            }
            else
                await _pageService.DisplayAlert("Error", "No se ha podido guardar las cantidades requeridas, por lo que no se realizó la solicitud", "Ok");
            IsBusy = false;
        }

        private void Ignore() =>
            SelectedProjectEquipment = null;           
        #endregion
    }
}
