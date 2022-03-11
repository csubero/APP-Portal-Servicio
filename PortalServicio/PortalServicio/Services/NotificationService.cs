using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PortalServicio.Services
{
    public static class NotificationService
    {
        private static IToastNotificator notification;

        public static void DisplayMessage(string title, string description, bool clickable = false)
        {
            Init();
            var options = new NotificationOptions()
            {
                Title = title,
                Description = description,
                IsClickable = clickable
            };
            notification.Notify(options);
        }

        private static void Init()
        {
            if(notification== null)
                notification = DependencyService.Get<IToastNotificator>();
        }
    }
}
