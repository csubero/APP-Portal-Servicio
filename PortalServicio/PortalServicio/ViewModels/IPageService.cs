using Rg.Plugins.Popup.Pages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public interface IPageService
    {
        Task PushAsync(Page page);
        Task PopAsync();
        Task<bool> DisplayAlert(string title, string message, string ok, string cancel);
        Task DisplayAlert(string title, string message, string ok);
        Task PopUpPushAsync(PopupPage page, bool animate = true);
        Task PopUpPopAsync(bool animate = true);
        bool RemovePages(int quantity);
    }
}
