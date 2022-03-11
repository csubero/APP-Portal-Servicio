using PortalServicio.Configuration;
using Microsoft.Crm.Sdk.Messages.Samples;
using Microsoft.Xrm.Sdk.Query.Samples;
using Microsoft.Xrm.Sdk.Samples;
using System;
using System.Threading.Tasks;
using PortalServicio.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PortalServicio.ViewModels;
using PortalAPI.Contracts;
using PortalAPI.DTO;
using System.Threading;
using System.Linq;
using SQLiteNetExtensionsAsync.Extensions;
using PortalServicio.Services;
using PortalServicio.DTO;

namespace PortalServicio
{
    /// <summary>
    /// <para>Establece el punto de conexión entre la app y el CRM.</para>
    /// <para>Además contiene los metodos CRUD de la app.</para>
    /// </summary>
    static public class CRMConnector
    {
#if __ANDROID__
        static public Android.App.Activity activity
        {
            set { Proxy.activity = value; }
        }
#elif __IOS__
        static public UIKit.UIViewController uiViewController
        {
            set { Proxy.uiViewController = value; }
        }
#endif
        /// <summary>
        /// Establece comunicación al CRM mediante ADAL.
        /// </summary>
        static private OrganizationProxy proxy;

        /// <summary>
        /// Retorna una instancia única del proxy o crea una en caso de no haber ninguna.
        /// </summary>
        static public OrganizationProxy Proxy
        {
            get
            {
                if (proxy == null)
                    proxy = new OrganizationProxy();
                return proxy;
            }
        }

        static private SQLiteDB local;
        static public SQLiteDB Local
        {
            get
            {
                if (local == null)
                    local = new SQLiteDB();
                return local;
            }
        }

        /// <summary>
        /// Obtiene el nombre del usuario que ha iniciado sesión.
        /// </summary>
        /// <returns>El nombre del usuario que ingresó a la app</returns>
        static public async Task<string> GetLoginUser()
        {
            WhoAmIResponse result = (WhoAmIResponse)await Proxy.Execute(new WhoAmIRequest());
            Entity user = await Proxy.Retrieve(Config.SYSUSER, result.UserId, new ColumnSet(Config.SYSUSER_FULLNAME));
            var conn = await Local.GetConnection();
            SystemUser local = (await conn.GetAllWithChildrenAsync<SystemUser>(su => su.InternalId.Equals(result.UserId), true)).FirstOrDefault();
            SystemUser logged = new SystemUser
            {
                SQLiteRecordId = local is null ? 0 : local.SQLiteRecordId,
                InternalId = result.UserId,
                Name = (string)user[Config.SYSUSER_FULLNAME],
                Token = Proxy.AccessToken
            };
            if (local != null)
                logged.SQLiteRecordId = local.SQLiteRecordId;
            await conn.InsertOrReplaceWithChildrenAsync(logged, true);
            Proxy.LoggedUser = logged;
            return user[Config.SYSUSER_FULLNAME].ToString();
        }

        /// <summary>
        /// Gets the last user that logged in.
        /// </summary>
        /// <returns>A system user that corresponds to the last user that logged in</returns>
        static public async Task<SystemUser> GetLoginUserOffline()
        {
            var conn = await Local.GetConnection();
            await Proxy.Authenticate();
            SystemUser local = (await conn.GetAllWithChildrenAsync<SystemUser>(su => su.Token.Equals(Proxy.AccessToken), true)).FirstOrDefault();
            if (local != null)
                local = await conn.GetWithChildrenAsync<SystemUser>(local.SQLiteRecordId);
            else
                throw new Exception("No se encontró una sesión guardada a nivel local. Debe conectarse a internet");
            Proxy.LoggedUser = local;
            return local;
        }

        /// <summary>
        ///  Obtiene el usuario que ha iniciado sesión.
        /// </summary>
        /// <returns>Entidad del usuario que inició sesión.</returns>
        static public async Task<Entity> GetLoggedUser()
        {
            WhoAmIResponse result = (WhoAmIResponse)await Proxy.Execute(new WhoAmIRequest());
            Entity user = await Proxy.Retrieve(Config.SYSUSER, result.UserId, new ColumnSet(Config.SYSUSER_FULLNAME));
            return user;
        }

