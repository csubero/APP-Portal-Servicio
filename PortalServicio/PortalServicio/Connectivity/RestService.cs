using Newtonsoft.Json;
using PortalAPI.DTO;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PortalServicio.Connectivity
{
    public static class RestService
    {
        private static HttpClient Client;
        private static string AccessToken;

        private static void Init(string accessToken = null)
        {
            if (Client == null)
            {
#if DEBUG
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif
                Client = new HttpClient();
                AccessToken = accessToken;
                if (!string.IsNullOrEmpty(accessToken))
                    Client.DefaultRequestHeaders.Add("Authorization", accessToken);
            }
        }

        public static async Task<IEnumerable<DTO_IncidentLookUp>> GetIncidents()
        {
            Init("test");
            IEnumerable<DTO_IncidentLookUp> incidents = null;
            var uri = new Uri(string.Format("{0}/{1}", Config.SERVER, Config.GETINCIDENTS));
            var response = await Client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                incidents = JsonConvert.DeserializeObject<IEnumerable<DTO_IncidentLookUp>>(content);
            }
            return incidents;
        }

        public static async Task<IEnumerable<IncidentViewModel>> GetIncidentsViewModel()
        {
            List<DTO_IncidentLookUp> incidents = new List<DTO_IncidentLookUp>(await GetIncidents());
            List<IncidentViewModel> incidentsViewModel = new List<IncidentViewModel>();
            //foreach (DTO_IncidentLookUp incident in incidents)
            //    incidentsViewModel.Add(new IncidentViewModel(incident));
            return incidentsViewModel;
        }

        public static async Task<IEnumerable<DTO_IncidentLookUp>> FindIncidents(string searchText)
        {
            Init("test");
            IEnumerable<DTO_IncidentLookUp> incidents = null;            
            var uri = new Uri(string.Format("{0}/{1}", Config.SERVER, Config.FINDINCIDENTS));
            var response = await Client.PostAsync(uri, new StringContent(searchText,Encoding.UTF8,"application/json"));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                incidents = JsonConvert.DeserializeObject<IEnumerable<DTO_IncidentLookUp>>(content);
            }
            return incidents;
        }

        public static async Task<IEnumerable<IncidentViewModel>> FindIncidentsViewModel(string searchText)
        {
            IEnumerable<DTO_IncidentLookUp> incidents = await FindIncidents(searchText);
            List<IncidentViewModel> incidentsViewModel = new List<IncidentViewModel>();
            //if (incidents != null)
            //    foreach (DTO_IncidentLookUp incident in incidents)
            //        incidentsViewModel.Add(new IncidentViewModel(incident));
            return incidentsViewModel;
        }

        public static async Task<Incident> GetIncident(Guid id)
        {
            Init("test");
            Incident incident = null;
            var uri = new Uri(string.Format("{0}/{1}/{2}", Config.SERVER, Config.GETINCIDENT, id));
            var response = await Client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                incident = JsonConvert.DeserializeObject<Incident>(content);
            }
            return incident;
        }

        public static async Task<ServiceTicket> CreateNewServiceTicket(Incident InIncident)
        {
            Init("test");
            ServiceTicket incident = null;
            var uri = new Uri(string.Format("{0}/{1}", Config.SERVER, Config.CREATESERVICETICKET));
            var response = await Client.PostAsync(uri,new StringContent(JsonConvert.SerializeObject(InIncident),Encoding.UTF8,"application/json"));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                incident = JsonConvert.DeserializeObject<ServiceTicket>(content);
            }
            return incident;
        }

        public static async Task DeleteServiceTicket(ServiceTicket serviceTicket)
        {
            Init("test");         
            var uri = new Uri(string.Format("{0}/{1}/{2}", Config.SERVER, Config.DELETESERVICETICKET, serviceTicket.InternalId));
            var response = await Client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }
            else
                throw new Exception(response.ReasonPhrase);
        }
    }
}
