using System;

namespace DSMBF_DRMF.Domain
{
    public class DistributorBudgetReport
    {

        public string Area { get; set; }
        public string AccountCode { get; set; }
        public string CompanyName_EN { get; set; }
        public string CompanyName_SC { get; set; }
        public string ProgramType { get; set; }
        public Decimal FullYearTotalBudget { get; set; }
        public Decimal EMCH_Jan_Mar_LO_TotalBudget { get; set; }
        public Decimal EMCH_Jan_Mar_LO_BudgetUtilized { get; set; }
        public Decimal EMCH_Jan_Mar_LO_BudgetRemaining { get; set; }
        public Decimal EMCH_Jan_Mar_G_TotalBudget { get; set; }
        public Decimal EMCH_Jan_Mar_G_BudgetUtilized { get; set; }
        public Decimal EMCH_Jan_Mar_G_BudgetRemaining { get; set; }
        public Decimal EMCH_Apr_Jun_LO_TotalBudget { get; set; }
        public Decimal EMCH_Apr_Jun_LO_BudgetUtilized { get; set; }
        public Decimal EMCH_Apr_Jun_LO_BudgetRemaining { get; set; }
        public Decimal EMCH_Apr_Jun_G_TotalBudget { get; set; }
        public Decimal EMCH_Apr_Jun_G_BudgetUtilized { get; set; }
        public Decimal EMCH_Apr_Jun_G_BudgetRemaining { get; set; }

        public Decimal EMCH_July_Dec_LO_TotalBudget { get; set; }
        public Decimal EMCH_July_Dec_LO_BudgetUtilized { get; set; }
        public Decimal EMCH_July_Dec_LO_BudgetRemaining { get; set; }
        public Decimal EMCH_July_Dec_G_TotalBudget { get; set; }
        public Decimal EMCH_July_Dec_G_BudgetUtilized { get; set; }
        public Decimal EMCH_July_Dec_G_BudgetRemaining { get; set; }

        public Decimal EMCSS_Jan_Mar_LO_TotalBudget { get; set; }
        public Decimal EMCSS_Jan_Mar_LO_BudgetUtilized { get; set; }
        public Decimal EMCSS_Jan_Mar_LO_BudgetRemaining { get; set; }
        public Decimal EMCSS_Jan_Mar_CR_TotalBudget { get; set; }
        public Decimal EMCSS_Jan_Mar_CR_BudgetUtilized { get; set; }
        public Decimal EMCSS_Jan_Mar_CR_BudgetRemaining { get; set; }
        public Decimal EMCSS_Apr_Jun_LO_TotalBudget { get; set; }
        public Decimal EMCSS_Apr_Jun_LO_BudgetUtilized { get; set; }
        public Decimal EMCSS_Apr_Jun_LO_BudgetRemaining { get; set; }
        public Decimal EMCSS_Apr_Jun_CR_TotalBudget { get; set; }
        public Decimal EMCSS_Apr_Jun_CR_BudgetUtilized { get; set; }
        public Decimal EMCSS_Apr_Jun_CR_BudgetRemaining { get; set; }

        public Decimal EMCSS_July_Dec_LO_TotalBudget { get; set; }
        public Decimal EMCSS_July_Dec_LO_BudgetUtilized { get; set; }
        public Decimal EMCSS_July_Dec_LO_BudgetRemaining { get; set; }
        public Decimal EMCSS_July_Dec_CR_TotalBudget { get; set; }
        public Decimal EMCSS_July_Dec_CR_BudgetUtilized { get; set; }
        public Decimal EMCSS_July_Dec_CR_BudgetRemaining { get; set; }

        public Decimal FullYearTotalBudgetUtilized { get; set; }
        public Decimal FullYearTotalBudgetRemaining { get; set; }

    }
}
