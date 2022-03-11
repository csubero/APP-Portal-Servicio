using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PortalServicio.Services
{
    public class PageService : IPageService
    {
        public async Task<bool> DisplayAlert(string title, string message, string ok, string cancel)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);
        }

        public async Task DisplayAlert(string title, string message, string ok)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, ok);
        }

        public async Task PopAsync()
        {
            if (Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
                await Application.Current.MainPage.Navigation.PopAsync();
        }

        public async Task PushAsync(Page page)
        {
            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public async Task PopUpPushAsync(PopupPage page, bool animate = true)
        {
            await PopupNavigation.Instance.PushAsync(page, animate);
        }

        public async Task PopUpPopAsync(bool animate = true)
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync(animate);
        }

        public bool RemovePages(int quantity)
        {
            try
            {
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack[Application.Current.MainPage.Navigation.NavigationStack.Count - quantity]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
