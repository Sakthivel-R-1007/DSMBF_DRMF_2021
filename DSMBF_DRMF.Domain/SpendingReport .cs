namespace DSMBF_DRMF.Domain
{
    public class SpendingReport
    {
        public string Area { get; set; }
        public string AccountCode { get; set; }
        public string CompanyName_EN { get; set; }
        public string CompanyName_SC { get; set; }
        public string ProgramType_SC { get; set; }
        public string Period { get; set; }
        public decimal InvoiceAmountWithTax { get; set; }
        public decimal InvoiceAmountwithTaxPVDRMF { get; set; }
        public decimal InvoiceAmountwithTaxCVDRMF { get; set; }
        public decimal PVDRMFTotal { get; set; }
        public decimal CVDRMFTotal { get; set; }
    }
}
