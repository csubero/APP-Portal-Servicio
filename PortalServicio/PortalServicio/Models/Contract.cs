using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class Contract
    {
        #region Properties
        public Guid InternalId { get; set; }
        public Contractor Contractor { get; set; }
        public Currency Currency { get; set; }
        public CDT Cdt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public PaymentCondition Payment { get; set; }
        public List<ContractorWorker> Workers { get; set; }
        public int ContractNumber { get; set; }
        public decimal AmountTotal { get; set; }
        public decimal AmountPaid { get; set; }
        public float Progress { get; set; }
        public bool Signed { get; set; }
        public string AmountTotalFormatted {
            get
            {
                return String.Format("{0}{1:0.00}", Currency.Symbol, AmountTotal);
            }
        }
        public string AmountPaidFormatted
        {
            get
            {
                return String.Format("{0}{1:0.00}", Currency.Symbol, AmountPaid);
            }
        }
        #endregion

        #region Constructors
        public Contract(Guid internalId, Contractor contractor, Currency currency, CDT cdt,PaymentCondition payment,DateTime start, DateTime finish, int contractnumber, decimal amounttotal, decimal amountpaid, float progress, bool signed )
        {
            InternalId = internalId;
            Contractor = contractor;
            Currency = currency;
            Cdt = cdt;
            Payment = payment;
            ContractNumber = contractnumber;
            StartDate = start;
            FinishDate = finish;
            AmountTotal = amounttotal;
            AmountPaid = amountpaid;
            Signed = signed;
            Progress = progress;
            Workers = new List<ContractorWorker>();
        }
        #endregion
    }
}
