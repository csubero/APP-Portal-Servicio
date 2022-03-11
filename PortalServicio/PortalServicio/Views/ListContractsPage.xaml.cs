using PortalServicio.Configuration;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Samples;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListContractsPage : ContentPage
    {
        private List<Contract> ContractsList { get; set; }

        public ListContractsPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (ContractsList == null)
            {
                LoaderContractList.IsVisible = true;
                Loading();
                Init();
                LoaderContractList.IsVisible = false;
            }
            else //force update
            {
                ListViewContracts.ItemsSource = null;
                ListViewContracts.ItemsSource = ContractsList;
            }
        }

        private async void Init()
        {
            var res = await CRMConnector.GetContractsOfLoggedContractor();
            List<Contract> contracts = new List<Contract>();
            foreach (Entity record in res.Entities)
            {
                Guid internalId = record.Id;
                Guid contractorid = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_CONTRACTOR]).Id;
                string contractorName = (string)((AliasedValue)record.Attributes["EContract." + Config.SPCCONTRACTOR_NAME]).Value;
                string contractorPhone = (string)((AliasedValue)record.Attributes["EContract." + Config.SPCCONTRACTOR_PHONE]).Value;
                string contractorAddress = (string)((AliasedValue)record.Attributes["EContract." + Config.SPCCONTRACTOR_ADDRESS]).Value;
                string contractorIdentification = (string)((AliasedValue)record.Attributes["EContract." + Config.SPCCONTRACTOR_IDENTIFICATION]).Value;
                Guid currencyid = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_CURRENCY]).Id;
                Guid cdtid = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_CDT]).Id;
                string cdtNumber = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_CDT]).Name;
                Guid paymentid = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_PAYMENT]).Id;
                string description = ((EntityReference)record.Attributes[Config.SPCEXTERNALCONTRACT_PAYMENT]).Name;
                int contractnumber = (int)record.Attributes[Config.SPCEXTERNALCONTRACT_CONTRACTNUMBER];
                decimal amountTotal = ((Money)record.Attributes[Config.SPCEXTERNALCONTRACT_AMOUNTTOTAL]).Value;
                decimal amountPaid = ((Money)record.Attributes[Config.SPCEXTERNALCONTRACT_AMOUNTPAID]).Value;
                float progress = record.Contains(Config.SPCEXTERNALCONTRACT_PROGRESS) ? ((OptionSetValue)record.Attributes[Config.SPCEXTERNALCONTRACT_PROGRESS]).Value : 0;
                bool signed = (bool)record.Attributes[Config.SPCEXTERNALCONTRACT_SIGNED];
                DateTime start = (DateTime)record.Attributes[Config.SPCEXTERNALCONTRACT_STARTDATE];
                DateTime finish = (DateTime)record.Attributes[Config.SPCEXTERNALCONTRACT_FINISHDATE];
                Currency currency = await CRMConnector.GetCurrency(currencyid);
                CDT cdt = new CDT
                {
                    InternalId = cdtid,
                    Number = cdtNumber
                };
                PaymentCondition payment = new PaymentCondition(paymentid, description);
                Contractor contractor = new Contractor(contractorid, contractorName, contractorAddress, contractorPhone, contractorIdentification);
                contracts.Add(new Contract(internalId, contractor, currency, cdt, payment, start, finish, contractnumber, amountTotal, amountPaid, progress, signed));
            }
            ContractsList = contracts;
            ListViewContracts.ItemsSource = ContractsList;
        }

        private async void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.Length == 0)
            {
                ListViewContracts.ItemsSource = ContractsList;
            }
            else
                ListViewContracts.ItemsSource = ContractsList.FindAll(contract => (contract.ContractNumber.ToString().Contains(contractSearch.Text)));
        }

        private async void Loading()
        {
            await LoaderContractList.RotateTo(360, 2000);
            LoaderContractList.Rotation = 0;
            Loading();
        }

        private async void Handle_ContractTapped(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            Contract contract = (ContractsList[ContractsList.IndexOf((Contract)((ListView)sender).SelectedItem)]);
            EntityCollection res = await CRMConnector.GetEntitiesRelatedTo(contract.InternalId, Config.SPCWORKER, new string[] { Config.SPCWORKER_NAME, Config.SPCWORKER_IDENTIFICATION, Config.SPCWORKER_POLIZA, Config.SPCWORKER_DELINCUENCIA, Config.SPCWORKER_CCSS }, Config.SPCWORKER_CONTRACT);
            List<ContractorWorker> workerlist = new List<ContractorWorker>();
            foreach (Entity worker in res.Entities)
            {
                Guid internalId = worker.Id;
                string Name = worker.Contains(Config.SPCWORKER_NAME) ? (string)worker.Attributes[Config.SPCWORKER_NAME] : "Trabajador Sin Nombre";
                string Identification = worker.Contains(Config.SPCWORKER_IDENTIFICATION) ? (string)worker.Attributes[Config.SPCWORKER_IDENTIFICATION] : "Trabajador Sin Identificación";
                bool CCSS = (bool)worker.Attributes[Config.SPCWORKER_CCSS];
                bool Poliza = (bool)worker.Attributes[Config.SPCWORKER_POLIZA];
                bool Delincuencia = (bool)worker.Attributes[Config.SPCWORKER_DELINCUENCIA];
                ContractorWorker Worker = new ContractorWorker(internalId, Name, Identification, Poliza, CCSS, Delincuencia);
                workerlist.Add(Worker);
            }
            contract.Workers = workerlist;
            if (contract.Signed)
                await Navigation.PushAsync(new SignedContractPage(contract));
            else
                await Navigation.PushAsync(new UnsignedContractPage(ref contract));
            ((ListView)sender).SelectedItem = null;
        }
    }
}