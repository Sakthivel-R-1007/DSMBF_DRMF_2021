using DSMBF_DRMF.Domain;
using System;
using System.Collections.Generic;

namespace DSMBF_DRMF.Persistence.Interfaces
{
    public interface IClaimDao
    {
        IList<Category> GetCategoryList();

        IList<Claim> GetAccountCodeList(string AccountCode);

        long SaveClaim(Claim c);

        bool ValidateAccountCode(string AccountCode=null);

        IList<Claim> GetClaimLists(string AccountNo = null, string SortBy = null, string SortDirection = null, int PageIndex = 0, int PageSize = 10, Int64 CategoryId = 0,string InvoiceDate=null,string Company=null, string Invoice = null, Int64 InvoiceTypeId = 0, string Company_SC = null, string Quarter = null);

        Claim GetClaim(string Id);

        int DeleteClaim(Claim C);

        Int64 CheckProgramType(string AccountCode);

        bool ValidateBudgetAmount(string AccountCode = null,Int64 ProgramTypeId=0,Decimal InvoiceAmount=0);

        IList<ProgramType> GetProgramTypeList(string AccountCode);

        int PaidClaim(Claim C);
    }
}
