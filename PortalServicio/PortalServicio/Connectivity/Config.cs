using PortalAPI.Contracts;
using PortalServicio.Models;
using System;
using System.Collections.Generic;

namespace PortalServicio.Configuration
{

    /// <summary>
    /// Contiene configuración sobre nombres de atributos y entidades del CRM, parámetros de conexión y algunas constantes.
    /// </summary>
    public static class Config
    {
        //Configuration for Authentication using ADAL v3
        #region CRM Connection String
        //SPC Information needed in order to connect to the CRM.
        public const string CLIENT_ID = "d95cf106-b01f-41b2-92fa-081f28dc5c03";
        public const string REDIRECT_URI = "https://portalservicio.spctccr.com";
        public const string RESOURCE = "https://spctc.spctccr.com:446";
        public const string AUTHORITY = "https://spctccr.com/adfs/services/trust";
        public static string SERVER = "https://spc-32.grupospc.local";
        //public static string SERVER = "https://192.168.0.19:443";
        #endregion

        #region API Actions
        public const string GETINCIDENTS = "/api/Incident";
        public const string FINDINCIDENTS = "/api/Incident/FindIncidents";
        public const string GETINCIDENT = "/api/Incident";
        public const string CREATESERVICETICKET = "api/ServiceTicket/Create";
        public const string DELETESERVICETICKET = "api/ServiceTicket";
        #endregion

        #region CRM Filter Criteria 
        //How many months of cases to retrieve from CRM
        public const int LASTXMONTHS = 3;
        //How many days have to pass to update non critical entities [currency, technician]
        public const int NONCRITICALXDAYS = 2;
        //How manu minute have to pass to update critial entities []
        public const int CRITICALXMINUTES = 1;
        #endregion

        #region CRM Entities
        public const string SPCACCOUNT = "account";
        public const string SPCBUSINESSUNIT = "businessunit";
        public const string SPCCATEGORY = "new_categoriacliente";
        public const string SPCCASE = "incident";
        public const string SPCCURRENCY = "transactioncurrency";
        public const string SPCCONTRACTOR = "new_contratista";
        public const string SPCCDT = "new_comienzosdetrabajo";
        public const string SPCEXTERNALCONTRACT = "new_contratomoexterna";
        public const string SPCLEGALIZATION = "new_legalizacion";
        public const string SPCMATERIAL = "new_materialesyrepuestos";
        public const string SPCNOTES = "annotation";
        public const string SPCWORKER = "new_trabajadores";
        public const string SPCPRICELIST = "pricelevel";
        public const string SPCPRODUCT = "product";
        public const string SPCPRODUCTPLIST = "productpricelevel";
        public const string SPCSERVTICKET = "new_boletadeservicio";
        public const string SPCPROJECTEQUIPMENT = "new_equiposcompletos";
        public const string SPCPROJECTMATERIAL = "new_materiales";
        public const string SPCREQUESTORDEREQUIPMENT = "new_solicituddeequipos";
        public const string SPCREQUESTORDERMATERIAL = "new_solicitudretiromateriales";
        public const string SPCLINEREQORDEREQUIP = "new_lineassolicitudequipos";
        public const string SPCLINEREQORDERMATERIAL = "new_lineassolicitudmateriales";
        public const string SPCEXTRAEQUIPMENT = "new_equiposadicionales";
        public const string SPCSTORAGE = "new_bodega";
        public const string SPCSYSTEM = "new_sistema";
        public const string SPCTECHNICIAN = "new_tecnicos";
        public const string SPCTICKET = "new_ticketcliente";
        public const string SYSUSER = "systemuser";
        public const string SPCFIELDPERMISSION = "fieldpermission";
        public const string SPCSECPROFILE = "fieldsecurityprofile";
        public const string SPCCDTTICKET = "new_supervisionpendientes";
        public const string SPCTECHREGISTRY = "new_horascdt";
        public const string SPCHOLYDAY = "calendar";
        public const string SPCLEGALIZATIONITEM = "new_itemlegalizacion";
        #endregion

