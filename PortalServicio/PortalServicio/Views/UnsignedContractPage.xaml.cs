using PortalServicio.Models;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PortalServicio.Services;
using PortalServicio.Configuration;
using SignaturePad.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Net;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UnsignedContractPage : ContentPage
    {
        public UnsignedContractPage(ref Contract contract)
        {
            InitializeComponent();
            BindingContext = contract;
            BtnViewContract.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                loadingIndicator.IsVisible = true;
                BtnViewContract.IsEnabled = false;
                BtnViewContract.Opacity = 0.5;
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                    if (results.ContainsKey(Permission.Storage))
                        status = results[Permission.Storage];
                }
                    if (status == PermissionStatus.Granted)
                    {
                        await PDFServices.CreateContract((Contract)BindingContext);
                    }
                    else
                        await DisplayAlert("Sin permiso de almacenamiento","Debe conceder permisos para acceder al almacenamiento interno", "Ok");
                    SignContainer.IsVisible = true;
                    loadingIndicator.IsVisible = false;
                    BtnViewContract.IsEnabled = true;
                    BtnViewContract.Opacity = 1;
                })
            });
            BtnSignContract.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    //Disable buttons in order to prevent new sign requests.
                    BtnViewContract.IsEnabled = false;
                    BtnViewContract.Opacity = 0.5;
                    BtnSignContract.IsEnabled = false;
                    BtnSignContract.Opacity = 0.5;
                    //Obtain sign as stream image
                    ImageConstructionSettings settings = new ImageConstructionSettings
                    {
                        BackgroundColor = Color.White,
                        StrokeColor = Color.Black,
                        DesiredSizeOrScale = new SizeOrScale(150, 50, SizeOrScaleType.Size)
                    };
                    Stream stream = await signature.GetImageStreamAsync(SignatureImageFormat.Jpeg, settings);
                    //Create document using the sign provided
                    await PDFServices.CreateContract((Contract)BindingContext, stream);
                    //Obtain bytes result of file creation
                    byte[] data = DependencyService.Get<ISave>().GetDocumentBytesAsync("Contrato.pdf");
                    //Insert note into CRM.
                    //Guid noteid = await CRMConnector.AddNote((BindingContext as Contract).InternalId, Config.SPCEXTERNALCONTRACT, "ContratoFirmado.pdf", "application/pdf", data);
                    //bool res1 = noteid != default(Guid);
                    bool res2 = false;
                   // if(res1)
                        res2 = await CRMConnector.SignContract((BindingContext as Contract).InternalId);
                    if (res2)
                    {
                        NotificationService.DisplayMessage("Operación exitosa", "Contrato firmado exitosamente.");
                        await Navigation.PopAsync();
                    }
                    else //In case of failure
                    {
                        await DisplayAlert("Error", "No se pudo realizar la operación, intente de nuevo o comuníquese con TI", "Ok");
                        BtnViewContract.IsEnabled = true;
                        BtnViewContract.Opacity = 1;
                        BtnSignContract.IsEnabled = true;
                        BtnSignContract.Opacity = 1;
                    }
                })
            });
        }     

        private void Handle_WorkerTapped(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            ((ListView)sender).SelectedItem = null;
        }

    }
}