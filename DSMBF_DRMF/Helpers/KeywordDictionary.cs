using System.Collections.Generic;

namespace DSMBF_DRMF.Helpers
{
    public class KeywordDictionary
    {
        public static string FindEnglishText(string TextName)
        {
            IDictionary<string, string> keywordDic = new Dictionary<string, string>();

            #region Add English words to dictionary





            keywordDic.Add("Area", "Area");
            keywordDic.Add("AccountCode", "Account Code");
            keywordDic.Add("CompanyName_EN", "Distributor Company Name (EN)");
            keywordDic.Add("CompanyName_SC", "Distributor Company Name (SC)");

            keywordDic.Add("InvoiceDate", "Invoice Date");
            keywordDic.Add("InvoiceAmount", "Invoice Amount with tax (RMB)");

            keywordDic.Add("TaxPercentage", "Tax Percentage");
            keywordDic.Add("TaxAmount", "Tax Amount (RMB)");
            keywordDic.Add("InvoiceAmountwithouttax", "Invoice Amount without tax (RMB)");
            keywordDic.Add("InvoiceType", "Invoice Type");
            keywordDic.Add("Invoice", "Invoice#");
            keywordDic.Add("Status", "Status (Approved or Declined)");
            keywordDic.Add("DeclinedReason", "Declined Reason");
            keywordDic.Add("CategoryName", "Claim Category");
            keywordDic.Add("ProgramTypeName", "Program Type");
            keywordDic.Add("ProgramType", "Program Type");

            keywordDic.Add("InvoicePeriod", "Invoice Period");
            keywordDic.Add("PO", "PO#");

  
            keywordDic.Add("FullYearTotalBudget", "Full-Year Total Budget (RMB)");

       
            keywordDic.Add("FullYearTotalBudgetUtilized", "Full-Year Budget Utilized (RMB)");


     
            keywordDic.Add("FullYearTotalBudgetRemaining", "Full-Year Budget Remaining (RMB)");


            keywordDic.Add("EMCH_Jan_Mar_LO_TotalBudget", "EMCH Jan~Mar (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCH_Jan_Mar_LO_BudgetUtilized", "EMCH Jan~Mar (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_Jan_Mar_LO_BudgetRemaining", "EMCH Jan~Mar (润滑油) Budget Remaining(RMB)");


            keywordDic.Add("EMCH_Jan_Mar_G_TotalBudget", "EMCH Jan~Mar (润滑脂) Total Budget(RMB)");
            keywordDic.Add("EMCH_Jan_Mar_G_BudgetUtilized", "EMCH Jan~Mar (润滑脂) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_Jan_Mar_G_BudgetRemaining", "EMCH Jan~Mar (润滑脂) Budget Remaining(RMB)");

    

            keywordDic.Add("EMCH_Apr_Jun_LO_TotalBudget", "EMCH Apr~Jun (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCH_Apr_Jun_LO_BudgetUtilized", "EMCH Apr~Jun (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_Apr_Jun_LO_BudgetRemaining", "EMCH Apr~Jun (润滑油) Budget Remaining(RMB)");




            keywordDic.Add("EMCH_Apr_Jun_G_TotalBudget", "EMCH Apr~Jun (润滑脂) Total Budget(RMB)");
            keywordDic.Add("EMCH_Apr_Jun_G_BudgetUtilized", "EMCH Apr~Jun (润滑脂) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_Apr_Jun_G_BudgetRemaining", "EMCH Apr~Jun (润滑脂) Budget Remaining(RMB)");




           



            keywordDic.Add("EMCSS_Jan_Mar_LO_TotalBudget", "EMCSS Jan~Mar (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCSS_Jan_Mar_LO_BudgetUtilized", "EMCSS Jan~Mar (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_Jan_Mar_LO_BudgetRemaining", "EMCSS Jan~Mar (润滑油) Budget Remaining(RMB)");



            keywordDic.Add("EMCSS_Jan_Mar_CR_TotalBudget", "EMCSS Jan~Mar (化学试剂) Total Budget(RMB)");
            keywordDic.Add("EMCSS_Jan_Mar_CR_BudgetUtilized", "EMCSS Jan~Mar (化学试剂) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_Jan_Mar_CR_BudgetRemaining", "EMCSS Jan~Mar (化学试剂) Budget Remaining(RMB)");



            keywordDic.Add("EMCSS_Apr_Jun_LO_TotalBudget", "EMCSS Apr~Jun (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCSS_Apr_Jun_LO_BudgetUtilized", "EMCSS Apr~Jun (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_Apr_Jun_LO_BudgetRemaining", "EMCSS Apr~Jun (润滑油) Budget Remaining(RMB)");


            keywordDic.Add("EMCSS_Apr_Jun_CR_TotalBudget", "EMCSS Apr~Jun (化学试剂) Total Budget(RMB)");
            keywordDic.Add("EMCSS_Apr_Jun_CR_BudgetUtilized", "EMCSS Apr~Jun (化学试剂) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_Apr_Jun_CR_BudgetRemaining", "EMCSS Apr~Jun (化学试剂) Budget Remaining(RMB)");

            keywordDic.Add("EMCH_July_Dec_LO_TotalBudget", "EMCH July~Dec (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCH_July_Dec_LO_BudgetUtilized", "EMCH July~Dec (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_July_Dec_LO_BudgetRemaining", "EMCH July~Dec (润滑油) Budget Remaining(RMB)");

            keywordDic.Add("EMCH_July_Dec_G_TotalBudget", "EMCH July~Dec (润滑脂) Total Budget(RMB)");
            keywordDic.Add("EMCH_July_Dec_G_BudgetUtilized", "EMCH July~Dec (润滑脂) Budget Utilized(RMB)");
            keywordDic.Add("EMCH_July_Dec_G_BudgetRemaining", "EMCH July~Dec (润滑脂) Budget Remaining(RMB)");

            keywordDic.Add("EMCSS_July_Dec_LO_TotalBudget", "EMCSS July~Dec (润滑油) Total Budget(RMB)");
            keywordDic.Add("EMCSS_July_Dec_LO_BudgetUtilized", "EMCSS July~Dec (润滑油) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_July_Dec_LO_BudgetRemaining", "EMCSS July~Dec (润滑油) Budget Remaining(RMB)");


            keywordDic.Add("EMCSS_July_Dec_CR_TotalBudget", "EMCSS July~Dec (化学试剂) Total Budget(RMB)");
            keywordDic.Add("EMCSS_July_Dec_CR_BudgetUtilized", "EMCSS July~Dec (化学试剂) Budget Utilized(RMB)");
            keywordDic.Add("EMCSS_July_Dec_CR_BudgetRemaining", "EMCSS July~Dec (化学试剂) Budget Remaining(RMB)");

            keywordDic.Add("DeliveryAddress", "Delivery Address");
            keywordDic.Add("ContactPerson", "Contact Person");
            keywordDic.Add("ContactNumber", "Contact Number");



            keywordDic.Add("TotalBudget", "Fund Project (RMB)");
            keywordDic.Add("TotalBudgetUtilized", "Budget Climed (RMB)");
            keywordDic.Add("TotalBudgetRemaining", "Budget Remaining (RMB)");



            keywordDic.Add("EMCH_LO_BudgetDetectedVAT", " EMCH 销售返利（润滑油）(RMB)");
            keywordDic.Add("EMCH_G_BudgetDetectedVAT", " EMCH 销售返利（润滑脂）(RMB)");
            keywordDic.Add("EMCSS_G_BudgetDetectedVAT", " EMCSS 销售返利（润滑油）(RMB)");
            keywordDic.Add("EMCSS_O_BudgetDetectedVAT", " EMCSS 销售返利（其他）(RMB)");
            keywordDic.Add("EMCSS_BudgetDetectedTotalVAT", " Total (RMB)");
            keywordDic.Add("EMCH_LO_BudgetDetectedTax", "EMCH 销售返利（润滑油）(RMB)");
            keywordDic.Add("EMCH_G_BudgetDetectedTax", "EMCH 销售返利（润滑脂）(RMB)");
            keywordDic.Add("EMCSS_G_BudgetDetectedTax", "EMCSS 销售返利（润滑油）(RMB)");
            keywordDic.Add("EMCSS_O_BudgetDetectedTax", "EMCSS 销售返利（其他）(RMB)");
            keywordDic.Add("EMCSS_BudgetDetectedTotalTax", "Total (RMB)");

            keywordDic.Add("ProgramType_SC", "基金项目");
            keywordDic.Add("Period", "月份");
            keywordDic.Add("InvoiceAmountWithTax", "积分项目运营费 群英会订货平台使用费 (RMB)");
            keywordDic.Add("InvoiceAmountwithTaxPVDRMF", "B2C积分项目兑换礼品费用 (RMB)");
            keywordDic.Add("InvoiceAmountwithTaxCVDRMF", "B2B积分项目兑换礼品费用 (RMB)");
            keywordDic.Add("PVDRMFTotal", "B2C费用合计 (RMB)");
            keywordDic.Add("CVDRMFTotal", "B2B费用合计 (RMB)");



            #endregion


            #region 2020

            //keywordDic.Add("InvoiceDate", "Jan - Mar 润滑油 (EMCSS) RMB");
            //keywordDic.Add("InvoiceAmount", "Jan - Mar 化学试剂 (EMCSS) RMB");
            //keywordDic.Add("InvoiceAmount", "Jan - Mar 润滑油 (EMCH) RMB");
            //keywordDic.Add("InvoiceAmount", "Jan - Mar 润滑脂 (EMCH) RMB");
            //keywordDic.Add("InvoiceAmount", "Apr - Jun 润滑油 (EMCH) RMB");

            //keywordDic.Add("InvoiceAmount", "Apr - Jun 润滑脂(EMCH) RMB");

            #endregion
            string EnglishText = string.Empty;
            keywordDic.TryGetValue(TextName, out EnglishText);
            return EnglishText;
        }
    }
}