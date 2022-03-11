using System.Threading.Tasks;

namespace PortalServicio.ViewModels
{
    public class ProgressBasicPopUpViewModel : BaseViewModel
    {
        #region Properties
        private bool _IsLoading;
        private string _CurrentState;
        private int _CurrentStep;
        private int _TotalSteps;
        private decimal _CurrentProgress;
        private string _Title;
        private readonly IPageService _pageService;

        public bool IsLoading
        {
            get { return _IsLoading; }
            set { SetValue(ref _IsLoading, value); ClosePopUpAfterCompletion().ConfigureAwait(false); }
        }
        public string CurrentState
        {
            get { return _CurrentState; }
            set { SetValue(ref _CurrentState, value); }
        }
        public int CurrentStep
        {
            get { return _CurrentStep; }
            set { SetValue(ref _CurrentStep, value); if(TotalSteps!= 0) CurrentProgress = (decimal)value / (decimal)TotalSteps; }
        }
        public int TotalSteps
        {
            get { return _TotalSteps; }
            set { SetValue(ref _TotalSteps, value); if (value != 0) CurrentProgress = (decimal)CurrentStep / (decimal)value; }
        }
        public decimal CurrentProgress
        {
            get { return _CurrentProgress; }
            private set { SetValue(ref _CurrentProgress, value); }
        }
        public string Title
        {
            get { return _Title; }
            set { SetValue(ref _Title, value); }
        }
        #endregion

        #region Constructors
        public ProgressBasicPopUpViewModel(IPageService pageService, string InitialTitle, string InitialState = "Cargando", int InitialStep = 0, int InitialTotalSteps = 1)
        {
            _pageService = pageService;
            CurrentState = InitialState;
            Title = InitialTitle;
            CurrentStep = InitialStep;
            TotalSteps = InitialTotalSteps;
            IsLoading = true;
        }
        #endregion

        #region Events
        public void ProgressUp(string state = "Cargando")
        {
            CurrentStep++;
            CurrentState = state;
        }

        private async Task ClosePopUpAfterCompletion()
        {
            if (!IsLoading)
            {
                await Task.Delay(1000);
                await _pageService.PopUpPopAsync();
            }
        }
        #endregion
    }
}