        #region CRM Entity Fields
        public const string GENERAL_CREATEDON = "createdon";
        public const string GENERAL_CREATEDONOVERRIDE = "overriddencreatedon";
        public const string GENERAL_OWNER = "ownerid";
        public const string GENERAL_STATUSCODE = "statuscode";
        public const string GENERAL_STATECODE = "statecode";
        public const string SPCACCOUNT_ID = "accountid";
        public const string SPCACCOUNT_NUMBER = "accountnumber";
        public const string SPCACCOUNT_ADDRESS = "address1_line2";
        public const string SPCACCOUNT_ALIAS = "new_alias";
        public const string SPCACCOUNT_COUNTRY = "new_paisid";
        public const string SPCACCOUNT_EMAIL = "emailaddress1";
        public const string SPCACCOUNT_LOCATION = "new_geo";
        public const string SPCACCOUNT_NAME = "name";
        public const string SPCACCOUNT_PHONE = "telephone1";
        public const string SPCACCOUNT_PRICELIST = "defaultpricelevelid";
        public const string SPCACCOUNT_REPORTTYPE = "new_tipoboleta";
        public const string SPCACCOUNT_EXEMPT = "new_exento_impuesto";
        public const string SPCACCOUNT_CATEGORY = "new_categoria_clienteid";
        public const string SPCCDT_ID = "new_comienzosdetrabajoid";
        public const string SPCCDT_NUMBER = "new_name";
        public const string SPCCDT_CLIENT = "new_clienteid";
        public const string SPCCDT_FINALCLIENT = "new_clientefinal";
        public const string SPCCDT_MONITORACCOUNTNUMBER = "new_noctamonitoreo";
        public const string SPCCDT_MONITORACCOUNTNAME = "new_nombreenmonitoreo";
        public const string SPCCDT_MAINCONTACT = "new_contactoprimario";
        public const string SPCCDT_SECONDARYCONTACT = "new_contactosecundario";
        public const string SPCCDT_MAINCONTACTEMAIL = "new_emailcontacto1";
        public const string SPCCDT_SECONDARYCONTACTEMAIL = "new_emailcontacto2";
        public const string SPCCDT_MAINCONTACTPHONE = "new_telefonocontacto1";
        public const string SPCCDT_SECONDARYCONTACTPHONE = "new_telefonocontato2";
        public const string SPCCDT_DESCRIPTION = "new_descripcion";
        public const string SPCCDT_SYSTEM = "new_sistemaid";
        public const string SPCCDT_PROJECTSTATE = "new_estadodelproyecto";
        public const string SPCCDT_PROJECTSTARTDATE = "new_fechainicioproyecto";
        public const string SPCCDT_PROJECTDEADLINE = "new_fechalimiteinstalar";
        public const string SPCCDT_ISAPPROVEDADMINISTRATION = "new_vbgerenciaadministrativa";
        public const string SPCCDT_ISAPPROVEDCOMERCIAL = "new_vbgerenciacomercial";
        public const string SPCCDT_ISAPPROVEDFINANCIAL = "new_vbgerenciafinanciera";
        public const string SPCCDT_ISAPPROVEDINSTALLATION = "new_vbgerenciainstalaciones";
        public const string SPCCDT_ISAPPROVEDOPERATIONS = "new_vbgerenciaoperaciones";
        public const string SPCCDT_ISAPPROVEDPLANNING = "new_vbgerenciaplaneacion";
        public const string SPCCDT_ISAPPROVEDCUSTOMERSERVICE = "new_vbservicioalcliente";
        public const string SPCCDT_APPROVEDADMINISTRATIONBY = "new_u2";
        public const string SPCCDT_APPROVEDCOMERCIALBY = "new_u3";
        public const string SPCCDT_APPROVEDFINANCIALBY = "new_u1";
        public const string SPCCDT_APPROVEDINSTALLATIONBY = "new_u5";
        public const string SPCCDT_APPROVEDOPERATIONSBY = "new_u7";
        public const string SPCCDT_APPROVEDPLANNINGBY = "new_u4";
        public const string SPCCDT_APPROVEDCUSTOMERSERVICEBY = "new_u6";
        public const string SPCCDT_ISAPPROVED = "new_tienevb";
        public const string SPCCATEGORY_ID = "new_categoriaclienteid";
        public const string SPCCATEGORY_CODE = "new_codigo";
        public const string SPCCATEGORY_NAME = "new_name";
        public const string SPCCASE_ID = "incidentid";
        public const string SPCCASE_TITLE = "title";
        public const string SPCCASE_DESCRIPTION = "description";
        public const string SPCCASE_PAYMENT = "new_opcionesdelcobro";
        public const string SPCCASE_REPRESENTATIVE = "new_asesordeservicioalclienteid";
        public const string SPCCASE_CURRENCY = "transactioncurrencyid";
        public const string SPCCASE_TECHNICIAN1 = "new_tecnicoid";
        public const string SPCCASE_TECHNICIAN2 = "new_tecnico2";
        public const string SPCCASE_TECHNICIAN3 = "new_tenico3";
        public const string SPCCASE_SYSTEM = "new_sistema";
        public const string SPCCASE_PROGRAMMED = "new_fechadeprogramacion";
        public const string SPCCASE_CASENUMBER = "ticketnumber";
        public const string SPCCASE_TICKET = "new_ticketid";
        public const string SPCCASE_CLIENT = "customerid";
        public const string SPCCASE_INCIDENCE = "new_incidencia";
        public const string SPCCASE_CONTROL = "new_controldeatencin";
        public const string SPCCASE_CIA = "new_cia";
        public const string SPCCASE_FEEDBACK1 = "new_p1";
        public const string SPCCASE_FEEDBACK2 = "new_p2";
        public const string SPCCASE_FEEDBACKOPINION = "new_opinion";
        public const string SPCCASE_REVIEWED = "new_revisado";
        public const string SPCCASE_REQUESTEDDATE = "new_fechadesolicitud";
        public const string SPCCASE_MONEYINADVANCE = "new_montoadelantado";
        public const string SPCCURRENCY_CODE = "isocurrencycode";
        public const string SPCCURRENCY_NAME = "currencyname";
        public const string SPCCURRENCY_QUETZAL = "GTQ";
        public const string SPCCURRENCY_SYMBOL = "currencysymbol";
        public const string SPCCURRENCY_ID = "transactioncurrencyid";
        public const string SPCBUSINESSUNIT_ID = "businessunitid";
        public const string SPCBUSINESSUNIT_NAME = "name";
        public const string SPCLEGALIZATION_ID = "new_legalizacionid";
        public const string SPCLEGALIZATION_SERVTICKET_ID = "new_boletaid";
        public const string SPCLEGALIZATION_INCIDENT_ID = "new_caso";
        public const string SPCLEGALIZATION_CDT_ID = "new_cdt";
        public const string SPCLEGALIZATION_CURRENCYID = "transactioncurrencyid";
        public const string SPCLEGALIZATION_LEGALIZATOR = "new_legalizador";
        public const string SPCLEGALIZATION_TYPE = "new_tipo";
        public const string SPCLEGALIZATION_PROJECTISSUE = "new_imprevistodelproyecto";
        public const string SPCLEGALIZATION_DETAIL = "new_detalle";
        public const string SPCLEGALIZATION_MONEYREQUESTED = "new_montosolicitado";
        public const string SPCLEGALIZATION_MONEYPAID = "new_montolegalizado";
        public const string SPCLEGALIZATION_LEGALIZATORSIGN = "new_firma";
        public const string SPCLEGALIZATION_MANAGERSIGN = "new_firmagerentearea";
        public const string SPCLEGALIZATION_FINANCIALSIGN = "new_revisadayaprobadatesoreria";
        public const string SPCLEGALIZATION_NUMBER = "new_nodelegalizacion";
        public const string SPCLEGALIZATION_COMPANY = "new_empresa";
        public const string SPCLEGALIZATION_LASTCREDITCARDNUMBERS = "new_ultimos4digitostarjetacorporativa";
        public const string SPCLEGALIZATIONITEM_ID = "new_itemlegalizacionid";
        public const string SPCLEGALIZATIONITEM_CURRENCY = "transactioncurrencyid";
        public const string SPCLEGALIZATIONITEM_SPENTON = "new_fechadelgasto";
        public const string SPCLEGALIZATIONITEM_BILL = "new_factura";
        public const string SPCLEGALIZATIONITEM_PROJECTISSUE = "new_imprevisto";
        public const string SPCLEGALIZATIONITEM_LEGALIZATION = "new_legalizacion";
        public const string SPCLEGALIZATIONITEM_MONEYSPENT = "new_monto";
        public const string SPCLEGALIZATIONITEM_PAIDTO = "new_name";
        public const string SPCLEGALIZATIONITEM_TYPE = "new_tipogasto";
        public const string SPCMATERIAL_DESTINATION = "new_ubicacion";
        public const string SPCMATERIAL_PRODUCT = "new_articulo";
        public const string SPCMATERIAL_QUANTITY = "new_cantidad";
        public const string SPCMATERIAL_REQUESTNUMBER = "new_numerodesolicitud";
        public const string SPCMATERIAL_SERIALS = "new_serie";
        public const string SPCMATERIAL_SERVICETICKET = "new_boletaid";
        public const string SPCMATERIAL_TREATMENT = "new_tratamiento";
        public const string SPCNOTES_FILENAME = "filename";
        public const string SPCNOTES_CONTENT = "documentbody";
        public const string SPCNOTES_LINK = "objectid";
        public const string SPCNOTES_TITLE = "subject";
        public const string SPCNOTES_MIME = "mimetype";
        public const string SPCREQUESTORDEREQUIPMENT_ID = "new_solicituddeequiposid";
        public const string SPCREQUESTORDEREQUIPMENT_NUMBER = "new_nosolicitud";
        public const string SPCREQUESTORDEREQUIPMENT_ISAPPROVED = "new_vbbodega";
        public const string SPCREQUESTORDEREQUIPMENT_APPROVEDDATE = "new_fechavb";
        public const string SPCREQUESTORDEREQUIPMENT_CDT = "new_cdt";
        public const string SPCLINEREQORDEREQUIP_PRODUCTCODE = "new_name";
        public const string SPCLINEREQORDEREQUIP_ID = "new_lineassolicitudequiposid";
        public const string SPCLINEREQORDEREQUIP_PRODUCT = "new_equipo";
        public const string SPCLINEREQORDEREQUIP_EQUIPMENTORDER = "new_solicitudequipos";
        public const string SPCLINEREQORDEREQUIP_REQUESTED = "new_cantidadretirada";

