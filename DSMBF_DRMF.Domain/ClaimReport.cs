using System;

namespace DSMBF_DRMF.Domain
{
    public class ClaimReport
    {

        public string Area { get; set; }
        public string AccountCode { get; set; }
        public string CompanyName_EN { get; set; }
        public string CompanyName_SC { get; set; }
        public string PO { get; set; }
        public string InvoicePeriod { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public string TaxPercentage { get; set; }
        public Decimal TaxAmount { get; set; }
        public Decimal InvoiceAmountwithouttax { get; set; }
        public string InvoiceType { get; set; }
        public string Invoice { get; set; }
        public string Status { get; set; }
        public string DeclinedReason { get; set; }
        //public string CategoryName { get; set; }
        public string ProgramTypeName { get; set; }

    }
}