        /// <summary>
        /// Gets a collection of entities that represent all the contracts of the currently logged in user 
        /// </summary>
        /// <returns>A collection of contracts as entities</returns>
        static public async Task<EntityCollection> GetContractsOfLoggedContractor()
        {
            Entity user = await GetLoggedUser();
            QueryExpression queryContracts = new QueryExpression(Config.SPCEXTERNALCONTRACT)
            {
                ColumnSet = new ColumnSet(new string[]
                {
                    Config.SPCEXTERNALCONTRACT_CONTRACTNUMBER,
                    Config.SPCEXTERNALCONTRACT_AMOUNTTOTAL,
                    Config.SPCEXTERNALCONTRACT_AMOUNTPAID,
                    Config.SPCEXTERNALCONTRACT_CONTRACTOR,
                    Config.SPCEXTERNALCONTRACT_CURRENCY,
                    Config.SPCEXTERNALCONTRACT_CDT,
                    Config.SPCEXTERNALCONTRACT_NUMBER,
                    Config.SPCEXTERNALCONTRACT_PAYMENT,
                    Config.SPCEXTERNALCONTRACT_SIGNED,
                    Config.SPCEXTERNALCONTRACT_STARTDATE,
                    Config.SPCEXTERNALCONTRACT_FINISHDATE,
                    Config.SPCEXTERNALCONTRACT_PROGRESS
                }),
                Criteria = new FilterExpression()
            };
            queryContracts.Criteria.FilterOperator = LogicalOperator.And;
            queryContracts.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            LinkEntity EntityContract = new LinkEntity
            {
                LinkFromEntityName = Config.SPCEXTERNALCONTRACT,
                LinkToEntityName = Config.SPCCONTRACTOR,
                LinkFromAttributeName = Config.SPCEXTERNALCONTRACT_CONTRACTOR,
                LinkToAttributeName = Config.SPCCONTRACTOR_ID,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCCONTRACTOR_ID,
                    Config.SPCCONTRACTOR_ADDRESS,
                    Config.SPCCONTRACTOR_IDENTIFICATION,
                    Config.SPCCONTRACTOR_NAME,
                    Config.SPCCONTRACTOR_PHONE
                }),
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "EContract"
            };
            FilterExpression filterid = new FilterExpression();
            filterid.Conditions.Add(new ConditionExpression(Config.SYSUSER_ID, ConditionOperator.Equal, user.Id));
            LinkEntity EntityContractor = new LinkEntity
            {
                LinkFromEntityName = "EContract",
                LinkToEntityName = Config.SYSUSER,
                LinkFromAttributeName = Config.SPCCONTRACTOR_USER,
                LinkToAttributeName = Config.SYSUSER_ID,
                LinkCriteria = filterid,
                Columns = new ColumnSet(false),
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "EContractor"
            };
            EntityContract.LinkEntities.Add(EntityContractor);
            queryContracts.LinkEntities.Add(EntityContract);
            return await Proxy.RetrieveMultiple(queryContracts);
        }

        //static public void GetLastIncidents()
        //{
        //    var request = WebRequest.Create(@"http://192.168.17.150:50000/");
        //    request.ContentType = "application/json";
        //    request.Method = "GET";
        //    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //    {
        //        if (response.StatusCode != HttpStatusCode.OK)
        //            Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
        //        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            var content = reader.ReadToEnd();
        //            if (string.IsNullOrWhiteSpace(content))
        //            {
        //                Console.WriteLine("Response contained empty body...");
        //            }
        //            else
        //            {
        //                Console.WriteLine(String.Format("Response Body: \r\n {0}", content));
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Clears the cache so the user has to sign in again.
        /// </summary>
        static public void Logout()
        {
            Proxy.DeleteToken();
        }

        #region CRM new Version
        /// <summary>
        /// Gets all the pdfs of reports from service cases stored in the local database.
        /// </summary>
        /// <returns>A collection  of pdfs as notes</returns>
        public static async Task<IEnumerable<Note>> GetAllLocalReports()
        {
            var conn = await Local.GetConnection();
            List<Note> reports = await conn.GetAllWithChildrenAsync<Note>(note => note.Filename.StartsWith("ReporteDeServicio"), true);
            return reports;
        }

        /// <summary>
        /// Gets a List of partial Incidents model objects that correspond to the last incidents opened.
        /// </summary>
        /// <returns>A List of partial Incidents</returns>
        public static async Task<List<DTO_IncidentLookUp>> GetLastCasesDTO()
        {
            List<DTO_IncidentLookUp> result = new List<DTO_IncidentLookUp>();
            QueryExpression queryLastXMonths = new QueryExpression(Config.SPCCASE)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.SPCCASE_CASENUMBER,
                    Config.GENERAL_CREATEDON,
                    Config.SPCCASE_CLIENT
                }),
                Criteria = new FilterExpression()
            };
            string NameClient = "client";
            queryLastXMonths.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCASE,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCASE_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
               {
                    Config.SPCACCOUNT_ALIAS
               })
            });
            queryLastXMonths.Criteria.FilterOperator = LogicalOperator.And;
            queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_CREATEDON, ConditionOperator.LastXMonths, Config.LASTXMONTHS));
            queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATECODE, ConditionOperator.Equal, 0));
            EntityCollection queryresult = await Proxy.RetrieveMultiple(queryLastXMonths);
            foreach (Entity incident in queryresult.Entities)
            {
                Guid internalId = incident.Id;
                string ticketNumber = incident.Contains(Config.SPCCASE_CASENUMBER) ? (string)incident.Attributes[Config.SPCCASE_CASENUMBER] : "Sin número";
                DateTime creationDate = (DateTime)incident.Attributes[Config.GENERAL_CREATEDON];
                EntityReference client = (EntityReference)incident.Attributes[Config.SPCCASE_CLIENT];
                Guid clientid = client.Id;
                string clientName = client.Name;
                string AliasVariable = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS);
                string clientAlias = incident.Contains(AliasVariable) ? (string)((AliasedValue)incident.Attributes[AliasVariable]).Value : clientName;
                DTO_IncidentLookUp resultIncident = new DTO_IncidentLookUp()
                {
                    InternalId = internalId,
                    TicketNumber = ticketNumber,
                    CreatedOn = creationDate,
                    Client = new DTO_ClientPartial
                    {
                        InternalId = clientid,
                        Name = clientName,
                        Alias = clientAlias
                    }
                };
                result.Add(resultIncident);
            }
            return result;
        }

        /// <summary>
        /// Gets a list of partial CDTs model objects that correspond to the last cdts opened.
        /// </summary>
        /// <returns>A list of partial CDTs</returns>
        public static async Task<List<DTO_CDTLookUp>> GetLastCDTsDTO()
        {
            List<DTO_CDTLookUp> result = new List<DTO_CDTLookUp>();
            QueryExpression queryLastXMonths = new QueryExpression(Config.SPCCDT)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.SPCCDT_NUMBER,
                    Config.GENERAL_CREATEDON
                }),
                Criteria = new FilterExpression()
            };
            string NameClient = "client";
            queryLastXMonths.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCDT,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCDT_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
               {
                   Config.SPCACCOUNT_ID,
                    Config.SPCACCOUNT_ALIAS,
                    Config.SPCACCOUNT_NAME
               })
            });
            queryLastXMonths.Criteria.FilterOperator = LogicalOperator.And;
            queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_CREATEDON, ConditionOperator.LastXMonths, Config.LASTXMONTHS));
            //queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            EntityCollection queryresult = await Proxy.RetrieveMultiple(queryLastXMonths);
            foreach (Entity ecdt in queryresult.Entities)
            {
                DTO_CDTLookUp cdt = new DTO_CDTLookUp
                {
                    InternalId = ecdt.Id,
                    Number = ecdt.Contains(Config.SPCCDT_NUMBER) ? (string)ecdt[Config.SPCCDT_NUMBER] : "Sin número",
                    CreatedOn = ecdt.Contains(Config.GENERAL_CREATEDON) ? (DateTime)ecdt[Config.GENERAL_CREATEDON] : default(DateTime),
                    Client = new DTO_ClientPartial
                    {
                        InternalId = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.CLIENT_ID)) ? (Guid)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ID)]).Value : default(Guid),
                        Alias = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)) ? (string)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)]).Value : "Sin nombre",
                        Name = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME)) ? (string)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME)]).Value : "Sin nombre",
                    }
                };
                result.Add(cdt);
            }
            return result;
        }

        /// <summary>
        /// Gives an approval for an specific CDT by a Customer Service Manager.
        /// </summary>
        /// <param name="cdt">Cdt to be approved by the user</param>
        /// <returns>A boolean value indicating if operation was successful.</returns>
        public static async Task<bool> ApproveCustomerServiceForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDCUSTOMERSERVICE));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDCUSTOMERSERVICE] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives an approval for an specific CDT by a financial deparment manager.
        /// </summary>
        /// <param name="cdt">Cdt to be approved by the user</param>
        /// <returns>A boolean value indicating if operation was successful.</returns>
        public static async Task<bool> ApproveFinancialForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDFINANCIAL));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDFINANCIAL] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives an approval for an specific CDT by a comercial department manager.
        /// </summary>
        /// <param name="cdt">CDT to be approved by the user.</param>
        /// <returns>A boolean value indicating if operation was successful.</returns>
        public static async Task<bool> ApproveComercialForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDCOMERCIAL));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDCOMERCIAL] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives an approval for an specific CDT by an Administration Department manager.
        /// </summary>
        /// <param name="cdt">CDT to be approved by the user.</param>
        /// <returns>A boolean value indicating if operation was successful</returns>
        public static async Task<bool> ApproveAdministrationForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDADMINISTRATION));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDADMINISTRATION] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives an approval for an specific CDT by an Installation Department Manager.
        /// </summary>
        /// <param name="cdt">CDT to be approved by the user.</param>
        /// <returns>A boolean value indicating if operation was successful</returns>
        public static async Task<bool> ApproveInstallationsForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDINSTALLATION));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDINSTALLATION] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives and approval for an specific CDT by an Operations Department Manager.
        /// </summary>
        /// <param name="cdt"></param>
        /// <returns>A boolean value indicating if operation was successful</returns>
        public static async Task<bool> ApproveOperationsForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDOPERATIONS));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDOPERATIONS] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gives and approval for an specific CDT by an Planning Department Manager.
        /// </summary>
        /// <param name="cdt"></param>
        /// <returns>A boolean value indicating if operation was successful</returns>
        public static async Task<bool> ApprovePlanningForCDT(CDTViewModel cdt)
        {
            EntityCollection res = await Proxy.RetrieveMultiple(await BuildQueryGetPrivilegeOfApprovement(Config.SPCCDT_ISAPPROVEDPLANNING));
            if (res.Entities.Count == 0)
                return false;
            Entity cdtEntity = new Entity(Config.SPCCDT, cdt.InternalId);
            cdtEntity[Config.SPCCDT_ISAPPROVEDPLANNING] = true;
            try
            {
                await Proxy.Update(cdtEntity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Builds a Query that ask the CRM if the currently logged in user has access to a specific field.
        /// </summary>
        /// <param name="field">Field to be checked for access</param>
        /// <returns>A fully built query ready to be used.</returns>
        private static async Task<QueryExpression> BuildQueryGetPrivilegeOfApprovement(string field) =>
           new QueryExpression
           {
               EntityName = Config.SPCFIELDPERMISSION,
               //ColumnSet = new ColumnSet(new string[] { "attributelogicalname", "entityname", "canupdate" }),
               Criteria =
               {
                    Conditions =
                    {
                         new ConditionExpression(Config.SPCFIELDPERMISSION_ATTRIBUTE, ConditionOperator.Equal, field),
                         new ConditionExpression(Config.SPCFIELDPERMISSION_ENTITYNAME, ConditionOperator.Equal, Config.SPCCDT),
                         new ConditionExpression(Config.SPCFIELDPERMISSION_CANUPDATE, ConditionOperator.Equal, 4),
                    }
               },
               LinkEntities =
               {
                    new LinkEntity
                    {
                        LinkFromEntityName = Config.SPCFIELDPERMISSION,
                        LinkToEntityName = Config.SPCSECPROFILE,
                        LinkFromAttributeName =Config.SPCFIELDPERMISSION_SECPROFILE,
                        LinkToAttributeName = Config.SPCSECPROFILE_ID,
                        JoinOperator = JoinOperator.Inner,
                        LinkEntities =
                        {
                             new LinkEntity
                             {
                                LinkFromEntityName = Config.SPCSECPROFILE,
                                LinkToEntityName = Config.SYSUSER,
                                LinkCriteria =
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression(Config.SYSUSER_ID, ConditionOperator.Equal, (await GetLoginUserOffline()).InternalId)
                                    }
                                }
                             }
                        }
                    }
               }
           };

        /// <summary>
        /// Gets a collection of all local-stored incident viewmodels from the phone.
        /// </summary>
        /// <returns>A collection of ALL incident viewmodels stored in the phone.</returns>
        public static async Task<IEnumerable<IncidentViewModel>> GetLocalIncidents()
        {
            var conn = await Local.GetConnection();
            List<Incident> localIncidents = await conn.GetAllWithChildrenAsync<Incident>(recursive: true);
            List<IncidentViewModel> localIncidentViewModels = new List<IncidentViewModel>();
            foreach (Incident i in localIncidents)
                localIncidentViewModels.Add(new IncidentViewModel(i));
            return localIncidentViewModels;
        }

        /// <summary>
        /// Gets a collection of all local-stored CDT viewmodels from the phone.
        /// </summary>
        /// <returns>A collection of ALL cdt viewmodels stored in the phone</returns>
        public static async Task<IEnumerable<CDTViewModel>> GetLocalCDTs()
        {
            var conn = await Local.GetConnection();
            List<CDT> localCDTs = await conn.GetAllWithChildrenAsync<CDT>(recursive: true);
            List<CDTViewModel> localCDTViewModels = new List<CDTViewModel>();
            foreach (CDT c in localCDTs)
                localCDTViewModels.Add(new CDTViewModel(c));
            return localCDTViewModels;
        }

        /// <summary>
        /// Gets a collection of the last partial Incidents ViewModels from CRM.
        /// </summary>
        /// <returns>A collection of partial Incident ViewModels</returns>
        public static async Task<IEnumerable<IncidentViewModel>> GetIncidentsViewModel()
        {
            List<DTO_IncidentLookUp> incidents = new List<DTO_IncidentLookUp>(await GetLastCasesDTO());
            List<IncidentViewModel> incidentsViewModel = new List<IncidentViewModel>();
            foreach (DTO_IncidentLookUp incident in incidents)
                incidentsViewModel.Add(new IncidentViewModel(incident));
            return incidentsViewModel;
        }

        /// <summary>
        /// Gets a collection of the last partial Incidents ViewModels from CRM.
        /// </summary>
        /// <returns>A collection of partial Incident ViewModels</returns>
        public static async Task<IEnumerable<CDTViewModel>> GetCDTsViewModel()
        {
            List<DTO_CDTLookUp> cdts = new List<DTO_CDTLookUp>(await GetLastCDTsDTO());
            List<CDTViewModel> cdtsViewModel = new List<CDTViewModel>();
            foreach (DTO_CDTLookUp cdt in cdts)
                cdtsViewModel.Add(new CDTViewModel(cdt));
            return cdtsViewModel;
        }

        /// <summary>
        /// Gets an specific service ticket (Incident) object model from the CRM.
        /// </summary>
        /// <param name="serviceTicketId">Id of the Incident to be obtained</param>
        /// <returns>A service ticket model object</returns>
        public static async Task<ServiceTicket> GetServiceTicket(Guid serviceTicketId)
        {
            var conn = await Local.GetConnection();
            #region Query
            Entity st = await Proxy.Retrieve(Config.SPCSERVTICKET, serviceTicketId, new ColumnSet(new string[] {
                Config.SPCSERVTICKET_NUMBER,
                Config.GENERAL_CREATEDON,
                Config.SPCSERVTICKET_TITLE,
                Config.SPCSERVTICKET_WORKDONE,
                Config.SPCSERVTICKET_SYSTEM,
                Config.SPCSERVTICKET_STARTED,
                Config.SPCSERVTICKET_HADLUNCH,
                Config.SPCSERVTICKET_FINISHED,
                Config.SPCSERVTICKET_TECHNICIAN1,
                Config.SPCSERVTICKET_TECHNICIAN2,
                Config.SPCSERVTICKET_TECHNICIAN3,
                Config.SPCSERVTICKET_TECHNICIAN4,
                Config.SPCSERVTICKET_TECHNICIAN5,
                Config.SPCSERVTICKET_DESCRIPTION,
                Config.SPCSERVTICKET_CURRENCY,
                Config.SPCSERVTICKET_FEEDBACK,
                Config.SPCSERVTICKET_GEN1OFFVOLTAGE,
                Config.SPCSERVTICKET_GEN1ONANODEVOLT,
                Config.SPCSERVTICKET_GEN1ONHIGHVOLT,
                Config.SPCSERVTICKET_GEN1ONVOLTAGE,
                Config.SPCSERVTICKET_GEN2OFFVOLTAGE,
                Config.SPCSERVTICKET_GEN2ONANODEVOLT,
                Config.SPCSERVTICKET_GEN2ONHIGHVOLT,
                Config.SPCSERVTICKET_GEN2ONVOLTAGE,
                Config.SPCSERVTICKET_CALIBRATIONDATE,
                Config.SPCSERVTICKET_CALIBRATIONDUEDATE,
                Config.SPCSERVTICKET_CALIBRATIONSTATE,
                Config.SPCSERVTICKET_CALMODEL,
                Config.SPCSERVTICKET_CALTRADEMARK,
                Config.SPCSERVTICKET_CHECKACSYSTEM,
                Config.SPCSERVTICKET_CHECKCONFIGURATION,
                Config.SPCSERVTICKET_CHECKCONTROLELEMENTS,
                Config.SPCSERVTICKET_CHECKCONVEYORBELT,
                Config.SPCSERVTICKET_CHECKEMERGENCYSTOP,
                Config.SPCSERVTICKET_CHECKENGINECONTROL,
                Config.SPCSERVTICKET_CHECKENGINETRACTION,
                Config.SPCSERVTICKET_CHECKHAVEUPS,
                Config.SPCSERVTICKET_CHECKINOUTSYSTEM,
                Config.SPCSERVTICKET_CHECKINTERLOCK,
                Config.SPCSERVTICKET_CHECKIRFENCES,
                Config.SPCSERVTICKET_CHECKISOLATIONTRANSF,
                Config.SPCSERVTICKET_CHECKKEYBOARD,
                Config.SPCSERVTICKET_CHECKLABELS,
                Config.SPCSERVTICKET_CHECKLINEDETMODS,
                Config.SPCSERVTICKET_CHECKMONITORCONFIG,
                Config.SPCSERVTICKET_CHECKOS,
                Config.SPCSERVTICKET_CHECKRADINDICATORS,
                Config.SPCSERVTICKET_CHECKROLLERS,
                Config.SPCSERVTICKET_CHECKSECCIRCUIT,
                Config.SPCSERVTICKET_CHECKTWOWAY,
                Config.SPCSERVTICKET_CHECKVOLTMONITOR,
                Config.SPCSERVTICKET_CHECKXRCONE,
                Config.SPCSERVTICKET_CLEANAREA,
                Config.SPCSERVTICKET_COMMENTS,
                Config.SPCSERVTICKET_GROUNDVOLTAGE,
                Config.SPCSERVTICKET_HAVEHDA,
                Config.SPCSERVTICKET_HAVEHISPOT,
                Config.SPCSERVTICKET_HAVEHITIP,
                Config.SPCSERVTICKET_HAVEIMS,
                Config.SPCSERVTICKET_HAVESEN,
                Config.SPCSERVTICKET_HAVEXACT,
                Config.SPCSERVTICKET_HAVEXPLORE,
                Config.SPCSERVTICKET_HAVEXPORT,
                Config.SPCSERVTICKET_HAVEXTRAIN,
                Config.SPCSERVTICKET_HWSTATE,
                Config.SPCSERVTICKET_INVOLTAGE,
                Config.SPCSERVTICKET_LEADSTATE,
                Config.SPCSERVTICKET_OPERATORRAD,
                Config.SPCSERVTICKET_PHYSICALDONGLE,
                Config.SPCSERVTICKET_RADSTATE,
                Config.SPCSERVTICKET_RXCREATIONDATE,
                Config.SPCSERVTICKET_RXMODEL,
                Config.SPCSERVTICKET_RXSERIAL,
                Config.SPCSERVTICKET_RXSYNERGY,
                Config.SPCSERVTICKET_SCREENTYPE,
                Config.SPCSERVTICKET_SOFTTECHNOLOGY,
                Config.SPCSERVTICKET_SOFTVERSION,
                Config.SPCSERVTICKET_SOFTWAREDONGLE,
                Config.SPCSERVTICKET_STEELPENETRATION,
                Config.SPCSERVTICKET_TEST1,
                Config.SPCSERVTICKET_TEST2,
                Config.SPCSERVTICKET_TEST3,
                Config.SPCSERVTICKET_TEST4,
                Config.SPCSERVTICKET_TUNNELRADIN,
                Config.SPCSERVTICKET_TUNNELRADOUT,
                Config.SPCSERVTICKET_UPSCAP,
                Config.SPCSERVTICKET_UPSGROUNDVOLT,
                Config.SPCSERVTICKET_UPSINVOLT,
                Config.SPCSERVTICKET_VISITNUMBER,
                Config.SPCSERVTICKET_WIRERESOLUTION
            }));
            #endregion
            ServiceTicket newst = new ServiceTicket
            {
                InternalId = st.Id,
                CreationDate = (DateTime)st[Config.GENERAL_CREATEDON],
                ProductsUsed = await GetMaterialsFromServiceTicket(st.Id)
            };
            ServiceTicket local = await conn.Table<ServiceTicket>().Where(s => s.InternalId.Equals(st.Id)).FirstOrDefaultAsync();
            if (local != null)
            {
                newst.SQLiteRecordId = local.SQLiteRecordId;
                newst.IncidentId = local.IncidentId;
            }
            #region Basic Service Ticket
            newst.FeedbackSubmitted = st.Contains(Config.SPCSERVTICKET_FEEDBACK) ? (bool)st[Config.SPCSERVTICKET_FEEDBACK] : false;
            newst.HadLunch = st.Contains(Config.SPCSERVTICKET_HADLUNCH) ? (bool)st[Config.SPCSERVTICKET_HADLUNCH] : false;
            newst.TicketNumber = st.Contains(Config.SPCSERVTICKET_NUMBER) ? st[Config.SPCSERVTICKET_NUMBER].ToString() : "Sin número";
            #region Subtype
            if (st.Contains(Config.SPCSERVTICKET_SYSTEM))
                newst.Type = await GetSubtype(((EntityReference)st[Config.SPCSERVTICKET_SYSTEM]).Id);
            if (newst.Type != null)
                newst.TypeId = newst.Type.SQLiteRecordId;
            #endregion
            #region Currency
            if (st.Contains(Config.SPCSERVTICKET_CURRENCY))
                newst.MoneyCurrency = await GetCurrency(((EntityReference)st[Config.SPCSERVTICKET_CURRENCY]).Id);
            if (newst.MoneyCurrency != null)
                newst.MoneyCurrencyId = newst.MoneyCurrency.SQLiteRecordId;
            #endregion
            if (st.Contains(Config.SPCSERVTICKET_STARTED))
                newst.Started = (DateTime)st[Config.SPCSERVTICKET_STARTED];
            if (st.Contains(Config.SPCSERVTICKET_TITLE))
                newst.Title = (string)st[Config.SPCSERVTICKET_TITLE];
            if (st.Contains(Config.SPCSERVTICKET_DESCRIPTION))
                newst.Description = (string)st[Config.SPCSERVTICKET_DESCRIPTION];
            if (st.Contains(Config.SPCSERVTICKET_FINISHED))
                newst.Finished = (DateTime)st[Config.SPCSERVTICKET_FINISHED];
            if (st.Contains(Config.SPCSERVTICKET_WORKDONE))
                newst.WorkDone = (string)st[Config.SPCSERVTICKET_WORKDONE];
            newst.Technicians = new List<Technician>();
            #region Technician 1
            if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN1))
            {
                Technician tech1 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN1]).Id);
                if (tech1 == null)
                    throw new Exception("Técnico 1 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                newst.Technicians.Add(tech1);
            }
            #endregion
            #region Technician 2
            if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN2))
            {
                Technician tech2 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN2]).Id);
                if (tech2 == null)
                    throw new Exception("Técnico 2 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                newst.Technicians.Add(tech2);
            }
            #endregion
            #region Technician 3
            if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN3))
            {
                Technician tech3 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN3]).Id);
                if (tech3 == null)
                    throw new Exception("Técnico 3 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                newst.Technicians.Add(tech3);
            }
            #endregion
            #region Technician 4
            if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN4))
            {
                Technician tech4 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN4]).Id);
                if (tech4 == null)
                    throw new Exception("Técnico 4 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                newst.Technicians.Add(tech4);
            }
            #endregion
            #region Technician 5
            if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN5))
            {
                Technician tech5 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN5]).Id);
                if (tech5 == null)
                    throw new Exception("Técnico 5 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                newst.Technicians.Add(tech5);
            }
            #endregion
            #endregion
            #region RX
            #region General
            if (st.Contains(Config.SPCSERVTICKET_COMMENTS))
                newst.RXGenComments = (string)st[Config.SPCSERVTICKET_COMMENTS];
            if (st.Contains(Config.SPCSERVTICKET_RXCREATIONDATE))
                newst.RXGenCreationDate = (string)st[Config.SPCSERVTICKET_RXCREATIONDATE];
            if (st.Contains(Config.SPCSERVTICKET_RXMODEL))
                newst.RXGenModel = (string)st[Config.SPCSERVTICKET_RXMODEL];
            if (st.Contains(Config.SPCSERVTICKET_HWSTATE))
                newst.RXGenHWState = (Types.SPCSERVTICKET_HWSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HWSTATE]).Value;
            if (st.Contains(Config.SPCSERVTICKET_RXSERIAL))
                newst.RXGenSerial = (string)st[Config.SPCSERVTICKET_RXSERIAL];
            if (st.Contains(Config.SPCSERVTICKET_VISITNUMBER))
                newst.RXGenVisitNumber = (Types.SPCSERVTICKET_VISITNUMBER)((OptionSetValue)st[Config.SPCSERVTICKET_VISITNUMBER]).Value;
            #endregion
            #region Mantainance                    
            newst.RXGenCleanArea = st.Contains(Config.SPCSERVTICKET_CLEANAREA) ? (bool)st[Config.SPCSERVTICKET_CLEANAREA] : false;
            newst.RXMantCheckConditioningSystem = st.Contains(Config.SPCSERVTICKET_CHECKACSYSTEM) ? (bool)st[Config.SPCSERVTICKET_CHECKACSYSTEM] : false;
            newst.RXMantCheckConfiguration = st.Contains(Config.SPCSERVTICKET_CHECKCONFIGURATION) ? (bool)st[Config.SPCSERVTICKET_CHECKCONFIGURATION] : false;
            newst.RXMantCheckControlElements = st.Contains(Config.SPCSERVTICKET_CHECKCONTROLELEMENTS) ? (bool)st[Config.SPCSERVTICKET_CHECKCONTROLELEMENTS] : false;
            newst.RXMantCheckConveyorBelt = st.Contains(Config.SPCSERVTICKET_CHECKCONVEYORBELT) ? (bool)st[Config.SPCSERVTICKET_CHECKCONVEYORBELT] : false;
            newst.RXMantCheckEmergencyStop = st.Contains(Config.SPCSERVTICKET_CHECKEMERGENCYSTOP) ? (bool)st[Config.SPCSERVTICKET_CHECKEMERGENCYSTOP] : false;
            newst.RXMantCheckEngineControl = st.Contains(Config.SPCSERVTICKET_CHECKENGINECONTROL) ? (bool)st[Config.SPCSERVTICKET_CHECKENGINECONTROL] : false;
            newst.RXMantCheckEngineTraction = st.Contains(Config.SPCSERVTICKET_CHECKENGINETRACTION) ? (bool)st[Config.SPCSERVTICKET_CHECKENGINETRACTION] : false;
            newst.RXMantCheckInOutSystem = st.Contains(Config.SPCSERVTICKET_CHECKINOUTSYSTEM) ? (bool)st[Config.SPCSERVTICKET_CHECKINOUTSYSTEM] : false;
            newst.RXMantCheckInterlock = st.Contains(Config.SPCSERVTICKET_CHECKINTERLOCK) ? (bool)st[Config.SPCSERVTICKET_CHECKINTERLOCK] : false;
            newst.RXMantCheckIRFences = st.Contains(Config.SPCSERVTICKET_CHECKIRFENCES) ? (bool)st[Config.SPCSERVTICKET_CHECKIRFENCES] : false;
            newst.RXMantCheckKeyboard = st.Contains(Config.SPCSERVTICKET_CHECKKEYBOARD) ? (bool)st[Config.SPCSERVTICKET_CHECKKEYBOARD] : false;
            newst.RXMantCheckLabels = st.Contains(Config.SPCSERVTICKET_CHECKLABELS) ? (bool)st[Config.SPCSERVTICKET_CHECKLABELS] : false;
            newst.RXMantCheckLineAndDetectionModules = st.Contains(Config.SPCSERVTICKET_CHECKLINEDETMODS) ? (bool)st[Config.SPCSERVTICKET_CHECKLINEDETMODS] : false;
            newst.RXMantCheckMonitorConfiguration = st.Contains(Config.SPCSERVTICKET_CHECKMONITORCONFIG) ? (bool)st[Config.SPCSERVTICKET_CHECKMONITORCONFIG] : false;
            newst.RXMantCheckOS = st.Contains(Config.SPCSERVTICKET_CHECKOS) ? (bool)st[Config.SPCSERVTICKET_CHECKOS] : false;
            newst.RXMantCheckRadiationIndicators = st.Contains(Config.SPCSERVTICKET_CHECKRADINDICATORS) ? (bool)st[Config.SPCSERVTICKET_CHECKRADINDICATORS] : false;
            newst.RXMantCheckRollers = st.Contains(Config.SPCSERVTICKET_CHECKROLLERS) ? (bool)st[Config.SPCSERVTICKET_CHECKROLLERS] : false;
            newst.RXMantCheckSecurityCircuit = st.Contains(Config.SPCSERVTICKET_CHECKSECCIRCUIT) ? (bool)st[Config.SPCSERVTICKET_CHECKSECCIRCUIT] : false;
            newst.RXMantCheckTwoWayMode = st.Contains(Config.SPCSERVTICKET_CHECKTWOWAY) ? (bool)st[Config.SPCSERVTICKET_CHECKTWOWAY] : false;
            newst.RXMantCheckVoltMonitor = st.Contains(Config.SPCSERVTICKET_CHECKVOLTMONITOR) ? (bool)st[Config.SPCSERVTICKET_CHECKVOLTMONITOR] : false;
            newst.RXMantCheckXRCone = st.Contains(Config.SPCSERVTICKET_CHECKXRCONE) ? (bool)st[Config.SPCSERVTICKET_CHECKXRCONE] : false;
            newst.RXVoltCheckHaveUPS = st.Contains(Config.SPCSERVTICKET_CHECKHAVEUPS) ? (bool)st[Config.SPCSERVTICKET_CHECKHAVEUPS] : false;
            newst.RXVoltCheckIsolationTransformator = st.Contains(Config.SPCSERVTICKET_CHECKISOLATIONTRANSF) ? (bool)st[Config.SPCSERVTICKET_CHECKISOLATIONTRANSF] : false;
            newst.RXCalType1 = st.Contains(Config.SPCSERVTICKET_TEST1) ? (bool)st[Config.SPCSERVTICKET_TEST1] : false;
            newst.RXCalType2 = st.Contains(Config.SPCSERVTICKET_TEST2) ? (bool)st[Config.SPCSERVTICKET_TEST2] : false;
            newst.RXCalType3 = st.Contains(Config.SPCSERVTICKET_TEST3) ? (bool)st[Config.SPCSERVTICKET_TEST3] : false;
            newst.RXCalType4 = st.Contains(Config.SPCSERVTICKET_TEST4) ? (bool)st[Config.SPCSERVTICKET_TEST4] : false;
            if (st.Contains(Config.SPCSERVTICKET_SCREENTYPE))
                newst.RXMantCheckScreenType = (Types.SPCSERVTICKET_SCREENTYPE)((OptionSetValue)st[Config.SPCSERVTICKET_SCREENTYPE]).Value;
            if (st.Contains(Config.SPCSERVTICKET_LEADSTATE))
                newst.RXMantLeadState = (Types.SPCSERVTICKET_LEADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_LEADSTATE]).Value;
            #endregion
            #region Voltages
            if (st.Contains(Config.SPCSERVTICKET_GROUNDVOLTAGE))
                newst.RXVoltGroundVoltage = (string)st[Config.SPCSERVTICKET_GROUNDVOLTAGE];
            if (st.Contains(Config.SPCSERVTICKET_INVOLTAGE))
                newst.RXVoltInVoltage = (string)st[Config.SPCSERVTICKET_INVOLTAGE];
            if (newst.RXVoltCheckHaveUPS)
            {
                if (st.Contains(Config.SPCSERVTICKET_UPSCAP))
                    newst.RXVoltUPSCapacity = (string)st[Config.SPCSERVTICKET_UPSCAP];
                if (st.Contains(Config.SPCSERVTICKET_UPSGROUNDVOLT))
                    newst.RXVoltUPSGroundVoltage = (string)st[Config.SPCSERVTICKET_UPSGROUNDVOLT];
                if (st.Contains(Config.SPCSERVTICKET_UPSINVOLT))
                    newst.RXVoltUPSInVoltage = (string)st[Config.SPCSERVTICKET_UPSINVOLT];
            }
            if (st.Contains(Config.SPCSERVTICKET_GEN1OFFVOLTAGE))
                newst.RXVoltGenerator1XROffVoltage = (string)st[Config.SPCSERVTICKET_GEN1OFFVOLTAGE];
            if (st.Contains(Config.SPCSERVTICKET_GEN1ONANODEVOLT))
                newst.RXVoltGenerator1XROnAnodeVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONANODEVOLT];
            if (st.Contains(Config.SPCSERVTICKET_GEN1ONHIGHVOLT))
                newst.RXVoltGenerator1XROnHighVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONHIGHVOLT];
            if (st.Contains(Config.SPCSERVTICKET_GEN1ONVOLTAGE))
                newst.RXVoltGenerator1XROnVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONVOLTAGE];
            if (st.Contains(Config.SPCSERVTICKET_GEN2OFFVOLTAGE))
                newst.RXVoltGenerator2XROffVoltage = (string)st[Config.SPCSERVTICKET_GEN2OFFVOLTAGE];
            if (st.Contains(Config.SPCSERVTICKET_GEN2ONANODEVOLT))
                newst.RXVoltGenerator2XROnAnodeVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONANODEVOLT];
            if (st.Contains(Config.SPCSERVTICKET_GEN2ONHIGHVOLT))
                newst.RXVoltGenerator2XROnHighVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONHIGHVOLT];
            if (st.Contains(Config.SPCSERVTICKET_GEN2ONVOLTAGE))
                newst.RXVoltGenerator2XROnVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONVOLTAGE];
            #endregion
            #region Radiation
            if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONDATE))
                newst.RXRadHWCalibrationDate = (DateTime)st[Config.SPCSERVTICKET_CALIBRATIONDATE];
            if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONDUEDATE))
                newst.RXRadHWCalibrationDueDate = (DateTime)st[Config.SPCSERVTICKET_CALIBRATIONDUEDATE];
            if (st.Contains(Config.SPCSERVTICKET_RADSTATE))
                newst.RXRadRadiationState = (Types.SPCSERVTICKET_RADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_RADSTATE]).Value;
            if (st.Contains(Config.SPCSERVTICKET_CALMODEL))
                newst.RXRadHWModel = (string)st[Config.SPCSERVTICKET_CALMODEL];
            if (st.Contains(Config.SPCSERVTICKET_CALTRADEMARK))
                newst.RXRadHWTrademark = (string)st[Config.SPCSERVTICKET_CALTRADEMARK];
            if (st.Contains(Config.SPCSERVTICKET_OPERATORRAD))
                newst.RXRadOperatorRad = (string)st[Config.SPCSERVTICKET_OPERATORRAD];
            if (st.Contains(Config.SPCSERVTICKET_TUNNELRADIN))
                newst.RXRadTunnelRadIn = (string)st[Config.SPCSERVTICKET_TUNNELRADIN];
            if (st.Contains(Config.SPCSERVTICKET_TUNNELRADOUT))
                newst.RXRadTunnelRadOut = (string)st[Config.SPCSERVTICKET_TUNNELRADOUT];
            #endregion
            #region Calibration
            if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONSTATE))
                newst.RXCalCalibrationState = (Types.SPCSERVTICKET_RADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_CALIBRATIONSTATE]).Value;
            if (st.Contains(Config.SPCSERVTICKET_STEELPENETRATION))
                newst.RXCalSteelPenetration = (string)st[Config.SPCSERVTICKET_STEELPENETRATION];
            if (st.Contains(Config.SPCSERVTICKET_WIRERESOLUTION))
                newst.RXCalWireResolution = (string)st[Config.SPCSERVTICKET_WIRERESOLUTION];
            #endregion
            #region Software
            if (st.Contains(Config.SPCSERVTICKET_HAVEHDA))
                newst.RXSoftHaveHDA = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHDA]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEHISPOT))
                newst.RXSoftHaveHISPOT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHISPOT]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEHITIP))
                newst.RXSoftHaveHITIP = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHITIP]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEIMS))
                newst.RXSoftHaveIMS = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEIMS]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVESEN))
                newst.RXSoftHaveSEN = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVESEN]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEXACT))
                newst.RXSoftHaveXACT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXACT]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEXPLORE))
                newst.RXSoftHaveXPLORE = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXPLORE]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEXPORT))
                newst.RXSoftHaveXPORT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXPORT]).Value;
            if (st.Contains(Config.SPCSERVTICKET_HAVEXTRAIN))
                newst.RXSoftHaveXTRAIN = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXTRAIN]).Value;
            if (st.Contains(Config.SPCSERVTICKET_PHYSICALDONGLE))
                newst.RXSoftPhysicalDongleSerial = (string)st[Config.SPCSERVTICKET_PHYSICALDONGLE];
            if (st.Contains(Config.SPCSERVTICKET_SOFTWAREDONGLE))
                newst.RXSoftSoftwareDongleSerial = (string)st[Config.SPCSERVTICKET_SOFTWAREDONGLE];
            if (st.Contains(Config.SPCSERVTICKET_SOFTVERSION))
                newst.RXSoftSoftwareVersion = (string)st[Config.SPCSERVTICKET_SOFTVERSION];
            if (st.Contains(Config.SPCSERVTICKET_SOFTTECHNOLOGY))
                newst.RXSoftTechnology = (Types.SPCSERVTICKET_TECHNOLOGY)((OptionSetValue)st[Config.SPCSERVTICKET_SOFTTECHNOLOGY]).Value;
            #endregion
            #endregion
            return newst;
        }

        /// <summary>
        /// Get a collection of all service tickets related to an incident.
        /// </summary>
        /// <param name="caseNumber">Number of case of which service tickets are going to be obtained</param>
        /// <returns>A collection of service tickets model objects</returns>
        public static async Task<List<ServiceTicket>> GetXRServiceTickets(string caseNumber)
        {
            var conn = await Local.GetConnection();
            #region Query
            QueryExpression queryTicketsOfCase = new QueryExpression(Config.SPCSERVTICKET)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCSERVTICKET_NUMBER,
                Config.GENERAL_CREATEDON,
                Config.SPCSERVTICKET_TITLE,
                Config.SPCSERVTICKET_WORKDONE,
                Config.SPCSERVTICKET_SYSTEM,
                Config.SPCSERVTICKET_STARTED,
                Config.SPCSERVTICKET_HADLUNCH,
                Config.SPCSERVTICKET_FINISHED,
                Config.SPCSERVTICKET_TECHNICIAN1,
                Config.SPCSERVTICKET_TECHNICIAN2,
                Config.SPCSERVTICKET_TECHNICIAN3,
                Config.SPCSERVTICKET_TECHNICIAN4,
                Config.SPCSERVTICKET_TECHNICIAN5,
                Config.SPCSERVTICKET_DESCRIPTION,
                Config.SPCSERVTICKET_CURRENCY,
                Config.SPCSERVTICKET_FEEDBACK,
                Config.SPCSERVTICKET_GEN1OFFVOLTAGE,
                Config.SPCSERVTICKET_GEN1ONANODEVOLT,
                Config.SPCSERVTICKET_GEN1ONHIGHVOLT,
                Config.SPCSERVTICKET_GEN1ONVOLTAGE,
                Config.SPCSERVTICKET_GEN2OFFVOLTAGE,
                Config.SPCSERVTICKET_GEN2ONANODEVOLT,
                Config.SPCSERVTICKET_GEN2ONHIGHVOLT,
                Config.SPCSERVTICKET_GEN2ONVOLTAGE,
                Config.SPCSERVTICKET_CALIBRATIONDATE,
                Config.SPCSERVTICKET_CALIBRATIONDUEDATE,
                Config.SPCSERVTICKET_CALIBRATIONSTATE,
                Config.SPCSERVTICKET_CALMODEL,
                Config.SPCSERVTICKET_CALTRADEMARK,
                Config.SPCSERVTICKET_CHECKACSYSTEM,
                Config.SPCSERVTICKET_CHECKCONFIGURATION,
                Config.SPCSERVTICKET_CHECKCONTROLELEMENTS,
                Config.SPCSERVTICKET_CHECKCONVEYORBELT,
                Config.SPCSERVTICKET_CHECKEMERGENCYSTOP,
                Config.SPCSERVTICKET_CHECKENGINECONTROL,
                Config.SPCSERVTICKET_CHECKENGINETRACTION,
                Config.SPCSERVTICKET_CHECKHAVEUPS,
                Config.SPCSERVTICKET_CHECKINOUTSYSTEM,
                Config.SPCSERVTICKET_CHECKINTERLOCK,
                Config.SPCSERVTICKET_CHECKIRFENCES,
                Config.SPCSERVTICKET_CHECKISOLATIONTRANSF,
                Config.SPCSERVTICKET_CHECKKEYBOARD,
                Config.SPCSERVTICKET_CHECKLABELS,
                Config.SPCSERVTICKET_CHECKLINEDETMODS,
                Config.SPCSERVTICKET_CHECKMONITORCONFIG,
                Config.SPCSERVTICKET_CHECKOS,
                Config.SPCSERVTICKET_CHECKRADINDICATORS,
                Config.SPCSERVTICKET_CHECKROLLERS,
                Config.SPCSERVTICKET_CHECKSECCIRCUIT,
                Config.SPCSERVTICKET_CHECKTWOWAY,
                Config.SPCSERVTICKET_CHECKVOLTMONITOR,
                Config.SPCSERVTICKET_CHECKXRCONE,
                Config.SPCSERVTICKET_CLEANAREA,
                Config.SPCSERVTICKET_COMMENTS,
                Config.SPCSERVTICKET_GROUNDVOLTAGE,
                Config.SPCSERVTICKET_HAVEHDA,
                Config.SPCSERVTICKET_HAVEHISPOT,
                Config.SPCSERVTICKET_HAVEHITIP,
                Config.SPCSERVTICKET_HAVEIMS,
                Config.SPCSERVTICKET_HAVESEN,
                Config.SPCSERVTICKET_HAVEXACT,
                Config.SPCSERVTICKET_HAVEXPLORE,
                Config.SPCSERVTICKET_HAVEXPORT,
                Config.SPCSERVTICKET_HAVEXTRAIN,
                Config.SPCSERVTICKET_HWSTATE,
                Config.SPCSERVTICKET_INVOLTAGE,
                Config.SPCSERVTICKET_LEADSTATE,
                Config.SPCSERVTICKET_OPERATORRAD,
                Config.SPCSERVTICKET_PHYSICALDONGLE,
                Config.SPCSERVTICKET_RADSTATE,
                Config.SPCSERVTICKET_RXCREATIONDATE,
                Config.SPCSERVTICKET_RXMODEL,
                Config.SPCSERVTICKET_RXSERIAL,
                Config.SPCSERVTICKET_RXSYNERGY,
                Config.SPCSERVTICKET_SCREENTYPE,
                Config.SPCSERVTICKET_SOFTTECHNOLOGY,
                Config.SPCSERVTICKET_SOFTVERSION,
                Config.SPCSERVTICKET_SOFTWAREDONGLE,
                Config.SPCSERVTICKET_STEELPENETRATION,
                Config.SPCSERVTICKET_TEST1,
                Config.SPCSERVTICKET_TEST2,
                Config.SPCSERVTICKET_TEST3,
                Config.SPCSERVTICKET_TEST4,
                Config.SPCSERVTICKET_TUNNELRADIN,
                Config.SPCSERVTICKET_TUNNELRADOUT,
                Config.SPCSERVTICKET_UPSCAP,
                Config.SPCSERVTICKET_UPSGROUNDVOLT,
                Config.SPCSERVTICKET_UPSINVOLT,
                Config.SPCSERVTICKET_VISITNUMBER,
                Config.SPCSERVTICKET_WIRERESOLUTION
            }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };
            queryTicketsOfCase.Criteria.AddCondition(new ConditionExpression(Config.SPCSERVTICKET_CASENUMBER, ConditionOperator.Equal, caseNumber));
            queryTicketsOfCase.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATECODE, ConditionOperator.Equal, 0));
            EntityCollection result = await Proxy.RetrieveMultiple(queryTicketsOfCase);
            #endregion
            List<ServiceTicket> finalresult = new List<ServiceTicket>();
            if (result.Entities.Count > 0)
                foreach (Entity st in result.Entities)
                {
                    ServiceTicket newst = new ServiceTicket
                    {
                        InternalId = st.Id,
                        CreationDate = (DateTime)st[Config.GENERAL_CREATEDON],
                        ProductsUsed = await GetMaterialsFromServiceTicket(st.Id)
                    };
                    ServiceTicket local = await conn.Table<ServiceTicket>().Where(s => s.InternalId.Equals(st.Id)).FirstOrDefaultAsync();
                    if (local != null)
                    {
                        newst.SQLiteRecordId = local.SQLiteRecordId;
                        newst.IncidentId = local.IncidentId;
                    }
                    #region Basic Service Ticket
                    newst.FeedbackSubmitted = st.Contains(Config.SPCSERVTICKET_FEEDBACK) ? (bool)st[Config.SPCSERVTICKET_FEEDBACK] : false;
                    newst.HadLunch = st.Contains(Config.SPCSERVTICKET_HADLUNCH) ? (bool)st[Config.SPCSERVTICKET_HADLUNCH] : false;
                    newst.TicketNumber = st.Contains(Config.SPCSERVTICKET_NUMBER) ? st[Config.SPCSERVTICKET_NUMBER].ToString() : "Sin número";
                    #region Subtype
                    if (st.Contains(Config.SPCSERVTICKET_SYSTEM))
                        newst.Type = await GetSubtype(((EntityReference)st[Config.SPCSERVTICKET_SYSTEM]).Id);
                    if (newst.Type != null)
                        newst.TypeId = newst.Type.SQLiteRecordId;
                    #endregion
                    #region Currency
                    if (st.Contains(Config.SPCSERVTICKET_CURRENCY))
                        newst.MoneyCurrency = await GetCurrency(((EntityReference)st[Config.SPCSERVTICKET_CURRENCY]).Id);
                    if (newst.MoneyCurrency != null)
                        newst.MoneyCurrencyId = newst.MoneyCurrency.SQLiteRecordId;
                    #endregion
                    if (st.Contains(Config.SPCSERVTICKET_STARTED))
                        newst.Started = (DateTime)st[Config.SPCSERVTICKET_STARTED];
                    if (st.Contains(Config.SPCSERVTICKET_TITLE))
                        newst.Title = (string)st[Config.SPCSERVTICKET_TITLE];
                    if (st.Contains(Config.SPCSERVTICKET_DESCRIPTION))
                        newst.Description = (string)st[Config.SPCSERVTICKET_DESCRIPTION];
                    if (st.Contains(Config.SPCSERVTICKET_FINISHED))
                        newst.Finished = (DateTime)st[Config.SPCSERVTICKET_FINISHED];
                    if (st.Contains(Config.SPCSERVTICKET_WORKDONE))
                        newst.WorkDone = (string)st[Config.SPCSERVTICKET_WORKDONE];
                    newst.Technicians = new List<Technician>();
                    #region Technician 1
                    if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN1))
                    {
                        Technician tech1 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN1]).Id);
                        if (tech1 == null)
                            throw new Exception("Técnico 1 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                        newst.Technicians.Add(tech1);
                    }
                    #endregion
                    #region Technician 2
                    if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN2))
                    {
                        Technician tech2 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN2]).Id);
                        if (tech2 == null)
                            throw new Exception("Técnico 2 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                        newst.Technicians.Add(tech2);
                    }
                    #endregion
                    #region Technician 3
                    if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN3))
                    {
                        Technician tech3 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN3]).Id);
                        if (tech3 == null)
                            throw new Exception("Técnico 3 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                        newst.Technicians.Add(tech3);
                    }
                    #endregion
                    #region Technician 4
                    if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN4))
                    {
                        Technician tech4 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN4]).Id);
                        if (tech4 == null)
                            throw new Exception("Técnico 4 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                        newst.Technicians.Add(tech4);
                    }
                    #endregion
                    #region Technician 5
                    if (st.Contains(Config.SPCSERVTICKET_TECHNICIAN5))
                    {
                        Technician tech5 = await GetTechnician(((EntityReference)st[Config.SPCSERVTICKET_TECHNICIAN5]).Id);
                        if (tech5 == null)
                            throw new Exception("Técnico 5 de boleta " + newst.TicketNumber + " no se puede cargar dado que tiene información incompleta.");
                        newst.Technicians.Add(tech5);
                    }
                    #endregion
                    #endregion
                    #region RX
                    #region General
                    if (st.Contains(Config.SPCSERVTICKET_COMMENTS))
                        newst.RXGenComments = (string)st[Config.SPCSERVTICKET_COMMENTS];
                    if (st.Contains(Config.SPCSERVTICKET_RXCREATIONDATE))
                        newst.RXGenCreationDate = (string)st[Config.SPCSERVTICKET_RXCREATIONDATE];
                    if (st.Contains(Config.SPCSERVTICKET_RXMODEL))
                        newst.RXGenModel = (string)st[Config.SPCSERVTICKET_RXMODEL];
                    if (st.Contains(Config.SPCSERVTICKET_HWSTATE))
                        newst.RXGenHWState = (Types.SPCSERVTICKET_HWSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HWSTATE]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_RXSERIAL))
                        newst.RXGenSerial = (string)st[Config.SPCSERVTICKET_RXSERIAL];
                    if (st.Contains(Config.SPCSERVTICKET_VISITNUMBER))
                        newst.RXGenVisitNumber = (Types.SPCSERVTICKET_VISITNUMBER)((OptionSetValue)st[Config.SPCSERVTICKET_VISITNUMBER]).Value;
                    #endregion
                    #region Mantainance                    
                    newst.RXGenCleanArea = st.Contains(Config.SPCSERVTICKET_CLEANAREA) ? (bool)st[Config.SPCSERVTICKET_CLEANAREA] : false;
                    newst.RXMantCheckConditioningSystem = st.Contains(Config.SPCSERVTICKET_CHECKACSYSTEM) ? (bool)st[Config.SPCSERVTICKET_CHECKACSYSTEM] : false;
                    newst.RXMantCheckConfiguration = st.Contains(Config.SPCSERVTICKET_CHECKCONFIGURATION) ? (bool)st[Config.SPCSERVTICKET_CHECKCONFIGURATION] : false;
                    newst.RXMantCheckControlElements = st.Contains(Config.SPCSERVTICKET_CHECKCONTROLELEMENTS) ? (bool)st[Config.SPCSERVTICKET_CHECKCONTROLELEMENTS] : false;
                    newst.RXMantCheckConveyorBelt = st.Contains(Config.SPCSERVTICKET_CHECKCONVEYORBELT) ? (bool)st[Config.SPCSERVTICKET_CHECKCONVEYORBELT] : false;
                    newst.RXMantCheckEmergencyStop = st.Contains(Config.SPCSERVTICKET_CHECKEMERGENCYSTOP) ? (bool)st[Config.SPCSERVTICKET_CHECKEMERGENCYSTOP] : false;
                    newst.RXMantCheckEngineControl = st.Contains(Config.SPCSERVTICKET_CHECKENGINECONTROL) ? (bool)st[Config.SPCSERVTICKET_CHECKENGINECONTROL] : false;
                    newst.RXMantCheckEngineTraction = st.Contains(Config.SPCSERVTICKET_CHECKENGINETRACTION) ? (bool)st[Config.SPCSERVTICKET_CHECKENGINETRACTION] : false;
                    newst.RXMantCheckInOutSystem = st.Contains(Config.SPCSERVTICKET_CHECKINOUTSYSTEM) ? (bool)st[Config.SPCSERVTICKET_CHECKINOUTSYSTEM] : false;
                    newst.RXMantCheckInterlock = st.Contains(Config.SPCSERVTICKET_CHECKINTERLOCK) ? (bool)st[Config.SPCSERVTICKET_CHECKINTERLOCK] : false;
                    newst.RXMantCheckIRFences = st.Contains(Config.SPCSERVTICKET_CHECKIRFENCES) ? (bool)st[Config.SPCSERVTICKET_CHECKIRFENCES] : false;
                    newst.RXMantCheckKeyboard = st.Contains(Config.SPCSERVTICKET_CHECKKEYBOARD) ? (bool)st[Config.SPCSERVTICKET_CHECKKEYBOARD] : false;
                    newst.RXMantCheckLabels = st.Contains(Config.SPCSERVTICKET_CHECKLABELS) ? (bool)st[Config.SPCSERVTICKET_CHECKLABELS] : false;
                    newst.RXMantCheckLineAndDetectionModules = st.Contains(Config.SPCSERVTICKET_CHECKLINEDETMODS) ? (bool)st[Config.SPCSERVTICKET_CHECKLINEDETMODS] : false;
                    newst.RXMantCheckMonitorConfiguration = st.Contains(Config.SPCSERVTICKET_CHECKMONITORCONFIG) ? (bool)st[Config.SPCSERVTICKET_CHECKMONITORCONFIG] : false;
                    newst.RXMantCheckOS = st.Contains(Config.SPCSERVTICKET_CHECKOS) ? (bool)st[Config.SPCSERVTICKET_CHECKOS] : false;
                    newst.RXMantCheckRadiationIndicators = st.Contains(Config.SPCSERVTICKET_CHECKRADINDICATORS) ? (bool)st[Config.SPCSERVTICKET_CHECKRADINDICATORS] : false;
                    newst.RXMantCheckRollers = st.Contains(Config.SPCSERVTICKET_CHECKROLLERS) ? (bool)st[Config.SPCSERVTICKET_CHECKROLLERS] : false;
                    newst.RXMantCheckSecurityCircuit = st.Contains(Config.SPCSERVTICKET_CHECKSECCIRCUIT) ? (bool)st[Config.SPCSERVTICKET_CHECKSECCIRCUIT] : false;
                    newst.RXMantCheckTwoWayMode = st.Contains(Config.SPCSERVTICKET_CHECKTWOWAY) ? (bool)st[Config.SPCSERVTICKET_CHECKTWOWAY] : false;
                    newst.RXMantCheckVoltMonitor = st.Contains(Config.SPCSERVTICKET_CHECKVOLTMONITOR) ? (bool)st[Config.SPCSERVTICKET_CHECKVOLTMONITOR] : false;
                    newst.RXMantCheckXRCone = st.Contains(Config.SPCSERVTICKET_CHECKXRCONE) ? (bool)st[Config.SPCSERVTICKET_CHECKXRCONE] : false;
                    newst.RXVoltCheckHaveUPS = st.Contains(Config.SPCSERVTICKET_CHECKHAVEUPS) ? (bool)st[Config.SPCSERVTICKET_CHECKHAVEUPS] : false;
                    newst.RXVoltCheckIsolationTransformator = st.Contains(Config.SPCSERVTICKET_CHECKISOLATIONTRANSF) ? (bool)st[Config.SPCSERVTICKET_CHECKISOLATIONTRANSF] : false;
                    newst.RXCalType1 = st.Contains(Config.SPCSERVTICKET_TEST1) ? (bool)st[Config.SPCSERVTICKET_TEST1] : false;
                    newst.RXCalType2 = st.Contains(Config.SPCSERVTICKET_TEST2) ? (bool)st[Config.SPCSERVTICKET_TEST2] : false;
                    newst.RXCalType3 = st.Contains(Config.SPCSERVTICKET_TEST3) ? (bool)st[Config.SPCSERVTICKET_TEST3] : false;
                    newst.RXCalType4 = st.Contains(Config.SPCSERVTICKET_TEST4) ? (bool)st[Config.SPCSERVTICKET_TEST4] : false;
                    if (st.Contains(Config.SPCSERVTICKET_SCREENTYPE))
                        newst.RXMantCheckScreenType = (Types.SPCSERVTICKET_SCREENTYPE)((OptionSetValue)st[Config.SPCSERVTICKET_SCREENTYPE]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_LEADSTATE))
                        newst.RXMantLeadState = (Types.SPCSERVTICKET_LEADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_LEADSTATE]).Value;
                    #endregion
                    #region Voltages
                    if (st.Contains(Config.SPCSERVTICKET_GROUNDVOLTAGE))
                        newst.RXVoltGroundVoltage = (string)st[Config.SPCSERVTICKET_GROUNDVOLTAGE];
                    if (st.Contains(Config.SPCSERVTICKET_INVOLTAGE))
                        newst.RXVoltInVoltage = (string)st[Config.SPCSERVTICKET_INVOLTAGE];
                    if (newst.RXVoltCheckHaveUPS)
                    {
                        if (st.Contains(Config.SPCSERVTICKET_UPSCAP))
                            newst.RXVoltUPSCapacity = (string)st[Config.SPCSERVTICKET_UPSCAP];
                        if (st.Contains(Config.SPCSERVTICKET_UPSGROUNDVOLT))
                            newst.RXVoltUPSGroundVoltage = (string)st[Config.SPCSERVTICKET_UPSGROUNDVOLT];
                        if (st.Contains(Config.SPCSERVTICKET_UPSINVOLT))
                            newst.RXVoltUPSInVoltage = (string)st[Config.SPCSERVTICKET_UPSINVOLT];
                    }
                    if (st.Contains(Config.SPCSERVTICKET_GEN1OFFVOLTAGE))
                        newst.RXVoltGenerator1XROffVoltage = (string)st[Config.SPCSERVTICKET_GEN1OFFVOLTAGE];
                    if (st.Contains(Config.SPCSERVTICKET_GEN1ONANODEVOLT))
                        newst.RXVoltGenerator1XROnAnodeVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONANODEVOLT];
                    if (st.Contains(Config.SPCSERVTICKET_GEN1ONHIGHVOLT))
                        newst.RXVoltGenerator1XROnHighVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONHIGHVOLT];
                    if (st.Contains(Config.SPCSERVTICKET_GEN1ONVOLTAGE))
                        newst.RXVoltGenerator1XROnVoltage = (string)st[Config.SPCSERVTICKET_GEN1ONVOLTAGE];
                    if (st.Contains(Config.SPCSERVTICKET_GEN2OFFVOLTAGE))
                        newst.RXVoltGenerator2XROffVoltage = (string)st[Config.SPCSERVTICKET_GEN2OFFVOLTAGE];
                    if (st.Contains(Config.SPCSERVTICKET_GEN2ONANODEVOLT))
                        newst.RXVoltGenerator2XROnAnodeVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONANODEVOLT];
                    if (st.Contains(Config.SPCSERVTICKET_GEN2ONHIGHVOLT))
                        newst.RXVoltGenerator2XROnHighVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONHIGHVOLT];
                    if (st.Contains(Config.SPCSERVTICKET_GEN2ONVOLTAGE))
                        newst.RXVoltGenerator2XROnVoltage = (string)st[Config.SPCSERVTICKET_GEN2ONVOLTAGE];
                    #endregion
                    #region Radiation
                    if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONDATE))
                        newst.RXRadHWCalibrationDate = (DateTime)st[Config.SPCSERVTICKET_CALIBRATIONDATE];
                    if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONDUEDATE))
                        newst.RXRadHWCalibrationDueDate = (DateTime)st[Config.SPCSERVTICKET_CALIBRATIONDUEDATE];
                    if (st.Contains(Config.SPCSERVTICKET_RADSTATE))
                        newst.RXRadRadiationState = (Types.SPCSERVTICKET_RADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_RADSTATE]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_CALMODEL))
                        newst.RXRadHWModel = (string)st[Config.SPCSERVTICKET_CALMODEL];
                    if (st.Contains(Config.SPCSERVTICKET_CALTRADEMARK))
                        newst.RXRadHWTrademark = (string)st[Config.SPCSERVTICKET_CALTRADEMARK];
                    if (st.Contains(Config.SPCSERVTICKET_OPERATORRAD))
                        newst.RXRadOperatorRad = (string)st[Config.SPCSERVTICKET_OPERATORRAD];
                    if (st.Contains(Config.SPCSERVTICKET_TUNNELRADIN))
                        newst.RXRadTunnelRadIn = (string)st[Config.SPCSERVTICKET_TUNNELRADIN];
                    if (st.Contains(Config.SPCSERVTICKET_TUNNELRADOUT))
                        newst.RXRadTunnelRadOut = (string)st[Config.SPCSERVTICKET_TUNNELRADOUT];
                    #endregion
                    #region Calibration
                    if (st.Contains(Config.SPCSERVTICKET_CALIBRATIONSTATE))
                        newst.RXCalCalibrationState = (Types.SPCSERVTICKET_RADSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_CALIBRATIONSTATE]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_STEELPENETRATION))
                        newst.RXCalSteelPenetration = (string)st[Config.SPCSERVTICKET_STEELPENETRATION];
                    if (st.Contains(Config.SPCSERVTICKET_WIRERESOLUTION))
                        newst.RXCalWireResolution = (string)st[Config.SPCSERVTICKET_WIRERESOLUTION];
                    #endregion
                    #region Software
                    if (st.Contains(Config.SPCSERVTICKET_HAVEHDA))
                        newst.RXSoftHaveHDA = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHDA]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEHISPOT))
                        newst.RXSoftHaveHISPOT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHISPOT]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEHITIP))
                        newst.RXSoftHaveHITIP = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEHITIP]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEIMS))
                        newst.RXSoftHaveIMS = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEIMS]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVESEN))
                        newst.RXSoftHaveSEN = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVESEN]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEXACT))
                        newst.RXSoftHaveXACT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXACT]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEXPLORE))
                        newst.RXSoftHaveXPLORE = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXPLORE]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEXPORT))
                        newst.RXSoftHaveXPORT = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXPORT]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_HAVEXTRAIN))
                        newst.RXSoftHaveXTRAIN = (Types.SPCSERVTICKET_POSSESIONSTATE)((OptionSetValue)st[Config.SPCSERVTICKET_HAVEXTRAIN]).Value;
                    if (st.Contains(Config.SPCSERVTICKET_PHYSICALDONGLE))
                        newst.RXSoftPhysicalDongleSerial = (string)st[Config.SPCSERVTICKET_PHYSICALDONGLE];
                    if (st.Contains(Config.SPCSERVTICKET_SOFTWAREDONGLE))
                        newst.RXSoftSoftwareDongleSerial = (string)st[Config.SPCSERVTICKET_SOFTWAREDONGLE];
                    if (st.Contains(Config.SPCSERVTICKET_SOFTVERSION))
                        newst.RXSoftSoftwareVersion = (string)st[Config.SPCSERVTICKET_SOFTVERSION];
                    if (st.Contains(Config.SPCSERVTICKET_SOFTTECHNOLOGY))
                        newst.RXSoftTechnology = (Types.SPCSERVTICKET_TECHNOLOGY)((OptionSetValue)st[Config.SPCSERVTICKET_SOFTTECHNOLOGY]).Value;
                    #endregion
                    #endregion
                    finalresult.Add(newst);
                }
            return finalresult;
        }

        /// <summary>
        /// Save Changes of a service ticket in the CRM.
        /// </summary>
        /// <param name="ticket">Ticket to be updated or saved</param>
        /// <returns>Boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveChangesOfXRC(ServiceTicket ticket)
        {
            Entity serviceTicket = new Entity(Config.SPCSERVTICKET, ticket.InternalId);
            #region General
            serviceTicket[Config.SPCSERVTICKET_CLEANAREA] = ticket.RXGenCleanArea;
            if (!String.IsNullOrEmpty(ticket.RXGenComments))
                serviceTicket[Config.SPCSERVTICKET_COMMENTS] = ticket.RXGenComments;
            if (!String.IsNullOrEmpty(ticket.RXGenCreationDate))
                serviceTicket[Config.SPCSERVTICKET_RXCREATIONDATE] = ticket.RXGenCreationDate;
            if (ticket.RXGenHWState != Types.SPCSERVTICKET_HWSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HWSTATE] = new OptionSetValue((int)ticket.RXGenHWState);
            if (!String.IsNullOrEmpty(ticket.RXGenModel))
                serviceTicket[Config.SPCSERVTICKET_RXMODEL] = ticket.RXGenModel;
            if (!String.IsNullOrEmpty(ticket.RXGenSerial))
                serviceTicket[Config.SPCSERVTICKET_RXSERIAL] = ticket.RXGenSerial;
            if (ticket.RXGenVisitNumber != Types.SPCSERVTICKET_VISITNUMBER.Undefined)
                serviceTicket[Config.SPCSERVTICKET_VISITNUMBER] = new OptionSetValue((int)ticket.RXGenVisitNumber);
            #endregion
            #region Manteinance
            serviceTicket[Config.SPCSERVTICKET_CHECKACSYSTEM] = ticket.RXMantCheckConditioningSystem;
            serviceTicket[Config.SPCSERVTICKET_CHECKCONFIGURATION] = ticket.RXMantCheckConfiguration;
            serviceTicket[Config.SPCSERVTICKET_CHECKCONTROLELEMENTS] = ticket.RXMantCheckControlElements;
            serviceTicket[Config.SPCSERVTICKET_CHECKCONVEYORBELT] = ticket.RXMantCheckConveyorBelt;
            serviceTicket[Config.SPCSERVTICKET_CHECKEMERGENCYSTOP] = ticket.RXMantCheckEmergencyStop;
            serviceTicket[Config.SPCSERVTICKET_CHECKENGINECONTROL] = ticket.RXMantCheckEngineControl;
            serviceTicket[Config.SPCSERVTICKET_CHECKENGINETRACTION] = ticket.RXMantCheckEngineTraction;
            serviceTicket[Config.SPCSERVTICKET_CHECKINOUTSYSTEM] = ticket.RXMantCheckInOutSystem;
            serviceTicket[Config.SPCSERVTICKET_CHECKINTERLOCK] = ticket.RXMantCheckInterlock;
            serviceTicket[Config.SPCSERVTICKET_CHECKIRFENCES] = ticket.RXMantCheckIRFences;
            serviceTicket[Config.SPCSERVTICKET_CHECKKEYBOARD] = ticket.RXMantCheckKeyboard;
            serviceTicket[Config.SPCSERVTICKET_CHECKLABELS] = ticket.RXMantCheckLabels;
            serviceTicket[Config.SPCSERVTICKET_CHECKLINEDETMODS] = ticket.RXMantCheckLineAndDetectionModules;
            serviceTicket[Config.SPCSERVTICKET_CHECKMONITORCONFIG] = ticket.RXMantCheckMonitorConfiguration;
            serviceTicket[Config.SPCSERVTICKET_CHECKOS] = ticket.RXMantCheckOS;
            serviceTicket[Config.SPCSERVTICKET_CHECKRADINDICATORS] = ticket.RXMantCheckRadiationIndicators;
            serviceTicket[Config.SPCSERVTICKET_CHECKROLLERS] = ticket.RXMantCheckRollers;
            if (ticket.RXMantCheckScreenType != Types.SPCSERVTICKET_SCREENTYPE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_SCREENTYPE] = new OptionSetValue((int)ticket.RXMantCheckScreenType);
            serviceTicket[Config.SPCSERVTICKET_CHECKSECCIRCUIT] = ticket.RXMantCheckSecurityCircuit;
            serviceTicket[Config.SPCSERVTICKET_CHECKTWOWAY] = ticket.RXMantCheckTwoWayMode;
            serviceTicket[Config.SPCSERVTICKET_CHECKVOLTMONITOR] = ticket.RXMantCheckVoltMonitor;
            serviceTicket[Config.SPCSERVTICKET_CHECKXRCONE] = ticket.RXMantCheckXRCone;
            if (ticket.RXMantLeadState != Types.SPCSERVTICKET_LEADSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_LEADSTATE] = new OptionSetValue((int)ticket.RXMantLeadState);
            #endregion
            #region Voltages
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator1XROffVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN1OFFVOLTAGE] = ticket.RXVoltGenerator1XROffVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator1XROnAnodeVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN1ONANODEVOLT] = ticket.RXVoltGenerator1XROnAnodeVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator1XROnHighVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN1ONHIGHVOLT] = ticket.RXVoltGenerator1XROnHighVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator1XROnVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN1ONVOLTAGE] = ticket.RXVoltGenerator1XROnVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator2XROffVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN2OFFVOLTAGE] = ticket.RXVoltGenerator2XROffVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator2XROnAnodeVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN2ONANODEVOLT] = ticket.RXVoltGenerator2XROnAnodeVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator2XROnHighVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN2ONHIGHVOLT] = ticket.RXVoltGenerator2XROnHighVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGenerator2XROnVoltage))
                serviceTicket[Config.SPCSERVTICKET_GEN2ONVOLTAGE] = ticket.RXVoltGenerator2XROnVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltGroundVoltage))
                serviceTicket[Config.SPCSERVTICKET_GROUNDVOLTAGE] = ticket.RXVoltGroundVoltage;
            if (!String.IsNullOrEmpty(ticket.RXVoltInVoltage))
                serviceTicket[Config.SPCSERVTICKET_INVOLTAGE] = ticket.RXVoltInVoltage;
            serviceTicket[Config.SPCSERVTICKET_CHECKHAVEUPS] = ticket.RXVoltCheckHaveUPS;
            if (ticket.RXVoltCheckHaveUPS)
            {
                if (!String.IsNullOrEmpty(ticket.RXVoltUPSCapacity))
                    serviceTicket[Config.SPCSERVTICKET_UPSCAP] = ticket.RXVoltUPSCapacity;
                if (!String.IsNullOrEmpty(ticket.RXVoltUPSGroundVoltage))
                    serviceTicket[Config.SPCSERVTICKET_UPSGROUNDVOLT] = ticket.RXVoltUPSGroundVoltage;
                if (!String.IsNullOrEmpty(ticket.RXVoltUPSInVoltage))
                    serviceTicket[Config.SPCSERVTICKET_UPSINVOLT] = ticket.RXVoltUPSInVoltage;
            }
            serviceTicket[Config.SPCSERVTICKET_CHECKISOLATIONTRANSF] = ticket.RXVoltCheckIsolationTransformator;
            #endregion
            #region Radiation
            if (!ticket.RXRadHWCalibrationDate.Equals(default(DateTime)))
                serviceTicket[Config.SPCSERVTICKET_CALIBRATIONDATE] = ticket.RXRadHWCalibrationDate;
            if (!ticket.RXRadHWCalibrationDueDate.Equals(default(DateTime)))
                serviceTicket[Config.SPCSERVTICKET_CALIBRATIONDUEDATE] = ticket.RXRadHWCalibrationDueDate;
            if (!String.IsNullOrEmpty(ticket.RXRadHWModel))
                serviceTicket[Config.SPCSERVTICKET_CALMODEL] = ticket.RXRadHWModel;
            if (!String.IsNullOrEmpty(ticket.RXRadHWTrademark))
                serviceTicket[Config.SPCSERVTICKET_CALTRADEMARK] = ticket.RXRadHWTrademark;
            if (!String.IsNullOrEmpty(ticket.RXRadOperatorRad))
                serviceTicket[Config.SPCSERVTICKET_OPERATORRAD] = ticket.RXRadOperatorRad;
            if (!String.IsNullOrEmpty(ticket.RXRadTunnelRadIn))
                serviceTicket[Config.SPCSERVTICKET_TUNNELRADIN] = ticket.RXRadTunnelRadIn;
            if (!String.IsNullOrEmpty(ticket.RXRadTunnelRadOut))
                serviceTicket[Config.SPCSERVTICKET_TUNNELRADOUT] = ticket.RXRadTunnelRadOut;
            if (ticket.RXRadRadiationState != Types.SPCSERVTICKET_RADSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_RADSTATE] = new OptionSetValue((int)ticket.RXRadRadiationState);
            #endregion
            #region Calibration          
            //if (ticket.RXCalTestType != Types.SPCSERVTICKET_TESTTYPE.Undefined)
            //    serviceTicket[Config.SPCSERVTICKET_TESTTYPE] = new OptionSetValue((int)ticket.RXCalTestType);
            if (ticket.RXCalCalibrationState != Types.SPCSERVTICKET_RADSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_CALIBRATIONSTATE] = new OptionSetValue((int)ticket.RXCalCalibrationState);
            if (!String.IsNullOrEmpty(ticket.RXCalSteelPenetration))
                serviceTicket[Config.SPCSERVTICKET_STEELPENETRATION] = ticket.RXCalSteelPenetration;
            if (!String.IsNullOrEmpty(ticket.RXCalWireResolution))
                serviceTicket[Config.SPCSERVTICKET_WIRERESOLUTION] = ticket.RXCalWireResolution;
            serviceTicket[Config.SPCSERVTICKET_TEST1] = ticket.RXCalType1;
            serviceTicket[Config.SPCSERVTICKET_TEST2] = ticket.RXCalType2;
            serviceTicket[Config.SPCSERVTICKET_TEST3] = ticket.RXCalType3;
            serviceTicket[Config.SPCSERVTICKET_TEST4] = ticket.RXCalType4;
            #endregion
            #region Software
            if (!String.IsNullOrEmpty(ticket.RXSoftSoftwareVersion))
                serviceTicket[Config.SPCSERVTICKET_SOFTVERSION] = ticket.RXSoftSoftwareVersion;
            if (!String.IsNullOrEmpty(ticket.RXSoftPhysicalDongleSerial))
                serviceTicket[Config.SPCSERVTICKET_PHYSICALDONGLE] = ticket.RXSoftPhysicalDongleSerial;
            if (!String.IsNullOrEmpty(ticket.RXSoftSoftwareDongleSerial))
                serviceTicket[Config.SPCSERVTICKET_SOFTWAREDONGLE] = ticket.RXSoftSoftwareDongleSerial;
            if (ticket.RXSoftHaveHDA != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEHDA] = new OptionSetValue((int)ticket.RXSoftHaveHDA);
            if (ticket.RXSoftHaveHISPOT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEHISPOT] = new OptionSetValue((int)ticket.RXSoftHaveHISPOT);
            if (ticket.RXSoftHaveHITIP != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEHITIP] = new OptionSetValue((int)ticket.RXSoftHaveHITIP);
            if (ticket.RXSoftHaveIMS != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEIMS] = new OptionSetValue((int)ticket.RXSoftHaveIMS);
            if (ticket.RXSoftHaveSEN != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVESEN] = new OptionSetValue((int)ticket.RXSoftHaveSEN);
            if (ticket.RXSoftHaveXACT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEXACT] = new OptionSetValue((int)ticket.RXSoftHaveXACT);
            if (ticket.RXSoftHaveXPLORE != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEXPLORE] = new OptionSetValue((int)ticket.RXSoftHaveXPLORE);
            if (ticket.RXSoftHaveXPORT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEXPORT] = new OptionSetValue((int)ticket.RXSoftHaveXPORT);
            if (ticket.RXSoftHaveXTRAIN != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                serviceTicket[Config.SPCSERVTICKET_HAVEXTRAIN] = new OptionSetValue((int)ticket.RXSoftHaveXTRAIN);
            if (ticket.RXSoftTechnology != Types.SPCSERVTICKET_TECHNOLOGY.Undefined)
                serviceTicket[Config.SPCSERVTICKET_SOFTTECHNOLOGY] = new OptionSetValue((int)ticket.RXSoftTechnology);
            #endregion
            try
            {
                await Proxy.Update(serviceTicket);
                var conn = await Local.GetConnection();
                await conn.InsertOrReplaceWithChildrenAsync(ticket, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves Changes of a service ticket in the local database.
        /// </summary>
        /// <param name="ticket">Ticket to be updated or saved</param>
        /// <returns>Boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveChangesOfXRCOffline(ServiceTicket ticket)
        {
            var conn = await Local.GetConnection();
            await conn.UpdateWithChildrenAsync(ticket);
            await RegisterPendingOperation(Types.CRUDOperation.Update, ticket.SQLiteRecordId, nameof(ServiceTicket));
            return true;
        }

        /// <summary>
        /// Gets an specific incident from the CRM.
        /// </summary>
        /// <param name="guid">Id of the incident to be obtained</param>
        /// <returns>An incident model object</returns>
        public static async Task<Incident> GetIncident(Guid guid)
        {
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), Config.MSG_TITLE_LOAD_INCIDENT, Config.MSG_LOAD_INCIDENT, 0, 7);
            PageService ps = new PageService();
            await ps.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            var conn = await Local.GetConnection();
            #region Query
            string NameClient = "Client";
            string NameCurrency = "Currency";
            QueryExpression queryIncident = new QueryExpression(Config.SPCCASE)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.GENERAL_CREATEDON,
                    Config.SPCCASE_CASENUMBER,
                    Config.SPCCASE_CLIENT,
                    Config.SPCCASE_CONTROL,
                    Config.SPCCASE_FEEDBACK1,
                    Config.SPCCASE_FEEDBACK2,
                    Config.SPCCASE_FEEDBACKOPINION,
                    Config.SPCCASE_INCIDENCE,
                    Config.SPCCASE_REVIEWED,
                    Config.SPCCASE_TICKET,
                    Config.SPCCASE_CURRENCY,
                    Config.SPCCASE_DESCRIPTION,
                    Config.SPCCASE_PAYMENT,
                    Config.SPCCASE_SYSTEM,
                    Config.SPCCASE_PROGRAMMED,
                    Config.SPCCASE_REPRESENTATIVE,
                    Config.SPCCASE_TECHNICIAN1,
                    Config.SPCCASE_TECHNICIAN2,
                    Config.SPCCASE_TECHNICIAN3,
                    Config.SPCCASE_TITLE
            }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or
                }
            };
            queryIncident.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCASE,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCASE_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCACCOUNT_ALIAS,
                    Config.SPCACCOUNT_ADDRESS,
                    Config.SPCACCOUNT_COUNTRY,
                    Config.SPCACCOUNT_EMAIL,
                    Config.SPCACCOUNT_LOCATION,
                    Config.SPCACCOUNT_NAME,
                    Config.SPCACCOUNT_NUMBER,
                    Config.SPCACCOUNT_PHONE,
                    Config.SPCACCOUNT_PRICELIST,
                    Config.SPCACCOUNT_REPORTTYPE,
                    Config.SPCACCOUNT_CATEGORY,
                    Config.SPCACCOUNT_EXEMPT
                })
            });
            queryIncident.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCASE,
                LinkToEntityName = Config.SPCCURRENCY,
                LinkFromAttributeName = Config.SPCCASE_CURRENCY,
                LinkToAttributeName = Config.SPCCURRENCY_ID,
                Columns = new ColumnSet(new string[] { Config.SPCCURRENCY_SYMBOL }),
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameCurrency
            });
            queryIncident.Criteria.AddCondition(new ConditionExpression(Config.SPCCASE_ID, ConditionOperator.Equal, guid));
            EntityCollection result = await Proxy.RetrieveMultiple(queryIncident);
            #endregion
            Incident inc = new Incident();
            if (result.Entities.Count > 0)
            {
                vm.ProgressUp(Config.MSG_LOAD_CLIENT);
                Entity incident = result.Entities[0];
                #region Client
                Client client = new Client
                {
                    //Cdts = new List<CDT>()
                };
                #region General Info
                if (incident.Contains(Config.SPCCASE_CLIENT))
                    client.InternalId = ((EntityReference)incident[Config.SPCCASE_CLIENT]).Id;
                string concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ADDRESS);
                if (incident.Contains(concatName))
                    client.Address = (string)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS);
                if (incident.Contains(concatName))
                    client.Alias = (string)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_COUNTRY);
                if (incident.Contains(concatName))
                    client.Country = ((EntityReference)((AliasedValue)incident[concatName]).Value).Name;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_EMAIL);
                if (incident.Contains(concatName))
                    client.Email = (string)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_PHONE);
                if (incident.Contains(concatName))
                    client.Phone = (string)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME);
                if (incident.Contains(concatName))
                    client.Name = (string)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_EXEMPT);
                if (incident.Contains(concatName))
                    client.DoesPayTaxes = !(bool)((AliasedValue)incident[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_REPORTTYPE);
                if (incident.Contains(concatName))
                    client.ReportType = (Types.SPCCLIENT_REPORTTYPEOPTION)((OptionSetValue)((AliasedValue)incident[concatName]).Value).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_PRICELIST);
                if (incident.Contains(concatName))
                    client.PriceList = ((EntityReference)((AliasedValue)incident[concatName]).Value).Id;
                #endregion
                #region Coordinates
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_LOCATION);
                if (incident.Contains(concatName))
                {
                    try
                    {
                        string[] precoordinates = ((string)((AliasedValue)incident[concatName]).Value).Split(',');
                        client.Coordinates = new Coord { Latitude = Double.Parse(precoordinates[0]), Longitude = Double.Parse(precoordinates[1]) };
                    }
                    catch (Exception)
                    {
                        throw new Exception("Coordenadas del cliente con formato inválido");
                    }
                }
                #endregion
                #region Category
                vm.ProgressUp(Config.MSG_LOAD_CATEGORY);
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_CATEGORY);
                if (incident.Contains(concatName))
                {
                    client.Category = await GetCategory(((EntityReference)((AliasedValue)incident[concatName]).Value).Id);
                    client.CategoryId = client.Category.SQLiteRecordId;
                }
                #endregion
                #region Obtain Client SQLite foreign keys & update obtained Client
                Client localClient = (await conn.GetAllWithChildrenAsync<Client>(lc => lc.InternalId.Equals(client.InternalId))).FirstOrDefault();
                if (localClient != null)
                {
                    client.SQLiteRecordId = localClient.SQLiteRecordId;
                    if (client.Coordinates != null)
                    {
                        client.Coordinates.Id = localClient.CoordinatesId;
                        client.CoordinatesId = localClient.CoordinatesId;
                    }
                    //client.Incidents = localClient.Incidents;
                }
                await conn.InsertOrReplaceWithChildrenAsync(client, true);
                #endregion
                #endregion
                #region Currency
                vm.ProgressUp(Config.MSG_LOAD_CURRENCY);
                concatName = string.Format("{0}.{1}", NameCurrency, Config.SPCCURRENCY_SYMBOL);
                if (incident.Contains(Config.SPCCASE_CURRENCY))
                    inc.MoneyCurrency = await GetCurrency(((EntityReference)incident[Config.SPCCASE_CURRENCY]).Id);
                if (inc.MoneyCurrency != null)
                    inc.MoneyCurrencyId = inc.MoneyCurrency.SQLiteRecordId;
                #endregion
                #region Subtype
                vm.ProgressUp(Config.MSG_LOAD_SYSTEM);
                if (incident.Contains(Config.SPCCASE_SYSTEM))
                    inc.Type = await GetSubtype(((EntityReference)incident[Config.SPCCASE_SYSTEM]).Id);
                if (inc.Type != null)
                    inc.TypeId = inc.Type.SQLiteRecordId;
                #endregion
                #region Technicians
                vm.ProgressUp(Config.MSG_LOAD_TECHNICIANS);
                inc.TechniciansAssigned = new List<Technician>();
                if (incident.Contains(Config.SPCCASE_TECHNICIAN1))
                {
                    Technician tech1 = await GetTechnician(((EntityReference)incident[Config.SPCCASE_TECHNICIAN1]).Id);
                    if (tech1 != null)
                        inc.TechniciansAssigned.Add(tech1);
                    else
                        throw new Exception(Config.MSG_ERR_LOAD_TECH1_PROG);
                }
                if (incident.Contains(Config.SPCCASE_TECHNICIAN2))
                {
                    Technician tech2 = await GetTechnician(((EntityReference)incident[Config.SPCCASE_TECHNICIAN2]).Id);
                    if (tech2 != null)
                        inc.TechniciansAssigned.Add(tech2);
                    else
                        throw new Exception(Config.MSG_ERR_LOAD_TECH2_PROG);
                }
                if (incident.Contains(Config.SPCCASE_TECHNICIAN3))
                {
                    Technician tech3 = await GetTechnician(((EntityReference)incident[Config.SPCCASE_TECHNICIAN3]).Id);
                    if (tech3 != null)
                        inc.TechniciansAssigned.Add(tech3);
                    else
                        throw new Exception(Config.MSG_ERR_LOAD_TECH3_PROG);
                }
                #endregion
                #region Asign Client To Incident and set relationship
                inc.Client = client;
                if (inc.Client != null)
                    inc.ClientId = inc.Client.SQLiteRecordId;
                inc.InternalId = incident.Id;
                #endregion
                #region Incident General Information
                inc.CreatedOn = (DateTime)incident[Config.GENERAL_CREATEDON];
                if (incident.Contains(Config.SPCCASE_DESCRIPTION))
                    inc.Description = (string)incident[Config.SPCCASE_DESCRIPTION];
                if (incident.Contains(Config.SPCCASE_TITLE))
                    inc.Title = (string)incident[Config.SPCCASE_TITLE];
                if (incident.Contains(Config.SPCCASE_INCIDENCE))
                    inc.Incidence = (string)incident[Config.SPCCASE_INCIDENCE];
                if (incident.Contains(Config.SPCCASE_REVIEWED))
                    inc.Reviewed = (bool)incident[Config.SPCCASE_REVIEWED];
                if (incident.Contains(Config.SPCCASE_REPRESENTATIVE))
                    inc.Representative = ((EntityReference)incident[Config.SPCCASE_REPRESENTATIVE]).Name;
                if (incident.Contains(Config.SPCCASE_CASENUMBER))
                    inc.TicketNumber = (string)incident[Config.SPCCASE_CASENUMBER];
                if (incident.Contains(Config.SPCCASE_PROGRAMMED))
                    inc.ProgrammedDate = (DateTime)incident[Config.SPCCASE_PROGRAMMED];
                if (incident.Contains(Config.SPCCASE_CONTROL))
                    inc.ControlOption = (Types.SPCINCIDENT_CONTROLOPTION)((OptionSetValue)incident[Config.SPCCASE_CONTROL]).Value;
                if (incident.Contains(Config.SPCCASE_PAYMENT))
                    inc.PaymentOption = (Types.SPCINCIDENT_PAYMENTOPTION)((OptionSetValue)incident[Config.SPCCASE_PAYMENT]).Value;
                #endregion
                #region Load Service Tickets
                vm.ProgressUp(Config.MSG_LOAD_SERVTICKETS);
                try
                {
                    inc.ServiceTickets = await GetXRServiceTickets(inc.TicketNumber);
                }
                catch (System.Xml.XmlException)
                {
                    inc.ServiceTickets = await GetXRServiceTickets(inc.TicketNumber);
                }
                #endregion
                #region Detect if incident is updating or inserting and doing so
                Incident local = (await conn.GetAllWithChildrenAsync<Incident>(e => e.InternalId.Equals(inc.InternalId), true)).FirstOrDefault();
                if (local != null)
                {
                    inc.SQLiteRecordId = local.SQLiteRecordId;
                    List<ServiceTicket> offlineServiceTickets = await conn.GetAllWithChildrenAsync<ServiceTicket>(offst => offst.InternalId.Equals(default(Guid)) && offst.IncidentId == inc.SQLiteRecordId);
                    inc.ServiceTickets.AddRange(offlineServiceTickets);
                }
                await conn.InsertOrReplaceWithChildrenAsync(inc, true);
                #endregion
                vm.ProgressUp(Config.MSG_LOAD_SUCCESS);
                vm.IsLoading = false;
            }
            return inc;
        }

        /// <summary>
        /// Gets a CDT model object from the CRM.
        /// </summary>
        /// <param name="guid">Id of the cdt to be retrieved.</param>
        /// <returns>A CDT model object</returns>
        public static async Task<CDT> GetCDT(Guid guid)
        {
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), Config.MSG_TITLE_LOAD_CDT, Config.MSG_LOAD_CDT, 0, 9);
            PageService ps = new PageService();
            await ps.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            var conn = await Local.GetConnection();
            #region Query
            string NameClient = "Client";
            #region Creating the query
            QueryExpression queryCDT = new QueryExpression(Config.SPCCDT)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.GENERAL_CREATEDON,
                    Config.SPCCDT_DESCRIPTION,
                    Config.SPCCDT_FINALCLIENT,
                    Config.SPCCDT_ID,
                    Config.SPCCDT_ISAPPROVED,
                    Config.SPCCDT_ISAPPROVEDADMINISTRATION,
                    Config.SPCCDT_ISAPPROVEDCOMERCIAL,
                    Config.SPCCDT_ISAPPROVEDCUSTOMERSERVICE,
                    Config.SPCCDT_ISAPPROVEDFINANCIAL,
                    Config.SPCCDT_ISAPPROVEDINSTALLATION,
                    Config.SPCCDT_ISAPPROVEDOPERATIONS,
                    Config.SPCCDT_ISAPPROVEDPLANNING,
                    Config.SPCCDT_APPROVEDADMINISTRATIONBY,
                    Config.SPCCDT_APPROVEDCOMERCIALBY,
                    Config.SPCCDT_APPROVEDCUSTOMERSERVICEBY,
                    Config.SPCCDT_APPROVEDFINANCIALBY,
                    Config.SPCCDT_APPROVEDINSTALLATIONBY,
                    Config.SPCCDT_APPROVEDOPERATIONSBY,
                    Config.SPCCDT_APPROVEDPLANNINGBY,
                    Config.SPCCDT_MAINCONTACT,
                    Config.SPCCDT_MAINCONTACTEMAIL,
                    Config.SPCCDT_MAINCONTACTPHONE,
                    Config.SPCCDT_MONITORACCOUNTNAME,
                    Config.SPCCDT_MONITORACCOUNTNUMBER,
                    Config.SPCCDT_NUMBER,
                    Config.SPCCDT_PROJECTDEADLINE,
                    Config.SPCCDT_PROJECTSTARTDATE,
                    Config.SPCCDT_PROJECTSTATE,
                    Config.SPCCDT_SECONDARYCONTACT,
                    Config.SPCCDT_SECONDARYCONTACTEMAIL,
                    Config.SPCCDT_SECONDARYCONTACTPHONE,
                    Config.SPCCDT_SYSTEM
            }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };
            #endregion
            #region Add Client Join
            queryCDT.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCDT,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCDT_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCACCOUNT_ID,
                    Config.SPCACCOUNT_ALIAS,
                    Config.SPCACCOUNT_ADDRESS,
                    Config.SPCACCOUNT_COUNTRY,
                    Config.SPCACCOUNT_EMAIL,
                    Config.SPCACCOUNT_LOCATION,
                    Config.SPCACCOUNT_NAME,
                    Config.SPCACCOUNT_NUMBER,
                    Config.SPCACCOUNT_PHONE,
                    Config.SPCACCOUNT_PRICELIST,
                    Config.SPCACCOUNT_REPORTTYPE,
                    Config.SPCACCOUNT_CATEGORY
                })
            });
            #endregion
            queryCDT.Criteria.AddCondition(new ConditionExpression(Config.SPCCDT_ID, ConditionOperator.Equal, guid));
            EntityCollection result = await Proxy.RetrieveMultiple(queryCDT);
            #endregion
            CDT cdt = new CDT();
            if (result.Entities.Count > 0)
            {
                vm.ProgressUp(Config.MSG_LOAD_CLIENT);
                Entity cdtentity = result.Entities[0];
                #region Client
                Client client = new Client();
                #region General Info
                string concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ID);
                if (cdtentity.Contains(concatName))
                    client.InternalId = (Guid)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ADDRESS);
                if (cdtentity.Contains(concatName))
                    client.Address = (string)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS);
                if (cdtentity.Contains(concatName))
                    client.Alias = (string)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_COUNTRY);
                if (cdtentity.Contains(concatName))
                    client.Country = ((EntityReference)((AliasedValue)cdtentity[concatName]).Value).Name;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_EMAIL);
                if (cdtentity.Contains(concatName))
                    client.Email = (string)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_PHONE);
                if (cdtentity.Contains(concatName))
                    client.Phone = (string)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME);
                if (cdtentity.Contains(concatName))
                    client.Name = (string)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_EXEMPT);
                if (cdtentity.Contains(concatName))
                    client.DoesPayTaxes = !(bool)((AliasedValue)cdtentity[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_REPORTTYPE);
                if (cdtentity.Contains(concatName))
                    client.ReportType = (Types.SPCCLIENT_REPORTTYPEOPTION)((OptionSetValue)((AliasedValue)cdtentity[concatName]).Value).Value;
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_PRICELIST);
                if (cdtentity.Contains(concatName))
                    client.PriceList = ((EntityReference)((AliasedValue)cdtentity[concatName]).Value).Id;
                #endregion
                #region Coordinates
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_LOCATION);
                if (cdtentity.Contains(concatName))
                {
                    try
                    {
                        string[] precoordinates = ((string)((AliasedValue)cdtentity[concatName]).Value).Split(',');
                        client.Coordinates = new Coord { Latitude = Double.Parse(precoordinates[0]), Longitude = Double.Parse(precoordinates[1]) };
                    }
                    catch (Exception)
                    {
                        throw new Exception("Coordenadas del cliente con formato inválido");
                    }
                }
                #endregion
                #region Category
                vm.ProgressUp(Config.MSG_LOAD_CATEGORY);
                concatName = string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_CATEGORY);
                if (cdtentity.Contains(concatName))
                {
                    client.Category = await GetCategory(((EntityReference)((AliasedValue)cdtentity[concatName]).Value).Id);
                    client.CategoryId = client.Category.SQLiteRecordId;
                }
                #endregion
                #region Obtain Client SQLite foreign keys & update obtained Client
                Client localClient = await conn.Table<Client>().Where(lc => lc.InternalId.Equals(client.InternalId)).FirstOrDefaultAsync();
                if (localClient != null)
                {
                    localClient = await conn.GetWithChildrenAsync<Client>(localClient.SQLiteRecordId, true);
                    client.SQLiteRecordId = localClient.SQLiteRecordId;
                    if (client.Coordinates != null)
                    {
                        client.Coordinates.Id = localClient.CoordinatesId;
                        client.CoordinatesId = localClient.CoordinatesId;
                    }
                    if (client.Category != null)
                    {
                        client.Category.SQLiteRecordId = localClient.CategoryId;
                        client.CategoryId = localClient.CategoryId;
                    }
                }
                await conn.InsertOrReplaceWithChildrenAsync(client, true);
                #endregion
                #endregion
                #region Subtype
                vm.ProgressUp(Config.MSG_LOAD_SYSTEM);
                if (cdtentity.Contains(Config.SPCCDT_SYSTEM))
                    cdt.System = await GetSubtype(((EntityReference)cdtentity[Config.SPCCDT_SYSTEM]).Id);
                if (cdt.System != null)
                    cdt.SystemId = cdt.System.SQLiteRecordId;
                #endregion
                #region Asign Client To Incident and set relationship
                cdt.Client = client;
                if (cdt.Client != null)
                    cdt.ClientId = cdt.Client.SQLiteRecordId;
                cdt.InternalId = cdtentity.Id;
                #endregion
                #region CDT General Information
                cdt.CreatedOn = (DateTime)cdtentity[Config.GENERAL_CREATEDON];
                if (cdtentity.Contains(Config.SPCCDT_DESCRIPTION))
                    cdt.Description = (string)cdtentity[Config.SPCCDT_DESCRIPTION];
                if (cdtentity.Contains(Config.SPCCDT_FINALCLIENT))
                    cdt.IsFinalClient = ((OptionSetValue)cdtentity[Config.SPCCDT_FINALCLIENT]).Value == 100000000;
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVED))
                    cdt.IsApproved = (bool)cdtentity[Config.SPCCDT_ISAPPROVED];
                #region Load Approvers
                vm.ProgressUp(Config.MSG_LOAD_APPROVERS);
                await LoadSystemUsers();
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDADMINISTRATIONBY))
                {
                    cdt.ApproverAdministration = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDADMINISTRATIONBY]).Id);
                    cdt.ApproverAdministrationId = cdt.ApproverAdministration.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDCOMERCIALBY))
                {
                    cdt.ApproverComercial = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDCOMERCIALBY]).Id);
                    cdt.ApproverComercialId = cdt.ApproverComercial.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDCUSTOMERSERVICEBY))
                {
                    cdt.ApproverCustomerService = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDCUSTOMERSERVICEBY]).Id);
                    cdt.ApproverCustomerServiceId = cdt.ApproverCustomerService.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDFINANCIALBY))
                {
                    cdt.ApproverFinancial = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDFINANCIALBY]).Id);
                    cdt.ApproverFinancialId = cdt.ApproverFinancial.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDINSTALLATIONBY))
                {
                    cdt.ApproverInstallation = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDINSTALLATIONBY]).Id);
                    cdt.ApproverInstallationId = cdt.ApproverInstallation.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDADMINISTRATIONBY))
                {
                    cdt.ApproverAdministration = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDADMINISTRATIONBY]).Id);
                    cdt.ApproverAdministrationId = cdt.ApproverAdministration.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDPLANNINGBY))
                {
                    cdt.ApproverPlanning = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDPLANNINGBY]).Id);
                    cdt.ApproverPlanningId = cdt.ApproverPlanning.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_APPROVEDOPERATIONSBY))
                {
                    cdt.ApproverOperations = await GetSystemUser(((EntityReference)cdtentity[Config.SPCCDT_APPROVEDOPERATIONSBY]).Id);
                    cdt.ApproverOperationsId = cdt.ApproverOperations.SQLiteRecordId;
                }
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDADMINISTRATION))
                    cdt.IsApprovedAdministration = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDADMINISTRATION];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDCOMERCIAL))
                    cdt.IsApprovedComercial = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDCOMERCIAL];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDCUSTOMERSERVICE))
                    cdt.IsApprovedCustomerService = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDCUSTOMERSERVICE];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDFINANCIAL))
                    cdt.IsApprovedFinancial = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDFINANCIAL];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDINSTALLATION))
                    cdt.IsApprovedInstallation = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDINSTALLATION];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDOPERATIONS))
                    cdt.IsApprovedOperations = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDOPERATIONS];
                if (cdtentity.Contains(Config.SPCCDT_ISAPPROVEDPLANNING))
                    cdt.IsApprovedPlanning = (bool)cdtentity[Config.SPCCDT_ISAPPROVEDPLANNING];
                #endregion
                #region Main Contact Information
                if (cdtentity.Contains(Config.SPCCDT_MAINCONTACT))
                    cdt.MainContact = (string)cdtentity[Config.SPCCDT_MAINCONTACT];
                if (cdtentity.Contains(Config.SPCCDT_MAINCONTACTEMAIL))
                    cdt.MainContactEmail = (string)cdtentity[Config.SPCCDT_MAINCONTACTEMAIL];
                if (cdtentity.Contains(Config.SPCCDT_MAINCONTACTPHONE))
                    cdt.MainContactPhone = (string)cdtentity[Config.SPCCDT_MAINCONTACTPHONE];
                #endregion
                #region Secondary Contact
                if (cdtentity.Contains(Config.SPCCDT_SECONDARYCONTACT))
                    cdt.SecondaryContact = (string)cdtentity[Config.SPCCDT_SECONDARYCONTACT];
                if (cdtentity.Contains(Config.SPCCDT_SECONDARYCONTACTEMAIL))
                    cdt.SecondaryContactEmail = (string)cdtentity[Config.SPCCDT_SECONDARYCONTACTEMAIL];
                if (cdtentity.Contains(Config.SPCCDT_SECONDARYCONTACTPHONE))
                    cdt.SecondaryContactPhone = (string)cdtentity[Config.SPCCDT_SECONDARYCONTACTPHONE];
                #endregion
                if (cdtentity.Contains(Config.SPCCDT_MONITORACCOUNTNAME))
                    cdt.MonitoringAccountName = (string)cdtentity[Config.SPCCDT_MONITORACCOUNTNAME];
                if (cdtentity.Contains(Config.SPCCDT_MONITORACCOUNTNUMBER))
                    cdt.MonitoringAccountNumber = (string)cdtentity[Config.SPCCDT_MONITORACCOUNTNUMBER];
                if (cdtentity.Contains(Config.SPCCDT_NUMBER))
                    cdt.Number = cdtentity[Config.SPCCDT_NUMBER].ToString();
                if (cdtentity.Contains(Config.SPCCDT_PROJECTDEADLINE))
                    cdt.ProjectClientDeadline = (DateTime)cdtentity[Config.SPCCDT_PROJECTDEADLINE];
                if (cdtentity.Contains(Config.SPCCDT_PROJECTSTARTDATE))
                    cdt.ProjectStartDate = (DateTime)cdtentity[Config.SPCCDT_PROJECTSTARTDATE];
                if (cdtentity.Contains(Config.SPCCDT_PROJECTSTATE))
                    cdt.ProjectState = (Types.SPCCDT_PROJECTSTATE)((OptionSetValue)cdtentity[Config.SPCCDT_PROJECTSTATE]).Value;
                #endregion
                #region Obtain CDT Project Equipment
                vm.ProgressUp(Config.MSG_LOAD_PROJEQUIP);
                cdt.ProjectEquipment = (await GetAllProjectEquipmentFromCDT(cdt)).OrderByDescending(equip => equip.Remaining).ToList();
                #endregion
                #region Obtain CDT Project Materials
                vm.ProgressUp(Config.MSG_LOAD_PROJMATS);
                cdt.ProjectMaterials = (await GetAllProjectMaterialsFromCDT(cdt)).OrderByDescending(mat => mat.Remaining).ToList();
                #endregion
                #region Obtain CDT Equipment Request Orders
                vm.ProgressUp(Config.MSG_LOAD_PROJEQUIPREQS);
                cdt.EquipmentRequestedOrders = await GetAllEquipmentRequestsOrdersFromCDT(cdt);
                #endregion
                #region Obtain CDT Material Request Orders
                vm.ProgressUp(Config.MSG_LOAD_PROJMATSREQS);
                cdt.MaterialRequestedOrders = await GetAllMaterialRequestsOrdersFromCDT(cdt);
                #endregion
                #region Obtain CDT Extra Equipment Orders
                vm.ProgressUp("Obteniendo equipos adicionales");
                cdt.ExtraEquipment = await GetExtraEquipmentFromCDT(cdt.InternalId);
                #endregion   
                #region Detect if cdt is updating or inserting and doing so
                CDT local = (await conn.GetAllWithChildrenAsync<CDT>(e => e.InternalId == cdt.InternalId, true)).FirstOrDefault();
                if (local != null)
                    cdt.SQLiteRecordId = local.SQLiteRecordId;
                #endregion
                #region Obtain CDT Tickets
                cdt.CDTTickets = await GetAllTicketsForCDT(cdt);
                #endregion
                await conn.InsertOrReplaceWithChildrenAsync(cdt, true);
                vm.ProgressUp(Config.MSG_LOAD_SUCCESS);
                vm.IsLoading = false;
            }
            return cdt;
        }

        /// <summary>
        /// Gets all visit tickets related to a CDT from the CRM.
        /// </summary>
        /// <param name="cdt">Cdt of which visit tickets are going to be obtained.</param>
        /// <returns>A collection of visit tickets for a CDT</returns>
        private static async Task<List<CDTTicket>> GetAllTicketsForCDT(CDT cdt)
        {
            var conn = await Local.GetConnection();
            List<CDTTicket> tickets = new List<CDTTicket>();
            #region Query
            QueryExpression queryTicketsOfCDT = new QueryExpression(Config.SPCCDTTICKET)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.SPCCDTTICKET_FINISHED,
                    Config.SPCCDTTICKET_HADLUNCH,
                    Config.SPCCDTTICKET_NUMBER,
                    Config.SPCCDTTICKET_STARTED,
                    Config.SPCCDTTICKET_WORKDONE,
                    Config.SPCCDTTICKET_AGREEMENTS,
                    Config.SPCCDTTICKET_EMAIL
            }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or
                }
            };
            queryTicketsOfCDT.Criteria.AddCondition(new ConditionExpression(Config.SPCCDTTICKET_CDT, ConditionOperator.Equal, cdt.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryTicketsOfCDT);
            #endregion
            foreach (Entity e in result.Entities)
            {
                CDTTicket ticket = BuildCDTTicketFromEntity(e);
                CDTTicket local = (await conn.GetAllWithChildrenAsync<CDTTicket>(t => t.InternalId == ticket.InternalId)).FirstOrDefault();
                if (local != null)
                    ticket.SQLiteRecordId = local.SQLiteRecordId;
                ticket.CDTId = cdt.SQLiteRecordId;
                #region Obtain Technicians
                ticket.TechniciansRegistered = await GetTechniciansRegistryForCDTTicket(ticket);
                #endregion
                tickets.Add(ticket);
            }
            return tickets;
        }

        /// <summary>
        /// Gets all holydays registered in the CRM as a collection
        /// </summary>
        /// <returns>A collection of dates which are the holydays stored in the CRM</returns>
        public static async Task<List<DateTime>> GetAllHolydays()
        {
            var conn = await Local.GetConnection();
            List<DateTime> holydays = new List<DateTime>();
            #region Query
            QueryExpression query = new QueryExpression(Config.SPCHOLYDAY)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or
                }
            };
            query.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, "Test"));
            //query.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.Equal, 0));
            EntityCollection result = await Proxy.RetrieveMultiple(query);
            Entity calendar = result[0];
            #endregion
            foreach (Entity e in ((EntityCollection)calendar["calendarrules"]).Entities)
                holydays.Add((DateTime)e["starttime"]);
            return holydays;
        }

        /// <summary>
        /// Gets all the technician registries related to a visit ticket of a CDT 
        /// </summary>
        /// <param name="ticket">ticket of which technician registries are going to be obtained.</param>
        /// <returns>A collection of technician registries related to a CDT Ticket</returns>
        public static async Task<List<TechnicianRegistry>> GetTechniciansRegistryForCDTTicket(CDTTicket ticket)
        {
            List<TechnicianRegistry> registries = new List<TechnicianRegistry>();
            #region Query
            QueryExpression query = new QueryExpression(Config.SPCTECHREGISTRY)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.SPCTECHREGISTRY_DAYTIME_EXTRA,
                    Config.SPCTECHREGISTRY_DAYTIME_OFFDAY,
                    Config.SPCTECHREGISTRY_DAYTIME_OFFDAY_EXTRA,
                    Config.SPCTECHREGISTRY_FINISHED,
                    Config.SPCTECHREGISTRY_HOLYDAY_DAYTIME,
                    Config.SPCTECHREGISTRY_HOLYDAY_NIGHT,
                    Config.SPCTECHREGISTRY_HOURS_NORMAL,
                    Config.SPCTECHREGISTRY_HOURS_NORMAL_NIGHT,
                    Config.SPCTECHREGISTRY_NIGHT_EXTRA,
                    Config.SPCTECHREGISTRY_NIGHT_OFFDAY,
                    Config.SPCTECHREGISTRY_NIGHT_OFFDAY_EXTRA,
                    Config.SPCTECHREGISTRY_STARTED,
                    Config.SPCTECHREGISTRY_TECH,
                    Config.SPCTECHREGISTRY_ISDATETIMESET
            }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(Config.SPCTECHREGISTRY_TICKET, ConditionOperator.Equal, ticket.InternalId),
                        new ConditionExpression(Config.GENERAL_STATECODE, ConditionOperator.Equal, 0)
                    }
                }
            };
            #endregion
            EntityCollection result = await Proxy.RetrieveMultiple(query);
            var conn = await Local.GetConnection();
            foreach (Entity e in result.Entities)
            {
                TechnicianRegistry registry = BuildTechnicianRegistryFromEntity(e);
                if (e.Contains(Config.SPCTECHREGISTRY_TECH))
                {
                    registry.Technician = await GetTechnician(((EntityReference)e[Config.SPCTECHREGISTRY_TECH]).Id);
                    registry.TechnicianId = registry.Technician.SQLiteRecordId;
                }
                TechnicianRegistry local = (await conn.GetAllWithChildrenAsync<TechnicianRegistry>(r => r.InternalId == registry.InternalId)).FirstOrDefault();
                if (local != null)
                {
                    registry.SQLiteRecordId = local.SQLiteRecordId;
                    registry.CDTTicketId = local.CDTTicketId;
                }
                registries.Add(registry);
            }
            return registries;
        }

        /// <summary>
        /// Converts a CDT Ticket from an entity obtained of CRM.
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns> A CDT ticket result of the conversion</returns>
        private static CDTTicket BuildCDTTicketFromEntity(Entity entity) =>
            new CDTTicket
            {
                InternalId = entity.Id,
                Number = entity.Contains(Config.SPCCDTTICKET_NUMBER) ? (string)entity[Config.SPCCDTTICKET_NUMBER] : string.Empty,
                Workdone = entity.Contains(Config.SPCCDTTICKET_WORKDONE) ? (string)entity[Config.SPCCDTTICKET_WORKDONE] : string.Empty,
                Email = entity.Contains(Config.SPCCDTTICKET_EMAIL) ? (string)entity[Config.SPCCDTTICKET_EMAIL] : string.Empty,
                Finished = entity.Contains(Config.SPCCDTTICKET_FINISHED) ? (DateTime)entity[Config.SPCCDTTICKET_FINISHED] : default(DateTime),
                Started = entity.Contains(Config.SPCCDTTICKET_STARTED) ? (DateTime)entity[Config.SPCCDTTICKET_STARTED] : default(DateTime),
                HadLunch = entity.Contains(Config.SPCCDTTICKET_HADLUNCH) ? (bool)entity[Config.SPCCDTTICKET_HADLUNCH] : false,
                Agreements = entity.Contains(Config.SPCCDTTICKET_AGREEMENTS) ? (string)entity[Config.SPCCDTTICKET_AGREEMENTS] : string.Empty
            };

        /// <summary>
        /// Converts an Entity to a Technician Registry
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns>A Technician Registry result of the conversion.</returns>
        private static TechnicianRegistry BuildTechnicianRegistryFromEntity(Entity entity) =>
            new TechnicianRegistry
            {
                InternalId = entity.Id,
                HoursDaytimeExtra = entity.Contains(Config.SPCTECHREGISTRY_DAYTIME_EXTRA) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_DAYTIME_EXTRA] : 0d,
                HoursHolydayDaytime = entity.Contains(Config.SPCTECHREGISTRY_HOLYDAY_DAYTIME) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_HOLYDAY_DAYTIME] : 0d,
                HoursHolydayNight = entity.Contains(Config.SPCTECHREGISTRY_HOLYDAY_NIGHT) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_HOLYDAY_NIGHT] : 0d,
                HoursNightExtra = entity.Contains(Config.SPCTECHREGISTRY_NIGHT_EXTRA) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_NIGHT_EXTRA] : 0d,
                HoursNormal = entity.Contains(Config.SPCTECHREGISTRY_HOURS_NORMAL) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_HOURS_NORMAL] : 0d,
                HoursNormalNight = entity.Contains(Config.SPCTECHREGISTRY_HOURS_NORMAL_NIGHT) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_HOURS_NORMAL_NIGHT] : 0d,
                HoursOffdayDaytime = entity.Contains(Config.SPCTECHREGISTRY_DAYTIME_OFFDAY) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_DAYTIME_OFFDAY] : 0d,
                HoursOffdayDaytimeExtra = entity.Contains(Config.SPCTECHREGISTRY_DAYTIME_OFFDAY_EXTRA) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_DAYTIME_OFFDAY_EXTRA] : 0d,
                HoursOffdayNight = entity.Contains(Config.SPCTECHREGISTRY_NIGHT_OFFDAY) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_NIGHT_OFFDAY] : 0d,
                HoursOffdayNightExtra = entity.Contains(Config.SPCTECHREGISTRY_NIGHT_OFFDAY_EXTRA) ? (double)(decimal)entity[Config.SPCTECHREGISTRY_NIGHT_OFFDAY_EXTRA] : 0d,
                Started = entity.Contains(Config.SPCTECHREGISTRY_STARTED) ? (DateTime)entity[Config.SPCTECHREGISTRY_STARTED] : default(DateTime),
                Finished = entity.Contains(Config.SPCTECHREGISTRY_FINISHED) ? (DateTime)entity[Config.SPCTECHREGISTRY_FINISHED] : default(DateTime),
                IsDatetimeSet = entity.Contains(Config.SPCTECHREGISTRY_ISDATETIMESET) ? (bool)entity[Config.SPCTECHREGISTRY_ISDATETIMESET] : false
            };

        /// <summary>
        /// Gets all systemusers from CRM
        /// </summary>
        /// <returns>Void</returns>
        private static async Task LoadSystemUsers()
        {
            var conn = await Local.GetConnection();
            LogTable lastupdate = await conn.FindAsync<LogTable>(nameof(SystemUser));
            if (lastupdate == null || lastupdate.LastTimeUpdate.AddDays(2) < DateTime.Now)
            {
                QueryExpression query = new QueryExpression(Config.SYSUSER, new ColumnSet(new string[] {
                Config.SYSUSER_FULLNAME,
                Config.SYSUSER_ID,
            }));
                query.Criteria.AddCondition(new ConditionExpression("isdisabled", ConditionOperator.Equal, false));
                EntityCollection queryresult = await Proxy.RetrieveMultiple(query);
                foreach (Entity user in queryresult.Entities)
                {
                    SystemUser sysuser = (await conn.GetAllWithChildrenAsync<SystemUser>(u => u.InternalId.Equals(user.Id))).FirstOrDefault();
                    if (sysuser == null)
                        sysuser = new SystemUser
                        {
                            InternalId = user.Id,
                            Name = user.Contains(Config.SYSUSER_FULLNAME) ? (string)user[Config.SYSUSER_FULLNAME] : string.Empty
                        };
                    else
                        sysuser.Name = user.Contains(Config.SYSUSER_FULLNAME) ? (string)user[Config.SYSUSER_FULLNAME] : string.Empty;
                    await conn.InsertOrReplaceWithChildrenAsync(sysuser);
                }
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(SystemUser), LastTimeUpdate = DateTime.Now });
            }
        }

        /// <summary>
        /// Gets an specific system user
        /// </summary>
        /// <param name="id">Id of the system user to be retrieved.</param>
        /// <returns></returns>
        public static async Task<SystemUser> GetSystemUser(Guid id)
        {
            var conn = await Local.GetConnection();
            await LoadSystemUsers();
            return (await conn.GetAllWithChildrenAsync<SystemUser>(u => u.InternalId.Equals(id), true)).FirstOrDefault();
        }

        /// <summary>
        /// Gets an specific CDT from the local storage of the phone
        /// </summary>
        /// <param name="guid">Id of the CDT to be retrieved</param>
        /// <returns>A CDT model object</returns>
        public static async Task<CDT> GetLocalCDT(Guid guid)
        {
            var conn = await Local.GetConnection();
            return (await conn.GetAllWithChildrenAsync<CDT>(cdt => cdt.InternalId.Equals(guid), true)).FirstOrDefault();
        }

        /// <summary>
        /// Gets an specific Incident from the local storage
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>An Incident obtained from local storage</returns>
        public static async Task<Incident> GetLocalIncident(Guid guid)
        {
            var conn = await Local.GetConnection();
            return (await conn.GetAllWithChildrenAsync<Incident>(inc => inc.InternalId.Equals(guid), true)).FirstOrDefault();
        }

        /// <summary>
        /// Finds all Incidents (with partial info) that meet the expression provided.
        /// </summary>
        /// <param name="expression">Expression to be used as filter</param>
        /// <returns>A collection of PARTIAL Incidents</returns>
        public static async Task<List<DTO_IncidentLookUp>> FindIncidents(string expression)
        {
            List<DTO_IncidentLookUp> result = new List<DTO_IncidentLookUp>();
            QueryExpression query = new QueryExpression(Config.SPCCASE);
            string NameClient = "client";
            query.ColumnSet = new ColumnSet(new string[] { Config.SPCCASE_CASENUMBER, Config.GENERAL_CREATEDON, Config.SPCCASE_CLIENT });
            query.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCASE,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCASE_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCACCOUNT_ALIAS, Config.SPCACCOUNT_NAME, Config.SPCACCOUNT_NUMBER
                }),
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or
                }
            });
            if (!expression.StartsWith("*"))
            {
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_ALIAS, ConditionOperator.Like, "%" + expression + "%"));
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_NAME, ConditionOperator.Like, "%" + expression + "%"));
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_NUMBER, ConditionOperator.Like, "%" + expression + "%"));
            }
            else
            {
                query.Criteria = new FilterExpression { FilterOperator = LogicalOperator.Or };
                query.Criteria.AddCondition(new ConditionExpression(Config.SPCCASE_CASENUMBER, ConditionOperator.Like, "%" + expression.Substring(1) + "%"));
            }
            EntityCollection incidents = await Proxy.RetrieveMultiple(query);
            foreach (Entity incident in incidents.Entities)
            {
                Guid internalId = incident.Id;
                string ticketNumber = incident.Contains(Config.SPCCASE_CASENUMBER) ? (string)incident[Config.SPCCASE_CASENUMBER] : "Sin número";
                DateTime creationDate = (DateTime)incident[Config.GENERAL_CREATEDON];
                EntityReference client = (EntityReference)incident[Config.SPCCASE_CLIENT];
                Guid clientid = client.Id;
                string clientName = client.Name;
                string clientAlias = incident.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)) ? (string)((AliasedValue)incident[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)]).Value : String.Empty;
                DTO_IncidentLookUp resultIncident = new DTO_IncidentLookUp()
                {
                    InternalId = internalId,
                    TicketNumber = ticketNumber,
                    CreatedOn = creationDate,
                    Client = new DTO_ClientPartial()
                    {
                        InternalId = clientid,
                        Name = clientName,
                        Alias = clientAlias
                    }
                };
                result.Add(resultIncident);
            }
            return result;
        }

        /// <summary>
        /// Finds all Incidents Viewmodels(PARTIAL) that meet the expression provided
        /// </summary>
        /// <param name="expression">Expression to be used as filter</param>
        /// <returns>A collection of Incidents</returns>
        public static async Task<IEnumerable<IncidentViewModel>> FindIncidentsViewModel(string expression)
        {
            IEnumerable<DTO_IncidentLookUp> incidents = await FindIncidents(expression);
            List<IncidentViewModel> incidentsViewModel = new List<IncidentViewModel>();
            if (incidents != null)
                foreach (DTO_IncidentLookUp incident in incidents)
                    incidentsViewModel.Add(new IncidentViewModel(incident));
            return incidentsViewModel;
        }

        /// <summary>
        /// Gets a list of partial CDTs that meet the expression provided.
        /// </summary>
        /// <param name="expression">Expression to be used as filter</param>
        /// <returns>A collection of CDT (partial) objects</returns>
        public static async Task<List<DTO_CDTLookUp>> FindCDTs(string expression)
        {
            List<DTO_CDTLookUp> result = new List<DTO_CDTLookUp>();
            QueryExpression query = new QueryExpression(Config.SPCCDT)
            {
                ColumnSet = new ColumnSet(new string[] {
                    Config.SPCCDT_NUMBER,
                    Config.GENERAL_CREATEDON
                }),
                Criteria = new FilterExpression()
            };
            string NameClient = "client";
            query.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCDT,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCDT_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
               {
                   Config.SPCACCOUNT_ID,
                    Config.SPCACCOUNT_ALIAS,
                    Config.SPCACCOUNT_NAME
               })
            });
            query.Criteria.FilterOperator = LogicalOperator.And;
            //query.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_CREATEDON, ConditionOperator.LastXMonths, Config.LASTXMONTHS));
            //queryLastXMonths.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            if (!expression.StartsWith("*"))
            {
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_ALIAS, ConditionOperator.Like, "%" + expression + "%"));
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_NAME, ConditionOperator.Like, "%" + expression + "%"));
                query.LinkEntities[0].LinkCriteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_NUMBER, ConditionOperator.Like, "%" + expression + "%"));
            }
            else
            {
                query.Criteria = new FilterExpression { FilterOperator = LogicalOperator.Or };
                query.Criteria.AddCondition(new ConditionExpression(Config.SPCCDT_NUMBER, ConditionOperator.Like, "%" + expression.Substring(1) + "%"));
            }
            EntityCollection cdts = await Proxy.RetrieveMultiple(query);
            foreach (Entity ecdt in cdts.Entities)
            {
                DTO_CDTLookUp cdt = new DTO_CDTLookUp
                {
                    InternalId = ecdt.Id,
                    Number = ecdt.Contains(Config.SPCCDT_NUMBER) ? (string)ecdt[Config.SPCCDT_NUMBER] : "Sin número",
                    Client = new DTO_ClientPartial
                    {
                        InternalId = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.CLIENT_ID)) ? (Guid)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ID)]).Value : default(Guid),
                        Alias = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)) ? (string)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)]).Value : "Sin nombre",
                        Name = ecdt.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME)) ? (string)((AliasedValue)ecdt[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_NAME)]).Value : "Sin nombre",
                    }
                };
                result.Add(cdt);
            }
            return result;
        }

        /// <summary>
        /// Gets a collection of PARTIAL CDTs View Models (used is listings and filters) that contains an expression in their customer alias or CDT number
        /// </summary>
        /// <param name="expression">Expression to be used as filter (non logical)</param>
        /// <returns>A collection of PARTIAL CDTs ViewModels</returns>
        public static async Task<IEnumerable<CDTViewModel>> FindCDTsViewModel(string expression)
        {
            IEnumerable<DTO_CDTLookUp> cdts = await FindCDTs(expression);
            List<CDTViewModel> cdtsViewModel = new List<CDTViewModel>();
            if (cdts != null)
                foreach (DTO_CDTLookUp cdt in cdts)
                    cdtsViewModel.Add(new CDTViewModel(cdt));
            return cdtsViewModel;
        }

        /// <summary>
        /// Creates a new Service ticket in the CRM
        /// </summary>
        /// <param name="incident">Incident that is going to be related to this service ticket (boleta de servicio)</param>
        /// <param name="GpsError">Indicates if a gps error occured prior to create a service ticket so the function does not update client coordinates. Could be considered as a flag</param>
        /// <returns>Service Ticket created by the CRM</returns>
        public static async Task<ServiceTicket> CreateNewServiceTicket(Incident incident, bool GpsError)
        {
            var conn = await Local.GetConnection();
            #region Obtain Lead Technician of Service Ticket
            SystemUser loggeduser = (await conn.GetAllWithChildrenAsync<SystemUser>(lu => lu.InternalId.Equals(Proxy.LoggedUser.InternalId))).FirstOrDefault();
            if (loggeduser == null)
                throw new Exception("Usuario de la sesión no encontrado");
            Technician tech1 = (await conn.GetAllWithChildrenAsync<Technician>(t => t.RelatedUserId == loggeduser.SQLiteRecordId, true)).FirstOrDefault();
            if (tech1 == null)
                throw new Exception("Técnico 1 no encontrado");
            #endregion
            Entity serviceTicket = new Entity(Config.SPCSERVTICKET);
            EntityReference ETechnician1 = new EntityReference(Config.SPCTECHNICIAN, tech1.InternalId);
            serviceTicket[Config.SPCSERVTICKET_TECHNICIAN1] = ETechnician1; //required
            EntityReference ECaso = new EntityReference(Config.SPCCASE, incident.InternalId);
            serviceTicket[Config.SPCSERVTICKET_CASEID] = ECaso;         //required
            if (incident.Client != null)
            {
                EntityReference Eclient = new EntityReference(Config.SPCACCOUNT, incident.Client.InternalId);
                serviceTicket[Config.SPCSERVTICKET_CLIENT] = Eclient;       //required
            }
            if (incident.Type != null)
            {
                EntityReference ESystem = new EntityReference(Config.SPCSYSTEM, incident.Type.InternalId);
                serviceTicket[Config.SPCSERVTICKET_SYSTEM] = ESystem;
            }
            else
                throw new Exception("El caso no tiene asignado el tipo de Sistema a atender. Contacta con TI para corregir la situación.");
            if (incident.MoneyCurrency != null)
            {
                EntityReference ECurrency = new EntityReference(Config.SPCCURRENCY, incident.MoneyCurrency.InternalId);
                serviceTicket[Config.SPCSERVTICKET_CURRENCY] = ECurrency;
            }
            serviceTicket[Config.SPCSERVTICKET_CASENUMBER] = incident.TicketNumber;  //required
            if (!string.IsNullOrEmpty(incident.Title))
                serviceTicket[Config.SPCSERVTICKET_TITLE] = incident.Title;
            if (!string.IsNullOrEmpty(incident.Description))
                serviceTicket[Config.SPCSERVTICKET_DESCRIPTION] = incident.Description;
            serviceTicket[Config.SPCSERVTICKET_STARTED] = DateTime.Now;
            Guid id = await Proxy.Create(serviceTicket);
            serviceTicket = await Proxy.Retrieve(Config.SPCSERVTICKET, id, new ColumnSet(new string[]
            {
                         Config.GENERAL_CREATEDON,
                         Config.SPCSERVTICKET_STARTED,
                         Config.SPCSERVTICKET_NUMBER
            }));
            ServiceTicket result = new ServiceTicket
            {
                CreationDate = (DateTime)serviceTicket[Config.GENERAL_CREATEDON],
                Started = (DateTime)serviceTicket[Config.SPCSERVTICKET_STARTED],
                TicketNumber = ((int)serviceTicket[Config.SPCSERVTICKET_NUMBER]).ToString(),
                Description = incident.Description,
                Title = incident.Title,
                ProductsUsed = new List<MaterialYRepuesto>(),
                InternalId = id,
                MoneyCurrency = incident.MoneyCurrency,
                MoneyCurrencyId = incident.MoneyCurrencyId,
                Type = incident.Type,
                TypeId = incident.TypeId,
                Finished = default(DateTime),
                Technicians = new List<Technician>
                {
                    tech1
                },
                IncidentId = incident.SQLiteRecordId
            };
            incident.ServiceTickets.Add(result);
            if (incident.Client != null && incident.Client.Coordinates != null && !GpsError)
            {
                Entity client = new Entity(Config.SPCACCOUNT, incident.Client.InternalId);
                client[Config.SPCACCOUNT_LOCATION] = String.Format("{0},{1}", incident.Client.Coordinates.Latitude, incident.Client.Coordinates.Longitude);
                await Proxy.Update(client);
            }
            await conn.InsertOrReplaceWithChildrenAsync(incident, true);
            return result;
        }

        /// <summary>
        /// Creates a new Service Ticket in the local storage of the phone.
        /// </summary>
        /// <param name="incident">Incident which is related to the service ticket</param>
        /// <param name="GpsError">Indicates if a gps error occured prior to create a service ticket so the function does not update client coordinates. Could be considered as a flag</param>
        /// <returns></returns>
        public static async Task<ServiceTicket> CreateNewServiceTicketOffline(Incident incident, bool GpsError)
        {
            var conn = await Local.GetConnection();
            #region Obtain Lead Technician of Service Ticket
            SystemUser loggeduser = (await conn.GetAllWithChildrenAsync<SystemUser>(lu => lu.InternalId.Equals(Proxy.LoggedUser.InternalId))).FirstOrDefault();
            if (loggeduser == null)
                throw new Exception("Usuario logueado no debe ser nulo.");
            Technician tech1 = (await conn.GetAllWithChildrenAsync<Technician>(t => t.RelatedUserId == loggeduser.SQLiteRecordId, true)).FirstOrDefault();
            if (tech1 == null)
                throw new Exception("Técnico 1 no debe ser nulo.");
            #endregion
            #region Obtain Incident as Store in localDB
            Incident localIncident = await conn.GetWithChildrenAsync<Incident>(incident.SQLiteRecordId, true);
            #endregion
            #region Create Local Instance of Service Ticket
            ServiceTicket newServiceTicket = new ServiceTicket
            {
                CreationDate = DateTime.Now,
                Started = DateTime.Now,
                TicketNumber = "OFF-" + localIncident.TicketNumber.Substring(3).Remove(5, 7) + "-" + tech1.ProductStorage.Name.Substring(2) + "-" + (localIncident.ServiceTickets.Count + 1),
                IncidentId = localIncident.SQLiteRecordId,
                Description = localIncident.Description,
                Technicians = new List<Technician>
                {
                      tech1
                },
                ProductsUsed = new List<MaterialYRepuesto>(),
                MoneyCurrency = localIncident.MoneyCurrency,
                MoneyCurrencyId = localIncident.MoneyCurrencyId,
                Title = localIncident.Title,
                Type = localIncident.Type,
                TypeId = localIncident.TypeId,
            };
            await conn.InsertOrReplaceWithChildrenAsync(newServiceTicket, true);
            #endregion
            #region Register local creation pending
            await RegisterPendingOperation(Types.CRUDOperation.Create, newServiceTicket.SQLiteRecordId, nameof(ServiceTicket));
            #endregion
            #region Create Local Instance of Client with GPS coords updated
            if (incident.Client != null && incident.Client.Coordinates != null && !GpsError)
            {
                if (localIncident.Client.CoordinatesId != 0)
                {
                    incident.Client.Coordinates.Id = localIncident.Client.CoordinatesId;
                    incident.Client.CoordinatesId = localIncident.Client.CoordinatesId;
                }
                localIncident.Client.Coordinates = incident.Client.Coordinates;
                localIncident.Client.CoordinatesId = incident.Client.CoordinatesId;
                localIncident.ServiceTickets.Add(newServiceTicket);
                await conn.InsertOrReplaceWithChildrenAsync(localIncident, true);
                await RegisterPendingOperation(Types.CRUDOperation.Update, localIncident.Client.Coordinates.Id, nameof(Coord));
            }
            #endregion
            return newServiceTicket;
        }

        /// <summary>
        /// Register there´s a pending sync operation to be performed so it is executed when internet is available again.
        /// </summary>
        /// <param name="action">Type of action to be performed</param>
        /// <param name="objectId">Local database id of the object to be upsert in the CRM</param>
        /// <param name="objectType">Name of entity in CRM of this object</param>
        /// <param name="additionalInfo">Additional info of this object (used in some cases)</param>
        /// <returns></returns>
        public static async Task RegisterPendingOperation(Types.CRUDOperation action, int objectId, string objectType, string additionalInfo = null) =>
            await (await Local.GetConnection()).InsertOrReplaceWithChildrenAsync(new CrudTable
            {
                Action = action,
                OperationDate = DateTime.Now,
                ObjectId = objectId,
                ObjectType = objectType,
                AdditionalInfo = additionalInfo
            });

        /// <summary>
        /// Creates a new visit Ticket (CDT Ticket) in the CRM
        /// </summary>
        /// <param name="cdt">CDT Model object to be created in the CRM</param>
        /// <returns>CDT Ticket model object result of the creation</returns>
        public static async Task<CDTTicket> CreateNewCDTTicket(CDT cdt)
        {
            var conn = await Local.GetConnection();
            Entity ticket = new Entity(Config.SPCCDTTICKET);
            //EntityReference ETechnician1 = new EntityReference(Config.SPCTECHNICIAN, tech1.InternalId);
            //ticket[Config.SPCSERVTICKET_TECHNICIAN1] = ETechnician1; //required
            ticket[Config.SPCCDTTICKET_STARTED] = DateTime.Now;
            ticket[Config.SPCCDTTICKET_CLIENT] = new EntityReference(Config.SPCACCOUNT, cdt.Client.InternalId);
            ticket[Config.SPCCDTTICKET_CDT] = new EntityReference(Config.SPCCDT, cdt.InternalId);
            Guid id = await Proxy.Create(ticket);
            ticket = await Proxy.Retrieve(Config.SPCCDTTICKET, id, new ColumnSet(new string[]
            {
                    Config.SPCCDTTICKET_FINISHED,
                    Config.SPCCDTTICKET_HADLUNCH,
                    Config.SPCCDTTICKET_NUMBER,
                    Config.SPCCDTTICKET_STARTED,
                    Config.SPCCDTTICKET_WORKDONE,
                    Config.SPCCDTTICKET_EMAIL
            }));
            CDTTicket cdtTicket = BuildCDTTicketFromEntity(ticket);
            cdtTicket.CDTId = cdt.SQLiteRecordId;
            //incident.ServiceTickets.Add(result);
            await conn.InsertOrReplaceWithChildrenAsync(cdtTicket, true);
            return cdtTicket;
        }

        /// <summary>
        /// Executes a workflow to create an equipment order request 
        /// </summary>
        /// <param name="cdt">CDT of which equipment order request is going to be created</param>
        /// <returns>Boolean result of the operation</returns>
        public static async Task<bool> OrderEquipment(CDTViewModel cdt)
        {
            #region Validate that there's at least 1 equipment to request.
            bool continueExecution = true;
            for (int i = 0; i < cdt.ProjectEquipment.Count && continueExecution; i++)
                continueExecution = cdt.ProjectEquipment[i].Requested == 0;
            if (continueExecution)
                return true;
            #endregion
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {
                WorkflowId = new Guid("8555DC73-9CE0-45D1-B8EB-537203DB9E61"), //Order equipment Guid
                EntityId = cdt.InternalId
            };
            try
            {
                ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)await Proxy.Execute(request);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Executes a workflow to create a material order request
        /// </summary>
        /// <param name="cdt">CDT of which a material order request is going to be created</param>
        /// <returns>Boolean value indicating result of the operation</returns>
        public static async Task<bool> OrderMaterials(CDTViewModel cdt)
        {
            #region Validate that there's at least 1 material to request.
            bool continueExecution = true;
            for (int i = 0; i < cdt.ProjectMaterials.Count && continueExecution; i++)
                continueExecution = cdt.ProjectMaterials[i].Requested == 0;
            if (continueExecution)
                return true;
            #endregion
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {
                WorkflowId = new Guid("9A917E1D-BC93-48FF-9FDB-600E5EEBEE9D"), //Order material Guid
                EntityId = cdt.InternalId
            };
            try
            {
                ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)await Proxy.Execute(request);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Saves any changes made in CDT entity.
        /// </summary>
        /// <param name="cdt">CDT to be saved/updated</param>
        /// <returns>Boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveProjectChanges(CDTViewModel cdt)
        {
            foreach (ProjectEquipmentViewModel equip in cdt.ProjectEquipment)
            {
                if (equip.Requested > 0)
                {
                    Entity toSave = new Entity(Config.SPCPROJECTEQUIPMENT, equip.InternalId);
                    toSave[Config.SPCPROJECTEQUIPMENT_REQUESTED] = (decimal)equip.Requested;
                    try
                    {
                        await Proxy.Update(toSave);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            foreach (ProjectMaterialViewModel mat in cdt.ProjectMaterials)
            {
                if (mat.Requested > 0)
                {
                    Entity toSave = new Entity(Config.SPCPROJECTMATERIAL, mat.InternalId);
                    toSave[Config.SPCPROJECTMATERIAL_REQUESTED] = (decimal)mat.Requested;
                    //try
                    //{
                    await Proxy.Update(toSave);
                    //}
                    //catch (Exception)
                    //{
                    //    return false;
                    //}
                }
            }
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(cdt.ToModel(), true);
            return true;
        }

        /// <summary>
        /// Save any changes made in Service Ticket (Boleta de Servicio) entity in the CRM
        /// </summary>
        /// <param name="serviceTicket">Service Ticket model object to be updated/saved</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveServiceTicket(ServiceTicket serviceTicket)
        {
            Entity toSave = new Entity(Config.SPCSERVTICKET, serviceTicket.InternalId);
            if (serviceTicket.Technicians.Count > 4)
                toSave[Config.SPCSERVTICKET_TECHNICIAN5] = new EntityReference(Config.SPCTECHNICIAN, serviceTicket.Technicians[4].InternalId);
            if (serviceTicket.Technicians.Count > 3)
                toSave[Config.SPCSERVTICKET_TECHNICIAN4] = new EntityReference(Config.SPCTECHNICIAN, serviceTicket.Technicians[3].InternalId);
            if (serviceTicket.Technicians.Count > 2)
                toSave[Config.SPCSERVTICKET_TECHNICIAN3] = new EntityReference(Config.SPCTECHNICIAN, serviceTicket.Technicians[2].InternalId);
            if (serviceTicket.Technicians.Count > 1)
                toSave[Config.SPCSERVTICKET_TECHNICIAN2] = new EntityReference(Config.SPCTECHNICIAN, serviceTicket.Technicians[1].InternalId);
            toSave[Config.SPCSERVTICKET_WORKDONE] = string.IsNullOrEmpty(serviceTicket.WorkDone) ? String.Empty : serviceTicket.WorkDone;
            toSave[Config.SPCSERVTICKET_HADLUNCH] = serviceTicket.HadLunch;
            try
            {
                await Proxy.Update(toSave);
            }
            catch (Exception)
            {
                return false;
            }
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(serviceTicket, true);
            return true;
        }

        /// <summary>
        /// Save changes of a Service Ticket locally so it can be updated later on.
        /// </summary>
        /// <param name="serviceTicket">Service Ticket that needs to be updated</param>
        /// <returns>Boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveServiceTicketOffline(ServiceTicket serviceTicket)
        {
            var conn = await Local.GetConnection();
            await conn.UpdateWithChildrenAsync(serviceTicket);
            await RegisterPendingOperation(Types.CRUDOperation.Update, serviceTicket.SQLiteRecordId, nameof(ServiceTicket));
            return true;
        }

        /// <summary>
        /// Save changes of a visit ticket (CDT Ticket) in the CRM
        /// </summary>
        /// <param name="ticket">CDT Ticket to be updated</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> SaveCDTTicket(CDTTicket ticket)
        {
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), "Guardando", "Guardando boleta", 0, 1);
            PageService ps = new PageService();
            await ps.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            Entity toSave = new Entity(Config.SPCCDTTICKET, ticket.InternalId);
            toSave[Config.SPCCDTTICKET_EMAIL] = string.IsNullOrEmpty(ticket.Email) ? string.Empty : ticket.Email;
            toSave[Config.SPCCDTTICKET_WORKDONE] = string.IsNullOrEmpty(ticket.Workdone) ? string.Empty : ticket.Workdone;
            toSave[Config.SPCCDTTICKET_AGREEMENTS] = string.IsNullOrEmpty(ticket.Agreements) ? string.Empty : ticket.Agreements;
            try
            {
                await Proxy.Update(toSave);
                vm.ProgressUp("Boleta guardada");
            }
            catch (Exception)
            {
                vm.ProgressUp("No se pudo realizar");
                vm.IsLoading = false;
                return false;
            }
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(ticket, true);
            vm.IsLoading = false;
            return true;
        }

        /// <summary>
        /// Closes an open CDT Ticket in the CRM and uploads the corresponding report.
        /// </summary>
        /// <param name="ticket">Ticket to be closed</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> FinishCDTTicket(CDTTicket ticket)
        {
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), "Cerrando", "Guardando boleta", 0, 1);
            PageService ps = new PageService();
            await ps.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            Entity toSave = new Entity(Config.SPCCDTTICKET, ticket.InternalId);
            toSave[Config.SPCCDTTICKET_EMAIL] = string.IsNullOrEmpty(ticket.Email) ? String.Empty : ticket.Email;
            toSave[Config.SPCCDTTICKET_WORKDONE] = string.IsNullOrEmpty(ticket.Workdone) ? String.Empty : ticket.Workdone;
            toSave[Config.SPCCDTTICKET_FINISHED] = ticket.Finished;
            try
            {
                await Proxy.Update(toSave);
                vm.ProgressUp("Boleta guardada");
            }
            catch (Exception)
            {
                vm.ProgressUp("No se pudo realizar");
                vm.IsLoading = false;
                return false;
            }
            foreach (TechnicianRegistry tech in ticket.TechniciansRegistered)
                await SaveChangesTechnicianRegistry(tech);
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(ticket, true);
            vm.IsLoading = false;
            return true;
        }

        /// <summary>
        /// Converts an entity into a legalization model object
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns>Legalization result of the conversion</returns>
        public static async Task<Legalization> BuildLegalization(Entity entity)
        {
            var conn = await Local.GetConnection();
            Currency currency = entity.Contains(Config.SPCLEGALIZATION_CURRENCYID) ? await GetCurrency(((EntityReference)entity[Config.SPCLEGALIZATION_CURRENCYID]).Id) : null;
            CDT cdt = entity.Contains(Config.SPCLEGALIZATION_CDT_ID) ? await GetCDT(((EntityReference)entity[Config.SPCLEGALIZATION_CDT_ID]).Id) : null;
            Incident incident = entity.Contains(Config.SPCLEGALIZATION_INCIDENT_ID) ? await GetIncident(((EntityReference)entity[Config.SPCLEGALIZATION_INCIDENT_ID]).Id) : null;
            Company company = entity.Contains(Config.SPCLEGALIZATION_COMPANY) ? await GetCompany(((EntityReference)entity[Config.SPCLEGALIZATION_COMPANY]).Id) : null;
            //ServiceTicket serviceticket = entity.Contains(Config.SPCLEGALIZATION_SERVTICKET_ID) ? await GetServiceTicket(((EntityReference)entity[Config.SPCLEGALIZATION_SERVTICKET_ID]).Id) : null;
            Legalization local = (await conn.GetAllWithChildrenAsync<Legalization>(c => c.InternalId.Equals(entity.Id))).FirstOrDefault();
            Legalization result = new Legalization
            {
                InternalId = entity.Id,
                SQLiteRecordId = local != null ? local.SQLiteRecordId : 0,
                MoneyCurrency = currency,
                MoneyCurrencyId = currency?.SQLiteRecordId ?? 0,
                RelatedCDT = cdt,
                RelatedCDTId = cdt?.SQLiteRecordId ?? 0,
                RelatedIncident = incident,
                RelatedIncidentId = incident?.SQLiteRecordId ?? 0,
                Company = company,
                CompanyId = company?.SQLiteRecordId ?? 0,
                //RelatedServiceTicket = serviceticket,
                //RelatedServiceTicketId = serviceticket?.SQLiteRecordId ?? 0,
                LastCreditCardDigits = entity.Contains(Config.SPCLEGALIZATION_LASTCREDITCARDNUMBERS) ? (string)entity[Config.SPCLEGALIZATION_LASTCREDITCARDNUMBERS] : string.Empty,
                Detail = entity.Contains(Config.SPCLEGALIZATION_DETAIL) ? (string)entity[Config.SPCLEGALIZATION_DETAIL] : string.Empty,
                LegalizationNumber = entity.Contains(Config.SPCLEGALIZATION_NUMBER) ? ((int)entity[Config.SPCLEGALIZATION_NUMBER]).ToString() : string.Empty,
                MoneyRequested = entity.Contains(Config.SPCLEGALIZATION_MONEYREQUESTED) ? ((Money)entity[Config.SPCLEGALIZATION_MONEYREQUESTED]).Value : 0,
                MoneyPaid = entity.Contains(Config.SPCLEGALIZATION_MONEYPAID) ? ((Money)entity[Config.SPCLEGALIZATION_MONEYPAID]).Value : 0,
                ProjectIssue = entity.Contains(Config.SPCLEGALIZATION_PROJECTISSUE) ? (bool)entity[Config.SPCLEGALIZATION_PROJECTISSUE] : false,
                LegalizationType = entity.Contains(Config.SPCLEGALIZATION_TYPE) ? (Types.SPCLEGALIZATION_TYPE)((OptionSetValue)entity[Config.SPCLEGALIZATION_TYPE]).Value : Types.SPCLEGALIZATION_TYPE.Undefined
            };
            if (entity.Contains(Config.SPCLEGALIZATION_LEGALIZATORSIGN) && (bool)entity[Config.SPCLEGALIZATION_LEGALIZATORSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedOwner;
            if (entity.Contains(Config.SPCLEGALIZATION_MANAGERSIGN) && (bool)entity[Config.SPCLEGALIZATION_MANAGERSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedManager;
            if (entity.Contains(Config.SPCLEGALIZATION_FINANCIALSIGN) && (bool)entity[Config.SPCLEGALIZATION_FINANCIALSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedFinancial;
            await conn.InsertOrReplaceWithChildrenAsync(result);
            return result;
        }

        /// <summary>
        /// Obtains money legalizations of last month from CRM as ViewModels
        /// </summary>
        /// <returns>Collection of Legalization View Models for a specific user</returns>
        public static async Task<ObservableCollection<LegalizationViewModel>> GetMoneyLegalizationsViewModel()
        {
            ObservableCollection<LegalizationViewModel> legalizationViewModels = new ObservableCollection<LegalizationViewModel>();
            List<Legalization> legalizations = await GetMoneyLegalizations();
            foreach (Legalization legalization in legalizations)
                legalizationViewModels.Add(new LegalizationViewModel(legalization));
            return legalizationViewModels;
        }

        /// <summary>
        /// Obtains an Observable Collection of Legalization Viewmodels of the CURRENTLY logged in user. However this ViewModels are incomplete as they only fetch some of the data required, because of optimization and network usage
        /// </summary>
        /// <returns>An Observable Collection of Legalization Viewmodels</returns>
        public static async Task<ObservableCollection<LegalizationViewModel>> GetMoneyLegalizationsDTOViewModel() //Be careful. This might be seen as a complete viewmodel but it is not. This is just for listing and searches.
        {
            ObservableCollection<LegalizationViewModel> legalizationViewModels = new ObservableCollection<LegalizationViewModel>();
            List<DTO_LegalizationLookUp> legalizations = await GetMoneyLegalizationsDTO();
            foreach (DTO_LegalizationLookUp legalization in legalizations)
                legalizationViewModels.Add(new LegalizationViewModel(legalization));
            return legalizationViewModels;
        }

        /// <summary>
        /// Gets an specific legalization viewmodel related to the logged in user.
        /// </summary>
        /// <param name="internalId">Guid of the legalization to be retrieve from CRM</param>
        /// <returns>A viewmodel of the legalization obtained from CRM</returns>
        public static async Task<LegalizationViewModel> GetMoneyLegalizationViewModel(Guid internalId) =>
            new LegalizationViewModel(await GetMoneyLegalization(internalId));
        
        /// <summary>
        /// Gets an specific legalization model object related to the logged in user.
        /// </summary>
        /// <param name="internalId">Guid of the legalization to be retrieve from CRM</param>
        /// <returns>a model object which corresponds to the Guid provided</returns>
        public static async Task<Legalization> GetMoneyLegalization(Guid internalId)
        {
            try
            {
                Entity legalizationEntity = await Proxy.Retrieve(Config.SPCLEGALIZATION, internalId, new ColumnSet(
                Config.SPCLEGALIZATION_CDT_ID,
                Config.SPCLEGALIZATION_CURRENCYID,
                Config.SPCLEGALIZATION_DETAIL,
                Config.SPCLEGALIZATION_ID,
                Config.SPCLEGALIZATION_INCIDENT_ID,
                Config.SPCLEGALIZATION_LEGALIZATOR,
                Config.SPCLEGALIZATION_LEGALIZATORSIGN,
                Config.SPCLEGALIZATION_MONEYPAID,
                Config.SPCLEGALIZATION_MONEYREQUESTED,
                Config.SPCLEGALIZATION_NUMBER,
                Config.SPCLEGALIZATION_PROJECTISSUE,
                Config.SPCLEGALIZATION_SERVTICKET_ID,
                Config.SPCLEGALIZATION_TYPE,
                Config.SPCLEGALIZATION_LEGALIZATORSIGN,
                Config.SPCLEGALIZATION_MANAGERSIGN,
                Config.SPCLEGALIZATION_FINANCIALSIGN,
                Config.SPCLEGALIZATION_COMPANY,
                Config.SPCLEGALIZATION_LASTCREDITCARDNUMBERS
            ));
                Legalization legalization = await BuildLegalization(legalizationEntity);
                return legalization;
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets a list of Legalizations related to the logged user. This model objects are incomplete and should not be used in other areas than listings.
        /// </summary>
        /// <returns>A list of incomplete legalizations related to an user.</returns>
        public static async Task<List<DTO_LegalizationLookUp>> GetMoneyLegalizationsDTO()
        {
            EntityCollection queryresult = await GetMoneyLegalizationsQuery();
            List<DTO_LegalizationLookUp> result = new List<DTO_LegalizationLookUp>();
            foreach (Entity lg in queryresult.Entities)
            {
                DTO_LegalizationLookUp toAdd = await BuildDTOLegalization(lg);
                result.Add(toAdd);
            }
            return result;
        }

        /// <summary>
        /// Converts an entity into a partial Legalization model object
        /// </summary>
        /// <param name="entity">Entity to convert into a partial legalization</param>
        /// <returns>A legalization model object result of the conversion</returns>
        public static async Task<DTO_LegalizationLookUp> BuildDTOLegalization(Entity entity)
        {
            Currency currency = entity.Contains(Config.SPCLEGALIZATION_CURRENCYID) ? await GetCurrency(((EntityReference)entity[Config.SPCLEGALIZATION_CURRENCYID]).Id) : null;
            DTO_LegalizationLookUp result = new DTO_LegalizationLookUp
            {
                InternalId = entity.Id,
                LegalizationNumber = entity.Contains(Config.SPCLEGALIZATION_NUMBER) ? ((int)entity[Config.SPCLEGALIZATION_NUMBER]).ToString() : string.Empty,
                MoneyRequested = entity.Contains(Config.SPCLEGALIZATION_MONEYREQUESTED) ? ((Money)entity[Config.SPCLEGALIZATION_MONEYREQUESTED]).Value : 0,
                MoneyPaid = entity.Contains(Config.SPCLEGALIZATION_MONEYPAID) ? ((Money)entity[Config.SPCLEGALIZATION_MONEYPAID]).Value : 0,
                LegalizationType = entity.Contains(Config.SPCLEGALIZATION_TYPE) ? (Types.SPCLEGALIZATION_TYPE)((OptionSetValue)entity[Config.SPCLEGALIZATION_TYPE]).Value : Types.SPCLEGALIZATION_TYPE.Undefined,
                MoneyCurrency = currency
            };
            if (entity.Contains(Config.SPCLEGALIZATION_LEGALIZATORSIGN) && (bool)entity[Config.SPCLEGALIZATION_LEGALIZATORSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedOwner;
            if (entity.Contains(Config.SPCLEGALIZATION_MANAGERSIGN) && (bool)entity[Config.SPCLEGALIZATION_MANAGERSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedManager;
            if (entity.Contains(Config.SPCLEGALIZATION_FINANCIALSIGN) && (bool)entity[Config.SPCLEGALIZATION_FINANCIALSIGN])
                result.SignState = Types.SPCLEGALIZATION_SIGNSTATE.SignedFinancial;
            return result;
        }

        /// <summary>
        /// Gets from CRM the collection of entities that represent legalizations of money from an specific user.
        /// </summary>
        /// <returns></returns>
        public static async Task<EntityCollection> GetMoneyLegalizationsQuery()
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = Config.SPCLEGALIZATION,
                ColumnSet = new ColumnSet(
                     Config.SPCLEGALIZATION_CDT_ID,
                     Config.SPCLEGALIZATION_CURRENCYID,
                     Config.SPCLEGALIZATION_DETAIL,
                     Config.SPCLEGALIZATION_ID,
                     Config.SPCLEGALIZATION_INCIDENT_ID,
                     Config.SPCLEGALIZATION_LEGALIZATOR,
                     Config.SPCLEGALIZATION_LEGALIZATORSIGN,
                     Config.SPCLEGALIZATION_MONEYPAID,
                     Config.SPCLEGALIZATION_MONEYREQUESTED,
                     Config.SPCLEGALIZATION_NUMBER,
                     Config.SPCLEGALIZATION_PROJECTISSUE,
                     Config.SPCLEGALIZATION_SERVTICKET_ID,
                     Config.SPCLEGALIZATION_TYPE,
                     Config.SPCLEGALIZATION_LEGALIZATORSIGN,
                     Config.SPCLEGALIZATION_MANAGERSIGN,
                     Config.SPCLEGALIZATION_FINANCIALSIGN,
                     Config.SPCLEGALIZATION_COMPANY,
                     Config.SPCLEGALIZATION_LASTCREDITCARDNUMBERS
                     ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                     {
                         //Conditions
                         new ConditionExpression(Config.GENERAL_STATECODE, ConditionOperator.Equal, 0),
                         new ConditionExpression(Config.GENERAL_OWNER, ConditionOperator.Equal, Proxy.LoggedUser.InternalId),
                         new ConditionExpression(Config.GENERAL_CREATEDON, ConditionOperator.LastXWeeks,4)
                     }
                }
            };
            try
            {
                return await Proxy.RetrieveMultiple(query);
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
        }

        /// <summary>
        /// Gets from the CRM A collection of Legalization model objects from the last month for the logged in user
        /// </summary>
        /// <returns> A collection of Legalization Model Objects</returns>
        public static async Task<List<Legalization>> GetMoneyLegalizations()
        {
            EntityCollection queryresult = await GetMoneyLegalizationsQuery();
            List<Legalization> result = new List<Legalization>();
            foreach (Entity lg in queryresult.Entities)
            {
                Legalization toAdd = await BuildLegalization(lg);
                result.Add(toAdd);
            }
            return result;
        }

        /// <summary>
        /// Update/Save programming changes made for an Incident in the CRM
        /// </summary>
        /// <param name="incident">Incident to be saved/updated</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> UpdateIncidentProgrammingInformation(Incident incident)
        {
            Entity toSave = new Entity(Config.SPCCASE, incident.InternalId);
            if (incident.TechniciansAssigned[0] != null)
                toSave[Config.SPCCASE_TECHNICIAN1] = new EntityReference(Config.SPCTECHNICIAN, incident.TechniciansAssigned[0].InternalId);
            if (incident.TechniciansAssigned[1] != null)
                toSave[Config.SPCCASE_TECHNICIAN2] = new EntityReference(Config.SPCTECHNICIAN, incident.TechniciansAssigned[1].InternalId);
            if (incident.TechniciansAssigned[2] != null)
                toSave[Config.SPCCASE_TECHNICIAN3] = new EntityReference(Config.SPCTECHNICIAN, incident.TechniciansAssigned[2].InternalId);
            if (!incident.ProgrammedDate.Equals(default(DateTime)))
                toSave[Config.SPCCASE_PROGRAMMED] = incident.ProgrammedDate;
            if (incident.PaymentOption != Types.SPCINCIDENT_PAYMENTOPTION.Undefined)
                toSave[Config.SPCCASE_PAYMENT] = new OptionSetValue((int)incident.PaymentOption);
            if (incident.ControlOption != Types.SPCINCIDENT_CONTROLOPTION.Undefined)
                toSave[Config.SPCCASE_CONTROL] = new OptionSetValue((int)incident.ControlOption);
            toSave[Config.SPCCASE_REQUESTEDDATE] = incident.CreatedOn;
            toSave[Config.SPCCASE_MONEYINADVANCE] = new decimal(0);
            toSave[Config.SPCCASE_REVIEWED] = incident.Reviewed;
            toSave[Config.SPCCASE_CIA] = new EntityReference(Config.SPCBUSINESSUNIT, new Guid("e161cd27-cf63-e511-80de-3863bb3c26b0")); //SPC TELECENTINEL
            try
            {
                await Proxy.Update(toSave);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new Photo attachment in the CRM
        /// </summary>
        /// <param name="note">Attachment model object to be created in the CRM</param>
        /// <returns>A Guid corresponding to the Id of the attachment created</returns>
        public static async Task<Guid> AddPhoto(Note note)
        {
            var conn = await Local.GetConnection();
            Entity annotation = new Entity(Config.SPCNOTES);
            annotation[Config.SPCNOTES_FILENAME] = note.Filename;
            annotation[Config.SPCNOTES_CONTENT] = note.Content;
            annotation[Config.SPCNOTES_MIME] = note.Mime;
            if (!string.IsNullOrEmpty(note.Title))
                annotation[Config.SPCNOTES_TITLE] = note.Title;
            string EntityName = null;
            Guid EntityGuid = default(Guid);
            if (note.ServiceTicket != null)
            {
                EntityName = Config.SPCSERVTICKET;
                EntityGuid = note.ServiceTicket.InternalId;
            }
            else if (note.Incident != null)
            {
                EntityName = Config.SPCCASE;
                EntityGuid = note.Incident.InternalId;
            }
            else if (note.Ticket != null)
            {
                EntityName = Config.SPCCDTTICKET;
                EntityGuid = note.Ticket.InternalId;
            }
            else if (note.CDT != null)
            {
                EntityName = Config.SPCCDT;
                EntityGuid = note.CDT.InternalId;
            }
            annotation[Config.SPCNOTES_LINK] = new EntityReference(EntityName, EntityGuid);
            try
            {
                Guid guid = await Proxy.Create(annotation);
                note.InternalId = guid;
                await conn.InsertOrReplaceWithChildrenAsync(note);
                return guid;
            }
            catch (Exception)
            {
                return default(Guid);
            }
        }

        /// <summary>
        /// Creates a locally saved photo attachment so it can be created in the CRM once the internet is back.
        /// </summary>
        /// <param name="note">Attachment model object to be created in the CRM</param>
        /// <returns>Void</returns>
        public static async Task AddPhotoOffline(Note note)
        {
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(note);
            string additionalInfo = null;
            if (note.ServiceTicket != null)
                additionalInfo = nameof(ServiceTicket);
            else if (note.Incident != null)
                additionalInfo = nameof(Incident);
            else if (note.Ticket != null)
                additionalInfo = nameof(CDTTicket);
            await RegisterPendingOperation(Types.CRUDOperation.Create, note.SQLiteRecordId, nameof(Note), additionalInfo);
        }

        /// <summary>
        /// Obtains a collection of all Photo attachments registered in the CRM for a specific Service Ticket.
        /// </summary>
        /// <param name="serviceTicket">Service Ticket where photo attachments are related to.</param>
        /// <returns>A collection of Notes View Model corresponding to all the photos attachments of a service ticket.</returns>
        public static async Task<ObservableCollection<NoteViewModel>> GetPhotosOfServiceTicket(ServiceTicket serviceTicket)
        {
            var conn = await Local.GetConnection();
            ObservableCollection<NoteViewModel> photos = new ObservableCollection<NoteViewModel>();
            QueryExpression queryNotes = new QueryExpression(Config.SPCNOTES)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCNOTES_FILENAME, Config.SPCNOTES_CONTENT })
            };
            queryNotes.Criteria.FilterOperator = LogicalOperator.And;
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_LINK, ConditionOperator.Equal, serviceTicket.InternalId));
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_MIME, ConditionOperator.Equal, "image/jpeg"));
            EntityCollection result = await Proxy.RetrieveMultiple(queryNotes);
            foreach (Entity ephoto in result.Entities)
            {
                string name = (string)ephoto[Config.SPCNOTES_FILENAME];
                Note local = (await conn.GetAllWithChildrenAsync<Note>(n => n.Filename.Equals(name))).FirstOrDefault();
                Note toAdd = new Note
                {
                    SQLiteRecordId = local != null ? local.SQLiteRecordId : 0,
                    InternalId = ephoto.Id,
                    ServiceTicket = serviceTicket,
                    ServiceTicketId = serviceTicket.SQLiteRecordId,
                    Mime = "image/jpeg",
                    Filename = ephoto.Contains(Config.SPCNOTES_FILENAME) ? (string)ephoto[Config.SPCNOTES_FILENAME] : string.Empty,
                    ObjectId = serviceTicket.InternalId,
                    Title = ephoto.Contains(Config.SPCNOTES_FILENAME) ? (string)ephoto[Config.SPCNOTES_FILENAME] : string.Empty,
                    Content = (string)ephoto[Config.SPCNOTES_CONTENT]
                };
                photos.Add(new NoteViewModel(toAdd));
                await conn.InsertOrReplaceWithChildrenAsync(toAdd);
            }
            return photos;
        }

        /// <summary>
        /// Obtains a collection of all Photo attachments registered in the CRM for a specific Visit Ticket.
        /// </summary>
        /// <param name="cdtTicket">Cdt Ticket where photo attachments are related to.</param>
        /// <param name="cdt">CDT where the Ticket is related to.</param>
        /// <returns>A collection of Notes ViewModels corresponding to the photos attached to a visit ticket.</returns>
        public static async Task<ObservableCollection<NoteViewModel>> GetPhotosOfCDTTicket(CDTTicket cdtTicket, CDT cdt)
        {
            var conn = await Local.GetConnection();
            ObservableCollection<NoteViewModel> photos = new ObservableCollection<NoteViewModel>();
            QueryExpression queryNotes = new QueryExpression(Config.SPCNOTES)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCNOTES_FILENAME, Config.SPCNOTES_CONTENT })
            };
            queryNotes.Criteria.FilterOperator = LogicalOperator.And;
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_TITLE, ConditionOperator.Equal, cdtTicket.Number));
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_MIME, ConditionOperator.Equal, "image/jpeg"));
            EntityCollection result = await Proxy.RetrieveMultiple(queryNotes);
            foreach (Entity ephoto in result.Entities)
            {
                string name = (string)ephoto[Config.SPCNOTES_FILENAME];
                Note local = (await conn.GetAllWithChildrenAsync<Note>(n => n.Filename.Equals(name))).FirstOrDefault();
                Note toAdd = new Note
                {
                    SQLiteRecordId = local != null ? local.SQLiteRecordId : 0,
                    InternalId = ephoto.Id,
                    CDT = cdt,
                    CDTId = cdt.SQLiteRecordId,
                    Mime = "image/jpeg",
                    Filename = ephoto.Contains(Config.SPCNOTES_FILENAME) ? (string)ephoto[Config.SPCNOTES_FILENAME] : string.Empty,
                    ObjectId = cdt.InternalId,
                    Title = ephoto.Contains(Config.SPCNOTES_FILENAME) ? (string)ephoto[Config.SPCNOTES_FILENAME] : string.Empty,
                    Content = (string)ephoto[Config.SPCNOTES_CONTENT]
                };
                photos.Add(new NoteViewModel(toAdd));
                await conn.InsertOrReplaceWithChildrenAsync(toAdd);
            }
            return photos;
        }

        /// <summary>
        /// Obtains a collection of all Photo attachments locally saved in the phone so user can access them without internet.
        /// </summary>
        /// <param name="serviceTicket">Service Ticket where photos are going to be obtained from</param>
        /// <returns>A collection of Note View Models that contains the photos for an specific service Ticket.</returns>
        public static async Task<ObservableCollection<NoteViewModel>> GetPhotosOfServiceTicketOffline(ServiceTicket serviceTicket)
        {
            var conn = await Local.GetConnection();
            ObservableCollection<NoteViewModel> photos = new ObservableCollection<NoteViewModel>();
            List<Note> local = await conn.GetAllWithChildrenAsync<Note>(n => n.ServiceTicketId == serviceTicket.SQLiteRecordId);
            foreach (Note n in local)
                photos.Add(new NoteViewModel(n));
            return photos;
        }

        /// <summary>
        /// Obtains all reports(PDF) made for an incident
        /// </summary>
        /// <param name="incident">Incident where pdf reports are going to be obtained from</param>
        /// <returns>A collection of notes corresponding to the reports of an incident</returns>
        public static async Task<ObservableCollection<NoteViewModel>> GetReportsFromIncident(Guid incident)
        {
            ObservableCollection<NoteViewModel> notes = new ObservableCollection<NoteViewModel>();
            QueryExpression queryNotes = new QueryExpression(Config.SPCNOTES)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCNOTES_FILENAME, Config.SPCNOTES_CONTENT })
            };
            queryNotes.Criteria.FilterOperator = LogicalOperator.And;
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_LINK, ConditionOperator.Equal, incident));
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_MIME, ConditionOperator.Equal, "application/pdf"));
            EntityCollection result = await Proxy.RetrieveMultiple(queryNotes);
            foreach (Entity enote in result.Entities)
            {
                Note toAdd = new Note
                {
                    InternalId = enote.Id,
                    Filename = enote.Contains(Config.SPCNOTES_FILENAME) ? (string)enote[Config.SPCNOTES_FILENAME] : string.Empty,
                    ObjectId = incident,
                    Title = enote.Contains(Config.SPCNOTES_FILENAME) ? (string)enote[Config.SPCNOTES_FILENAME] : string.Empty,
                    Content = (string)enote[Config.SPCNOTES_CONTENT]
                };
                notes.Add(new NoteViewModel(toAdd));
            }
            return notes;
        }

        /// <summary>
        /// Obtains all project equipment related to a CDT
        /// </summary>
        /// <param name="cdt">CDT where project equipment is related to.</param>
        /// <returns>A collection of Project Equipment</returns>
        public static async Task<List<ProjectEquipment>> GetAllProjectEquipmentFromCDT(CDT cdt)
        {
            var conn = await Local.GetConnection();
            List<ProjectEquipment> equipment = new List<ProjectEquipment>();
            string NameProduct = "Product";
            #region Query
            QueryExpression queryEquipment = new QueryExpression(Config.SPCPROJECTEQUIPMENT)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCPROJECTEQUIPMENT_CDT,
                Config.SPCPROJECTEQUIPMENT_CLAIMED,
                Config.SPCPROJECTEQUIPMENT_CODE,
                Config.SPCPROJECTEQUIPMENT_ID,
                Config.SPCPROJECTEQUIPMENT_ISADDITIONAL,
                Config.SPCPROJECTEQUIPMENT_ISCANCELED,
                Config.SPCPROJECTEQUIPMENT_PRODUCT,
                Config.SPCPROJECTEQUIPMENT_QUANTITY,
                Config.SPCPROJECTEQUIPMENT_REMAINING,
                Config.SPCPROJECTEQUIPMENT_REQUESTED,
                Config.SPCPROJECTEQUIPMENT_SRE
            }),
                Criteria = new FilterExpression()
            };
            queryEquipment.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCPROJECTEQUIPMENT,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCPROJECTEQUIPMENT_PRODUCT,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCPRODUCT_GUID,
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            queryEquipment.Criteria.FilterOperator = LogicalOperator.Or;
            queryEquipment.Criteria.AddCondition(new ConditionExpression(Config.SPCPROJECTEQUIPMENT_CDT, ConditionOperator.Equal, cdt.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryEquipment);
            #endregion
            foreach (Entity equipEntity in result.Entities)
            {
                #region Product
                Product product = await BuildProductFromEntity(equipEntity, NameProduct);
                #endregion
                #region Project Equipment  
                ProjectEquipment localProjectEquipment = await conn.Table<ProjectEquipment>().Where(p => p.InternalId.Equals(equipEntity.Id)).FirstOrDefaultAsync();
                ProjectEquipment equip = new ProjectEquipment
                {
                    SQLiteRecordId = localProjectEquipment != null ? localProjectEquipment.SQLiteRecordId : 0,
                    InternalId = equipEntity.Id,
                    Product = product,
                    ProductId = product.SQLiteRecordId,
                    Claimed = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_CLAIMED) ? (int)(decimal)equipEntity[Config.SPCPROJECTEQUIPMENT_CLAIMED] : 0,
                    Code = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_CODE) ? (string)equipEntity[Config.SPCPROJECTEQUIPMENT_CODE] : string.Empty,
                    IsAdditional = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_ISADDITIONAL) ? (bool)equipEntity[Config.SPCPROJECTEQUIPMENT_ISADDITIONAL] : false,
                    IsCanceled = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_ISCANCELED) ? (bool)equipEntity[Config.SPCPROJECTEQUIPMENT_ISCANCELED] : false,
                    Quantity = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_QUANTITY) ? (int)(decimal)equipEntity[Config.SPCPROJECTEQUIPMENT_QUANTITY] : 0,
                    Remaining = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_REMAINING) ? (int)(decimal)equipEntity[Config.SPCPROJECTEQUIPMENT_REMAINING] : 0,
                    Requested = equipEntity.Contains(Config.SPCPROJECTEQUIPMENT_REQUESTED) ? (int)(decimal)equipEntity[Config.SPCPROJECTEQUIPMENT_REQUESTED] : 0
                };
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                equipment.Add(equip);
            }
            return equipment;
        }

        /// <summary>
        /// Converts an entity into a Product model object
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <param name="linkingName">Name used to aliased the entity</param>
        /// <returns>A product</returns>
        private static async Task<Product> BuildProductFromEntity(Entity entity, string linkingName = null)
        {
            //Check if product already exists so it sets its primary key.
            var conn = await Local.GetConnection();
            bool isLinked = !string.IsNullOrEmpty(linkingName);
            string concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_GUID) : Config.SPCPRODUCT_GUID;
            Product product = new Product();
            if (entity.Contains(concatName))
                product.InternalId = isLinked ? (Guid)((AliasedValue)entity[concatName]).Value : entity.Id;
            Product localProduct = (await conn.GetAllWithChildrenAsync<Product>(p => p.InternalId.Equals(product.InternalId))).FirstOrDefault();
            if (localProduct != null)
                product.SQLiteRecordId = localProduct.SQLiteRecordId;
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_ID) : Config.SPCPRODUCT_ID;
            if (entity.Contains(concatName))
                product.Id = (string)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_NAME) : Config.SPCPRODUCT_NAME;
            if (entity.Contains(concatName))
                product.Name = (string)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_BOUGHT) : Config.SPCPRODUCT_BOUGHT;
            if (entity.Contains(concatName))
                product.Bought = (string)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_COST) : Config.SPCPRODUCT_COST;
            if (entity.Contains(concatName))
                product.Cost = (decimal)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_MOSC) : Config.SPCPRODUCT_MOSC;
            if (entity.Contains(concatName))
                product.DoesHaveMOSC = ((string)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName])).Equals('S');
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_MOSCPRICE) : Config.SPCPRODUCT_MOSCPRICE;
            if (entity.Contains(concatName))
                product.Cost = (decimal)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_DAITAX) : Config.SPCPRODUCT_DAITAX;
            if (entity.Contains(concatName))
                product.DAITax = (decimal)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_LAWTAX) : Config.SPCPRODUCT_LAWTAX;
            if (entity.Contains(concatName))
                product.LawTax = (decimal)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            concatName = isLinked ? string.Format("{0}.{1}", linkingName, Config.SPCPRODUCT_SELECTIVETAX) : Config.SPCPRODUCT_SELECTIVETAX;
            if (entity.Contains(concatName))
                product.SelectiveTax = (decimal)(isLinked ? ((AliasedValue)entity[concatName]).Value : entity[concatName]);
            await conn.InsertOrReplaceWithChildrenAsync(product, true);
            return product;
        }

        /// <summary>
        /// Obtains all project materials from a CDT.
        /// </summary>
        /// <param name="cdt">Cdt where project materials are related to</param>
        /// <returns>A collection of Project Materials of a CDT</returns>
        public static async Task<List<ProjectMaterial>> GetAllProjectMaterialsFromCDT(CDT cdt)
        {
            var conn = await Local.GetConnection();
            List<ProjectMaterial> materials = new List<ProjectMaterial>();
            string NameProduct = "Product";
            #region Query
            QueryExpression queryMaterials = new QueryExpression(Config.SPCPROJECTMATERIAL)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCPROJECTMATERIAL_CDT,
                Config.SPCPROJECTMATERIAL_CLAIMED,
                Config.SPCPROJECTMATERIAL_ID,
                Config.SPCPROJECTMATERIAL_ISADDITIONAL,
                Config.SPCPROJECTMATERIAL_PRODUCT,
                Config.SPCPROJECTMATERIAL_QUANTITY,
                Config.SPCPROJECTMATERIAL_REMAINING,
                Config.SPCPROJECTMATERIAL_REQUESTED
            }),
                Criteria = new FilterExpression()
            };
            queryMaterials.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCPROJECTMATERIAL,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCPROJECTMATERIAL_PRODUCT,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCPRODUCT_GUID,
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            queryMaterials.Criteria.FilterOperator = LogicalOperator.Or;
            queryMaterials.Criteria.AddCondition(new ConditionExpression(Config.SPCPROJECTMATERIAL_CDT, ConditionOperator.Equal, cdt.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryMaterials);
            #endregion
            foreach (Entity matEntity in result.Entities)
            {
                #region Product
                Product product = await BuildProductFromEntity(matEntity, NameProduct);
                #endregion
                #region Project Equipment  
                ProjectMaterial localProjectMaterial = (await conn.GetAllWithChildrenAsync<ProjectMaterial>(m => m.InternalId.Equals(matEntity.Id))).FirstOrDefault();
                ProjectMaterial equip = new ProjectMaterial
                {
                    SQLiteRecordId = localProjectMaterial != null ? localProjectMaterial.SQLiteRecordId : 0,
                    InternalId = matEntity.Id,
                    Product = product,
                    ProductId = product.SQLiteRecordId,
                    Claimed = matEntity.Contains(Config.SPCPROJECTMATERIAL_CLAIMED) ? (int)(decimal)matEntity[Config.SPCPROJECTMATERIAL_CLAIMED] : 0,
                    IsAdditional = matEntity.Contains(Config.SPCPROJECTMATERIAL_ISADDITIONAL) ? (bool)matEntity[Config.SPCPROJECTMATERIAL_ISADDITIONAL] : false,
                    Quantity = matEntity.Contains(Config.SPCPROJECTMATERIAL_QUANTITY) ? (int)(decimal)matEntity[Config.SPCPROJECTMATERIAL_QUANTITY] : 0,
                    Remaining = matEntity.Contains(Config.SPCPROJECTMATERIAL_REMAINING) ? (int)(decimal)matEntity[Config.SPCPROJECTMATERIAL_REMAINING] : 0,
                    Requested = matEntity.Contains(Config.SPCPROJECTMATERIAL_REQUESTED) ? (int)(decimal)matEntity[Config.SPCPROJECTMATERIAL_REQUESTED] : 0
                };
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                materials.Add(equip);
            }
            return materials;
        }

        /// <summary>
        /// Obtains all equipment request orders of an specific CDT
        /// </summary>
        /// <param name="cdt">CDT where equipments request orders are related to</param>
        /// <returns>A collection of Equipment request orders</returns>
        public static async Task<List<EquipmentRequestOrder>> GetAllEquipmentRequestsOrdersFromCDT(CDT cdt)
        {
            var conn = await Local.GetConnection();
            List<EquipmentRequestOrder> orderRequests = new List<EquipmentRequestOrder>();
            //string NameProduct = "Product";
            #region Query
            QueryExpression queryEquipmentOrders = new QueryExpression(Config.SPCREQUESTORDEREQUIPMENT)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCREQUESTORDEREQUIPMENT_APPROVEDDATE,
                Config.SPCREQUESTORDEREQUIPMENT_CDT,
                Config.SPCREQUESTORDEREQUIPMENT_ID,
                Config.SPCREQUESTORDEREQUIPMENT_ISAPPROVED,
                Config.SPCREQUESTORDEREQUIPMENT_NUMBER
                }),
                Criteria = new FilterExpression()
            };
            queryEquipmentOrders.Criteria.FilterOperator = LogicalOperator.Or;
            queryEquipmentOrders.Criteria.AddCondition(new ConditionExpression(Config.SPCREQUESTORDEREQUIPMENT_CDT, ConditionOperator.Equal, cdt.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryEquipmentOrders);
            #endregion
            foreach (Entity orderEntity in result.Entities)
            {
                #region Project Equipment  
                EquipmentRequestOrder localOrder = (await conn.GetAllWithChildrenAsync<EquipmentRequestOrder>(m => m.InternalId.Equals(orderEntity.Id))).FirstOrDefault();
                EquipmentRequestOrder order = new EquipmentRequestOrder
                {
                    SQLiteRecordId = localOrder != null ? localOrder.SQLiteRecordId : 0,
                    InternalId = orderEntity.Id,
                    ApprovedDate = orderEntity.Contains(Config.SPCREQUESTORDEREQUIPMENT_APPROVEDDATE) ? (DateTime)orderEntity[Config.SPCREQUESTORDEREQUIPMENT_APPROVEDDATE] : default(DateTime),
                    IsApproved = orderEntity.Contains(Config.SPCREQUESTORDEREQUIPMENT_ISAPPROVED) ? (bool)orderEntity[Config.SPCREQUESTORDEREQUIPMENT_ISAPPROVED] : false,
                    CDTId = cdt.SQLiteRecordId,
                    Number = orderEntity.Contains(Config.SPCREQUESTORDEREQUIPMENT_NUMBER) ? (string)orderEntity[Config.SPCREQUESTORDEREQUIPMENT_NUMBER] : null
                };
                order.EquipmentRequested = await GetAllLinesOfEquipmentRequestOrder(order);
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                orderRequests.Add(order);
            }
            return orderRequests;
        }

        /// <summary>
        /// Obtains all material request orders for an specific CDT.
        /// </summary>
        /// <param name="cdt">CDT where material request orders are related to.</param>
        /// <returns>A collection of Equipment request orders a cdt is related to</returns>
        public static async Task<List<MaterialRequestOrder>> GetAllMaterialRequestsOrdersFromCDT(CDT cdt)
        {
            var conn = await Local.GetConnection();
            List<MaterialRequestOrder> orderRequests = new List<MaterialRequestOrder>();
            #region Query
            QueryExpression queryMaterialOrders = new QueryExpression(Config.SPCREQUESTORDERMATERIAL)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCREQUESTORDERMATERIAL_APPROVEDDATE,
                Config.SPCREQUESTORDERMATERIAL_CDT,
                Config.SPCREQUESTORDERMATERIAL_ID,
                Config.SPCREQUESTORDERMATERIAL_ISAPPROVED,
                Config.SPCREQUESTORDERMATERIAL_NUMBER
                }),
                Criteria = new FilterExpression()
            };
            queryMaterialOrders.Criteria.FilterOperator = LogicalOperator.Or;
            queryMaterialOrders.Criteria.AddCondition(new ConditionExpression(Config.SPCREQUESTORDERMATERIAL_CDT, ConditionOperator.Equal, cdt.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryMaterialOrders);
            #endregion
            foreach (Entity orderEntity in result.Entities)
            {
                #region Project Equipment  
                MaterialRequestOrder localOrder = (await conn.GetAllWithChildrenAsync<MaterialRequestOrder>(m => m.InternalId.Equals(orderEntity.Id))).FirstOrDefault();
                MaterialRequestOrder order = new MaterialRequestOrder
                {
                    SQLiteRecordId = localOrder != null ? localOrder.SQLiteRecordId : 0,
                    InternalId = orderEntity.Id,
                    ApprovedDate = orderEntity.Contains(Config.SPCREQUESTORDERMATERIAL_APPROVEDDATE) ? (DateTime)orderEntity[Config.SPCREQUESTORDERMATERIAL_APPROVEDDATE] : default(DateTime),
                    IsApproved = orderEntity.Contains(Config.SPCREQUESTORDERMATERIAL_ISAPPROVED) ? (bool)orderEntity[Config.SPCREQUESTORDERMATERIAL_ISAPPROVED] : false,
                    CDTId = cdt.SQLiteRecordId,
                    Number = orderEntity.Contains(Config.SPCREQUESTORDERMATERIAL_NUMBER) ? (string)orderEntity[Config.SPCREQUESTORDERMATERIAL_NUMBER] : null
                };
                order.MaterialsRequested = await GetAllLinesOfMaterialsRequestOrder(order);
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                orderRequests.Add(order);
            }
            return orderRequests;
        }

        /// <summary>
        /// Obtains a collection of equipment lines in Equipment request orders
        /// </summary>
        /// <param name="order">Order of which the lines are related to</param>
        /// <returns>A collection a lines with equipments requested in an order</returns>
        private static async Task<List<LineEquipmentRequestOrder>> GetAllLinesOfEquipmentRequestOrder(EquipmentRequestOrder order)
        {
            var conn = await Local.GetConnection();
            List<LineEquipmentRequestOrder> lines = new List<LineEquipmentRequestOrder>();
            string NameProduct = "Product";
            #region Query
            QueryExpression queryLines = new QueryExpression(Config.SPCLINEREQORDEREQUIP)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCLINEREQORDEREQUIP_ID,
                Config.SPCLINEREQORDEREQUIP_PRODUCT,
                Config.SPCLINEREQORDEREQUIP_PRODUCTCODE,
                Config.SPCLINEREQORDEREQUIP_REQUESTED
            }),
                Criteria = new FilterExpression()
            };
            queryLines.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCLINEREQORDEREQUIP,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCLINEREQORDEREQUIP_PRODUCT,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCPRODUCT_GUID,
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            queryLines.Criteria.FilterOperator = LogicalOperator.Or;
            queryLines.Criteria.AddCondition(new ConditionExpression(Config.SPCLINEREQORDEREQUIP_EQUIPMENTORDER, ConditionOperator.Equal, order.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryLines);
            #endregion
            foreach (Entity lineEntity in result.Entities)
            {
                #region Product
                Product product = await BuildProductFromEntity(lineEntity, NameProduct);
                #endregion
                #region Order Line  
                LineEquipmentRequestOrder localOrderLine = (await conn.GetAllWithChildrenAsync<LineEquipmentRequestOrder>(_ => _.InternalId.Equals(lineEntity.Id))).FirstOrDefault();
                LineEquipmentRequestOrder line = new LineEquipmentRequestOrder
                {
                    SQLiteRecordId = localOrderLine != null ? localOrderLine.SQLiteRecordId : 0,
                    InternalId = lineEntity.Id,
                    Product = product,
                    ProductId = product?.SQLiteRecordId ?? 0,
                    Requested = lineEntity.Contains(Config.SPCLINEREQORDEREQUIP_REQUESTED) ? (int)(decimal)lineEntity[Config.SPCLINEREQORDEREQUIP_REQUESTED] : 0,
                    EquipmentRequestOrderId = order.SQLiteRecordId,
                    ProductCode = lineEntity.Contains(Config.SPCLINEREQORDEREQUIP_PRODUCTCODE) ? (string)lineEntity[Config.SPCLINEREQORDEREQUIP_PRODUCTCODE] : null
                };
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// Obtains a collection of material lines in Material request orders 
        /// </summary>
        /// <param name="order">Order of which material lines are part of</param>
        /// <returns>A collection of material lines </returns>
        private static async Task<List<LineMaterialRequestOrder>> GetAllLinesOfMaterialsRequestOrder(MaterialRequestOrder order)
        {
            var conn = await Local.GetConnection();
            List<LineMaterialRequestOrder> lines = new List<LineMaterialRequestOrder>();
            string NameProduct = "Product";
            #region Query
            QueryExpression queryLines = new QueryExpression(Config.SPCLINEREQORDERMATERIAL)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCLINEREQORDERMAT_ID,
                Config.SPCLINEREQORDERMAT_MATERIAL,
                Config.SPCLINEREQORDERMAT_MATCODE,
                Config.SPCLINEREQORDERMAT_REQUESTED
            }),
                Criteria = new FilterExpression()
            };
            queryLines.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCLINEREQORDERMATERIAL,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCLINEREQORDERMAT_MATERIAL,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
                {
                    Config.SPCPRODUCT_GUID,
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            queryLines.Criteria.FilterOperator = LogicalOperator.Or;
            queryLines.Criteria.AddCondition(new ConditionExpression(Config.SPCLINEREQORDERMAT_MATERIALORDER, ConditionOperator.Equal, order.InternalId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryLines);
            #endregion
            foreach (Entity lineEntity in result.Entities)
            {
                #region Product
                Product product = await BuildProductFromEntity(lineEntity, NameProduct);
                #endregion
                #region Order Line  
                LineMaterialRequestOrder localOrderLine = (await conn.GetAllWithChildrenAsync<LineMaterialRequestOrder>(_ => _.InternalId.Equals(lineEntity.Id))).FirstOrDefault();
                LineMaterialRequestOrder line = new LineMaterialRequestOrder
                {
                    SQLiteRecordId = localOrderLine != null ? localOrderLine.SQLiteRecordId : 0,
                    InternalId = lineEntity.Id,
                    Material = product,
                    MaterialId = product?.SQLiteRecordId ?? 0,
                    Requested = lineEntity.Contains(Config.SPCLINEREQORDERMAT_REQUESTED) ? (int)(decimal)lineEntity[Config.SPCLINEREQORDERMAT_REQUESTED] : 0,
                    MaterialRequestOrderId = order.SQLiteRecordId,
                    MaterialCode = lineEntity.Contains(Config.SPCLINEREQORDERMAT_MATCODE) ? (string)lineEntity[Config.SPCLINEREQORDERMAT_MATCODE] : null
                };
                //await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// Obtains All equipment/material attached to a Service Ticket
        /// </summary>
        /// <param name="st">Service ticket where the materials/equipment are attached to</param>
        /// <returns>A Collection of equipment/materials of a service Ticket</returns>
        public static async Task<List<MaterialYRepuesto>> GetMaterialsFromServiceTicket(Guid st)
        {
            var conn = await Local.GetConnection();
            List<MaterialYRepuesto> materials = new List<MaterialYRepuesto>();
            string NameProduct = "Product";
            #region Query
            QueryExpression queryMaterials = new QueryExpression(Config.SPCMATERIAL)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCMATERIAL_PRODUCT,
                Config.SPCMATERIAL_QUANTITY,
                Config.SPCMATERIAL_TREATMENT,
                Config.SPCMATERIAL_SERIALS,
                Config.SPCMATERIAL_DESTINATION,
                Config.SPCMATERIAL_REQUESTNUMBER
            }),
                Criteria = new FilterExpression()
            };
            queryMaterials.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCMATERIAL,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCMATERIAL_PRODUCT,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
               {
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            queryMaterials.Criteria.FilterOperator = LogicalOperator.Or;
            queryMaterials.Criteria.AddCondition(new ConditionExpression(Config.SPCMATERIAL_SERVICETICKET, ConditionOperator.Equal, st));
            EntityCollection result = await Proxy.RetrieveMultiple(queryMaterials);
            #endregion
            foreach (Entity emat in result.Entities)
            {
                #region Material
                MaterialYRepuesto mat = new MaterialYRepuesto
                {
                    InternalId = emat.Id,
                    Product = new Product()
                };
                if (emat.Contains(Config.SPCMATERIAL_QUANTITY))
                    mat.Count = (int)emat[Config.SPCMATERIAL_QUANTITY];
                if (emat.Contains(Config.SPCMATERIAL_SERIALS))
                    mat.Serials = (string)emat[Config.SPCMATERIAL_SERIALS];
                if (emat.Contains(Config.SPCMATERIAL_REQUESTNUMBER))
                    mat.RequestNumber = (string)emat[Config.SPCMATERIAL_REQUESTNUMBER];
                if (emat.Contains(Config.SPCMATERIAL_TREATMENT))
                    mat.Treatment = (Types.SPCMATERIAL_TREATMENTOPTION)((OptionSetValue)emat[Config.SPCMATERIAL_TREATMENT]).Value;
                if (emat.Contains(Config.SPCMATERIAL_DESTINATION))
                    mat.Destination = (Types.SPCMATERIAL_DESTINATIONOPTION)((OptionSetValue)emat[Config.SPCMATERIAL_DESTINATION]).Value;
                MaterialYRepuesto lmat = (await conn.GetAllWithChildrenAsync<MaterialYRepuesto>(m => m.InternalId.Equals(mat.InternalId))).FirstOrDefault();
                if (lmat != null)
                    mat.SQLiteRecordId = lmat.SQLiteRecordId;
                #endregion
                #region Product             
                if (emat.Contains(Config.SPCMATERIAL_PRODUCT))
                    mat.Product.InternalId = ((EntityReference)emat[Config.SPCMATERIAL_PRODUCT]).Id;
                string concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_ID);
                if (emat.Contains(concatName))
                    mat.Product.Id = (string)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_NAME);
                if (emat.Contains(concatName))
                    mat.Product.Name = (string)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_BOUGHT);
                if (emat.Contains(concatName))
                    mat.Product.Bought = (string)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_COST);
                if (emat.Contains(concatName))
                    mat.Product.Cost = (decimal)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_MOSC);
                if (emat.Contains(concatName))
                    mat.Product.DoesHaveMOSC = ((string)((AliasedValue)emat[concatName]).Value).Equals('S');
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_MOSCPRICE);
                if (emat.Contains(concatName))
                    mat.Product.Cost = (decimal)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_DAITAX);
                if (emat.Contains(concatName))
                    mat.Product.DAITax = (decimal)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_LAWTAX);
                if (emat.Contains(concatName))
                    mat.Product.LawTax = (decimal)((AliasedValue)emat[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_SELECTIVETAX);
                if (emat.Contains(concatName))
                    mat.Product.SelectiveTax = (decimal)((AliasedValue)emat[concatName]).Value;
                Product local = await conn.Table<Product>().Where(p => p.InternalId.Equals(mat.Product.InternalId)).FirstOrDefaultAsync();
                if (local != null)
                    mat.Product.SQLiteRecordId = local.SQLiteRecordId;
                mat.UnitPrice = Config.CalculatePrice(mat.Product);
                await conn.InsertOrReplaceWithChildrenAsync(mat, true);
                #endregion
                materials.Add(mat);
            }
            return materials;
        }

        /// <summary>
        /// Obtains all extra equipment related to a cdt.
        /// </summary>
        /// <param name="cdt">CDT where extra equipment is attached to</param>
        /// <returns>A collection of extra equipment that is attached to a CDT</returns>
        public static async Task<List<ExtraEquipmentRequest>> GetExtraEquipmentFromCDT(Guid cdt)
        {
            var conn = await Local.GetConnection();
            List<ExtraEquipmentRequest> equipment = new List<ExtraEquipmentRequest>();
            string NameProduct = "Product";
            #region Query
            QueryExpression query = new QueryExpression(Config.SPCEXTRAEQUIPMENT)
            {
                ColumnSet = new ColumnSet(new string[] {
                Config.SPCEXTRAEQUIPMENT_ID,
                Config.SPCEXTRAEQUIPMENT_APPROVED,
                Config.SPCEXTRAEQUIPMENT_CDT,
                Config.SPCEXTRAEQUIPMENT_PROCESSTYPE,
                Config.SPCEXTRAEQUIPMENT_QUANTITY,
                Config.SPCEXTRAEQUIPMENT_REASON,
                Config.SPCEXTRAEQUIPMENT_EQUIPMENT
            }),
                Criteria = new FilterExpression()
            };
            query.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCEXTRAEQUIPMENT,
                LinkToEntityName = Config.SPCPRODUCT,
                LinkFromAttributeName = Config.SPCEXTRAEQUIPMENT_EQUIPMENT,
                LinkToAttributeName = Config.SPCPRODUCT_GUID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameProduct,
                Columns = new ColumnSet(new string[]
               {
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
               })
            });
            query.Criteria.FilterOperator = LogicalOperator.Or;
            query.Criteria.AddCondition(new ConditionExpression(Config.SPCEXTRAEQUIPMENT_CDT, ConditionOperator.Equal, cdt));
            EntityCollection result = await Proxy.RetrieveMultiple(query);
            #endregion
            foreach (Entity eeq in result.Entities)
            {
                #region Material
                ExtraEquipmentRequest eq = new ExtraEquipmentRequest
                {
                    InternalId = eeq.Id,
                    Equipment = new Product()
                };
                if (eeq.Contains(Config.SPCEXTRAEQUIPMENT_QUANTITY))
                    eq.Quantity = (int)(decimal)eeq[Config.SPCEXTRAEQUIPMENT_QUANTITY];
                if (eeq.Contains(Config.SPCEXTRAEQUIPMENT_REASON))
                    eq.Reason = (string)eeq[Config.SPCEXTRAEQUIPMENT_REASON];
                if (eeq.Contains(Config.SPCEXTRAEQUIPMENT_PROCESSTYPE))
                    eq.ProcessType = (Types.SPCEXTRAEQUIPMENT_PROCESSTYPE)((OptionSetValue)eeq[Config.SPCEXTRAEQUIPMENT_PROCESSTYPE]).Value;
                if (eeq.Contains(Config.SPCEXTRAEQUIPMENT_APPROVED))
                    eq.IsApproved = (bool)eeq[Config.SPCEXTRAEQUIPMENT_APPROVED];
                ExtraEquipmentRequest leq = (await conn.GetAllWithChildrenAsync<ExtraEquipmentRequest>(e => e.InternalId.Equals(eq.InternalId))).FirstOrDefault();
                if (leq != null)
                    eq.SQLiteRecordId = leq.SQLiteRecordId;
                #endregion
                #region Product             
                if (eeq.Contains(Config.SPCEXTRAEQUIPMENT_EQUIPMENT))
                    eq.Equipment.InternalId = ((EntityReference)eeq[Config.SPCEXTRAEQUIPMENT_EQUIPMENT]).Id;
                string concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_ID);
                if (eeq.Contains(concatName))
                    eq.Equipment.Id = (string)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_NAME);
                if (eeq.Contains(concatName))
                    eq.Equipment.Name = (string)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_BOUGHT);
                if (eeq.Contains(concatName))
                    eq.Equipment.Bought = (string)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_COST);
                if (eeq.Contains(concatName))
                    eq.Equipment.Cost = (decimal)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_MOSC);
                if (eeq.Contains(concatName))
                    eq.Equipment.DoesHaveMOSC = ((string)((AliasedValue)eeq[concatName]).Value).Equals('S');
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_MOSCPRICE);
                if (eeq.Contains(concatName))
                    eq.Equipment.Cost = (decimal)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_DAITAX);
                if (eeq.Contains(concatName))
                    eq.Equipment.DAITax = (decimal)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_LAWTAX);
                if (eeq.Contains(concatName))
                    eq.Equipment.LawTax = (decimal)((AliasedValue)eeq[concatName]).Value;
                concatName = string.Format("{0}.{1}", NameProduct, Config.SPCPRODUCT_SELECTIVETAX);
                if (eeq.Contains(concatName))
                    eq.Equipment.SelectiveTax = (decimal)((AliasedValue)eeq[concatName]).Value;
                Product local = (await conn.GetAllWithChildrenAsync<Product>(p => p.InternalId.Equals(eq.Equipment.InternalId))).FirstOrDefault();
                if (local != null)
                    eq.Equipment.SQLiteRecordId = local.SQLiteRecordId;
                await conn.InsertOrReplaceWithChildrenAsync(eq, true);
                #endregion
                equipment.Add(eq);
            }
            return equipment;
        }

        /// <summary>
        /// Obtains a set of products that match the word and currency type submitted in the CRM.
        /// </summary>
        /// <param name="word">Word to be used as filter in the name or id of the product</param>
        /// <param name="currencyType">Currency type that acts as filter for products that have several currency codes</param>
        /// <returns>A collection of Product View Models corresponding to the result of the search of product</returns>
        public static async Task<ObservableCollection<ProductViewModel>> GetProductsLikeExpression(string word, string currencyType)
        {
            var conn = await Local.GetConnection();
            ObservableCollection<ProductViewModel> resultList = new ObservableCollection<ProductViewModel>();
            #region Query
            QueryExpression queryProducts = new QueryExpression(Config.SPCPRODUCT)
            {
                ColumnSet = new ColumnSet(new string[]
                {
                    Config.SPCPRODUCT_ID,
                    Config.SPCPRODUCT_NAME,
                    Config.SPCPRODUCT_COST,
                    Config.SPCPRODUCT_DAITAX,
                    Config.SPCPRODUCT_LAWTAX,
                    Config.SPCPRODUCT_MOSC,
                    Config.SPCPRODUCT_MOSCPRICE,
                    Config.SPCPRODUCT_SELECTIVETAX,
                    Config.SPCPRODUCT_BOUGHT
                })
            };
            FilterExpression f1 = new FilterExpression
            {
                FilterOperator = LogicalOperator.And
            };
            f1.AddCondition(new ConditionExpression(Config.SPCPRODUCT_ID, ConditionOperator.Like, "%" + word + "%@" + currencyType));
            f1.AddCondition(new ConditionExpression(Config.SPCPRODUCT_STATE, ConditionOperator.Equal, 0));

            string[] fragments = word.Split(' ');
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = String.Format("%{0}%", fragments[i]);
            }
            FilterExpression f2 = new FilterExpression
            {
                FilterOperator = LogicalOperator.And
            };
            foreach (string fragment in fragments)
                f2.AddCondition(new ConditionExpression(Config.SPCPRODUCT_NAME, ConditionOperator.Like, fragment));
            f2.AddCondition(new ConditionExpression(Config.SPCPRODUCT_ID, ConditionOperator.Like, "%@" + currencyType + "%"));
            f2.AddCondition(new ConditionExpression(Config.SPCPRODUCT_STATE, ConditionOperator.Equal, 0));
            queryProducts.Criteria.AddFilter(f1);
            queryProducts.Criteria.AddFilter(f2);
            queryProducts.Criteria.FilterOperator = LogicalOperator.Or;
            EntityCollection result = await Proxy.RetrieveMultiple(queryProducts);
            #endregion
            foreach (Entity eProduct in result.Entities)
            {
                Product newProduct = new Product
                {
                    InternalId = eProduct.Id
                };
                if (eProduct.Contains(Config.SPCPRODUCT_NAME))
                    newProduct.Name = (string)eProduct[Config.SPCPRODUCT_NAME];
                if (eProduct.Contains(Config.SPCPRODUCT_ID))
                    newProduct.Id = (string)eProduct[Config.SPCPRODUCT_ID];
                if (eProduct.Contains(Config.SPCPRODUCT_BOUGHT))
                    newProduct.Bought = (string)eProduct[Config.SPCPRODUCT_BOUGHT];
                if (eProduct.Contains(Config.SPCPRODUCT_COST))
                    newProduct.Cost = (decimal)eProduct[Config.SPCPRODUCT_COST];
                if (eProduct.Contains(Config.SPCPRODUCT_MOSC))
                    newProduct.DoesHaveMOSC = ((string)eProduct[Config.SPCPRODUCT_MOSC]).Equals('S');
                if (eProduct.Contains(Config.SPCPRODUCT_MOSCPRICE) && newProduct.DoesHaveMOSC)
                    newProduct.Cost = (decimal)eProduct[Config.SPCPRODUCT_MOSCPRICE];
                if (eProduct.Contains(Config.SPCPRODUCT_DAITAX))
                    newProduct.DAITax = (decimal)eProduct[Config.SPCPRODUCT_DAITAX];
                if (eProduct.Contains(Config.SPCPRODUCT_LAWTAX))
                    newProduct.LawTax = (decimal)eProduct[Config.SPCPRODUCT_LAWTAX];
                if (eProduct.Contains(Config.SPCPRODUCT_SELECTIVETAX))
                    newProduct.SelectiveTax = (decimal)eProduct[Config.SPCPRODUCT_SELECTIVETAX];
                Product local = (await conn.GetAllWithChildrenAsync<Product>(p => p.InternalId.Equals(newProduct.InternalId))).FirstOrDefault();
                if (local != null)
                    newProduct.SQLiteRecordId = local.SQLiteRecordId;
                await conn.InsertOrReplaceWithChildrenAsync(newProduct, true);
                resultList.Add(new ProductViewModel(newProduct));
            }
            return resultList;
        }

        /// <summary>
        /// Obtains a set of products that match the word and currency type submitted in the local storage.
        /// </summary>
        /// <param name="word">Word to be used as filter in the name or id of the product</param>
        /// <param name="currencyType">Currency type that acts as filter for products that have several currency codes</param>
        /// <returns>A collection of Product View Models corresponding to the result of the search of product</returns>
        public static async Task<ObservableCollection<ProductViewModel>> GetProductsLikeExpressionOffline(string word, string currencyType)
        {
            ObservableCollection<ProductViewModel> resultList = new ObservableCollection<ProductViewModel>();
            var conn = await Local.GetConnection();
            string end = string.Format("@{0}", currencyType);
            List<Product> currencyProducts = await conn.Table<Product>().Where(p => p.Id.EndsWith(end)).ToListAsync();
            string[] fragments = word.Split(' ');
            for (int i = 0; i < fragments.Length; i++)
                fragments[i] = fragments[i].ToUpper();
            foreach (Product p in currencyProducts)
                foreach (string f in fragments)
                    if (p.Name.ToUpper().Contains(f) || p.Id.ToUpper().Contains(f))
                    {
                        resultList.Add(new ProductViewModel(await conn.GetWithChildrenAsync<Product>(p.SQLiteRecordId, true)));
                        break;
                    }
            return resultList;
        }

        /// <summary>
        /// Creates a new Material/Equipment in the CRM
        /// </summary>
        /// <param name="serviceTicketId">local service ticket id related to this new material.</param>
        /// <param name="material">material to be created</param>
        /// <returns>Material model object result of the creation process</returns>
        public static async Task<MaterialYRepuesto> CreateNewMaterial(int serviceTicketId, MaterialYRepuesto material)
        {
            var conn = await Local.GetConnection();
            ServiceTicket st = await conn.GetWithChildrenAsync<ServiceTicket>(serviceTicketId, true);
            Product product = (await conn.GetAllWithChildrenAsync<Product>(p => p.InternalId.Equals(material.Product.InternalId), true)).FirstOrDefault();
            if (product != null)
                material.Product = product;
            EntityReference EProduct = new EntityReference(Config.SPCPRODUCT, material.Product.InternalId);
            EntityReference EServiceTicket = new EntityReference(Config.SPCSERVTICKET, st.InternalId);
            Entity New_Material = new Entity(Config.SPCMATERIAL);
            New_Material[Config.SPCMATERIAL_PRODUCT] = EProduct; //required
            New_Material[Config.SPCMATERIAL_SERVICETICKET] = EServiceTicket;            //required
            New_Material[Config.SPCMATERIAL_QUANTITY] = material.Count;         //required
            New_Material[Config.SPCMATERIAL_TREATMENT] = new OptionSetValue((int)material.Treatment);
            if (material.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Desmonte)
                New_Material[Config.SPCMATERIAL_DESTINATION] = new OptionSetValue((int)material.Destination);
            if (!string.IsNullOrEmpty(material.Serials))
                New_Material[Config.SPCMATERIAL_SERIALS] = material.Serials;
            if (!string.IsNullOrEmpty(material.RequestNumber))
                New_Material[Config.SPCMATERIAL_REQUESTNUMBER] = material.RequestNumber;
            material.InternalId = await Proxy.Create(New_Material);
            material.ProductId = material.Product.SQLiteRecordId;
            material.ServiceTicketId = st.SQLiteRecordId;
            await conn.InsertOrReplaceWithChildrenAsync(material, true);
            return material;
        }

        /// <summary>
        /// Creates a new Material/Equipment locally so it can be created in the CRM afterwards
        /// </summary>
        /// <param name="serviceTicketId">local service ticket id related to this new material.</param>
        /// <param name="material">material to be created</param>
        /// <returns>Material model object result of the creation process</returns>
        public static async Task<MaterialYRepuesto> CreateNewMaterialOffline(int serviceTicketId, MaterialYRepuesto material)
        {
            var conn = await Local.GetConnection();
            material.ProductId = material.Product.SQLiteRecordId;
            material.ServiceTicketId = serviceTicketId;
            await conn.InsertOrReplaceWithChildrenAsync(material);
            await RegisterPendingOperation(Types.CRUDOperation.Create, material.SQLiteRecordId, nameof(MaterialYRepuesto));
            return material;
        }

        /// <summary>
        /// Obtains the service ticket code assigned to a specific service ticket
        /// </summary>
        /// <param name="guid">Id of the service ticket to be inspect or retrieved</param>
        /// <returns>A string literal corresponding to the code of a service ticket</returns>
        public static async Task<string> GetServiceTicketCode(Guid guid)
        {
            Entity result = await Proxy.Retrieve(Config.SPCSERVTICKET, guid, new ColumnSet(Config.SPCSERVTICKET_CODE));
            return result.Contains(Config.SPCSERVTICKET_CODE) ? (string)result[Config.SPCSERVTICKET_CODE] : "-";
        }

        /// <summary>
        /// Add a report to an incident
        /// </summary>
        /// <param name="note">Note to be created in the CRM</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> AddReportToIncident(Note note)
        {
            Entity annotation = new Entity(Config.SPCNOTES);
            annotation.Attributes[Config.SPCNOTES_FILENAME] = note.Filename;
            annotation.Attributes[Config.SPCNOTES_CONTENT] = note.Content;
            annotation.Attributes[Config.SPCNOTES_MIME] = "application/pdf";
            annotation.Attributes[Config.SPCNOTES_LINK] = new EntityReference(Config.SPCCASE, note.ObjectId);
            try
            {
                Guid internalId = await Proxy.Create(annotation);
                note.InternalId = internalId;
            }
            catch (Exception)
            {
                return false;
            }
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(note);
            return true;
        }

        /// <summary>
        /// Add a report to a CDT
        /// </summary>
        /// <param name="note">Note to be added</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> AddReportToCDT(Note note)
        {
            Entity annotation = new Entity(Config.SPCNOTES);
            annotation.Attributes[Config.SPCNOTES_FILENAME] = note.Filename;
            annotation.Attributes[Config.SPCNOTES_CONTENT] = note.Content;
            annotation.Attributes[Config.SPCNOTES_MIME] = "application/pdf";
            annotation.Attributes[Config.SPCNOTES_LINK] = new EntityReference(Config.SPCCDT, note.ObjectId);
            try
            {
                Guid internalId = await Proxy.Create(annotation);
                note.InternalId = internalId;
            }
            catch (Exception)
            {
                return false;
            }
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(note);
            return true;
        }

        /// <summary>
        /// Registers a pending operation of a service ticket report upload so it can be done when internet is back again.
        /// </summary>
        /// <param name="note">Report that needs to be locally stored</param>
        /// <returns>Void</returns>
        public static async Task AddServiceTicketSignedOffline(Note note)
        {
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(note);
            await RegisterPendingOperation(Types.CRUDOperation.Create, note.SQLiteRecordId, nameof(Note), nameof(Incident));
        }

        /// <summary>
        /// Registers a pending operation of a service ticket legalization report upload so it can be done once the internet is back
        /// </summary>
        /// <param name="note">Legalization Report that needs to be uploaded</param>
        /// <returns>Void</returns>
        public static async Task AddLegalizationReportOffline(Note note)
        {
            var conn = await Local.GetConnection();
            await conn.InsertOrReplaceWithChildrenAsync(note);
            await RegisterPendingOperation(Types.CRUDOperation.Create, note.SQLiteRecordId, nameof(Note), nameof(Incident));
        }

        /// <summary>
        /// Closes an active service ticket in the CRM
        /// </summary>
        /// <param name="st">Service ticket needed to be closed</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> FinishServiceTicket(ServiceTicket st)
        {
            var conn = await Local.GetConnection();
            Entity EditedServiceTicket = new Entity(Config.SPCSERVTICKET, st.InternalId);
            EditedServiceTicket[Config.SPCSERVTICKET_FINISHED] = st.Finished;
            if (!String.IsNullOrEmpty(st.Email))
                EditedServiceTicket[Config.SPCSERVTICKET_EMAIL] = st.Email;
            EditedServiceTicket[Config.SPCSERVTICKET_QUOTATION] = st.RequiresQuotation;
            await Proxy.Update(EditedServiceTicket);
            await conn.UpdateWithChildrenAsync(st);
            return true;
        }

        /// <summary>
        /// Closes a Service ticket locally so it can be closed in the CRM once the internet is back
        /// </summary>
        /// <param name="st">Service ticket wanted to be closed</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> FinishServiceTicketOffline(ServiceTicket st)
        {
            var conn = await Local.GetConnection();
            await conn.UpdateWithChildrenAsync(st);
            await RegisterPendingOperation(Types.CRUDOperation.Update, st.SQLiteRecordId, nameof(ServiceTicket), "close");
            return true;
        }

        /// <summary>
        /// Obtains a list of PARTIAL clients that meet the alias provided.
        /// </summary>
        /// <param name="alias">Alias needed to be check in the account entity</param>
        /// <returns>A collection of clients result of the search</returns>
        public static async Task<List<ClientViewModel>> SearchClientAccounts(string alias)
        {
            List<ClientViewModel> resultClients = new List<ClientViewModel>();
            string[] fragments = alias.Split(' ');
            for (int i = 0; i < fragments.Length; i++)
                fragments[i] = String.Format("%{0}%", fragments[i]);
            QueryExpression queryClientAccounts = new QueryExpression(Config.SPCACCOUNT)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCACCOUNT_ALIAS }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };
            foreach (string fragment in fragments)
                queryClientAccounts.Criteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_ALIAS, ConditionOperator.Like, fragment));
            queryClientAccounts.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            EntityCollection eClients = await Proxy.RetrieveMultiple(queryClientAccounts);
            foreach (Entity eclient in eClients.Entities)
                resultClients.Add(new ClientViewModel(new Client
                {
                    InternalId = eclient.Id,
                    Alias = eclient.Contains(Config.SPCACCOUNT_ALIAS) ? (string)eclient[Config.SPCACCOUNT_ALIAS] : "Sin nombre"
                }));
            return resultClients;
        }

        /// <summary>
        /// Creates a new client ticket in the CRM so the CRM creates a new incident based on the ticket created. 
        /// </summary>
        /// <param name="incident">Incident needed to be created as ticket</param>
        /// <returns>An incident result of the creation of the ticket</returns>
        public static async Task<IncidentViewModel> CreateNewTicket(IncidentViewModel incident)
        {
            Guid resultId = default(Guid);
            Entity ticket = new Entity(Config.SPCTICKET);
            ticket[Config.SPCTICKET_CLIENT] = new EntityReference(Config.SPCACCOUNT, incident.Client.InternalId);
            ticket[Config.SPCTICKET_EXPECTEDDATE] = DateTime.Now;
            ticket[Config.SPCTICKET_NEED] = incident.Description;
            ticket[Config.SPCTICKET_TITLE] = incident.Title;
            ticket[Config.SPCTICKET_PRIORITY] = new OptionSetValue(2); //Alta
            ticket[Config.SPCTICKET_TECHNICIANS] = true;
            resultId = await Proxy.Create(ticket);
            Thread.Sleep(3000);
            QueryExpression queryCase = new QueryExpression(Config.SPCCASE)
            {
                ColumnSet = new ColumnSet(Config.SPCCASE_TICKET),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or
                }
            };
            queryCase.Criteria.AddCondition(new ConditionExpression(Config.SPCCASE_TICKET, ConditionOperator.Equal, resultId));
            EntityCollection result = await Proxy.RetrieveMultiple(queryCase);
            if (result.Entities.Count == 0)
                throw new Exception("No match found");
            return new IncidentViewModel(await GetIncident(result.Entities[0].Id));
        }

        /// <summary>
        /// Creates a new technician registry in the CRM.
        /// </summary>
        /// <param name="registry">Registry needed to be created</param>
        /// <param name="cdt">CDT related to the registry</param>
        /// <param name="ticket">CDT Ticket related to the registry</param>
        /// <returns></returns>
        public static async Task<TechnicianRegistry> CreateNewTechnicianRegistry(TechnicianRegistry registry, CDT cdt, CDTTicket ticket)
        {
            var conn = await Local.GetConnection();
            Guid resultId = default(Guid);
            Entity reg = new Entity(Config.SPCTECHREGISTRY);
            //reg[Config.SPCTECHREGISTRY_FINISHED] = registry.Finished;
            reg[Config.SPCTECHREGISTRY_STARTED] = ticket.Started;
            reg[Config.SPCTECHREGISTRY_TECH] = new EntityReference(Config.SPCTECHNICIAN, registry.Technician.InternalId);
            reg[Config.SPCTECHREGISTRY_TICKET] = new EntityReference(Config.SPCCDTTICKET, ticket.InternalId);
            reg[Config.SPCTECHREGISTRY_CDT] = new EntityReference(Config.SPCCDT, cdt.InternalId);
            resultId = await Proxy.Create(reg);
            registry.InternalId = resultId;
            registry.CDTTicketId = ticket.SQLiteRecordId;
            registry.TechnicianId = registry.Technician.SQLiteRecordId;
            registry.Started = ticket.Started;
            await conn.InsertOrReplaceWithChildrenAsync(registry);
            return registry;
        }

        /// <summary>
        /// Sign a legalization by the currently logged in user
        /// </summary>
        /// <param name="internalId">Id of the legalization needed to be signed</param>
        /// <returns>Void</returns>
        public static async Task SignLegalizationByOwner(Guid internalId)
        {
            Entity legalization = new Entity(Config.SPCLEGALIZATION, internalId);
            legalization[Config.SPCLEGALIZATION_LEGALIZATORSIGN] = true;
            try
            {
                await Proxy.Update(legalization);
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
           
        }

        /// <summary>
        /// Connects to CRM and creates a new Legalization Record.
        /// </summary>
        /// <param name="toCreate">Legalization object to be created by CRM</param>
        /// <param name="linkToIdentifier">Integer identifier that indicates whether the legalization is link to a CDT or an Incident</param>
        /// <returns>Legalization object with the information of the record created by the CRM.</returns>
        public static async Task<Legalization> CreateLegalization(Legalization toCreate, int linkToIdentifier)
        {
            if (toCreate.LegalizationType == Types.SPCLEGALIZATION_TYPE.Undefined)
                throw new Exception("Debe seleccionar un tipo de legalización");
            if (toCreate.MoneyCurrency == null)
                throw new Exception("No se ha seleccionado divisa para esta legalización.");
            if (toCreate.LegalizationType == Types.SPCLEGALIZATION_TYPE.TarjetaEmpresarial && (string.IsNullOrEmpty(toCreate.LastCreditCardDigits) || toCreate.LastCreditCardDigits.Length < 4))
                throw new Exception("No se ha ingresado los últimos 4 dígitos de la tarjeta empresarial.");
            if (toCreate.Company == null)
                throw new Exception("No se ha seleccionado compañía para esta legalización.");
            if (string.IsNullOrEmpty(toCreate.Detail))
                throw new Exception("Debe ingresar un detalle para la legalización.");
            var conn = await Local.GetConnection();
            Guid resultId = default(Guid);
            Entity legalization = new Entity(Config.SPCLEGALIZATION, toCreate.InternalId);
            legalization[Config.SPCLEGALIZATION_DETAIL] = toCreate.Detail;
            legalization[Config.SPCLEGALIZATION_PROJECTISSUE] = toCreate.ProjectIssue;
            legalization[Config.SPCLEGALIZATION_TYPE] = new OptionSetValue((int)toCreate.LegalizationType);
            legalization[Config.SPCLEGALIZATION_CURRENCYID] = new EntityReference(Config.SPCCURRENCY, toCreate.MoneyCurrency.InternalId);
            legalization[Config.SPCLEGALIZATION_COMPANY] = new EntityReference(Config.SPCBUSINESSUNIT, toCreate.Company.InternalId);
            legalization[Config.SPCLEGALIZATION_MONEYREQUESTED] = new Money(toCreate.MoneyRequested);
            if (toCreate.LegalizationType == Types.SPCLEGALIZATION_TYPE.TarjetaEmpresarial)
                legalization[Config.SPCLEGALIZATION_LASTCREDITCARDNUMBERS] = toCreate.LastCreditCardDigits;
            switch (linkToIdentifier)
            {
                case 1: //related to Incident
                    if (toCreate.RelatedIncident == null)
                        throw new Exception("Debe relacionar un caso a esta legalización.");
                    legalization[Config.SPCLEGALIZATION_INCIDENT_ID] = new EntityReference(Config.SPCCASE, toCreate.RelatedIncident.InternalId);
                    break;
                case 2: //related to CDT
                    if (toCreate.RelatedCDT == null)
                        throw new Exception("Debe relacionar un CDT a esta legalización.");
                    legalization[Config.SPCLEGALIZATION_CDT_ID] = new EntityReference(Config.SPCCDT, toCreate.RelatedCDT.InternalId);
                    break;
                default:
                    break;
            }
            try
            {
                resultId = await Proxy.Create(legalization);
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
            legalization = await Proxy.Retrieve(Config.SPCLEGALIZATION, resultId, new ColumnSet(true)); //this will not crash the app as is used in a try-catch block where this is called.
            toCreate.InternalId = resultId;
            toCreate.LegalizationNumber = legalization[Config.SPCLEGALIZATION_NUMBER].ToString();
            await conn.InsertOrReplaceWithChildrenAsync(toCreate);
            return toCreate;
        }

        /// <summary>
        /// Obtains a CRM-based collection of the legalization items records related to an especific legalization through its Guid
        /// </summary>
        /// <param name="legalizationId">Guid of the legalization record</param>
        /// <returns>Collection of entities that represent all the legalization items related to a legalization.</returns>
        public static async Task<EntityCollection> GetLegalizationItemsQuery(Guid legalizationId)
        {
            try
            {
                return await Proxy.RetrieveMultiple(new QueryExpression
                {
                    ColumnSet = new ColumnSet(new string[]
                     {
                     Config.SPCLEGALIZATIONITEM_BILL,
                     Config.SPCLEGALIZATIONITEM_CURRENCY,
                     Config.SPCLEGALIZATIONITEM_ID,
                     Config.SPCLEGALIZATIONITEM_LEGALIZATION,
                     Config.SPCLEGALIZATIONITEM_MONEYSPENT,
                     Config.SPCLEGALIZATIONITEM_PAIDTO,
                     Config.SPCLEGALIZATIONITEM_PROJECTISSUE,
                     Config.SPCLEGALIZATIONITEM_SPENTON,
                     Config.SPCLEGALIZATIONITEM_TYPE
                     }),
                    EntityName = Config.SPCLEGALIZATIONITEM,
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions = new DataCollection<ConditionExpression>
                     {
                         new ConditionExpression(Config.SPCLEGALIZATIONITEM_LEGALIZATION, ConditionOperator.Equal, legalizationId),
                         new ConditionExpression(Config.GENERAL_STATECODE, ConditionOperator.Equal, 0)
                     }
                    }
                });
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
        }

        /// <summary>
        /// Obtains a model collection of legalization items related to a specific legalization.
        /// </summary>
        /// <param name="legalizationId">Guid of the legalization record that is going to be used as filter</param>
        /// <returns>List of legalization items model objects related to a legalization</returns>
        public static async Task<List<LegalizationItem>> GetLegalizationItems(Guid legalizationId)
        {
            List<LegalizationItem> legalizationItems = new List<LegalizationItem>();
            EntityCollection queryresult = await GetLegalizationItemsQuery(legalizationId);
            foreach (Entity item in queryresult.Entities)
                legalizationItems.Add(await BuildLegalizationItem(item));
            return legalizationItems;
        }

        /// <summary>
        /// Obtains a ViewModel collection of legalization items related to a specific legalization.
        /// </summary>
        /// <param name="legalizationId">Guid of the legalization record that is going to be used as filter</param>
        /// <returns>Observable collection of legalization items viewmodels related to a legalization</returns>
        public static async Task<ObservableCollection<LegalizationItemViewModel>> GetLegalizationItemsViewModel(Guid legalizationId)
        {
            ObservableCollection<LegalizationItemViewModel> legalizationItemsViewModels = new ObservableCollection<LegalizationItemViewModel>();
            List<LegalizationItem> legalizationItems = await GetLegalizationItems(legalizationId);
            foreach (LegalizationItem item in legalizationItems)
                legalizationItemsViewModels.Add(new LegalizationItemViewModel(item));
            return legalizationItemsViewModels;
        }

        /// <summary>
        /// Converts and entity previously obtained into a legalization item
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns>Legalization item result of the conversion</returns>
        private static async Task<LegalizationItem> BuildLegalizationItem(Entity entity) //if used on an entity that is definitely not a Legalization Item this might crash
        {
            var conn = await Local.GetConnection();
            Currency currency = null;
            Guid legId = ((EntityReference)entity[Config.SPCLEGALIZATIONITEM_LEGALIZATION]).Id;
            Legalization legalization = (await conn.GetAllWithChildrenAsync<Legalization>(leg => leg.InternalId.Equals(legId))).FirstOrDefault();
            if (entity.Contains(Config.SPCLEGALIZATIONITEM_CURRENCY))
                currency = await GetCurrency(((EntityReference)entity[Config.SPCLEGALIZATIONITEM_CURRENCY]).Id);
            LegalizationItem local = (await conn.GetAllWithChildrenAsync<LegalizationItem>(li => li.InternalId.Equals(entity.Id))).FirstOrDefault();
            LegalizationItem legalizationItem = new LegalizationItem
            {
                Bill = entity.Contains(Config.SPCLEGALIZATIONITEM_BILL) ? (string)entity[Config.SPCLEGALIZATIONITEM_BILL] : string.Empty,
                Amount = entity.Contains(Config.SPCLEGALIZATIONITEM_MONEYSPENT) ? ((Money)entity[Config.SPCLEGALIZATIONITEM_MONEYSPENT]).Value : 0,
                ExpenseType = entity.Contains(Config.SPCLEGALIZATIONITEM_TYPE) ? (Types.SPCLEGALIZATIONITEM_TYPE)((OptionSetValue)entity[Config.SPCLEGALIZATIONITEM_TYPE]).Value : Types.SPCLEGALIZATIONITEM_TYPE.Undefined,
                InternalId = entity.Id,
                PaidTo = entity.Contains(Config.SPCLEGALIZATIONITEM_PAIDTO) ? (string)entity[Config.SPCLEGALIZATIONITEM_PAIDTO] : string.Empty,
                ProjectIssue = entity.Contains(Config.SPCLEGALIZATIONITEM_PROJECTISSUE) ? (bool)entity[Config.SPCLEGALIZATIONITEM_PROJECTISSUE] : false,
                SpentOn = entity.Contains(Config.SPCLEGALIZATIONITEM_SPENTON) ? (DateTime)entity[Config.SPCLEGALIZATIONITEM_SPENTON] : default(DateTime),
                Currency = currency,
                CurrencyId = currency.SQLiteRecordId,
                SQLiteRecordId = local?.SQLiteRecordId ?? 0,
                LegalizationId = legalization?.SQLiteRecordId ?? 0
            };
            await conn.InsertOrReplaceWithChildrenAsync(legalizationItem);
            return legalizationItem;
        }

        /// <summary>
        /// Creates a new record of a legalization item in CRM. 
        /// </summary>
        /// <param name="linkedTo">Legalization of which the legalization item is related.</param>
        /// <param name="toCreate"></param>
        /// <returns></returns>
        public static async Task<LegalizationItem> CreateLegalizationItem(Legalization linkedTo, LegalizationItem toCreate)
        {
            if (toCreate.ExpenseType == Types.SPCLEGALIZATIONITEM_TYPE.Undefined)
                throw new Exception("Debe seleccionar un tipo de gasto");
            if (toCreate.Currency == null)
                throw new Exception("No se ha seleccionado divisa para este gasto.");
            if (string.IsNullOrEmpty(toCreate.Bill))
                throw new Exception("No se ha ingresado el numero de factura.");
            if (string.IsNullOrEmpty(toCreate.PaidTo))
                throw new Exception("No se ha ingresado la empresa o tienda donde se realizo el gasto.");
            var conn = await Local.GetConnection();
            Guid resultId = default(Guid);
            Entity legalizationitem = new Entity(Config.SPCLEGALIZATIONITEM, toCreate.InternalId);
            legalizationitem[Config.SPCLEGALIZATIONITEM_BILL] = toCreate.Bill;
            legalizationitem[Config.SPCLEGALIZATIONITEM_CURRENCY] = new EntityReference(Config.SPCCURRENCY, toCreate.Currency.InternalId);
            legalizationitem[Config.SPCLEGALIZATIONITEM_LEGALIZATION] = new EntityReference(Config.SPCLEGALIZATION, linkedTo.InternalId);
            legalizationitem[Config.SPCLEGALIZATIONITEM_MONEYSPENT] = new Money(toCreate.Amount);
            legalizationitem[Config.SPCLEGALIZATIONITEM_PAIDTO] = toCreate.PaidTo;
            legalizationitem[Config.SPCLEGALIZATIONITEM_PROJECTISSUE] = toCreate.ProjectIssue;
            legalizationitem[Config.SPCLEGALIZATIONITEM_SPENTON] = toCreate.SpentOn;
            legalizationitem[Config.SPCLEGALIZATIONITEM_TYPE] = new OptionSetValue((int)toCreate.ExpenseType);
            try
            {
                resultId = await Proxy.Create(legalizationitem);
                legalizationitem = await Proxy.Retrieve(Config.SPCLEGALIZATIONITEM, resultId, new ColumnSet(true));
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
            return await BuildLegalizationItem(legalizationitem);
        }

        /// <summary>
        /// Creates an Extra Equipment record in CRM.
        /// </summary>
        /// <param name="toadd">ExtraEquipment model object to be created by the CRM</param>
        /// <param name="cdt">CDT of which the ExtraEquipment is related to.</param>
        /// <returns></returns>
        public static async Task<ExtraEquipmentRequest> CreateNewExtraEquipment(ExtraEquipmentRequest toadd, CDT cdt)
        {
            var conn = await Local.GetConnection();
            Guid resultId = default(Guid);
            Entity reg = new Entity(Config.SPCEXTRAEQUIPMENT);
            reg[Config.SPCEXTRAEQUIPMENT_QUANTITY] = (decimal)toadd.Quantity;
            if (toadd.ProcessType != Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Undefined)
                reg[Config.SPCEXTRAEQUIPMENT_PROCESSTYPE] = new OptionSetValue((int)toadd.ProcessType);
            if (!string.IsNullOrEmpty(toadd.Reason))
                reg[Config.SPCEXTRAEQUIPMENT_REASON] = toadd.Reason;
            reg[Config.SPCEXTRAEQUIPMENT_EQUIPMENT] = new EntityReference(Config.SPCPRODUCT, toadd.Equipment.InternalId);
            reg[Config.SPCEXTRAEQUIPMENT_CDT] = new EntityReference(Config.SPCCDT, cdt.InternalId);
            try
            {
                resultId = await Proxy.Create(reg);
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
            toadd.InternalId = resultId;
            toadd.EquipmentId = toadd.Equipment.SQLiteRecordId;
            await conn.InsertOrReplaceWithChildrenAsync(toadd);
            return toadd;
        }

        /// <summary>
        /// Obtains an Observable collection of ALL Business Unit Viewmodels of SPC stored in the CRM.
        /// </summary>
        /// <returns>An ObservableCollection of viewmodels</returns>
        public static async Task<ObservableCollection<CompanyViewModel>> GetCompaniesViewModel()
        {
            ObservableCollection<CompanyViewModel> result = new ObservableCollection<CompanyViewModel>();
            List<Company> companies = await GetCompanies();
            foreach (Company company in companies)
                result.Add(new CompanyViewModel(company));
            return result;
        }

        /// <summary>
        /// Obtains a single Business Unit(BU) where its identifier is equal to the one provide in the params.
        /// </summary>
        /// <param name="internalId">Guid of the Business Unit (BU) to be obtained</param>
        /// <returns>Company model object that corresponds to the id provided</returns>
        public static async Task<Company> GetCompany(Guid internalId)
        {
            var conn = await Local.GetConnection();
            Company Company = (await conn.GetAllWithChildrenAsync<Company>(com => com.InternalId.Equals(internalId))).FirstOrDefault();
            if (Company == null)
            {
                List<Company> companies = await GetCompanies();
                Company = companies.Where(com => com.InternalId.Equals(internalId)).FirstOrDefault();
                if (Company == null)
                    throw new Exception("Compañía no encontrada, posiblemente desactivada o eliminada.");
            }
            return Company;
        }

        /// <summary>
        /// Obtains a collection of ALL the Business Units as model objects.
        /// </summary>
        /// <returns>A collection of ALL the Business Units</returns>
        public static async Task<List<Company>> GetCompanies()
        {
            var conn = await Local.GetConnection();
            List<Company> result = new List<Company>();
            EntityCollection queryResult = await GetCompaniesQuery();
            foreach (Entity entity in queryResult.Entities)
            {
                Company company = BuildCompanyFromEntity(entity);
                Company localStorageCompany = (await conn.GetAllWithChildrenAsync<Company>(com => com.InternalId.Equals(company.InternalId))).FirstOrDefault();
                if (localStorageCompany != null)
                    company.SQLiteRecordId = localStorageCompany.SQLiteRecordId;
                await conn.InsertOrReplaceWithChildrenAsync(company);
                result.Add(company);
            }
            return result;
        }

        /// <summary>
        /// Converts an entity to a Business Unit (BU) model object
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns>Object result of the conversion</returns>
        private static Company BuildCompanyFromEntity(Entity entity) => //notice this method might crash the app if the entity provided is definitely not a BU
            new Company
            {
                InternalId = entity.Contains(Config.SPCBUSINESSUNIT_ID) ? (Guid)entity[Config.SPCBUSINESSUNIT_ID] : default(Guid),
                Name = entity.Contains(Config.SPCBUSINESSUNIT_NAME) ? (string)entity[Config.SPCBUSINESSUNIT_NAME] : string.Empty
            };

        /// <summary>
        /// Obtains a collection of entities that correspond to all the business units created in the CRM
        /// </summary>
        /// <returns>Collection of business units as entities</returns>
        public static async Task<EntityCollection> GetCompaniesQuery() //Be careful that if BU in the CRM grow to a more considerable size this method might have memory issues.
        {
            try
            {
                return await Proxy.RetrieveMultiple(new QueryExpression(Config.SPCBUSINESSUNIT)
                {
                    ColumnSet = new ColumnSet(new string[] {
                        Config.SPCBUSINESSUNIT_ID,
                        Config.SPCBUSINESSUNIT_NAME
                    }),
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                    }
                }
            );
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }        
        }

        /// <summary>
        /// Saves the changes mate to a Technician Registry in the CRM.
        /// </summary>
        /// <param name="registry">Registry to be saved or updated. Must contain the changes to be made.</param>
        /// <returns>A boolean value indicating the result of this operation</returns>
        public static async Task<bool> SaveChangesTechnicianRegistry(TechnicianRegistry registry)
        {
            var conn = await Local.GetConnection();
            Entity reg = new Entity(Config.SPCTECHREGISTRY, registry.InternalId);
            reg[Config.SPCTECHREGISTRY_STARTED] = registry.Started;
            reg[Config.SPCTECHREGISTRY_FINISHED] = registry.Finished;
            reg[Config.SPCTECHREGISTRY_HOURS_NORMAL] = registry.HoursNormal;
            reg[Config.SPCTECHREGISTRY_HOURS_NORMAL_NIGHT] = registry.HoursNormalNight;
            reg[Config.SPCTECHREGISTRY_DAYTIME_EXTRA] = registry.HoursDaytimeExtra;
            reg[Config.SPCTECHREGISTRY_NIGHT_EXTRA] = registry.HoursNightExtra;
            reg[Config.SPCTECHREGISTRY_HOLYDAY_DAYTIME] = registry.HoursHolydayDaytime;
            reg[Config.SPCTECHREGISTRY_HOLYDAY_NIGHT] = registry.HoursHolydayNight;
            reg[Config.SPCTECHREGISTRY_DAYTIME_OFFDAY] = registry.HoursOffdayDaytime;
            reg[Config.SPCTECHREGISTRY_DAYTIME_OFFDAY_EXTRA] = registry.HoursOffdayDaytimeExtra;
            reg[Config.SPCTECHREGISTRY_NIGHT_OFFDAY] = registry.HoursOffdayNight;
            reg[Config.SPCTECHREGISTRY_NIGHT_OFFDAY_EXTRA] = registry.HoursOffdayNightExtra;
            await Proxy.Update(reg);
            await conn.InsertOrReplaceWithChildrenAsync(registry);
            return true;
        }

        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Category> GetCategory(Guid id)
        {
            var conn = await Local.GetConnection();
            Category found = (await conn.GetAllWithChildrenAsync<Category>(cat => cat.InternalId.Equals(id), true)).FirstOrDefault();
            LogTable lastupdate = await conn.FindAsync<LogTable>(nameof(Category));
            if (lastupdate == null || lastupdate.LastTimeUpdate.AddDays(2) < DateTime.Now || found == null)
            {
                #region Query
                QueryExpression getCategories = new QueryExpression(Config.SPCCATEGORY)
                {
                    ColumnSet = new ColumnSet(new string[] { Config.SPCCATEGORY_CODE, Config.SPCCATEGORY_NAME })
                };
                EntityCollection res = await Proxy.RetrieveMultiple(getCategories);
                #endregion
                List<Category> LocalCategories = await conn.GetAllWithChildrenAsync<Category>(recursive: true);
                foreach (Entity eCat in res.Entities)
                {
                    string catCode = eCat.Contains(Config.SPCCATEGORY_CODE) ? (string)eCat[Config.SPCCATEGORY_CODE] : "Sin código";
                    string catName = eCat.Contains(Config.SPCCATEGORY_NAME) ? (string)eCat[Config.SPCCATEGORY_NAME] : "Sin nombre";
                    Guid catId = eCat.Id;
                    Category cat = new Category { InternalId = catId, Name = catName, Code = catCode };
                    Category local = LocalCategories.Where(loc => cat.InternalId.Equals(loc.InternalId)).FirstOrDefault();
                    if (local != null)
                        cat.SQLiteRecordId = local.SQLiteRecordId;
                    await conn.InsertOrReplaceWithChildrenAsync(cat, true);
                    if (cat.InternalId.Equals(id))
                        found = cat;
                }
                lastupdate = new LogTable { TableName = nameof(Category), LastTimeUpdate = DateTime.Now };
                await conn.InsertOrReplaceAsync(lastupdate);
            }
            return found;
        }

        /// <summary>
        /// Filters a technical service product by a code
        /// </summary>
        /// <param name="code">Code used to find the correct technical service product</param>
        /// <returns>A product that corresponds to a technical service</returns>
        public static async Task<Product> GetTechnicalService(string code)
        {
            var conn = await Local.GetConnection();
            Product found = await conn.Table<Product>().Where(p => p.Id.Equals(code)).FirstOrDefaultAsync();
            if (found != null)
                found = await conn.GetWithChildrenAsync<Product>(found.SQLiteRecordId, true);
            return found;
        }

        /// <summary>
        /// Get and save all tecnical service products in the local storage of the phone
        /// </summary>
        /// <returns>Void</returns>
        public static async Task FetchTechnicalServiceProducts()
        {
            var conn = await Local.GetConnection();
            LogTable lastupdate = await conn.FindAsync<LogTable>(nameof(Product));
            if (lastupdate == null || lastupdate.LastTimeUpdate.AddDays(2) < DateTime.Now)
            {
                #region Query
                QueryExpression getProducts = new QueryExpression(Config.SPCPRODUCT)
                {
                    ColumnSet = new ColumnSet(new string[] {
                        Config.SPCPRODUCT_ID,
                        Config.SPCPRODUCT_NAME,
                        Config.SPCPRODUCT_COST,
                        Config.SPCPRODUCT_DAITAX,
                        Config.SPCPRODUCT_LAWTAX,
                        Config.SPCPRODUCT_MOSC,
                        Config.SPCPRODUCT_MOSCPRICE,
                        Config.SPCPRODUCT_SELECTIVETAX,
                        Config.SPCPRODUCT_BOUGHT
                    }),
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions = new DataCollection<ConditionExpression>
                        {
                               new ConditionExpression(Config.SPCPRODUCT_NAME,ConditionOperator.BeginsWith,"Servicio  Tecnico"),
                               new ConditionExpression(Config.SPCPRODUCT_STATE,ConditionOperator.Equal,0)
                        }
                    }
                };
                EntityCollection res = await Proxy.RetrieveMultiple(getProducts);
                #endregion
                foreach (Entity eprod in res.Entities)
                {
                    string prodCode = eprod.Contains(Config.SPCPRODUCT_ID) ? (string)eprod[Config.SPCPRODUCT_ID] : "Sin código";
                    string prodName = eprod.Contains(Config.SPCPRODUCT_NAME) ? (string)eprod[Config.SPCPRODUCT_NAME] : "Sin nombre";
                    Guid prodId = eprod.Id;
                    Product prod = new Product { InternalId = prodId, Name = prodName, Id = prodCode };
                    if (eprod.Contains(Config.SPCPRODUCT_BOUGHT))
                        prod.Bought = (string)eprod[Config.SPCPRODUCT_BOUGHT];
                    if (eprod.Contains(Config.SPCPRODUCT_COST))
                        prod.Cost = (decimal)eprod[Config.SPCPRODUCT_COST];
                    if (eprod.Contains(Config.SPCPRODUCT_MOSC))
                        prod.DoesHaveMOSC = ((string)eprod[Config.SPCPRODUCT_MOSC]).Equals("S");
                    if (eprod.Contains(Config.SPCPRODUCT_MOSCPRICE) && prod.DoesHaveMOSC)
                        prod.Cost = (decimal)eprod[Config.SPCPRODUCT_MOSCPRICE];
                    if (eprod.Contains(Config.SPCPRODUCT_DAITAX))
                        prod.DAITax = (decimal)eprod[Config.SPCPRODUCT_DAITAX];
                    if (eprod.Contains(Config.SPCPRODUCT_LAWTAX))
                        prod.LawTax = (decimal)eprod[Config.SPCPRODUCT_LAWTAX];
                    if (eprod.Contains(Config.SPCPRODUCT_SELECTIVETAX))
                        prod.SelectiveTax = (decimal)eprod[Config.SPCPRODUCT_SELECTIVETAX];
                    Product local = await conn.Table<Product>().Where(p => p.InternalId.Equals(prodId)).FirstOrDefaultAsync();
                    if (local != null)
                        prod.SQLiteRecordId = local.SQLiteRecordId;
                    await conn.InsertOrReplaceWithChildrenAsync(prod);
                }
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(Product), LastTimeUpdate = DateTime.Now });
            }
        }

        /// <summary>
        /// Obtains a specific Technical Service product through its code
        /// </summary>
        /// <param name="code">Code of the technical service product</param>
        /// <returns>A product</returns>
        public static async Task<Product> GetTechnicalServiceOffline(string code)
        {
            var conn = await Local.GetConnection();
            Product found = await conn.Table<Product>().Where(p => p.Id.Equals(code)).FirstOrDefaultAsync();
            if (found != null)
                found = await conn.GetWithChildrenAsync<Product>(found.SQLiteRecordId, true);
            return found;
        }

        /// <summary>
        /// Sends a review to the CRM so it can be saved.
        /// </summary>
        /// <param name="inc">Incident of the review</param>
        /// <param name="st">Service ticket related to the review</param>
        /// <returns>A boolean value indicating result of the operation</returns>
        public static async Task<bool> SendReview(Incident inc, ServiceTicket st)
        {
            var conn = await Local.GetConnection();
            try
            {
                Entity incident = new Entity(Config.SPCCASE, inc.InternalId);
                incident[Config.SPCCASE_FEEDBACK1] = inc.FeedbackAnswer1;
                incident[Config.SPCCASE_FEEDBACK2] = inc.FeedbackAnswer2;
                if (!string.IsNullOrEmpty(inc.ClientFeedback))
                    incident[Config.SPCCASE_FEEDBACKOPINION] = inc.ClientFeedback;
                await Proxy.Update(incident);
                Entity serviceTicket = new Entity(Config.SPCSERVTICKET, st.InternalId);
                serviceTicket[Config.SPCSERVTICKET_FEEDBACK] = true;
                await Proxy.Update(serviceTicket);
            }
            catch (Exception)
            {
                return false;
            }
            await conn.UpdateWithChildrenAsync(inc);
            st.FeedbackSubmitted = true;
            await conn.UpdateWithChildrenAsync(st);
            return true;
        }

        /// <summary>
        /// Saves a review locally so it can be processed once the internet is back.
        /// </summary>
        /// <param name="incident">Incident of the review</param>
        /// <param name="serviceTicket">Service ticket related to the review</param>
        /// <returns></returns>
        public static async Task<bool> SendReviewOffline(Incident incident, ServiceTicket serviceTicket)
        {
            var conn = await Local.GetConnection();
            serviceTicket.FeedbackSubmitted = true;
            await conn.UpdateWithChildrenAsync(incident);
            await RegisterPendingOperation(Types.CRUDOperation.Update, incident.SQLiteRecordId, nameof(Incident));
            await conn.UpdateWithChildrenAsync(serviceTicket);
            await RegisterPendingOperation(Types.CRUDOperation.Update, serviceTicket.SQLiteRecordId, nameof(ServiceTicket));
            return true;
        }

        /// <summary>
        /// Gets all the entities that are currencies stored in the CRM.
        /// </summary>
        /// <returns>a Entity Collection of currencies</returns>
        private static async Task<EntityCollection> GetCurrenciesQuery()
        {
            try
            {
                return await Proxy.RetrieveMultiple(new QueryExpression(Config.SPCCURRENCY)
                {
                    ColumnSet = new ColumnSet(new string[]
                    {
                        Config.SPCCURRENCY_NAME,
                        Config.SPCCURRENCY_CODE,
                        Config.SPCCURRENCY_SYMBOL
                    })
                });
            }
            catch (TaskCanceledException)
            {
                throw new Exception(Config.MSG_GENERIC_ERROR_TASKCANCELLED);
            }
        }          

        /// <summary>
        /// Converts an entity into a Currency model object
        /// </summary>
        /// <param name="entity">Entity object to be converted</param>
        /// <returns>A currency model object result of the conversion</returns>
        private static Currency BuildCurrencyFromEntity(Entity entity) =>
             new Currency
             {
                 InternalId = entity.Id,
                 Name = entity.Contains(Config.SPCCURRENCY_NAME) ? (string)entity[Config.SPCCURRENCY_NAME] : string.Empty,
                 Symbol = entity.Contains(Config.SPCCURRENCY_SYMBOL) ? (string)entity[Config.SPCCURRENCY_SYMBOL] : string.Empty,
                 Code = entity.Contains(Config.SPCCURRENCY_CODE) ? (string)entity[Config.SPCCURRENCY_CODE] : string.Empty
             };

        /// <summary>
        /// Gets all the currencies viewmodels available from the CRM.
        /// </summary>
        /// <returns>An observable collection of the currency viewmodels obtained from the CRM</returns>
        public static async Task<ObservableCollection<CurrencyViewModel>> GetCurrenciesViewModel()
        {
            ObservableCollection<CurrencyViewModel> result = new ObservableCollection<CurrencyViewModel>();
            List<Currency> currencies = await GetCurrencies();
            foreach (Currency currency in currencies)
                result.Add(new CurrencyViewModel(currency));
            return result;
        }

        /// <summary>
        /// Gets all the currency model objects stored in the CRM.
        /// </summary>
        /// <returns>A list of Currency model object which corresponds to the currencies stored in the CRM</returns>
        public static async Task<List<Currency>> GetCurrencies()
        {
            var conn = await Local.GetConnection();
            List<Currency> result = new List<Currency>();
            EntityCollection currenciescollection = await GetCurrenciesQuery();
            List<Currency> LocalCurrencies = await conn.GetAllWithChildrenAsync<Currency>(recursive: true);
            foreach (Entity currency in currenciescollection.Entities)
            {
                Currency curr = BuildCurrencyFromEntity(currency);
                Currency local = LocalCurrencies.Where(l => l.InternalId.Equals(curr.InternalId)).FirstOrDefault();
                if (local != null)
                    curr.SQLiteRecordId = local.SQLiteRecordId;
                await conn.InsertOrReplaceWithChildrenAsync(curr);
                result.Add(curr);
            }
            return result;
        }

        /// <summary>
        /// Obtains a single currency from the CRM by querying with its id
        /// </summary>
        /// <param name="id">Id of the currency needed</param>
        /// <returns>A currency model object result of the fetch operation</returns>
        public static async Task<Currency> GetCurrency(Guid id)
        {
            var conn = await Local.GetConnection();
            Currency found = (await conn.GetAllWithChildrenAsync<Currency>(c => c.InternalId.Equals(id))).FirstOrDefault();
            LogTable lastcurrencyupdate = await conn.FindAsync<LogTable>(nameof(Currency));
            if (lastcurrencyupdate == null || lastcurrencyupdate.LastTimeUpdate.AddDays(2) < DateTime.Now || found == null)
            {
                List<Currency> currencies = await GetCurrencies();
                Currency currency = currencies.Where(curr => curr.InternalId.Equals(id)).FirstOrDefault();
                found = currency ?? throw new Exception("Divisa no encontrada, podria haber sido eliminada o desactivada");
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(Currency), LastTimeUpdate = DateTime.Now });
            }
            return found;
        }

        /// <summary>
        /// Obtains a type of system from the CRM through its id
        /// </summary>
        /// <param name="id">Id of the subtype system required</param>
        /// <returns>A subtype model object result of the fetch operation</returns>
        public static async Task<Subtype> GetSubtype(Guid id)
        {
            var conn = await Local.GetConnection();
            Subtype found = (await conn.GetAllWithChildrenAsync<Subtype>(s => s.InternalId.Equals(id))).FirstOrDefault();
            LogTable lastsubtypeupdate = await conn.FindAsync<LogTable>(nameof(Subtype));
            if (lastsubtypeupdate == null || lastsubtypeupdate.LastTimeUpdate.AddDays(2) < DateTime.Now || found == null)
            {
                #region Query
                EntityCollection subtypescollection = await Proxy.RetrieveMultiple(new QueryExpression(Config.SPCSYSTEM)
                {
                    ColumnSet = new ColumnSet(new string[]
                    {
                        Config.SPCSYSTEM_NAME
                    })
                });
                #endregion
                List<Subtype> LocalSubtypes = await conn.GetAllWithChildrenAsync<Subtype>(recursive: true);
                foreach (Entity subtype in subtypescollection.Entities)
                {
                    string SubName = subtype.Contains(Config.SPCSYSTEM_NAME) ? (string)subtype[Config.SPCSYSTEM_NAME] : string.Empty;
                    Guid subId = subtype.Id;
                    Subtype sub = new Subtype { InternalId = subId, Name = SubName };
                    Subtype local = LocalSubtypes.Where(s => s.InternalId.Equals(sub.InternalId)).FirstOrDefault();
                    if (local != null)
                        sub.SQLiteRecordId = local.SQLiteRecordId;
                    await conn.InsertOrReplaceWithChildrenAsync(sub);
                    if (sub.InternalId.Equals(id))
                        found = sub;
                }
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(Subtype), LastTimeUpdate = DateTime.Now });
            }
            return found;
        }

        /// <summary>
        /// Delete all the local stored data.
        /// *Be careful as this deletes the entire tables*
        /// </summary>
        /// <returns>Void</returns>
        public static async Task ClearLocalData()
        {
            var conn = await Local.GetConnection();
            await conn.DropTableAsync<Technician>();
            await conn.DropTableAsync<Currency>();
            await conn.DropTableAsync<Client>();
            await conn.DropTableAsync<Category>();
            await conn.DropTableAsync<Incident>();
            await conn.DropTableAsync<Coord>();
            await conn.DropTableAsync<Subtype>();
            await conn.DropTableAsync<IncidentTechnician>();
            await conn.DropTableAsync<Product>();
            await conn.DropTableAsync<ProductStorage>();
            await conn.DropTableAsync<LogTable>();
            await conn.DropTableAsync<ServiceTicket>();
            await conn.DropTableAsync<ServiceTicketTechnician>();
            await conn.DropTableAsync<ProductStorage>();
            await conn.DropTableAsync<MaterialYRepuesto>();
        }

        /// <summary>
        /// Obtains a collection of Incident View Models so they can be schedule to be attended
        /// </summary>
        /// <returns>A collection of Incident View Models with their programming schedule set</returns>
        public static async Task<IEnumerable<IncidentViewModel>> GetIncidentsViewModelForProgramming()
        {
            List<DTO_IncidentLookUp> incidents = new List<DTO_IncidentLookUp>(await GetLastCasesForProgramming());
            List<IncidentViewModel> incidentsViewModel = new List<IncidentViewModel>();
            foreach (DTO_IncidentLookUp incident in incidents)
                incidentsViewModel.Add(new IncidentViewModel(incident));
            return incidentsViewModel;
        }

        /// <summary>
        /// Obtains a partial collection of Incidents so they can be selected to be schedule or programmed.
        /// </summary>
        /// <returns>A collection of PARTIAL incidents</returns>
        public static async Task<IEnumerable<DTO_IncidentLookUp>> GetLastCasesForProgramming()
        {
            QueryExpression queryCasesForProgramming = new QueryExpression(Config.SPCCASE)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCCASE_CASENUMBER, Config.GENERAL_CREATEDON, Config.SPCCASE_CLIENT, Config.SPCCASE_CONTROL }),
                Criteria = new FilterExpression()
            };
            string NameClient = "client";
            queryCasesForProgramming.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCCASE,
                LinkToEntityName = Config.SPCACCOUNT,
                LinkFromAttributeName = Config.SPCCASE_CLIENT,
                LinkToAttributeName = Config.SPCACCOUNT_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameClient,
                Columns = new ColumnSet(new string[]
               {
                    Config.SPCACCOUNT_ALIAS
               })
            });
            queryCasesForProgramming.Criteria.FilterOperator = LogicalOperator.And;
            queryCasesForProgramming.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_CREATEDON, ConditionOperator.LastXMonths, Config.LASTXMONTHS));
            queryCasesForProgramming.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            FilterExpression f2 = new FilterExpression
            {
                FilterOperator = LogicalOperator.Or
            };
            f2.AddCondition(new ConditionExpression(Config.SPCCASE_CONTROL, ConditionOperator.NotEqual, 100000000));
            f2.AddCondition(new ConditionExpression(Config.SPCCASE_CONTROL, ConditionOperator.Null));
            queryCasesForProgramming.Criteria.AddFilter(f2);
            EntityCollection queryResult = await Proxy.RetrieveMultiple(queryCasesForProgramming);
            List<DTO_IncidentLookUp> result = new List<DTO_IncidentLookUp>();
            foreach (Entity eIncident in queryResult.Entities)
            {
                EntityReference client = (EntityReference)eIncident[Config.SPCCASE_CLIENT];
                Guid clientid = client.Id;
                string clientName = client.Name;
                string clientAlias = eIncident.Contains(string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)) ? (string)((AliasedValue)eIncident[string.Format("{0}.{1}", NameClient, Config.SPCACCOUNT_ALIAS)]).Value : String.Empty;
                DTO_IncidentLookUp toAdd = new DTO_IncidentLookUp
                {
                    InternalId = eIncident.Id,
                    CreatedOn = (DateTime)eIncident[Config.GENERAL_CREATEDON],
                    TicketNumber = (string)eIncident[Config.SPCCASE_CASENUMBER],
                    Control = eIncident.Contains(Config.SPCCASE_CONTROL) ? (Types.SPCINCIDENT_CONTROLOPTION)((OptionSetValue)eIncident[Config.SPCCASE_CONTROL]).Value : 0,
                    Client = new DTO_ClientPartial
                    {
                        InternalId = clientid,
                        Alias = clientAlias,
                        Name = clientName
                    }
                };
                result.Add(toAdd);
            }
            return result;
        }
        #endregion

        /// <summary>
        /// Obtiene un caso específico por medio de su identificador interno en el CRM.
        /// </summary>
        /// <param name="guid">Identificador interno del caso.</param>
        /// <returns>Entidad caso obtenida del CRM.</returns>
        static public async Task<Entity> GetSpecificCase(Guid guid)
        {
            return await Proxy.Retrieve(Config.SPCCASE, guid, new ColumnSet(true));
        }

        /// <summary>
        /// Obtiene una cuenta de cliente específica por medio de su identificador interno.
        /// </summary>
        /// <param name="guid">Identificador interno de la entidad Cuenta.</param>
        /// <returns>Entidad Cuenta asociada al identificador interno del cliente.</returns>
        static public async Task<Entity> GetSpecificClientAccount(Guid guid)
        {
            return await Proxy.Retrieve(Config.SPCACCOUNT, guid, new ColumnSet(true));
        }

        /// <summary>
        /// Obtains a collection of Account Entities that matches the alias expression.
        /// </summary>
        /// <param name="alias">Alias needed for the account entities to be met</param>
        /// <returns>A collection of Account Entities</returns>
        static public async Task<EntityCollection> GetClientAccountsContaining(string alias)
        {
            string[] fragments = alias.Split(' ');
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = String.Format("%{0}%", fragments[i]);
            }
            QueryExpression queryClientAccounts = new QueryExpression(Config.SPCACCOUNT)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCACCOUNT_ALIAS }),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };
            foreach (string fragment in fragments)
                queryClientAccounts.Criteria.AddCondition(new ConditionExpression(Config.SPCACCOUNT_ALIAS, ConditionOperator.Like, fragment));
            queryClientAccounts.Criteria.AddCondition(new ConditionExpression(Config.GENERAL_STATUSCODE, ConditionOperator.NotEqual, 5));
            return await Proxy.RetrieveMultiple(queryClientAccounts);
        }

        /// <summary>
        /// Obtains a product price according to a pricelist.
        /// </summary>
        /// <param name="PriceListGuid">Id of the pricelist where price is going to be obtained</param>
        /// <param name="ProductGuid">Id of the product of which price is needed</param>
        /// <returns>A collection of prices that can be set to a specific product</returns>
        static public async Task<EntityCollection> GetProductPriceFromPricelist(Guid PriceListGuid, Guid ProductGuid)
        {
            QueryExpression queryProductPrice = new QueryExpression(Config.SPCPRODUCTPLIST)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCPRODUCTPLIST_PRICE }),
                Criteria = new FilterExpression()
            };
            queryProductPrice.Criteria.FilterOperator = LogicalOperator.And;
            queryProductPrice.Criteria.AddCondition(new ConditionExpression(Config.SPCPRODUCTPLIST_PRICELIST, ConditionOperator.Equal, PriceListGuid));
            queryProductPrice.Criteria.AddCondition(new ConditionExpression(Config.SPCPRODUCTPLIST_PRODUCT, ConditionOperator.Equal, ProductGuid));
            return await Proxy.RetrieveMultiple(queryProductPrice);
        }

        /// <summary>
        /// Obtiene todos los tecnicos activos registrados en el servidor.
        /// </summary>
        /// <returns>Una colección de entidades que contiene información sobre los tecnicos activos del servidor.</returns>
        static private async Task<EntityCollection> GetTechniciansRegistered()
        {
            QueryExpression queryActiveTechnicians = new QueryExpression(Config.SPCTECHNICIAN)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCTECHNICIAN_NAME, Config.SPCTECHNICIAN_STORAGE, Config.SPCTECHNICIAN_CATEGORY }),
                Criteria = new FilterExpression()
            };
            string NameStorage = "storage";
            string NameUser = "user";
            queryActiveTechnicians.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCTECHNICIAN,
                LinkToEntityName = Config.SPCSTORAGE,
                LinkFromAttributeName = Config.SPCTECHNICIAN_STORAGE,
                LinkToAttributeName = Config.SPCSTORAGE_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameStorage,
                Columns = new ColumnSet(new string[]
               {
                    Config.SPCSTORAGE_NAME,
                    Config.SPCSTORAGE_TECHNICIAN
               })
            });
            queryActiveTechnicians.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = Config.SPCTECHNICIAN,
                LinkToEntityName = Config.SYSUSER,
                LinkFromAttributeName = Config.SPCTECHNICIAN_USERNAME,
                LinkToAttributeName = Config.SYSUSER_ID,
                JoinOperator = JoinOperator.Inner,
                EntityAlias = NameUser,
                Columns = new ColumnSet(new string[]
               {
                    Config.SYSUSER_ID,
                    Config.SYSUSER_FULLNAME
               })
            });
            queryActiveTechnicians.Criteria.FilterOperator = LogicalOperator.Or;
            queryActiveTechnicians.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            return await Proxy.RetrieveMultiple(queryActiveTechnicians);
        }

        /// <summary>
        /// Obtains a list of all Technicians registered in the CRM and are currently active
        /// </summary>
        /// <returns>A collection of active technicians from CRM</returns>
        static public async Task<List<Technician>> GetTechnicians()
        {
            List<Technician> technicians = new List<Technician>();
            var conn = await Local.GetConnection();
            technicians = (await conn.GetAllWithChildrenAsync<Technician>(t => t.SQLiteRecordId != 0, true)).ToList();
            if (technicians.Count == 0)
            {
                EntityCollection technicianscollection = await GetTechniciansRegistered();
                List<SystemUser> LocalUsers = await conn.GetAllWithChildrenAsync<SystemUser>(recursive: true);
                foreach (Entity technician in technicianscollection.Entities)
                {
                    #region Storage
                    ProductStorage ps = null;
                    string nameStorage = "storage";
                    string concatName = string.Format("{0}.{1}", nameStorage, Config.SPCSTORAGE_NAME);
                    if (technician.Contains(Config.SPCTECHNICIAN_STORAGE))
                    {
                        Guid techStorage = ((EntityReference)technician[Config.SPCTECHNICIAN_STORAGE]).Id;
                        string techStorageName = string.Empty;
                        if (technician.Contains(concatName))
                            techStorageName = (string)((AliasedValue)technician[concatName]).Value;
                        ps = new ProductStorage { InternalId = techStorage, Name = techStorageName };
                        ProductStorage pslocal = (await conn.GetAllWithChildrenAsync<ProductStorage>(l => l.InternalId.Equals(ps.InternalId))).FirstOrDefault();
                        if (pslocal != null)
                            ps.SQLiteRecordId = pslocal.SQLiteRecordId;
                        await conn.InsertOrReplaceWithChildrenAsync(ps, true);
                    }
                    #endregion
                    #region SystemUser
                    SystemUser sysuser = null;
                    string nameUser = "user";
                    concatName = string.Format("{0}.{1}", nameUser, Config.SYSUSER_ID);
                    if (technician.Contains(concatName))
                    {
                        Guid sysid = (Guid)((AliasedValue)technician[concatName]).Value;
                        string sysfullName = string.Empty;
                        string sysToken = string.Empty;
                        concatName = string.Format("{0}.{1}", nameUser, Config.SYSUSER_FULLNAME);
                        if (technician.Contains(concatName))
                            sysfullName = (string)((AliasedValue)technician[concatName]).Value;
                        sysuser = new SystemUser { InternalId = sysid, Name = sysfullName, Token = Proxy.LoggedUser.Name.Equals(sysfullName) ? Proxy.LoggedUser.Token : null };
                        SystemUser userlocal = LocalUsers.Where(user => user.InternalId.Equals(sysuser.InternalId)).FirstOrDefault();
                        if (userlocal != null)
                            sysuser.SQLiteRecordId = userlocal.SQLiteRecordId;
                        await conn.InsertOrReplaceWithChildrenAsync(sysuser);
                    }
                    #endregion
                    #region Technician
                    string techName = technician.Contains(Config.SPCTECHNICIAN_NAME) ? (string)technician[Config.SPCTECHNICIAN_NAME] : string.Empty;
                    Guid techId = technician.Id;
                    Types.SPCTECHNICIAN_CATEGORY techCat = technician.Contains(Config.SPCTECHNICIAN_CATEGORY) ? (Types.SPCTECHNICIAN_CATEGORY)((OptionSetValue)technician[Config.SPCTECHNICIAN_CATEGORY]).Value : Types.SPCTECHNICIAN_CATEGORY.Undefined;
                    Technician local = (await conn.GetAllWithChildrenAsync<Technician>(e => e.InternalId.Equals(techId))).FirstOrDefault();
                    Technician tech = new Technician
                    {
                        InternalId = techId,
                        Name = techName,
                        ProductStorage = ps,
                        ProductStorageId = ps.SQLiteRecordId,
                        RelatedUser = sysuser,
                        RelatedUserId = sysuser.SQLiteRecordId,
                        Category = techCat
                    };
                    if (local != null)
                        tech.SQLiteRecordId = local.SQLiteRecordId;
                    await conn.InsertOrReplaceWithChildrenAsync(tech);
                    #endregion
                }
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(Technician), LastTimeUpdate = DateTime.Now });
                technicians = (await conn.GetAllWithChildrenAsync<Technician>(t => t.SQLiteRecordId != 0, true)).ToList();
            }
            return new List<Technician>(technicians.OrderBy(t => t.Name));
        }

        /// <summary>
        /// Obtains a collection of ALL active technicians from CRM as ViewModels
        /// </summary>
        /// <returns>A collection of technician ViewModels</returns>
        static public async Task<ObservableCollection<TechnicianViewModel>> GetTechniciansViewModel()
        {
            ObservableCollection<TechnicianViewModel> technicians = new ObservableCollection<TechnicianViewModel>();
            List<Technician> lTechs = await GetTechnicians();
            foreach (Technician tech in lTechs)
                technicians.Add(new TechnicianViewModel(tech));
            return technicians;
        }

        /// <summary>
        /// Obtains an specific Technician Model object from CR using the id provided
        /// </summary>
        /// <param name="internalId">Id of the Technician to be retrieved</param>
        /// <returns>A Technician obtained from CRM</returns>
        static public async Task<Technician> GetTechnician(Guid internalId)
        {
            var conn = await Local.GetConnection();
            Technician found = (await conn.GetAllWithChildrenAsync<Technician>(technician => technician.InternalId.Equals(internalId), true)).FirstOrDefault();
            LogTable lastcurrencyupdate = await conn.FindAsync<LogTable>(nameof(Technician));
            if (lastcurrencyupdate == null || lastcurrencyupdate.LastTimeUpdate.AddDays(2) < DateTime.Now || found == null)
            {
                EntityCollection technicianscollection = await GetTechniciansRegistered();
                List<SystemUser> LocalUsers = await conn.GetAllWithChildrenAsync<SystemUser>(recursive: true);
                foreach (Entity technician in technicianscollection.Entities)
                {
                    #region Storage
                    ProductStorage ps = null;
                    string nameStorage = "storage";
                    string concatName = string.Format("{0}.{1}", nameStorage, Config.SPCSTORAGE_NAME);
                    if (technician.Contains(Config.SPCTECHNICIAN_STORAGE))
                    {
                        Guid techStorage = ((EntityReference)technician[Config.SPCTECHNICIAN_STORAGE]).Id;
                        string techStorageName = string.Empty;
                        if (technician.Contains(concatName))
                            techStorageName = (string)((AliasedValue)technician[concatName]).Value;
                        ps = new ProductStorage { InternalId = techStorage, Name = techStorageName };
                        ProductStorage pslocal = (await conn.GetAllWithChildrenAsync<ProductStorage>(l => l.InternalId.Equals(ps.InternalId))).FirstOrDefault();
                        if (pslocal != null)
                            ps.SQLiteRecordId = pslocal.SQLiteRecordId;
                        await conn.InsertOrReplaceWithChildrenAsync(ps, true);
                    }
                    #endregion
                    #region SystemUser
                    SystemUser sysuser = null;
                    string nameUser = "user";
                    concatName = string.Format("{0}.{1}", nameUser, Config.SYSUSER_ID);
                    if (technician.Contains(concatName))
                    {
                        Guid sysid = (Guid)((AliasedValue)technician[concatName]).Value;
                        string sysfullName = string.Empty;
                        string sysToken = string.Empty;
                        concatName = string.Format("{0}.{1}", nameUser, Config.SYSUSER_FULLNAME);
                        if (technician.Contains(concatName))
                            sysfullName = (string)((AliasedValue)technician[concatName]).Value;
                        sysuser = new SystemUser { InternalId = sysid, Name = sysfullName, Token = Proxy.LoggedUser.Name.Equals(sysfullName) ? Proxy.LoggedUser.Token : null };
                        SystemUser userlocal = LocalUsers.Where(user => user.InternalId.Equals(sysuser.InternalId)).FirstOrDefault();
                        if (userlocal != null)
                            sysuser.SQLiteRecordId = userlocal.SQLiteRecordId;
                        await conn.InsertOrReplaceWithChildrenAsync(sysuser);
                    }
                    #endregion
                    #region Technician
                    string techName = technician.Contains(Config.SPCTECHNICIAN_NAME) ? (string)technician[Config.SPCTECHNICIAN_NAME] : string.Empty;
                    Guid techId = technician.Id;
                    Types.SPCTECHNICIAN_CATEGORY techCat = technician.Contains(Config.SPCTECHNICIAN_CATEGORY) ? (Types.SPCTECHNICIAN_CATEGORY)((OptionSetValue)technician[Config.SPCTECHNICIAN_CATEGORY]).Value : Types.SPCTECHNICIAN_CATEGORY.Undefined;
                    Technician local = (await conn.GetAllWithChildrenAsync<Technician>(e => e.InternalId.Equals(techId))).FirstOrDefault();
                    Technician tech = new Technician
                    {
                        InternalId = techId,
                        Name = techName,
                        ProductStorage = ps,
                        ProductStorageId = ps.SQLiteRecordId,
                        RelatedUser = sysuser,
                        RelatedUserId = sysuser.SQLiteRecordId,
                        Category = techCat
                    };
                    if (local != null)
                        tech.SQLiteRecordId = local.SQLiteRecordId;
                    await conn.InsertOrReplaceWithChildrenAsync(tech);
                    if (tech.InternalId.Equals(internalId))
                        found = tech;
                    #endregion
                }
                await conn.InsertOrReplaceAsync(new LogTable { TableName = nameof(Technician), LastTimeUpdate = DateTime.Now });
            }
            return found;
        }

        /// <summary>
        /// Obtains a collection of all storages from CRM as entities
        /// </summary>
        /// <returns>An entity collection object containing all storages retrieved from CRM</returns>
        static private async Task<EntityCollection> GetStorages()
        {
            QueryExpression queryStorages = new QueryExpression(Config.SPCSTORAGE)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCSTORAGE_NAME, Config.SPCSTORAGE_TECHNICIAN })
            };
            return await Proxy.RetrieveMultiple(queryStorages);
        }

        /// <summary>
        /// Actualiza una boleta de servicio.
        /// </summary>
        /// <param name="serviceTicket">Entidad a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        static public async Task<bool> UpdateServiceTicket(Entity serviceTicket)
        {
            try
            {
                await Proxy.Update(serviceTicket);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a technician registry from the CRM.
        /// </summary>
        /// <param name="registryId">Id of the registry to be deleted</param>
        /// <returns>Void</returns>
        static public async Task DeleteTechnicianRegistry(Guid registryId)
        {
            Entity registry = new Entity(Config.SPCTECHREGISTRY, registryId);
            registry[Config.GENERAL_STATECODE] = new OptionSetValue(1);
            registry[Config.GENERAL_STATUSCODE] = new OptionSetValue(2);
            await Proxy.Update(registry);
            //await Proxy.Delete(Config.SPCTECHREGISTRY, registryId);
        }

        /// <summary>
        /// Obtiene los documentos asociados a un caso.
        /// </summary>
        /// <param name="caseId">Id del caso que se desea consultar.</param>
        /// <returns>Colección de documentos asociados a un caso.</returns>
        static public async Task<EntityCollection> GetNotes(Guid caseId)
        {
            QueryExpression queryNotes = new QueryExpression(Config.SPCNOTES)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCNOTES_FILENAME })
            };
            FilterExpression f1 = new FilterExpression
            {
                FilterOperator = LogicalOperator.And
            };
            f1.AddCondition(new ConditionExpression(Config.SPCNOTES_LINK, ConditionOperator.Equal, caseId));
            queryNotes.Criteria.AddFilter(f1);
            queryNotes.Criteria.FilterOperator = LogicalOperator.Or;
            return await Proxy.RetrieveMultiple(queryNotes);
        }

        /// <summary>
        /// Obtains a collection of note entities corresponding to all the photos related to a service ticket
        /// </summary>
        /// <param name="serviceTicket">Id of the service ticket which photos are related to</param>
        /// <returns> A collection of entity that are actually notes of photographs</returns>
        static public async Task<EntityCollection> GetPhotosTaken(Guid serviceTicket)
        {
            QueryExpression queryNotes = new QueryExpression(Config.SPCNOTES)
            {
                ColumnSet = new ColumnSet(new string[] { Config.SPCNOTES_FILENAME, Config.SPCNOTES_CONTENT, "subject" })
            };
            queryNotes.Criteria.FilterOperator = LogicalOperator.And;
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_LINK, ConditionOperator.Equal, serviceTicket));
            queryNotes.Criteria.AddCondition(new ConditionExpression(Config.SPCNOTES_MIME, ConditionOperator.Equal, "image/jpeg"));
            return await Proxy.RetrieveMultiple(queryNotes);
        }

        /// <summary>
        /// Deletes a note corresponding to a photo from the CRM.
        /// </summary>
        /// <param name="photo">Note to be delete from CRM</param>
        /// <returns>Void</returns>
        static public async Task DeletePhotoTaken(Note photo)
        {
            var conn = await Local.GetConnection();
            await Proxy.Delete(Config.SPCNOTES, photo.InternalId);
            await conn.DeleteAsync(photo);
        }

        /// <summary>
        /// Registers a note of a photograph to be deleted so it can be deleted once the internet is back
        /// </summary>
        /// <param name="photo">Note object of a photograph</param>
        /// <returns>Void</returns>
        static public async Task DeletePhotoTakenOffline(Note photo)
        {
            var conn = await Local.GetConnection();
            Note toDelete = await conn.GetWithChildrenAsync<Note>(photo.SQLiteRecordId);
            toDelete.ServiceTicketId = 0;
            toDelete.ServiceTicket = null;
            await conn.UpdateWithChildrenAsync(toDelete);
            await RegisterPendingOperation(Types.CRUDOperation.Delete, toDelete.SQLiteRecordId, nameof(Note), nameof(ServiceTicket));
        }

        /// <summary>
        /// Obtains an specific note entity from CRM.
        /// </summary>
        /// <param name="id">Id of the note to be retrieved</param>
        /// <returns>Entity of the note retrieved.</returns>
        static public async Task<Entity> GetSpecificNote(Guid id)
        {
            return await Proxy.Retrieve(Config.SPCNOTES, id, new ColumnSet(new string[] { Config.SPCNOTES_FILENAME, Config.SPCNOTES_CONTENT }));
        }

        /// <summary>
        /// General query that obtains a related entity object of another entity object
        /// </summary>
        /// <param name="RelatedEntity">Id of the related entity to be retrieved</param>
        /// <param name="EntityNametoObtainRecords">Name registered in CRM of the related entity</param>
        /// <param name="desiredAttributes">Collection of atributes wanted to be fectch for the related entity</param>
        /// <param name="AtributeForComparison">Atribute of an entity used to compared the relation of the entity and its related entity</param>
        /// <returns></returns>
        static public async Task<EntityCollection> GetEntitiesRelatedTo(Guid RelatedEntity, string EntityNametoObtainRecords, string[] desiredAttributes, string AtributeForComparison)
        {
            QueryExpression query = new QueryExpression(EntityNametoObtainRecords)
            {
                ColumnSet = new ColumnSet(desiredAttributes),
                Criteria = new FilterExpression()
            };
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.AddCondition(new ConditionExpression(AtributeForComparison, ConditionOperator.Equal, RelatedEntity));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            return await Proxy.RetrieveMultiple(query);
        }

        /// <summary>
        /// Elimina un materialyProducto de una boleta de servicio.
        /// </summary>
        /// <param name="materialId">Identificador del materialyProducto a eliminar.</param>
        /// <returns></returns>
        static public async Task DeleteMaterial(Guid materialId)
        {
            await Proxy.Delete(Config.SPCMATERIAL, materialId);
        }

        /// <summary>
        /// Registers a material to be delete once the internet is back.
        /// </summary>
        /// <param name="toDelete">Material to be deleted from CRM</param>
        /// <returns>Void</returns>
        static public async Task DeleteMaterialOffline(MaterialYRepuesto toDelete)
        {
            var conn = await Local.GetConnection();
            toDelete.ServiceTicketId = 0;
            await conn.UpdateWithChildrenAsync(toDelete); //cannot be deleted yet because it is still needed for sync.
            await RegisterPendingOperation(Types.CRUDOperation.Delete, toDelete.SQLiteRecordId, nameof(MaterialYRepuesto));
        }

        /// <summary>
        /// Elimina una boleta de servicio.
        /// </summary>
        /// <param name="ticketId">Identificador de la boleta de servicio a eliminar.</param>
        /// <returns></returns>
        static public async Task DeleteServiceTicket(int ticketid)
        {
            var conn = await Local.GetConnection();
            SystemUser loggeduser = await conn.Table<SystemUser>().Where(lu => lu.InternalId.Equals(Proxy.LoggedUser.InternalId)).FirstOrDefaultAsync();
            Technician tech1 = (loggeduser != null) ? await conn.Table<Technician>().Where(t => t.RelatedUserId == loggeduser.SQLiteRecordId).FirstOrDefaultAsync() : null;
            ServiceTicket toDelete = await conn.GetWithChildrenAsync<ServiceTicket>(ticketid, true);
            if (toDelete.Technicians[0] == null)
                throw new Exception(String.Format("No se logrado determinar el dueño de la boleta. Contacte a TI para eliminarla."));
            if (toDelete.Technicians[0].InternalId != tech1.InternalId)
                throw new Exception(String.Format("Solo {0} puede eliminar la boleta.", toDelete.Technicians[0].Name));
            if ((DateTime.Now - toDelete.CreationDate).Minutes < 15)
                await Proxy.Delete(Config.SPCSERVTICKET, toDelete.InternalId);
            else
                throw new Exception("No puede eliminar una boleta después de 15 minutos de creada.");
        }

        /// <summary>
        /// Sign a contract previously created in the CRM.
        /// </summary>
        /// <param name="contract">Id of the contract to be signed</param>
        /// <returns>A boolean value corresponding to the result of the operation.</returns>
        public static async Task<bool> SignContract(Guid contract)
        {
            try
            {
                Entity eContract = new Entity(Config.SPCEXTERNALCONTRACT, contract);
                eContract.Attributes[Config.SPCEXTERNALCONTRACT_SIGNED] = true;
                await Proxy.Update(eContract);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Syncs all pending offline operations with CRM. This is called once the internet is back.
        /// </summary>
        /// <returns>Void</returns>
        public static async Task SyncOperations()
        {
            var conn = await Local.GetConnection();
            List<CrudTable> remainingOperations = await conn.Table<CrudTable>().OrderBy(o => o.SQLiteRecordId).ToListAsync();
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), "Sincronizando", "Sincronizando información local y CRM", 0, remainingOperations.Count);
            PageService ps = new PageService();
            await ps.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            try
            {
                for (int i = 0; i < remainingOperations.Count; i++)
                {
                    vm.Title = string.Format("Sincronización {0} de {1}", i + 1, remainingOperations.Count);
                    vm.CurrentStep++;
                    int id = remainingOperations[i].ObjectId;
                    switch (remainingOperations[i].Action)
                    {
                        case Types.CRUDOperation.Create:
                            switch (remainingOperations[i].ObjectType)
                            {
                                #region Create Service Ticket
                                case nameof(ServiceTicket):
                                    vm.CurrentState = "Creando boleta de servicio";
                                    ServiceTicket ticket = await conn.GetWithChildrenAsync<ServiceTicket>(remainingOperations[i].ObjectId, true);
                                    Incident parent = await conn.GetWithChildrenAsync<Incident>(ticket.IncidentId, true);
                                    await SyncCreateServiceTicket(parent, ticket);
                                    break;
                                #endregion
                                #region Create MaterialYRepuesto
                                case nameof(MaterialYRepuesto):
                                    vm.CurrentState = "Creando artículo en boleta";
                                    MaterialYRepuesto mat = await conn.GetWithChildrenAsync<MaterialYRepuesto>(remainingOperations[i].ObjectId, true);
                                    if (mat.ServiceTicketId != 0)
                                    {
                                        ServiceTicket st = await conn.GetWithChildrenAsync<ServiceTicket>(mat.ServiceTicketId, true);
                                        await SyncCreateMaterial(mat, st);
                                    }
                                    break;
                                #endregion
                                #region Create Note
                                case nameof(Note):
                                    Note note = await conn.GetWithChildrenAsync<Note>(id);
                                    switch (remainingOperations[i].AdditionalInfo)
                                    {
                                        #region Upload Note related to Incident
                                        case nameof(Incident):
                                            vm.CurrentState = "Subiendo adjunto a incidente";
                                            await SyncCreateNoteForIncident(note);
                                            break;
                                        #endregion
                                        #region Upload Note related to ServiceTicket
                                        case nameof(ServiceTicket):
                                            vm.CurrentState = "Subiendo adjunto a boleta de servicio";
                                            if (note.ServiceTicketId != 0)
                                                await SyncCreateNoteForServiceTicket(note);
                                            break;
                                        #endregion
                                        default:
                                            break;
                                    }
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            break;
                        case Types.CRUDOperation.Update:
                            switch (remainingOperations[i].ObjectType)
                            {
                                #region Update Coordinates
                                case nameof(Coord):
                                    vm.CurrentState = "Actualizando coordenadas de cliente";
                                    Client client = await conn.Table<Client>().Where(c => c.CoordinatesId == id).FirstOrDefaultAsync();
                                    client = await conn.GetWithChildrenAsync<Client>(client.SQLiteRecordId, true);
                                    await SyncUpdateClientCoords(client);
                                    break;
                                #endregion
                                #region Update ServiceTicket
                                case nameof(ServiceTicket):
                                    vm.CurrentState = "Actualizando boleta de servicio";
                                    if (string.IsNullOrEmpty(remainingOperations[i].AdditionalInfo))
                                        await SyncUpdateServiceTicket(id);
                                    else
                                        await SyncCloseServiceTicket(id);
                                    break;
                                #endregion
                                #region Update Incident
                                case nameof(Incident):
                                    vm.CurrentState = "Enviando encuesta";
                                    Incident incident = await conn.GetWithChildrenAsync<Incident>(id, true);
                                    await SyncUpdateIncident(incident);
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            break;
                        case Types.CRUDOperation.Delete:
                            switch (remainingOperations[i].ObjectType)
                            {
                                #region Delete Photo
                                case nameof(Note):
                                    vm.CurrentState = "Eliminando adjunto";
                                    Note note = await conn.GetWithChildrenAsync<Note>(id);
                                    if (nameof(ServiceTicket).Equals(remainingOperations[i].AdditionalInfo))
                                        await DeletePhotoTaken(note);
                                    break;
                                #endregion
                                #region Delete Material
                                case nameof(MaterialYRepuesto):
                                    vm.CurrentState = "Removiendo un artículo";
                                    MaterialYRepuesto mat = await conn.GetWithChildrenAsync<MaterialYRepuesto>(id);
                                    if (!mat.InternalId.Equals(default(Guid)))
                                        await DeleteMaterial(mat.InternalId);
                                    await conn.DeleteAsync(mat);
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    await conn.DeleteAsync(remainingOperations[i]);
                }
                vm.CurrentStep++;
                vm.CurrentState = "Sincronización finalizada";
                vm.IsLoading = false;
            }
            catch (Exception ex)
            {
                vm.IsLoading = false;
                if (await ps.DisplayAlert("Error", "Ha ocurrido un error durante la sincronización y se ha detenido.Desea ver el error?", "Si", "No"))
                    await ps.DisplayAlert(ex.Message, ex.StackTrace, "Ok");
            }
        }

        /// <summary>
        /// Synchronization operation to create a new Service Ticket
        /// </summary>
        /// <param name="incident">Incident that is origin of the ticket</param>
        /// <param name="ticket">Ticket pending to be created</param>
        /// <returns>Void</returns>
        private static async Task SyncCreateServiceTicket(Incident incident, ServiceTicket ticket)
        {
            var conn = await Local.GetConnection();
            Entity serviceTicket = new Entity(Config.SPCSERVTICKET);
            EntityReference Eclient = new EntityReference(Config.SPCACCOUNT, incident.Client.InternalId);
            EntityReference ETechnician1 = new EntityReference(Config.SPCTECHNICIAN, ticket.Technicians[0].InternalId);
            EntityReference ECaso = new EntityReference(Config.SPCCASE, incident.InternalId);
            EntityReference ESystem = new EntityReference(Config.SPCSYSTEM, ticket.Type.InternalId);
            EntityReference ECurrency = new EntityReference(Config.SPCCURRENCY, ticket.MoneyCurrency.InternalId);
            serviceTicket[Config.SPCSERVTICKET_TECHNICIAN1] = ETechnician1; //required
            serviceTicket[Config.SPCSERVTICKET_CASEID] = ECaso;         //required
            serviceTicket[Config.SPCSERVTICKET_CASENUMBER] = incident.TicketNumber;     //required
            serviceTicket[Config.SPCSERVTICKET_TITLE] = ticket.Title;
            serviceTicket[Config.SPCSERVTICKET_DESCRIPTION] = ticket.Description;
            serviceTicket[Config.SPCSERVTICKET_SYSTEM] = ESystem;
            serviceTicket[Config.SPCSERVTICKET_CURRENCY] = ECurrency;
            serviceTicket[Config.SPCSERVTICKET_CLIENT] = Eclient;       //required
            serviceTicket[Config.SPCSERVTICKET_STARTED] = ticket.Started;
            // serviceTicket[Config.GENERAL_CREATEDONOVERRIDE] = ticket.CreationDate;
            Guid id = await Proxy.Create(serviceTicket);
            serviceTicket = await Proxy.Retrieve(Config.SPCSERVTICKET, id, new ColumnSet(new string[]
            {
                Config.GENERAL_CREATEDON,
                Config.SPCSERVTICKET_NUMBER
            }));
            string offlineNumber = ticket.TicketNumber;
            ticket.TicketNumber = ((int)serviceTicket[Config.SPCSERVTICKET_NUMBER]).ToString();
            ticket.CreationDate = (DateTime)serviceTicket[Config.GENERAL_CREATEDON];
            ticket.InternalId = serviceTicket.Id;
            List<Note> notes = await conn.GetAllWithChildrenAsync<Note>(n => n.IncidentId == incident.SQLiteRecordId, true);
            foreach (Note note in notes)
            {
                if (note.Filename.Contains("ReporteDeServicio" + offlineNumber))
                    note.Filename = string.Format("ReporteDeServicio{0}.pdf", ticket.TicketNumber);
                else if (note.Filename.Contains("LegalizacionBoleta" + offlineNumber))
                    note.Filename = string.Format("LegalizacionBoleta{0}.pdf", ticket.TicketNumber);
            }
            await conn.InsertOrReplaceAllWithChildrenAsync(notes, true);
            await conn.InsertOrReplaceWithChildrenAsync(ticket);
        }

        /// <summary>
        /// Synchronization operation to create a new report note for an incident
        /// </summary>
        /// <param name="note">Note to be created</param>
        /// <returns>Void</returns>
        private static async Task SyncCreateNoteForIncident(Note note)
        {
            await AddReportToIncident(note);
        }

        /// <summary>
        /// Synchronization operation to create a new photo note for a service ticket
        /// </summary>
        /// <param name="note">Note to be created</param>
        /// <returns>Void</returns>
        private static async Task SyncCreateNoteForServiceTicket(Note note)
        {
            await AddPhoto(note);
        }

        /// <summary>
        /// Synchronization operation to update client coordinates 
        /// </summary>
        /// <param name="client">Client that is going to have its coordinates updated</param>
        /// <returns>Void</returns>
        private static async Task SyncUpdateClientCoords(Client client)
        {
            Entity clientEntity = new Entity(Config.SPCACCOUNT, client.InternalId);
            clientEntity[Config.SPCACCOUNT_LOCATION] = String.Format("{0},{1}", client.Coordinates.Latitude, client.Coordinates.Longitude);
            await Proxy.Update(clientEntity);
        }

        /// <summary>
        /// Synchronization operation to Create a new Material record in the CRM.
        /// </summary>
        /// <param name="material">Material to create</param>
        /// <param name="st">Service ticket related to the material being created</param>
        /// <returns></returns>
        private static async Task SyncCreateMaterial(MaterialYRepuesto material, ServiceTicket st)
        {
            var conn = await Local.GetConnection();
            if (material.Product == null)
                throw new Exception("El material no tiene un producto asignado");
            EntityReference EProduct = new EntityReference(Config.SPCPRODUCT, material.Product.InternalId);
            EntityReference EServiceTicket = new EntityReference(Config.SPCSERVTICKET, st.InternalId);
            Entity New_Material = new Entity(Config.SPCMATERIAL);
            New_Material[Config.SPCMATERIAL_PRODUCT] = EProduct; //required
            New_Material[Config.SPCMATERIAL_SERVICETICKET] = EServiceTicket;            //required
            New_Material[Config.SPCMATERIAL_QUANTITY] = material.Count;         //required
            New_Material[Config.SPCMATERIAL_TREATMENT] = new OptionSetValue((int)material.Treatment);
            if (material.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Desmonte)
                New_Material[Config.SPCMATERIAL_DESTINATION] = new OptionSetValue((int)material.Destination);
            if (!string.IsNullOrEmpty(material.Serials))
                New_Material[Config.SPCMATERIAL_SERIALS] = material.Serials;
            material.InternalId = await Proxy.Create(New_Material);
            await conn.InsertOrReplaceWithChildrenAsync(material, true);
        }

        /// <summary>
        /// Synchronization operation to Update a Service Ticket
        /// </summary>
        /// <param name="serviceticketid">Local id of the service ticket to update</param>
        /// <returns>Void</returns>
        private static async Task SyncUpdateServiceTicket(int serviceticketid)
        {
            var conn = await Local.GetConnection();
            ServiceTicket localServiceTicket = await conn.GetWithChildrenAsync<ServiceTicket>(serviceticketid, true);
            #region Update Basic Information
            Entity toSave = new Entity(Config.SPCSERVTICKET, localServiceTicket.InternalId);
            if (localServiceTicket.Technicians.Count > 4)
                toSave[Config.SPCSERVTICKET_TECHNICIAN5] = new EntityReference(Config.SPCTECHNICIAN, localServiceTicket.Technicians[4].InternalId);
            if (localServiceTicket.Technicians.Count > 3)
                toSave[Config.SPCSERVTICKET_TECHNICIAN4] = new EntityReference(Config.SPCTECHNICIAN, localServiceTicket.Technicians[3].InternalId);
            if (localServiceTicket.Technicians.Count > 2)
                toSave[Config.SPCSERVTICKET_TECHNICIAN3] = new EntityReference(Config.SPCTECHNICIAN, localServiceTicket.Technicians[2].InternalId);
            if (localServiceTicket.Technicians.Count > 1)
                toSave[Config.SPCSERVTICKET_TECHNICIAN2] = new EntityReference(Config.SPCTECHNICIAN, localServiceTicket.Technicians[1].InternalId);
            toSave[Config.SPCSERVTICKET_WORKDONE] = string.IsNullOrEmpty(localServiceTicket.WorkDone) ? String.Empty : localServiceTicket.WorkDone;
            toSave[Config.SPCSERVTICKET_HADLUNCH] = localServiceTicket.HadLunch;
            #endregion
            #region Update RX Checklist
            if (localServiceTicket.Type.Name.Equals("Rayos X"))
            {
                #region General
                toSave[Config.SPCSERVTICKET_CLEANAREA] = localServiceTicket.RXGenCleanArea;
                if (!String.IsNullOrEmpty(localServiceTicket.RXGenComments))
                    toSave[Config.SPCSERVTICKET_COMMENTS] = localServiceTicket.RXGenComments;
                if (!String.IsNullOrEmpty(localServiceTicket.RXGenCreationDate))
                    toSave[Config.SPCSERVTICKET_RXCREATIONDATE] = localServiceTicket.RXGenCreationDate;
                if (localServiceTicket.RXGenHWState != Types.SPCSERVTICKET_HWSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HWSTATE] = new OptionSetValue((int)localServiceTicket.RXGenHWState);
                if (!String.IsNullOrEmpty(localServiceTicket.RXGenModel))
                    toSave[Config.SPCSERVTICKET_RXMODEL] = localServiceTicket.RXGenModel;
                if (!String.IsNullOrEmpty(localServiceTicket.RXGenSerial))
                    toSave[Config.SPCSERVTICKET_RXSERIAL] = localServiceTicket.RXGenSerial;
                if (localServiceTicket.RXGenVisitNumber != Types.SPCSERVTICKET_VISITNUMBER.Undefined)
                    toSave[Config.SPCSERVTICKET_VISITNUMBER] = new OptionSetValue((int)localServiceTicket.RXGenVisitNumber);
                #endregion
                #region Manteinance
                toSave[Config.SPCSERVTICKET_CHECKACSYSTEM] = localServiceTicket.RXMantCheckConditioningSystem;
                toSave[Config.SPCSERVTICKET_CHECKCONFIGURATION] = localServiceTicket.RXMantCheckConfiguration;
                toSave[Config.SPCSERVTICKET_CHECKCONTROLELEMENTS] = localServiceTicket.RXMantCheckControlElements;
                toSave[Config.SPCSERVTICKET_CHECKCONVEYORBELT] = localServiceTicket.RXMantCheckConveyorBelt;
                toSave[Config.SPCSERVTICKET_CHECKEMERGENCYSTOP] = localServiceTicket.RXMantCheckEmergencyStop;
                toSave[Config.SPCSERVTICKET_CHECKENGINECONTROL] = localServiceTicket.RXMantCheckEngineControl;
                toSave[Config.SPCSERVTICKET_CHECKENGINETRACTION] = localServiceTicket.RXMantCheckEngineTraction;
                toSave[Config.SPCSERVTICKET_CHECKINOUTSYSTEM] = localServiceTicket.RXMantCheckInOutSystem;
                toSave[Config.SPCSERVTICKET_CHECKINTERLOCK] = localServiceTicket.RXMantCheckInterlock;
                toSave[Config.SPCSERVTICKET_CHECKIRFENCES] = localServiceTicket.RXMantCheckIRFences;
                toSave[Config.SPCSERVTICKET_CHECKKEYBOARD] = localServiceTicket.RXMantCheckKeyboard;
                toSave[Config.SPCSERVTICKET_CHECKLABELS] = localServiceTicket.RXMantCheckLabels;
                toSave[Config.SPCSERVTICKET_CHECKLINEDETMODS] = localServiceTicket.RXMantCheckLineAndDetectionModules;
                toSave[Config.SPCSERVTICKET_CHECKMONITORCONFIG] = localServiceTicket.RXMantCheckMonitorConfiguration;
                toSave[Config.SPCSERVTICKET_CHECKOS] = localServiceTicket.RXMantCheckOS;
                toSave[Config.SPCSERVTICKET_CHECKRADINDICATORS] = localServiceTicket.RXMantCheckRadiationIndicators;
                toSave[Config.SPCSERVTICKET_CHECKROLLERS] = localServiceTicket.RXMantCheckRollers;
                if (localServiceTicket.RXMantCheckScreenType != Types.SPCSERVTICKET_SCREENTYPE.Undefined)
                    toSave[Config.SPCSERVTICKET_SCREENTYPE] = new OptionSetValue((int)localServiceTicket.RXMantCheckScreenType);
                toSave[Config.SPCSERVTICKET_CHECKSECCIRCUIT] = localServiceTicket.RXMantCheckSecurityCircuit;
                toSave[Config.SPCSERVTICKET_CHECKTWOWAY] = localServiceTicket.RXMantCheckTwoWayMode;
                toSave[Config.SPCSERVTICKET_CHECKVOLTMONITOR] = localServiceTicket.RXMantCheckVoltMonitor;
                toSave[Config.SPCSERVTICKET_CHECKXRCONE] = localServiceTicket.RXMantCheckXRCone;
                if (localServiceTicket.RXMantLeadState != Types.SPCSERVTICKET_LEADSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_LEADSTATE] = new OptionSetValue((int)localServiceTicket.RXMantLeadState);
                #endregion
                #region Voltages
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator1XROffVoltage))
                    toSave[Config.SPCSERVTICKET_GEN1OFFVOLTAGE] = localServiceTicket.RXVoltGenerator1XROffVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator1XROnAnodeVoltage))
                    toSave[Config.SPCSERVTICKET_GEN1ONANODEVOLT] = localServiceTicket.RXVoltGenerator1XROnAnodeVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator1XROnHighVoltage))
                    toSave[Config.SPCSERVTICKET_GEN1ONHIGHVOLT] = localServiceTicket.RXVoltGenerator1XROnHighVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator1XROnVoltage))
                    toSave[Config.SPCSERVTICKET_GEN1ONVOLTAGE] = localServiceTicket.RXVoltGenerator1XROnVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator2XROffVoltage))
                    toSave[Config.SPCSERVTICKET_GEN2OFFVOLTAGE] = localServiceTicket.RXVoltGenerator2XROffVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator2XROnAnodeVoltage))
                    toSave[Config.SPCSERVTICKET_GEN2ONANODEVOLT] = localServiceTicket.RXVoltGenerator2XROnAnodeVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator2XROnHighVoltage))
                    toSave[Config.SPCSERVTICKET_GEN2ONHIGHVOLT] = localServiceTicket.RXVoltGenerator2XROnHighVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGenerator2XROnVoltage))
                    toSave[Config.SPCSERVTICKET_GEN2ONVOLTAGE] = localServiceTicket.RXVoltGenerator2XROnVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltGroundVoltage))
                    toSave[Config.SPCSERVTICKET_GROUNDVOLTAGE] = localServiceTicket.RXVoltGroundVoltage;
                if (!String.IsNullOrEmpty(localServiceTicket.RXVoltInVoltage))
                    toSave[Config.SPCSERVTICKET_INVOLTAGE] = localServiceTicket.RXVoltInVoltage;
                toSave[Config.SPCSERVTICKET_CHECKHAVEUPS] = localServiceTicket.RXVoltCheckHaveUPS;
                if (localServiceTicket.RXVoltCheckHaveUPS)
                {
                    if (!String.IsNullOrEmpty(localServiceTicket.RXVoltUPSCapacity))
                        toSave[Config.SPCSERVTICKET_UPSCAP] = localServiceTicket.RXVoltUPSCapacity;
                    if (!String.IsNullOrEmpty(localServiceTicket.RXVoltUPSGroundVoltage))
                        toSave[Config.SPCSERVTICKET_UPSGROUNDVOLT] = localServiceTicket.RXVoltUPSGroundVoltage;
                    if (!String.IsNullOrEmpty(localServiceTicket.RXVoltUPSInVoltage))
                        toSave[Config.SPCSERVTICKET_UPSINVOLT] = localServiceTicket.RXVoltUPSInVoltage;
                }
                toSave[Config.SPCSERVTICKET_CHECKISOLATIONTRANSF] = localServiceTicket.RXVoltCheckIsolationTransformator;
                #endregion
                #region Radiation
                if (!localServiceTicket.RXRadHWCalibrationDate.Equals(default(DateTime)))
                    toSave[Config.SPCSERVTICKET_CALIBRATIONDATE] = localServiceTicket.RXRadHWCalibrationDate;
                if (!localServiceTicket.RXRadHWCalibrationDueDate.Equals(default(DateTime)))
                    toSave[Config.SPCSERVTICKET_CALIBRATIONDUEDATE] = localServiceTicket.RXRadHWCalibrationDueDate;
                if (!String.IsNullOrEmpty(localServiceTicket.RXRadHWModel))
                    toSave[Config.SPCSERVTICKET_CALMODEL] = localServiceTicket.RXRadHWModel;
                if (!String.IsNullOrEmpty(localServiceTicket.RXRadHWTrademark))
                    toSave[Config.SPCSERVTICKET_CALTRADEMARK] = localServiceTicket.RXRadHWTrademark;
                if (!String.IsNullOrEmpty(localServiceTicket.RXRadOperatorRad))
                    toSave[Config.SPCSERVTICKET_OPERATORRAD] = localServiceTicket.RXRadOperatorRad;
                if (!String.IsNullOrEmpty(localServiceTicket.RXRadTunnelRadIn))
                    toSave[Config.SPCSERVTICKET_TUNNELRADIN] = localServiceTicket.RXRadTunnelRadIn;
                if (!String.IsNullOrEmpty(localServiceTicket.RXRadTunnelRadOut))
                    toSave[Config.SPCSERVTICKET_TUNNELRADOUT] = localServiceTicket.RXRadTunnelRadOut;
                if (localServiceTicket.RXRadRadiationState != Types.SPCSERVTICKET_RADSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_RADSTATE] = new OptionSetValue((int)localServiceTicket.RXRadRadiationState);
                #endregion
                #region Calibration          
                if (localServiceTicket.RXCalCalibrationState != Types.SPCSERVTICKET_RADSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_CALIBRATIONSTATE] = new OptionSetValue((int)localServiceTicket.RXCalCalibrationState);
                if (!String.IsNullOrEmpty(localServiceTicket.RXCalSteelPenetration))
                    toSave[Config.SPCSERVTICKET_STEELPENETRATION] = localServiceTicket.RXCalSteelPenetration;
                if (!String.IsNullOrEmpty(localServiceTicket.RXCalWireResolution))
                    toSave[Config.SPCSERVTICKET_WIRERESOLUTION] = localServiceTicket.RXCalWireResolution;
                toSave[Config.SPCSERVTICKET_TEST1] = localServiceTicket.RXCalType1;
                toSave[Config.SPCSERVTICKET_TEST2] = localServiceTicket.RXCalType2;
                toSave[Config.SPCSERVTICKET_TEST3] = localServiceTicket.RXCalType3;
                toSave[Config.SPCSERVTICKET_TEST4] = localServiceTicket.RXCalType4;
                #endregion
                #region Software
                if (!String.IsNullOrEmpty(localServiceTicket.RXSoftSoftwareVersion))
                    toSave[Config.SPCSERVTICKET_SOFTVERSION] = localServiceTicket.RXSoftSoftwareVersion;
                if (!String.IsNullOrEmpty(localServiceTicket.RXSoftPhysicalDongleSerial))
                    toSave[Config.SPCSERVTICKET_PHYSICALDONGLE] = localServiceTicket.RXSoftPhysicalDongleSerial;
                if (!String.IsNullOrEmpty(localServiceTicket.RXSoftSoftwareDongleSerial))
                    toSave[Config.SPCSERVTICKET_SOFTWAREDONGLE] = localServiceTicket.RXSoftSoftwareDongleSerial;
                if (localServiceTicket.RXSoftHaveHDA != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEHDA] = new OptionSetValue((int)localServiceTicket.RXSoftHaveHDA);
                if (localServiceTicket.RXSoftHaveHISPOT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEHISPOT] = new OptionSetValue((int)localServiceTicket.RXSoftHaveHISPOT);
                if (localServiceTicket.RXSoftHaveHITIP != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEHITIP] = new OptionSetValue((int)localServiceTicket.RXSoftHaveHITIP);
                if (localServiceTicket.RXSoftHaveIMS != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEIMS] = new OptionSetValue((int)localServiceTicket.RXSoftHaveIMS);
                if (localServiceTicket.RXSoftHaveSEN != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVESEN] = new OptionSetValue((int)localServiceTicket.RXSoftHaveSEN);
                if (localServiceTicket.RXSoftHaveXACT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEXACT] = new OptionSetValue((int)localServiceTicket.RXSoftHaveXACT);
                if (localServiceTicket.RXSoftHaveXPLORE != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEXPLORE] = new OptionSetValue((int)localServiceTicket.RXSoftHaveXPLORE);
                if (localServiceTicket.RXSoftHaveXPORT != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEXPORT] = new OptionSetValue((int)localServiceTicket.RXSoftHaveXPORT);
                if (localServiceTicket.RXSoftHaveXTRAIN != Types.SPCSERVTICKET_POSSESIONSTATE.Undefined)
                    toSave[Config.SPCSERVTICKET_HAVEXTRAIN] = new OptionSetValue((int)localServiceTicket.RXSoftHaveXTRAIN);
                if (localServiceTicket.RXSoftTechnology != Types.SPCSERVTICKET_TECHNOLOGY.Undefined)
                    toSave[Config.SPCSERVTICKET_SOFTTECHNOLOGY] = new OptionSetValue((int)localServiceTicket.RXSoftTechnology);
                #endregion
            }
            #endregion
            #region Update Feedback Submitted
            if (localServiceTicket.FeedbackSubmitted)
                toSave[Config.SPCSERVTICKET_FEEDBACK] = true;
            #endregion
            await Proxy.Update(toSave);
        }

        /// <summary>
        /// Synchronization operation to close a service ticket
        /// </summary>
        /// <param name="serviceticketid">Id of the service ticket wanted to be closed</param>
        /// <returns>void</returns>
        private static async Task SyncCloseServiceTicket(int serviceticketid)
        {
            var conn = await Local.GetConnection();
            ServiceTicket localServiceTicket = await conn.GetWithChildrenAsync<ServiceTicket>(serviceticketid, true);
            await FinishServiceTicket(localServiceTicket);
        }

        /// <summary>
        /// Synchronization operation to update an incident
        /// </summary>
        /// <param name="inc">Incident to update</param>
        /// <returns>Void</returns>
        private static async Task SyncUpdateIncident(Incident inc)
        {
            Entity incident = new Entity(Config.SPCCASE, inc.InternalId);
            incident.Attributes[Config.SPCCASE_FEEDBACK1] = inc.FeedbackAnswer1;
            incident.Attributes[Config.SPCCASE_FEEDBACK2] = inc.FeedbackAnswer2;
            if (!string.IsNullOrEmpty(inc.ClientFeedback))
                incident.Attributes[Config.SPCCASE_FEEDBACKOPINION] = inc.ClientFeedback;
            await Proxy.Update(incident);
        }
    }
}