        public const string SPCREQUESTORDERMATERIAL_ID = "new_solicitudretiromaterialesid";
        public const string SPCREQUESTORDERMATERIAL_NUMBER = "new_nosolicitud";
        public const string SPCREQUESTORDERMATERIAL_ISAPPROVED = "new_vbbodega";
        public const string SPCREQUESTORDERMATERIAL_APPROVEDDATE = "new_fechavb";
        public const string SPCREQUESTORDERMATERIAL_CDT = "new_cdt";
        public const string SPCLINEREQORDERMAT_MATCODE = "new_name";
        public const string SPCLINEREQORDERMAT_ID = "new_lineassolicitudmaterialesid";
        public const string SPCLINEREQORDERMAT_MATERIAL = "new_material";
        public const string SPCLINEREQORDERMAT_MATERIALORDER = "new_solicituid";
        public const string SPCLINEREQORDERMAT_REQUESTED = "new_cantsolicitada";

        public const string SPCPRODUCT_GUID = "productid";
        public const string SPCPRODUCT_ID = "productnumber";
        public const string SPCPRODUCT_NAME = "name";
        public const string SPCPRODUCT_STATE = "statecode";
        public const string SPCPRODUCT_MOSC = "new_mosc";
        public const string SPCPRODUCT_MOSCPRICE = "new_c1";
        public const string SPCPRODUCT_COST = "new_costo_std_dol";
        public const string SPCPRODUCT_LAWTAX = "new_ley";
        public const string SPCPRODUCT_DAITAX = "new_dai";
        public const string SPCPRODUCT_SELECTIVETAX = "new_selectivo";
        public const string SPCPRODUCT_BOUGHT = "new_localexterior";
        public const string SPCPRODUCTPLIST_PRICE = "amount";
        public const string SPCPRODUCTPLIST_PRICELIST = "pricelevelid";
        public const string SPCPRODUCTPLIST_PRODUCT = "productid";
        public const string SPCPROJECTEQUIPMENT_ID = "new_equiposcompletosid";
        public const string SPCPROJECTEQUIPMENT_QUANTITY = "new_cantidad";
        public const string SPCPROJECTEQUIPMENT_REMAINING = "new_cantdisponible";
        public const string SPCPROJECTEQUIPMENT_REQUESTED = "new_cantidadsolicitada";
        public const string SPCPROJECTEQUIPMENT_CLAIMED = "new_cantidadretirada";
        public const string SPCPROJECTEQUIPMENT_CDT = "new_cdtid";
        public const string SPCPROJECTEQUIPMENT_CODE = "new_codigo";
        public const string SPCPROJECTEQUIPMENT_PRODUCT = "new_equipo";
        public const string SPCPROJECTEQUIPMENT_ISADDITIONAL = "new_equipoadicional";
        public const string SPCPROJECTEQUIPMENT_ISCANCELED = "new_anularequipo";
        public const string SPCPROJECTEQUIPMENT_SRE = "new_sre";
        public const string SPCPROJECTMATERIAL_ID = "new_materialesid";
        public const string SPCPROJECTMATERIAL_REQUESTED = "new_cantsolicitada";
        public const string SPCPROJECTMATERIAL_CLAIMED = "new_cantretirada";
        public const string SPCPROJECTMATERIAL_CDT = "new_cdtid";
        public const string SPCPROJECTMATERIAL_PRODUCT = "new_productoid";
        public const string SPCPROJECTMATERIAL_QUANTITY = "new_cantidad";
        public const string SPCPROJECTMATERIAL_REMAINING = "new_saldoxentregar";
        public const string SPCPROJECTMATERIAL_ISADDITIONAL = "new_materialadicional";
        public const string SPCSERVTICKET_CASEID = "new_casoid";
        public const string SPCSERVTICKET_CASENUMBER = "new_nocaso"; //VISUAL
        public const string SPCSERVTICKET_CLIENT = "new_cliente";
        public const string SPCSERVTICKET_CURRENCY = "transactioncurrencyid";
        public const string SPCSERVTICKET_CODE = "new_codigo";
        public const string SPCSERVTICKET_DESCRIPTION = "new_descripcion";
        public const string SPCSERVTICKET_EMAIL = "emailaddress";
        public const string SPCSERVTICKET_FINISHED = "new_salida";
        public const string SPCSERVTICKET_HADLUNCH = "new_rebajaalmuerzo";
        public const string SPCSERVTICKET_NUMBER = "new_autonumber";
        public const string SPCSERVTICKET_STARTED = "new_entrada";
        public const string SPCSERVTICKET_SYSTEM = "new_sistemaid";
        public const string SPCSERVTICKET_TECHNICIAN1 = "new_tecnicoid";
        public const string SPCSERVTICKET_TECHNICIAN2 = "new_tecnico2";
        public const string SPCSERVTICKET_TECHNICIAN3 = "new_tecnico3";
        public const string SPCSERVTICKET_TECHNICIAN4 = "new_tecnico4";
        public const string SPCSERVTICKET_TECHNICIAN5 = "new_tecnico5";
        public const string SPCSERVTICKET_TITLE = "new_name";
        public const string SPCSERVTICKET_WORKDONE = "new_trabajosrealizados";
        public const string SPCSERVTICKET_FEEDBACK = "new_encuestarealizada";
        public const string SPCSERVTICKET_QUOTATION = "new_requiereval";
        #region General
        public const string SPCSERVTICKET_RXMODEL = "new_modelo";
        public const string SPCSERVTICKET_RXSERIAL = "new_noserial";
        public const string SPCSERVTICKET_RXCREATIONDATE = "new_fabricacion";
        public const string SPCSERVTICKET_RXSYNERGY = "new_noproyectosynergy";
        public const string SPCSERVTICKET_CLEANAREA = "new_areadespejada";
        public const string SPCSERVTICKET_COMMENTS = "new_observacionesyestadofinal";
        public const string SPCSERVTICKET_VISITNUMBER = "new_novisitapreventiva";
        public const string SPCSERVTICKET_HWSTATE = "new_estadoequipo";
        #endregion
        #region Manteinance
        public const string SPCSERVTICKET_CHECKLABELS = "new_cubiertaetiquetas";
        public const string SPCSERVTICKET_CHECKINOUTSYSTEM = "new_limpiezageneral";
        public const string SPCSERVTICKET_CHECKIRFENCES = "new_limpiezadebarreras";
        public const string SPCSERVTICKET_CHECKCONTROLELEMENTS = "new_verificacionelementoscontrol";
        public const string SPCSERVTICKET_CHECKENGINECONTROL = "new_verificacionrelevoscontroldemotor";
        public const string SPCSERVTICKET_CHECKCONVEYORBELT = "new_limpcalibbandatr";
        public const string SPCSERVTICKET_CHECKENGINETRACTION = "new_verificacionarrastremotor";
        public const string SPCSERVTICKET_CHECKROLLERS = "new_verificacionrodilloscargadescarga";
        public const string SPCSERVTICKET_CHECKEMERGENCYSTOP = "new_verificacionparadasemergencia";
        public const string SPCSERVTICKET_CHECKINTERLOCK = "new_verificacioninterlock";
        public const string SPCSERVTICKET_CHECKVOLTMONITOR = "new_verificacionmonitorcorriente";
        public const string SPCSERVTICKET_CHECKSECCIRCUIT = "new_verificacioncircuitoseguridad";
        public const string SPCSERVTICKET_CHECKACSYSTEM = "new_verificacionsistemadeventilacion";
        public const string SPCSERVTICKET_CHECKOS = "new_verificacionpruebasistemaoperativo";
        public const string SPCSERVTICKET_CHECKXRCONE = "new_verificacionconoemisionrx";
        public const string SPCSERVTICKET_CHECKLINEDETMODS = "new_verificacionlinea";
        public const string SPCSERVTICKET_CHECKCONFIGURATION = "new_verificacionparametrosgenerales";
        public const string SPCSERVTICKET_CHECKKEYBOARD = "new_verificacionteclado";
        public const string SPCSERVTICKET_CHECKMONITORCONFIG = "new_verificacionmonitor";
        public const string SPCSERVTICKET_CHECKTWOWAY = "new_inspeccionendoblesentido";
        public const string SPCSERVTICKET_CHECKRADINDICATORS = "new_verificacioncubiertasindrad";
        #endregion
        #region Voltage
        public const string SPCSERVTICKET_CHECKHAVEUPS = "new_tieneups";
        public const string SPCSERVTICKET_CHECKISOLATIONTRANSF = "new_transformadordeaislamiento";
        public const string SPCSERVTICKET_SCREENTYPE = "new_tipomonitor";
        public const string SPCSERVTICKET_LEADSTATE = "new_estadocortinasdeplomo";
        public const string SPCSERVTICKET_INVOLTAGE = "new_voltajealemntacionacometida";
        public const string SPCSERVTICKET_GROUNDVOLTAGE = "new_volatjeneutrotierraacometida";
        public const string SPCSERVTICKET_UPSCAP = "new_capacidadups";
        public const string SPCSERVTICKET_UPSINVOLT = "new_voltajesalidaups";
        public const string SPCSERVTICKET_UPSGROUNDVOLT = "new_voltajeneutrotierraups";
        public const string SPCSERVTICKET_GEN1ONVOLTAGE = "new_corrientedefilamentorxon";
        public const string SPCSERVTICKET_GEN1OFFVOLTAGE = "new_corrientefilamentorxoff";
        public const string SPCSERVTICKET_GEN1ONANODEVOLT = "new_corrienteanodo";
        public const string SPCSERVTICKET_GEN1ONHIGHVOLT = "new_altovoltaje";
        public const string SPCSERVTICKET_GEN2ONVOLTAGE = "new_corrientefilamentorxon2";
        public const string SPCSERVTICKET_GEN2OFFVOLTAGE = "new_corrientefilamentorxoff2";
        public const string SPCSERVTICKET_GEN2ONANODEVOLT = "new_corrienteanodo2";
        public const string SPCSERVTICKET_GEN2ONHIGHVOLT = "new_altovoltaje2";
        #endregion
        #region RadiationMetrics
        public const string SPCSERVTICKET_CALTRADEMARK = "new_marca";
        public const string SPCSERVTICKET_CALMODEL = "new_mod";
        public const string SPCSERVTICKET_CALIBRATIONDATE = "new_fechacalibracionmedidor";
        public const string SPCSERVTICKET_CALIBRATIONDUEDATE = "new_fechavenccalibmedidor";
        public const string SPCSERVTICKET_TUNNELRADIN = "new_ingresodeltunel";
        public const string SPCSERVTICKET_TUNNELRADOUT = "new_salidadeltunel";
        public const string SPCSERVTICKET_OPERATORRAD = "new_costadooperadora5cm";
        public const string SPCSERVTICKET_RADSTATE = "new_cumplimientosnivelradiacion";
        #endregion
        #region Software (Optional)
        public const string SPCSERVTICKET_PHYSICALDONGLE = "new_dongleserialfisico";
        public const string SPCSERVTICKET_SOFTWAREDONGLE = "new_dongleserialsoftware";
        public const string SPCSERVTICKET_SOFTVERSION = "new_versiondesoftware";
        public const string SPCSERVTICKET_HAVESEN = "new_sen";
        public const string SPCSERVTICKET_HAVEHITIP = "new_hiptip";
        public const string SPCSERVTICKET_HAVEXPLORE = "new_xplore";
        public const string SPCSERVTICKET_HAVEHISPOT = "new_hispot";
        public const string SPCSERVTICKET_HAVEIMS = "new_ims";
        public const string SPCSERVTICKET_HAVEXACT = "new_xact";
        public const string SPCSERVTICKET_HAVEHDA = "new_hda";
        public const string SPCSERVTICKET_HAVEXPORT = "new_xport";
        public const string SPCSERVTICKET_HAVEXTRAIN = "new_xtrain";
        public const string SPCSERVTICKET_SOFTTECHNOLOGY = "new_tegnologia";
        #endregion
        #region Calibration
        public const string SPCSERVTICKET_TESTTYPE = "new_testtype"; //doesn't exist
        public const string SPCSERVTICKET_TEST1 = "new_cunaescalonada";
        public const string SPCSERVTICKET_TEST2 = "new_maletindepruebas";
        public const string SPCSERVTICKET_TEST3 = "new_cuerpodecalibracion";
        public const string SPCSERVTICKET_TEST4 = "new_calibracionporsoftware";
        public const string SPCSERVTICKET_STEELPENETRATION = "new_penetraciondeacero";
        public const string SPCSERVTICKET_WIRERESOLUTION = "new_resoluciondealambre";
        public const string SPCSERVTICKET_CALIBRATIONSTATE = "new_cumplimientodecalibracion";
        #endregion
        public const string SPCSYSTEM_NAME = "new_name";
        public const string SPCSYSTEM_ID = "new_sistemaid";
        public const string SPCSTORAGE_ID = "new_bodegaid";
        public const string SPCSTORAGE_NAME = "new_name";
        public const string SPCSTORAGE_TECHNICIAN = "new_tecnicoid";
        public const string SPCTECHNICIAN_NAME = "new_name";
        public const string SPCTECHNICIAN_STORAGE = "new_bodega";
        public const string SPCTECHNICIAN_CATEGORY = "new_categoria";
        public const string SPCTECHNICIAN_USERNAME = "new_systemuserid";
        public const string SPCTICKET_CLIENT = "new_cuenta";
        public const string SPCTICKET_ID = "new_ticketclienteid";
        public const string SPCTICKET_NEED = "new_necesidad";
        public const string SPCTICKET_PRIORITY = "new_prioridad";
        public const string SPCTICKET_TITLE = "new_titulo";
        public const string SPCTICKET_TECHNICIANS = "new_tecnicos";
        public const string SPCTICKET_EXPECTEDDATE = "new_fechapropuestacliente";
        public const string SPCTICKET_STATE = "statecode";
        public const string SYSUSER_FULLNAME = "fullname";
        public const string SYSUSER_ID = "systemuserid";
        public const string SPCEXTERNALCONTRACT_CDT = "new_cdtid";
        public const string SPCEXTERNALCONTRACT_NUMBER = "new_autonumber";
        public const string SPCEXTERNALCONTRACT_PAYMENT = "new_condiciondepago";
        public const string SPCEXTERNALCONTRACT_CONTRACTNUMBER = "new_nocontrato";
        public const string SPCEXTERNALCONTRACT_CONTRACTOR = "new_cotratistaid";
        public const string SPCEXTERNALCONTRACT_CURRENCY = "transactioncurrencyid";
        public const string SPCEXTERNALCONTRACT_AMOUNTTOTAL = "new_montodelcontrato";
        public const string SPCEXTERNALCONTRACT_AMOUNTPAID = "new_montocancelado";
        public const string SPCEXTERNALCONTRACT_SIGNED = "new_contratofirmado";
        public const string SPCEXTERNALCONTRACT_STARTDATE = "new_fechainicio";
        public const string SPCEXTERNALCONTRACT_FINISHDATE = "new_fechadefinalizacion";
        public const string SPCEXTERNALCONTRACT_PROGRESS = "new_porcentajeavance";
        public const string SPCEXTRAEQUIPMENT_ID = "new_equiposadicionalesid";
        public const string SPCEXTRAEQUIPMENT_QUANTITY = "new_cantidad";
        public const string SPCEXTRAEQUIPMENT_CDT = "new_cdt";
        public const string SPCEXTRAEQUIPMENT_EQUIPMENT = "new_equipo";
        public const string SPCEXTRAEQUIPMENT_APPROVED = "new_autorizado";
        public const string SPCEXTRAEQUIPMENT_REASON = "new_motivo";
        public const string SPCEXTRAEQUIPMENT_PROCESSTYPE = "new_tipoproceso";
        public const string SPCCONTRACTOR_ID = "new_contratistaid";
        public const string SPCCONTRACTOR_NAME = "new_name";
        public const string SPCCONTRACTOR_ADDRESS = "new_direccion";
        public const string SPCCONTRACTOR_PHONE = "new_telefono";
        public const string SPCCONTRACTOR_IDENTIFICATION = "new_noidentificacion";
        public const string SPCCONTRACTOR_USER = "new_userid";
        public const string SPCWORKER_ID = "new_trabajadoresid";
        public const string SPCWORKER_NAME = "new_name";
        public const string SPCWORKER_IDENTIFICATION = "new_noidentificacion";
        public const string SPCWORKER_STATE = "new_statecode";
        public const string SPCWORKER_CONTRACT = "new_contratoid";
        public const string SPCWORKER_CCSS = "new_ordenpatronal";
        public const string SPCWORKER_DELINCUENCIA = "new_hojadelincuencia";
        public const string SPCWORKER_POLIZA = "new_polizart";
        public const string SPCCDTTICKET_ID = "new_supervisionpendientesid";
        public const string SPCCDTTICKET_CLIENT = "new_cliente";
        public const string SPCCDTTICKET_STARTED = "new_fechahoraentrada";
        public const string SPCCDTTICKET_FINISHED = "new_salida";
        public const string SPCCDTTICKET_WORKDONE = "new_actividad";
        public const string SPCCDTTICKET_AGREEMENTS = "new_acuerdos";
        public const string SPCCDTTICKET_NUMBER = "new_name";
        public const string SPCCDTTICKET_HADLUNCH = "new_rebajaalmuerzo";
        public const string SPCCDTTICKET_CDT = "new_supervisionplaneacionid";
        public const string SPCCDTTICKET_EMAIL = "emailaddress";
        public const string SPCFIELDPERMISSION_SECPROFILE = "fieldsecurityprofileid";
        public const string SPCFIELDPERMISSION_ENTITYNAME = "entityname";
        public const string SPCFIELDPERMISSION_ATTRIBUTE = "attributelogicalname";
        public const string SPCFIELDPERMISSION_CANUPDATE = "canupdate";
        public const string SPCSECPROFILE_ID = "fieldsecurityprofileid";
        public const string SPCTECHREGISTRY_ID = "new_horascdtid";
        public const string SPCTECHREGISTRY_TICKET = "new_boletacdtid";
        public const string SPCTECHREGISTRY_TECH = "new_tecnico";
        public const string SPCTECHREGISTRY_STARTED = "new_entrada";
        public const string SPCTECHREGISTRY_FINISHED = "new_salida";
        public const string SPCTECHREGISTRY_HOURS_NORMAL = "new_chn";
        public const string SPCTECHREGISTRY_HOURS_NORMAL_NIGHT = "new_chnn";
        public const string SPCTECHREGISTRY_DAYTIME_EXTRA = "new_ched";
        public const string SPCTECHREGISTRY_NIGHT_EXTRA = "new_chen";
        public const string SPCTECHREGISTRY_HOLYDAY_DAYTIME = "new_chdf";
        public const string SPCTECHREGISTRY_HOLYDAY_NIGHT = "new_chndf";
        public const string SPCTECHREGISTRY_DAYTIME_OFFDAY = "new_chddl";
        public const string SPCTECHREGISTRY_NIGHT_OFFDAY = "new_chndl";
        public const string SPCTECHREGISTRY_DAYTIME_OFFDAY_EXTRA = "new_cheddl";
        public const string SPCTECHREGISTRY_NIGHT_OFFDAY_EXTRA = "new_chendl";
        public const string SPCTECHREGISTRY_ISDATETIMESET = "new_flag";
        public const string SPCTECHREGISTRY_CDT = "new_cdt";
        #endregion

