using Dapper;
using DSMBF_DRMF.Core;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DSMBF_DRMF.Persistence.Implementations
{
    public class ClaimDao : IClaimDao
    {
        #region Private variables and constructors

        protected readonly IDbConnectionFactory factory;

        public ClaimDao(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        #endregion


        public IList<Category> GetCategoryList()
        {
            IList<Category> categories = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetClaimCategories]";
                categories = conn.Query<Category>(SQL, commandType: CommandType.StoredProcedure).ToList<Category>();
            }
            return categories;

        }

        public IList<Claim> GetAccountCodeList(string AccountCode)
        {
            IList<Claim> categories = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountCode", AccountCode, DbType.String);
                string SQL = @"[USP_GetAccountCodeLists]";
                categories = conn.Query<Claim>(SQL, param, commandType: CommandType.StoredProcedure).ToList<Claim>();
            }
            return categories;

        }

        public bool ValidateAccountCode(string AccountCode = null)
        {
            bool result = false;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountCode", AccountCode, DbType.String);
                string SQL = @"[USP_ValidateAccountCode]";
                result = conn.Query<bool>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<bool>();
            }
            return result;

        }
        public long SaveClaim(Claim c)
        {
            long Id = 0;

            DataTable InvoiceDocumentsTbl = new DataTable();
            InvoiceDocumentsTbl.Columns.Add("Id", typeof(long));
            InvoiceDocumentsTbl.Columns.Add("ClaimId", typeof(long));
            InvoiceDocumentsTbl.Columns.Add("Name", typeof(string));
            InvoiceDocumentsTbl.Columns.Add("GUID", typeof(string));
            InvoiceDocumentsTbl.Columns.Add("Extension", typeof(string));
            c.InvoiceDocuments.ForEach(d =>
            {
                if (!String.IsNullOrEmpty(d.Name) && !String.IsNullOrEmpty(d.GUID) && (d.ReAttach))
                { InvoiceDocumentsTbl.Rows.Add(string.IsNullOrEmpty(d.EncryptedId) ? 0 : Security.Decrypt<Int64>(d.EncryptedId), c.Id, d.Name, d.GUID, d.Extension); }
            });
            DynamicParameters param = new DynamicParameters();
            if (c.Id > 0)
            {
                param.Add("@Id", c.Id, DbType.Int64);
            }
            // param.Add("@CategoryId", CId, DbType.Int64);
            param.Add("@AccountCode", c.AccountCode, DbType.String);
            param.Add("@Invoice ", c.Invoice, DbType.String);
            param.Add("@Status ", c.Status, DbType.String);
            param.Add("@CreatedBy", c.CreatedBy, DbType.Int64);
            param.Add("@SystemIp ", c.SystemIp, DbType.String);
            param.Add("@InvoiceAmount ", c.InvoiceAmount, DbType.Decimal);

            param.Add("@TaxPercentage ", c.TaxPercentage, DbType.Decimal);
            param.Add("@TaxAmount ", c.TaxAmount, DbType.Decimal);
            param.Add("@InvoiceAmountwithouttax ", c.InvoiceAmountwithouttax, DbType.Decimal);
            param.Add("@InvoiceTypeId", c.InvoiceTypeId, DbType.Int64);

            param.Add("@InvoiceDate ", c.InvoiceDate, DbType.DateTime);
            param.Add("@ProgramTypeId ", (c.ProgramType != null ? c.ProgramType.Id : 0), DbType.Int64);
            param.Add("@InvoicePeriod ", c.InvoicePeriod, DbType.String);
            param.Add("@Quarter ", c.Quarter, DbType.String);
            param.Add("@PO ", c.PO, DbType.String);
            param.Add("@InvoiceDocumentsTbl", InvoiceDocumentsTbl);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_SaveClaim]";
                Id = conn.Query<long>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<long>();
                conn.Close();
            }
            return Id;

        }
        public IList<Claim> GetClaimLists(string AccountNo = null, string SortBy = null, string SortDirection = null, int PageIndex = 0, int PageSize = 10, Int64 ProgramTypeId = 0, string InvoiceDate = null,string Company = null, string Invoice = null, Int64 InvoiceTypeId = 0, string Company_SC = null,string Quarter=null)
        {
            IList<Claim> d = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@AccountNo", AccountNo, DbType.String);
            param.Add("@PageIndex", PageIndex, DbType.Int32);
            param.Add("@PageSize", PageSize, DbType.Int32);
            param.Add("@SortDirection", SortDirection, DbType.String);
            param.Add("@SortBy", SortBy, DbType.String);
            param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
            param.Add("@InvoiceDate", InvoiceDate, DbType.String);
            param.Add("@Company", Company, DbType.String);
            param.Add("@Invoice", Invoice, DbType.String);
            param.Add("@Company_SC", Company_SC, DbType.String);
            param.Add("@InvoiceTypeId", InvoiceTypeId, DbType.Int64);
            param.Add("@Quarter", Quarter, DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetClaimList]";
                d = conn.Query<Claim, ProgramType, Claim>(sql: SQL,
                    map: (DR, PT) =>
                    {
                        if (DR != null)
                        {
                            DR.ProgramType = PT;
                        }
                        return DR;
                    },
                    param: param,

                         commandType: CommandType.StoredProcedure).ToList<Claim>();
            }
            return d;
        }

        public Claim GetClaim(string Id)
        {
            Claim claim = new Claim();
            DynamicParameters param = new DynamicParameters();
            param.Add("@Id", Id, DbType.Int64);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetClaim]";
                claim = conn.Query<Claim, ProgramType,Distributor,Claim>(sql: SQL,
                    map: (CL, PT,D) =>
                    {
                        if (CL != null)
                        {

                            CL.ProgramType = PT;
                            CL.Distributor = D;
                        }
                        return CL;
                    },
                    param: param,
                commandType: CommandType.StoredProcedure).FirstOrDefault<Claim>();

                var result = conn.QueryMultiple(SQL, param, commandType: CommandType.StoredProcedure);
                if (result != null && claim != null)
                {
                    result.Read<Claim>().FirstOrDefault();
                    claim.InvoiceDocuments = result.Read<InvoiceDocument>().ToList();
                }
            }
            return claim;
        }

        public int DeleteClaim(Claim C)
        {
            int result = 0;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Id", C.Id, dbType: DbType.Int64);
            param.Add("@AdminUserId", C.CreatedBy, dbType: DbType.Int64);
            param.Add("@SystemIp", C.SystemIp, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_DeleteClaim]";
                result = conn.Execute(sql: SQL, param: param, commandType: CommandType.StoredProcedure);
                conn.Close();
            }
            return result;
        }

        public Int64 CheckProgramType(string AccountCode)
        {
            Int64 result = 0;
            DynamicParameters param = new DynamicParameters();
            param.Add("@AccountCode", AccountCode, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_CheckProgranType]";
                result = conn.Query<Int64>(sql: SQL, param: param, commandType: CommandType.StoredProcedure).FirstOrDefault();
                conn.Close();
            }
            return result;
        }

        public bool ValidateBudgetAmount(string AccountCode = null, Int64 ProgramTypeId = 0, Decimal InvoiceAmount = 0)
        {
            bool result = false;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountCode", AccountCode, DbType.String);
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                param.Add("@InvoiceAmount", InvoiceAmount, DbType.Decimal);
                string SQL = @"[USP_ValidateBudgetAmount]";
                result = conn.Query<bool>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<bool>();
            }
            return result;
        }

        public IList<ProgramType> GetProgramTypeList(string AccountCode)
        {
            IList<ProgramType> PT = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@AccountCode", AccountCode, DbType.String);
                string SQL = @"[USP_GetProgramTypeList]";
                PT = conn.Query<ProgramType>(SQL, param, commandType: CommandType.StoredProcedure).ToList<ProgramType>();
            }
            return PT;
        }


        public int PaidClaim(Claim C)
        {
            int result = 0;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Id", C.Id, dbType: DbType.Int64);
            param.Add("@AdminUserId", C.CreatedBy, dbType: DbType.Int64);
            param.Add("@SystemIp", C.SystemIp, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_PaidClaim]";
                result = conn.Execute(sql: SQL, param: param, commandType: CommandType.StoredProcedure);
                conn.Close();
            }
            return result;
        }
    }
}
