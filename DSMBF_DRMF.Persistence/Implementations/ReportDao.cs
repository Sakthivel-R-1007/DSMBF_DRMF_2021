using Dapper;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DSMBF_DRMF.Persistence.Implementations
{
    public class ReportDao : IReportDao
    {
        #region Private variables and constructors

        protected readonly IDbConnectionFactory factory;

        public ReportDao(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        #endregion


        public IList<ProgramType> GetProgramTypeList()
        {
            IList<ProgramType> ProgramTypes = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetProgramTypes]";
                ProgramTypes = conn.Query<ProgramType>(SQL, commandType: CommandType.StoredProcedure).ToList<ProgramType>();
            }
            return ProgramTypes;

        }

        public IList<Claim> GetInvoicePeriodList(bool IsFigureOnly)
        {
            IList<Claim> InvoicePeriods = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@IsFigureOnly", IsFigureOnly, DbType.Boolean);
                string SQL = @"[USP_GetInvoicePeriodList]";
                InvoicePeriods = conn.Query<Claim>(SQL, param, commandType: CommandType.StoredProcedure).ToList<Claim>();
            }
            return InvoicePeriods;
        }

        public IList<Report> GetReportList()
        {
            IList<Report> Reports = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetReportLists]";
                Reports = conn.Query<Report>(SQL, commandType: CommandType.StoredProcedure).ToList<Report>();
            }
            return Reports;

        }

        public List<ClaimReport> GetClaimReports(string Quarter, Int64 ProgramTypeId = 0)
        {
            List<ClaimReport> CR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                param.Add("@Quarter", Quarter, DbType.String);
                string SQL = @"[USP_ClaimReport]";
                CR = conn.Query<ClaimReport>(SQL, param, commandType: CommandType.StoredProcedure).ToList<ClaimReport>();
            }
            return CR;
        }

        public List<DistributorBudgetReport> GetDistributorBudgetReports(Int64 ProgramTypeId = 0)
        {
            List<DistributorBudgetReport> CR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                string SQL = @"[USP_GetDistributorBudgetReport]";
                CR = conn.Query<DistributorBudgetReport>(SQL, param, commandType: CommandType.StoredProcedure).ToList<DistributorBudgetReport>();
            }
            return CR;
        }

        public List<DistributorReport> GetDistributortReports(Int64 ProgramTypeId = 0)
        {
            List<DistributorReport> DR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                string SQL = @"[USP_GetDistributorReport]";
                DR = conn.Query<DistributorReport>(SQL, param, commandType: CommandType.StoredProcedure).ToList<DistributorReport>();
            }
            return DR;
        }

        public List<SpendingReport> GetSpendingReports(Int64 ProgramTypeId = 0)
        {
            List<SpendingReport> SR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                string SQL = @"[USP_GetSpendingReport]";
                SR = conn.Query<SpendingReport>(SQL, param, commandType: CommandType.StoredProcedure).ToList<SpendingReport>();
            }
            return SR;
        }

        public List<PaymentReportDetails> GetPaymentReports(string Quarter, string InvoicePeriod, Int64 ProgramTypeId = 0)
        {
            List<PaymentReportDetails> PR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);

                if (!string.IsNullOrEmpty(Quarter))
                    param.Add("@Quarter", Quarter, DbType.String);

                if (!string.IsNullOrEmpty(InvoicePeriod))
                    param.Add("@InvoicePeriod", InvoicePeriod, DbType.String);

                string SQL = @"[USP_GetPaymentReport]";
                PR = conn.Query<PaymentReportDetails>(SQL, param, commandType: CommandType.StoredProcedure).ToList<PaymentReportDetails>();
            }
            return PR;
        }

        public List<MonthlyReportDetails> GetMonthlyPaymentReports(string InvoicePeriod, Int64 ProgramTypeId = 0)
        {
            List<MonthlyReportDetails> MR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                param.Add("@InvoicePeriod", InvoicePeriod, DbType.String);
                string SQL = @"[USP_GetMonthlyPaymentReport]";
                MR = conn.Query<MonthlyReportDetails>(SQL, param, commandType: CommandType.StoredProcedure).ToList<MonthlyReportDetails>();
            }
            return MR;
        }

        public List<MonthlyClaimReport> GetMonthlyClaimReport(string InvoicePeriod, Int64 ProgramTypeId = 0)
        {
            List<MonthlyClaimReport> MCR = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                param.Add("@InvoicePeriod", InvoicePeriod, DbType.String);
                string SQL = @"[USP_GetMonthlyClaimReport]";
                MCR = conn.Query<MonthlyClaimReport>(SQL, param, commandType: CommandType.StoredProcedure).ToList();
            }
            return MCR;
        }
    }
}
