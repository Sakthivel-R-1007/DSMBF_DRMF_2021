namespace DSMBF_DRMF.Domain
{
    public class PaymentReportDetails
    {
        //public string Area { get; set; }
        //public string AccountCode { get; set; }
        //public string CompanyName_EN { get; set; }
        //public string CompanyName_SC { get; set; }
        //public decimal TaxPercentage { get; set; }
        //public decimal EMCH_LO_BudgetDetectedVAT { get; set; }
        //public decimal EMCH_G_BudgetDetectedVAT { get; set; }
        //public decimal EMCSS_G_BudgetDetectedVAT { get; set; }
        //public decimal EMCSS_O_BudgetDetectedVAT { get; set; }
        //public decimal EMCSS_BudgetDetectedTotalVAT { get; set; }
        //public decimal EMCH_LO_BudgetDetectedTax { get; set; }
        //public decimal EMCH_G_BudgetDetectedTax { get; set; }
        //public decimal EMCSS_G_BudgetDetectedTax { get; set; }
        //public decimal EMCSS_O_BudgetDetectedTax { get; set; }
        //public decimal EMCSS_BudgetDetectedTotalTax { get; set; }


        //public string Area { get; set; }
        public string AccountCode { get; set; }
        public string CompanyName_EN { get; set; }
        public string CompanyName_SC { get; set; }
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }        
        public decimal EMCH_Jan_Mar_LO_BudgetDetected { get; set; }
        public decimal EMCH_Jan_Mar_LO_BudgetDetectedTax { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetDetected { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetDetectedTax { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetDetected { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetDetectedTax { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetDetected { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetDetectedTax { get; set; }

        public decimal EMCH_July_Dec_LO_BudgetDetected { get; set; }
        public decimal EMCH_July_Dec_LO_BudgetDetectedTax { get; set; }
        public decimal EMCH_July_Dec_G_BudgetDetected { get; set; }
        public decimal EMCH_July_Dec_G_BudgetDetectedTax { get; set; }

        public decimal EMCSS_Jan_Mar_LO_BudgetDetected { get; set; }
        public decimal EMCSS_Jan_Mar_LO_BudgetDetectedTax { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetDetected { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetDetectedTax { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetDetected { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetDetectedTax { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetDetected { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetDetectedTax { get; set; }

        public decimal EMCSS_July_Dec_LO_BudgetDetected { get; set; }
        public decimal EMCSS_July_Dec_LO_BudgetDetectedTax { get; set; }
        public decimal EMCSS_July_Dec_CR_BudgetDetected { get; set; }
        public decimal EMCSS_July_Dec_CR_BudgetDetectedTax { get; set; }

    }
}
