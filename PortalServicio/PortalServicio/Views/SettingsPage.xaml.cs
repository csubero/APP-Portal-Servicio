using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void Synchronize(object sender, EventArgs e)
        {
            await CRMConnector.SyncOperations();
        }

        private async void LoadAllReports(object sender, EventArgs e)
        {
            List<Note> notes = new List<Note>(await CRMConnector.GetAllLocalReports());
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                if (results.ContainsKey(Permission.Storage))
                    status = results[Permission.Storage];
            }
            if (status == PermissionStatus.Granted)
            {
                foreach (Note note in notes)
                {
                    if (await DisplayAlert(string.Format("Caso {0}", note.Incident.TicketNumber), string.Format("Desea guardar '{0}'", note.Filename), "Sí", "No"))
                        try //Save to storage
                        {
                            byte[] decodedFile = Convert.FromBase64String(note.Content);
                            MemoryStream memstream = new MemoryStream(decodedFile);
                            DependencyService.Get<ISave>().SaveTextAsync(note.Filename, "application/pdf", memstream, true);
                        }
                        catch (Exception)
                        {
                            NotificationService.DisplayMessage("Error", String.Format("No se pudo acceder al archivo {0}", note.Filename));
                        }
                }
            }
            else
                NotificationService.DisplayMessage("Sin acceso a almacenamiento", "Se requiere permisos para realizar la operación");
        }
    }
}