        #region String messages
        public const string MSG_ERR_TITLE_GENERIC = "Error";
        public const string MSG_ERR_LOAD_TECH1_PROG = "Error al cargar información del primer técnico asignado a la programación de este caso.";
        public const string MSG_ERR_LOAD_TECH2_PROG = "Error al cargar información del segundo técnico asignado a la programación de este caso.";
        public const string MSG_ERR_LOAD_TECH3_PROG = "Error al cargar información del tercer técnico asignado a la programación de este caso.";
        public const string MSG_GENERIC_ERROR_TASKCANCELLED = "Operación cancelada debido a pérdida de conexión con el servidor. Intente de nuevo.";
        public const string MSG_LOAD_SUCCESS = "Carga Finalizada";
        public const string MSG_TITLE_LOAD_INCIDENTS = "Cargando casos";
        public const string MSG_TITLE_CREATE_SERVTICKET = "Creando boleta de servicio";
        public const string MSG_CREATE_SERVTICKET_GETCOORDS = "Obteniendo coordenadas GPS";
        public const string MSG_CREATE_SERVTICKET_CREATE = "Creando boleta de servicio";
        public const string MSG_TITLE_LOAD_INCIDENT = "Cargando caso";
        public const string MSG_TITLE_LOAD_CDT = "Cargando CDT";
        public const string MSG_LOAD_LOCAL = "Cargando información local";
        public const string MSG_LOAD_INCIDENTS = "Obteniendo casos del CRM";
        public const string MSG_LOAD_INCIDENT = "Obteniendo datos sobre el caso";
        public const string MSG_LOAD_CDT = "Obteniendo datos sobre el CDT";
        public const string MSG_LOAD_TECHSERVICE_PRODUCTS = "Obteniendo datos sobre servicio técnico";
        public const string MSG_LOAD_CLIENT = "Obteniendo información sobre el cliente";
        public const string MSG_LOAD_CATEGORY = "Obteniendo información de categoría";
        public const string MSG_LOAD_CURRENCY = "Obteniendo información de divisa";
        public const string MSG_LOAD_SYSTEM = "Obteniendo información de tipo de sistema";
        public const string MSG_LOAD_TECHNICIANS = "Obteniendo información de técnicos";
        public const string MSG_LOAD_TECHNICIAN = "Obteniendo información de técnico";
        public const string MSG_LOAD_SERVTICKETS = "Obteniendo información de boletas de servicio";
        public const string MSG_LOAD_APPROVERS = "Obteniendo información sobre aprobaciones";
        public const string MSG_LOAD_PROJEQUIP = "Obteniendo información de equipos del proyecto";
        public const string MSG_LOAD_PROJMATS = "Obteniendo información de materiales del proyecto";
        public const string MSG_LOAD_PROJEQUIPREQS = "Obteniendo información de solicitudes de equipo";
        public const string MSG_LOAD_PROJMATSREQS = "Obteniendo información de solicitudes de materiales";
        #endregion

