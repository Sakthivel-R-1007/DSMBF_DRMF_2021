using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Domain
{
    public class MonthlyClaimReport
    {
        public string Portal { get; set; }
        public string AccountCode { get; set; }
        public string InvoicePeriod { get; set; }
        public decimal InvoiceAmountWithTax { get; set; }
        public decimal TotalClaimAmountWithTax { get; set; }
        public decimal Balance { get; set; }
    }
}
