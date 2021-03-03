using DSMBF_DRMF.Domain;
using System;
using System.Collections.Generic;

namespace DSMBF_DRMF.Persistence.Interfaces
{
    public interface IDistributorDao
    {
        IList<Distributor> GetDistributorLists(string AccountNo = null, string SortBy = null, string SortDirection = null, int PageIndex = 0, int PageSize = 10, Int64 ProgramTypeId =0);
        Distributor GetDistributor(Int64 Id = 0);
        IList<ProgramType> GetProgramTypeList();
        DistributorsBudget GetDistributorsBudget(Guid GUID,Int64 ProgramTypeId = 0);

    }
}