        #region Rules
        public const float IVA = 0.13f;
        public const float EARNINGSMARGIN = 1.5f;
        public const float SHIPPINGTAX = 0.05f;
        public const float LOGISTICTAX = 0.08f;
        public const float FINANCINGTAX = 0.04f;
        public const float ADMINISTRATIONTAX = 0.16f;

        public static int CalculateWorkedTime(TimeSpan time, bool lunch) =>
            time.Hours <= 0 ? 1 : time.Hours + (time.Minutes >= 15 ? 1 : 0) + (lunch ? -1 : 0);

        public static string CalculateCode(string ClientCatName, string CurrencyCode, List<Technician> techs, int currentHours)
        {
            string cat = string.Empty;
            int EngineerChiefCount = 0;
            int TechsCount = 0;
            foreach (Technician tech in techs)
                if (tech.Category == Types.SPCTECHNICIAN_CATEGORY.Ingeniero || tech.Category == Types.SPCTECHNICIAN_CATEGORY.JefeTecnico)
                    EngineerChiefCount++;
                else
                    TechsCount++;
            int typePersonal = EngineerChiefCount > 0 ? TechsCount > 0 ? 4 : 3 : TechsCount >= 2 ? 2 : 1;
            switch (ClientCatName)
            {
                case "COMERCIAL":
                case "CORREO":
                    cat = "C";
                    break;
                case "FINANCIERO":
                    cat = "F";
                    break;
                case "INDUSTRIAL":
                    cat = "I";
                    break;
                case "RESIDENCIAL":
                    cat = "R";
                    break;
                case "Negocio Pequeño":
                    cat = "NP";
                    break;
                default:
                    break;
            }
            int hoursnumber = currentHours > 6 ? 6 : currentHours;
            string currency = CurrencyCode.Equals("USD") ? "D" : "L";
            return string.Format("ST{0}{1}-{2}@{3}", cat, typePersonal, hoursnumber, currency);
        }

