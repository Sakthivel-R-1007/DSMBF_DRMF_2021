using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Domain
{
    public class ClaimFigure : Entity<long>
    {
        public string AccountCode { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public decimal InvoiceAmountUtilized { get; set; }
        public decimal InvoiceAmountRemaining { get; set; }
        public Claim Claim { get; set; }
        public ProgramType ProgramType { get; set; }
        public Distributor Distributor { get; set; }
        public string InvoicePeriod { get; set; }
        public string Quarter { get; set; }
    }
}

