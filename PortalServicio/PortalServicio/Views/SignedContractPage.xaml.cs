using PortalServicio.Models;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignedContractPage : ContentPage
    {
        public SignedContractPage(Contract contract)
        {
            InitializeComponent();
            BindingContext = contract;
            AnimatePointer((int)(contract.AmountPaid / contract.AmountTotal) * 100, (int)contract.Progress).ConfigureAwait(false);
        }

        private void Handle_WorkerTapped(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            ((ListView)sender).SelectedItem = null;
        }

        private async void Test(object sender, EventArgs e)
        {          
            //var request = HttpWebRequest.Create(@"http://192.168.18.128:56393/api/HelloWorld/333.333/5946.45454/");
            //request.ContentType = "application/json";
            //request.Method = "GET";

            //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            //{
            //    if (response.StatusCode != HttpStatusCode.OK)
            //        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
            //    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //    {
            //        var content = reader.ReadToEnd();
            //        if (string.IsNullOrWhiteSpace(content))
            //        {
            //            await DisplayAlert("Vacio","Response contained empty body...",":(");
            //        }
            //        else
            //        {
            //            await DisplayAlert("Resultado",String.Format("Response Body: \r\n {0}", content),"Genial");
            //        }
            //    }
            //}
        }

        private async Task AnimatePointer(int destination1, int destination2)
        {
            int r = 216;
            int g = 35;            
            while (pointerPaid.Value <= destination1)
            {
                if (g < 211)
                    g = 35 + (int)((pointerPaid.Value/(50)) * 176);
                else if (r > 35)
                    r = 216 - (int)((pointerPaid.Value /100) * 176);
                headerGaugePaidP.Text = String.Format("{0}% Liquidado", pointerPaid.Value);
                headerGaugePaidV.Text = String.Format("{0}", (BindingContext as Contract).AmountPaidFormatted);
                pointerPaid.Value++;
                pointerPaid.Color = Color.FromRgb(r, g, 35);
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 25));
            }
            //rojo a amarillo
            //desde g=35 a g=211 dif=176
            //amarillo a verde
            //desde r=211 a r=35 dif=176
            r = 216;
            g = 35;
            while (pointerProgress.Value <= destination2)
            {
                if (g < 211)
                    g = 35 + (int)((pointerProgress.Value / (50)) * 176);
                else if (r > 35)
                    r = 216 - (int)((pointerProgress.Value / 100) * 176);
                headerGaugeProgressP.Text = String.Format("{0}% Realizado", pointerProgress.Value);               
                pointerProgress.Value++;
                pointerProgress.Color = Color.FromRgb(r, g, 35);
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 25));
            }
        }
    }
}