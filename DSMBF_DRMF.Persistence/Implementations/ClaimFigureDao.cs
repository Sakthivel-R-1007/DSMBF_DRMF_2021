using Dapper;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Persistence.Implementations
{
    public class ClaimFigureDao:IClaimFigureDao
    {
        #region Private variables and constructors

        protected readonly IDbConnectionFactory factory;

        public ClaimFigureDao(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        #endregion

        public long SaveClaimFigure(ClaimFigure cf)
        {
            long result=0;
            DynamicParameters param = new DynamicParameters();
            param.Add("@AccountCode", cf.AccountCode, DbType.String);
            param.Add("@Quarter", cf.Quarter, DbType.String);
            param.Add("@CreatedBy", cf.CreatedBy, DbType.Int64);
            param.Add("@SystemIp ", cf.SystemIp, DbType.String);
            param.Add("@InvoiceAmount ", cf.InvoiceAmount, DbType.Decimal);
            param.Add("@ProgramTypeId ", (cf.ProgramType != null ? cf.ProgramType.Id : 0), DbType.Int64);
            param.Add("@InvoicePeriod ", cf.InvoicePeriod, DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_SaveClaimFigure]";
                result = conn.Query<long>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<long>();
                conn.Close();
            }
            return result;
        }
    }
}
