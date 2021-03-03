using System;

namespace DSMBF_DRMF.Domain
{
    public class DistributorsBudget : Entity<Int64>
    {

        public Int64 DistributorId { get; set; }
        public decimal FullYearTotalBudgetUSD { get; set; }
        public decimal FullYearTotalBudget { get; set; }
        public decimal EMCH_Jan_Mar_LO_TotalBudgetUSD { get; set; }
        public decimal EMCH_Jan_Mar_LO_BudgetUtilizedUSD { get; set; }
        public decimal EMCH_Jan_Mar_LO_BudgetRemainingUSD { get; set; }
        public decimal EMCH_Jan_Mar_LO_TotalBudget { get; set; }
        public decimal EMCH_Jan_Mar_LO_BudgetUtilized { get; set; }
        public decimal EMCH_Jan_Mar_LO_BudgetRemaining { get; set; }
        public decimal EMCH_Jan_Mar_G_TotalBudgetUSD { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetUtilizedUSD { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetRemainingUSD { get; set; }
        public decimal EMCH_Jan_Mar_G_TotalBudget { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetUtilized { get; set; }
        public decimal EMCH_Jan_Mar_G_BudgetRemaining { get; set; }
        public decimal EMCH_Apr_Jun_LO_TotalBudgetUSD { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetUtilizedUSD { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetRemainingUSD { get; set; }
        public decimal EMCH_Apr_Jun_LO_TotalBudget { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetUtilized { get; set; }
        public decimal EMCH_Apr_Jun_LO_BudgetRemaining { get; set; }
        public decimal EMCH_Apr_Jun_G_TotalBudgetUSD { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetUtilizedUSD { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetRemainingUSD { get; set; }
        public decimal EMCH_Apr_Jun_G_TotalBudget { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetUtilized { get; set; }
        public decimal EMCH_Apr_Jun_G_BudgetRemaining { get; set; }
        public decimal EMCSS_Jan_Mar_LO_TotalBudgetUSD { get; set; }
        public decimal EMCSS_Jan_Mar_LO_BudgetUtilizedUSD { get; set; }
        public decimal EMCSS_Jan_Mar_LO_BudgetRemainingUSD { get; set; }
        public decimal EMCSS_Jan_Mar_LO_TotalBudget { get; set; }
        public decimal EMCSS_Jan_Mar_LO_BudgetUtilized { get; set; }
        public decimal EMCSS_Jan_Mar_LO_BudgetRemaining { get; set; }
        public decimal EMCSS_Jan_Mar_CR_TotalBudgetUSD { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetUtilizedUSD { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetRemainingUSD { get; set; }
        public decimal EMCSS_Jan_Mar_CR_TotalBudget { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetUtilized { get; set; }
        public decimal EMCSS_Jan_Mar_CR_BudgetRemaining { get; set; }
        public decimal EMCSS_Apr_Jun_LO_TotalBudgetUSD { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetUtilizedUSD { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetRemainingUSD { get; set; }
        public decimal EMCSS_Apr_Jun_LO_TotalBudget { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetUtilized { get; set; }
        public decimal EMCSS_Apr_Jun_LO_BudgetRemaining { get; set; }
        public decimal EMCSS_Apr_Jun_CR_TotalBudgetUSD { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetUtilizedUSD { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetRemainingUSD { get; set; }
        public decimal EMCSS_Apr_Jun_CR_TotalBudget { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetUtilized { get; set; }
        public decimal EMCSS_Apr_Jun_CR_BudgetRemaining { get; set; }
        public decimal FullYearTotalBudgetUtilizedUSD { get; set; }
        public decimal FullYearTotalBudgetUtilized { get; set; }
        public decimal FullYearTotalBudgetRemainingUSD { get; set; }
        public decimal FullYearTotalBudgetRemaining { get; set; }
        public Int64 ProgramTypeId { get; set; }
    }
}