        public static decimal CalculatePrice(Product product) =>
            product.DoesHaveMOSC ? product.Cost : (product.Cost + (!string.IsNullOrEmpty(product.Bought) && product.Bought.Equals('E') ? (product.DAITax + product.LawTax + product.SelectiveTax + (product.Cost * (decimal)SHIPPINGTAX) + (product.Cost * (decimal)LOGISTICTAX)) : 0) + (product.Cost * (decimal)FINANCINGTAX) + (product.Cost * (decimal)ADMINISTRATIONTAX)) * (decimal)EARNINGSMARGIN;

        public static double[] CalculateHours(DateTime started, DateTime Finished, List<DateTime> holydays)
        {
            //This is not intended for schedules longer than 24 hours,
            // Map of hours

            //Night Shift
            // [2]: Normal Night Hours                [19:00 - 00:00] sii Started=19
            // [2]: Normal Night Hours                [00:00 - 01:00] sii Started=19
            // [3]: Normal Night Extra Hours          [01:00 - 05:00] sii Started=19
            // [1]: Normal Daytime Extra Hours        [05:00 - 08:00] sii Started=19

            // [4]: Holyday Daytime Hours
            // [5]: Holyday Night Hours

            // [6]: Offday Daytime Hours              [08:00 - 17:30] sii Started=08 && Sunday || [      -      ] sii Sunday
            // [7]: Offday Night Hours                [17:30 - 19:00] sii Started=08 && Sunday || [05:00 - 08:00] sii Sunday

            // [8]: Offday/Holyday Daytime Extra Hours[      -      ] sii Sunday               || [19:00 - 01:00] sii Started=19 && Sunday
            // [9]: Offday/Holyday Night Extra Hours  [19:00 - 05:00] sii Started=08 && Sunday || [01:00 - 05:00] sii Started=19 && Sunday

            TimeSpan NightExtraEnd = new TimeSpan(5, 0, 0);
            TimeSpan DaytimeEarlyExtras = new TimeSpan(8, 0, 0);
            TimeSpan DaytimeEnd = new TimeSpan(17, 30, 0);
            TimeSpan DaytimeLateExtras = new TimeSpan(19, 0, 0);
            DateTime Current = started;

            double[] hours = new double[10];
            for (int i = 0; i < hours.Length; i++)
                hours[i] = 0d;

            // check if holyday
            //if (CheckHolydays(started, holydays) || CheckHolydays(Finished, holydays))
            //    ;
            //Day shift
            // [3]: Normal Night Extra Hours          [00:00 - 05:00] sii Started<15 
            // [1]: Normal Daytime Extra Hours        [05:00 - 08:00] sii Started<15 
            // [0]: Normal Daytime Hours              [08:00 - 17:30] sii Started<15 
            // [1]: Normal Daytime Extra Hours        [17:30 - 19:00] sii Started<15 
            // [3]: Normal Night Extra Hours          [19:00 - 00:00] sii Started<15 
            //Holyday Day shift
            // [9]: Offday/Holyday Night Extra Hours  [00:00 - 05:00] sii Started<15 && Holiday=true
            // [8]: Offday/Holyday Daytime Extra Hours[05:00 - 08:00] sii Started<15 && Holiday=true
            // [4]: Holyday Daytime Hours [0]also     [08:00 - 17:30] sii Started<15 && Holiday=true
            // [8]: Offday/Holyday Daytime Extra Hours[17:30 - 19:00] sii Started<15 && Holiday=true
            // [9]: Offday/Holyday Night Extra Hours  [19:00 - 00:00] sii Started<15 && Holiday=true
            //Offday Day Shift
            // [9]: Offday/Holyday Night Extra Hours  [00:00 - 05:00] sii Started<15 && Day=Sunday
            // [8]: Offday/Holyday Daytime Extra Hours[05:00 - 08:00] sii Started<15 && Day=Sunday
            // [6]: Offday Daytime Hours              [08:00 - 17:30] sii Started<15 && Day=Sunday
            // [8]: Offday/Holyday Daytime Extra Hours[17:30 - 19:00] sii Started<15 && Day=Sunday
            // [9]: Offday/Holyday Night Extra Hours  [19:00 - 00:00] sii Started<15 && Day=Sunday
            // Holyday Offday Day Shift
            // [9]: Offday/Holyday Night Extra Hours  [00:00 - 05:00] sii Started<15 && Day=Sunday && Holyday=true
            // [8]: Offday/Holyday Daytime Extra Hours[08:00 - 19:00] sii Started<15 && Day=Sunday && Holyday=true
            // [9]: Offday/Holyday Night Extra Hours  [19:00 - 00:00] sii Started<15 && Day=Sunday && Holyday=true
            double totalhours = (Finished - started).TotalHours;
            if (started.TimeOfDay.Ticks < new TimeSpan(15, 0, 0).Ticks)
                for (float i = 0; i < totalhours; i += .25f)
                    if (Current.DayOfWeek.Equals(DayOfWeek.Sunday))
                    {
                        if (CheckHolydays(Current.AddHours(i), holydays))
                        {
                            if (Current.AddHours(i).TimeOfDay.Ticks < NightExtraEnd.Ticks)
                                hours[9] += 0.25;
                            else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeLateExtras.Ticks)
                                hours[8] += 0.25;
                            else
                                hours[9] += 0.25;
                        }
                        else
                        {
                            if (Current.AddHours(i).TimeOfDay.Ticks < NightExtraEnd.Ticks)
                                hours[9] += 0.25;
                            else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeEarlyExtras.Ticks)
                                hours[8] += 0.25;
                            else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeEnd.Ticks)
                            {
                                hours[6] += 0.25;
                            }
                            else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeLateExtras.Ticks)
                                hours[8] += 0.25;
                            else
                                hours[9] += 0.25;
                        }
                    }
                    else
                    if (Current.AddHours(i).TimeOfDay.Ticks < NightExtraEnd.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 9 : 3] += 0.25;
                    else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeEarlyExtras.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 8 : 1] += 0.25;
                    else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeEnd.Ticks)
                    {
                        hours[0] += 0.25;
                        if (CheckHolydays(Current.AddHours(i), holydays))
                            hours[4] += 0.25;
                    }
                    else if (Current.AddHours(i).TimeOfDay.Ticks < DaytimeLateExtras.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 8 : 1] += 0.25;
                    else
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 5 : 3] += 0.25;
            else
            {
                //Night Shift
                // [2]: Normal Night Hours                [19:00 - 00:00] sii Started>=15
                // [2]: Normal Night Hours                [00:00 - 01:00] sii Started>=15
                // [3]: Normal Night Extra Hours          [01:00 - 05:00] sii Started>=15
                // [1]: Normal Daytime Extra Hours        [05:00 - 08:00] sii Started>=15
                //Holyday Night shift
                // [5]: Holyday Night Hours               [19:00 - 00:00] sii Started>=15 && Holiday=true
                // [5]: Holyday Night Hours               [00:00 - 01:00] sii Started>=15 && Holiday=true
                // [9]: Offday/Holyday Night Extra Hours  [01:00 - 05:00] sii Started>=15 && Holiday=true
                // [8]: Offday/Holyday Daytime Extra Hours[05:00 - 08:00] sii Started>=15 && Holiday=true
                //Offday Night Shift
                // [7]: Offday Night Hours                [19:00 - 00:00] sii Started>=15 && Day=Sunday
                // [7]: Offday Night Hours                [00:00 - 01:00] sii Started>=15 && Day=Sunday
                // [9]: Offday/Holyday Night Extra Hours  [01:00 - 05:00] sii Started>=15 && Day=Sunday
                // [8]: Offday/Holyday Daytime Extra Hours[05:00 - 08:00] sii Started>=15 && Day=Sunday
                // Holyday Offday Night Shift
                // [9]: Offday/Holyday Night Extra Hours  [19:00 - 00:00] sii Started>=15 && Day=Sunday && Holyday=true
                // [9]: Offday/Holyday Night Extra Hours  [00:00 - 05:00] sii Started>=15 && Day=Sunday && Holyday=true
                // [8]: Offday/Holyday Daytime Extra Hours  [05:00 - 08:00] sii Started>=15 && Day=Sunday && Holyday=true
                TimeSpan NightShiftStart = new TimeSpan(19, 0, 0);
                TimeSpan NightShiftExtraStart = started.AddHours(6).TimeOfDay;
                TimeSpan DaytimeExtraHoursStart = new TimeSpan(5, 0, 0);
                TimeSpan DaytimeExtraHoursEnd = new TimeSpan(8, 0, 0);
                TimeSpan EndOfDay = new TimeSpan(23, 59, 59);

                for (float i = 0; i < totalhours; i += .25f)
                    if (Current.DayOfWeek.Equals(DayOfWeek.Sunday))
                    {
                        if ((Current.AddHours(i).TimeOfDay.Ticks < EndOfDay.Ticks && Current.AddHours(i).TimeOfDay.Ticks > NightShiftStart.Ticks) || Current.AddHours(i).TimeOfDay.Ticks < NightShiftExtraStart.Ticks)
                        {         
                            hours[5] += 0.25;
                            if (CheckHolydays(Current.AddHours(i), holydays))
                                hours[7] += 0.25;
                        }
                        else if (Current.AddHours(i).TimeOfDay.Ticks >= NightShiftExtraStart.Ticks && Current.AddHours(i).TimeOfDay.Ticks < DaytimeExtraHoursStart.Ticks)
                            hours[9] += 0.25;
                        else if (Current.AddHours(i).TimeOfDay.Ticks >= DaytimeExtraHoursStart.Ticks && Current.AddHours(i).TimeOfDay.Ticks < DaytimeExtraHoursEnd.Ticks)
                            hours[8] += 0.25;
                    }
                    else
                    if ((Current.AddHours(i).TimeOfDay.Ticks < EndOfDay.Ticks && Current.AddHours(i).TimeOfDay.Ticks > NightShiftStart.Ticks) || Current.AddHours(i).TimeOfDay.Ticks < NightShiftExtraStart.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 5 : 2] += 0.25;
                    else if (Current.AddHours(i).TimeOfDay.Ticks >= NightShiftExtraStart.Ticks && Current.AddHours(i).TimeOfDay.Ticks < DaytimeExtraHoursStart.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 9 : 3] += 0.25;
                    else if (Current.AddHours(i).TimeOfDay.Ticks >= DaytimeExtraHoursStart.Ticks && Current.AddHours(i).TimeOfDay.Ticks < DaytimeExtraHoursEnd.Ticks)
                        hours[CheckHolydays(Current.AddHours(i), holydays) ? 8 : 1] += 0.25;
            }
            return hours;
        }

        private static bool CheckHolydays(DateTime toCheck, List<DateTime> Holydays)
        {
            foreach (DateTime holyday in Holydays)
                if (holyday.DayOfYear == toCheck.DayOfYear)
                    return true;
            return false;
        }
        #endregion
    }
}