using System;
using System.Collections.Generic;
using System.Web;

namespace DSMBF_DRMF.Domain
{
    public class Claim : Entity<Int64>
    {
        public string AccountCode { get; set; }
        public Category Category { get; set; }
        public string Invoice { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ClaimDate { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public Decimal TaxPercentage { get; set; }
        public Decimal TaxAmount { get; set; }
        public Decimal InvoiceAmountwithouttax { get; set; }
        public string Status { get; set; }
        public string DeclinedReason { get; set; }
        public string CompanyName { get; set; }
        public List<InvoiceDocument> InvoiceDocuments { get; set; }
        public HttpPostedFileBase[] files { get; set; }
        //public File files { get; set; }
        public Int64 TotalCount { get; set; }

        public ProgramType ProgramType { get; set; }
        public Distributor Distributor { get; set; }
        public string InvoicePeriod { get; set; }

        public string PO { get; set; }

        public string Quarter { get; set; }

        public bool IsPaid { get; set; }
        public DateTime IsPaidOn { get; set; }
        public Int64 InvoiceTypeId { get; set; }
        public ClaimFigure ClaimFigure { get; set; }

    }
}
