using Dapper;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;

using System.Data;
using System.Linq;

namespace DSMBF_DRMF.Persistence.Implementations
{
    public class DistributorDao : IDistributorDao
    {
        #region Private variables and constructors

        protected readonly IDbConnectionFactory factory;

        public DistributorDao(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        #endregion

        public IList<Distributor> GetDistributorLists(string AccountNo = null, string SortBy = null, string SortDirection = null, int PageIndex = 0, int PageSize = 10, Int64 ProgramTypeId = 0)
        {
            IList<Distributor> d = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@AccountNo", AccountNo, DbType.String);
            param.Add("@PageIndex", PageIndex, DbType.Int32);
            param.Add("@PageSize", PageSize, DbType.Int32);
            param.Add("@SortDirection", SortDirection, DbType.String);
            param.Add("@SortBy", SortBy, DbType.String);
            param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetDistributorList]";
                d = conn.Query<Distributor>(SQL, param, commandType: CommandType.StoredProcedure).ToList<Distributor>();
                //d = conn.Query<Distributor, ProgramType, Distributor>(sql: SQL,
                //    map: (DR,PT) =>
                //    {
                //        if (DR != null)
                //        {
                //            DR.ProgramType = PT;
                //        }
                //        return DR;
                //    },
                //    param: param,

                //         commandType: CommandType.StoredProcedure).ToList<Distributor>();
            }
            return d;
        }

        public Distributor GetDistributor(Int64 Id = 0)
        {
            Distributor d = null;
            List<ProgramType> PT = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Id", Id, DbType.Int64);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetDistributor]";
                d = conn.Query<Distributor>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<Distributor>();

                using (var multi = conn.QueryMultiple("[USP_GetDistributorProgramType]", new { @DistributorId = Id }, commandType: CommandType.StoredProcedure))
                {

                    PT = multi.Read<ProgramType>().ToList();
                }
                if (d != null)
                {
                    if (PT != null)
                    {
                        d.ProgramTypes = PT;
                    }
                }
            }
            return d;
        }

        public IList<ProgramType> GetProgramTypeList()
        {

            IList<ProgramType> PT = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetProgramTypes]";
                PT = conn.Query<ProgramType>(SQL, commandType: CommandType.StoredProcedure).ToList<ProgramType>();
            }
            return PT;
        }

        public DistributorsBudget GetDistributorsBudget(Guid GUID, Int64 ProgramTypeId = 0)
        {

            DistributorsBudget PT = null;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                DynamicParameters param = new DynamicParameters();
                param.Add("@GUID", GUID, DbType.Guid);
                param.Add("@ProgramTypeId", ProgramTypeId, DbType.Int64);
                string SQL = @"[USP_GetDistributorsBudget]";
                PT = conn.Query<DistributorsBudget>(SQL, param, commandType: CommandType.StoredProcedure).FirstOrDefault<DistributorsBudget>();
            }
            return PT;
        }
    }
}
