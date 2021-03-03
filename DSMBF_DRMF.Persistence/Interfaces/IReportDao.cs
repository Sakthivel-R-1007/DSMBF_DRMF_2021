using DSMBF_DRMF.Domain;
using System;
using System.Collections.Generic;
using System.Data;

namespace DSMBF_DRMF.Persistence.Interfaces
{
    public interface IReportDao
    {
        IList<ProgramType> GetProgramTypeList();
        IList<Report> GetReportList();
        IList<Claim> GetInvoicePeriodList(bool IsFigureOnly);
        List<ClaimReport> GetClaimReports(string Quarter,Int64 ProgramTypeId =0);
        List<DistributorBudgetReport> GetDistributorBudgetReports(Int64 ProgramTypeId = 0);
        List<DistributorReport> GetDistributortReports(Int64 ProgramTypeId = 0);
        List<SpendingReport> GetSpendingReports(Int64 ProgramTypeId = 0);
        List<PaymentReportDetails> GetPaymentReports(string Quarter,string InvoicePeriod, Int64 ProgramTypeId = 0);
        List<MonthlyReportDetails> GetMonthlyPaymentReports(string InvoicePeriod, Int64 ProgramTypeId = 0);
        List<MonthlyClaimReport> GetMonthlyClaimReport(string InvoicePeriod, Int64 ProgramTypeId = 0);
    }
}
