using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class CDT
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(25)]
        public string Number { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Client Client { get; set; }
        [ForeignKey(typeof(Client))]
        public int ClientId { get; set; }
        public bool IsFinalClient { get; set; }
        [MaxLength(25)]
        public string MonitoringAccountNumber { get; set; }
        [MaxLength(50)]
        public string MonitoringAccountName { get; set; }
        [MaxLength(50)]
        public string MainContact { get; set; }
        [MaxLength(50)]
        public string MainContactEmail { get; set; }
        [MaxLength(25)]
        public string MainContactPhone { get; set; }
        [MaxLength(50)]
        public string SecondaryContact { get; set; }
        [MaxLength(50)]
        public string SecondaryContactEmail { get; set; }
        [MaxLength(25)]
        public string SecondaryContactPhone { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Subtype System { get; set; }
        [ForeignKey(typeof(Subtype))]
        public int SystemId { get; set; }
        public Types.SPCCDT_PROJECTSTATE ProjectState { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectClientDeadline { get; set; }
        public bool IsApprovedAdministration { get; set; }
        public bool IsApprovedComercial { get; set; }
        public bool IsApprovedFinancial { get; set; }
        public bool IsApprovedInstallation { get; set; }
        public bool IsApprovedOperations { get; set; }
        public bool IsApprovedPlanning { get; set; }
        public bool IsApprovedCustomerService { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverAdministration { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedAdministration")]
        public int ApproverAdministrationId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverComercial { get; set; }
        [ForeignKey(typeof(SystemUser),Name = "CDTsApprovedComercial")]
        public int ApproverComercialId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverFinancial { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedFinancial")]
        public int ApproverFinancialId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverInstallation { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedInstallation")]
        public int ApproverInstallationId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverOperations { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedOperations")]
        public int ApproverOperationsId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverPlanning { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedPlanning")]
        public int ApproverPlanningId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public SystemUser ApproverCustomerService { get; set; }
        [ForeignKey(typeof(SystemUser), Name = "CDTsApprovedCustomerService")]
        public int ApproverCustomerServiceId { get; set; }
        public bool IsApproved { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<ProjectEquipment> ProjectEquipment { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<ProjectMaterial> ProjectMaterials { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<EquipmentRequestOrder> EquipmentRequestedOrders { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<MaterialRequestOrder> MaterialRequestedOrders { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<CDTTicket> CDTTickets { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<ExtraEquipmentRequest> ExtraEquipment { get; set; }
        #endregion
    }
}
