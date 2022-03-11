using PortalServicio.Services;
using PortalServicio.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class MyLegalizationSummaryViewModel : BaseViewModel
    {
        #region Properties
        private LegalizationViewModel _Legalization;
        private bool _IsBusy;
        private bool _IsLoading;
        private bool _IsLoadingLegalizationItems;
        private readonly IPageService _pageService;

        public LegalizationViewModel Legalization
        {
            get { return _Legalization; }
            set { SetValue(ref _Legalization, value); }
        }
        public ref LegalizationViewModel RefLegalization
        {
            get { return ref _Legalization; }
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
        public bool IsLoadingLegalizationItems
        {
            get { return _IsLoadingLegalizationItems; }
            set { SetValue(ref _IsLoadingLegalizationItems, value); }
        }
        #endregion

        #region Commands
        public ICommand CreateLegalizationItemCommand { get; private set; }
        public ICommand LoadLegalizationItemsCommand { get; private set; } //Network Operations
        public ICommand SignByOwnerCommand { get; private set; } //Network Operations
        #endregion

        public MyLegalizationSummaryViewModel(IPageService pageService,LegalizationViewModel legalization)
        {
            _pageService = pageService;
            Legalization = legalization;
            CreateLegalizationItemCommand = new Command(async () => await CreateLegalizationItem());
            LoadLegalizationItemsCommand = new Command(async () => await LoadLegalizationItems());
            SignByOwnerCommand = new Command(async () => await SignByOwner());
            LoadLegalizationItemsCommand.Execute(null);
        }

        #region Actions(Implementation of Commands)
        /// <summary>
        /// Creates a new record of a Legalization Expense in the CRM.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreateLegalizationItem() =>
            await _pageService.PushAsync(new CreateNewLegalizationItemPage {
                BindingContext = new CreateNewLegalizationItemViewModel(new PageService(),ref RefLegalization)
            });

        /// <summary>
        /// Loads all the legalization expenses registered for an specific legalization passed to this viewmodel.
        /// </summary>
        /// <returns>Void</returns>
        private async Task LoadLegalizationItems()
        {
            IsLoadingLegalizationItems = true;
            try
            {
                Legalization.LegalizationItems = await CRMConnector.GetLegalizationItemsViewModel(Legalization.InternalId);
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error",ex.Message, "Ok");
            }     
            IsLoadingLegalizationItems = false;
        }

        /// <summary>
        /// Sign the current legalization with the current logged in user.
        /// </summary>
        /// <returns>Void</returns>
        private async Task SignByOwner()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                await CRMConnector.SignLegalizationByOwner(Legalization.InternalId);
                Legalization.SignState++;
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error",ex.Message,"Ok");
            }        
            IsBusy = false;
        }
        #endregion
    }
}
