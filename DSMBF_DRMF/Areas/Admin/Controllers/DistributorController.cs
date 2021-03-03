using DSMBF_DRMF.Core;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Filters;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.Admin.Controllers
{
    [AuthenticationFilter(Type= "Portal Administrator,Invoice Verification")]
    public class DistributorController : Controller
    {
        // GET: Admin/Distributor
        #region Private variables


        private IDistributorDao _distributorDao = null;
      

        public DistributorController(IDistributorDao distributorDao)
        {

            this._distributorDao = distributorDao;
            


        }

        #endregion
        public ActionResult Index()
        {
            IList<ProgramType> ProgramType = null;
            ProgramType = _distributorDao.GetProgramTypeList();
            ViewBag.ProgramTypeList = (ProgramType == null) ? new List<SelectListItem>() : ProgramType.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();
            string Acode = string.Empty;
            IList<Distributor> distributor = _distributorDao.GetDistributorLists(Acode,null,null,1,10,0);
            return View(distributor);
        }
        [HttpPost]
        public ActionResult Index(string AccountCode,string ProgramType=null)
        {
            IList<ProgramType> ProgramTypes = null;
            Int64 ProgramTypeId = 0;
            if (!string.IsNullOrEmpty(ProgramType))
            {
                ProgramTypeId = Security.Decrypt<Int64>(ProgramType);

            }
            ProgramTypes = _distributorDao.GetProgramTypeList();
            ViewBag.ProgramTypeList = (ProgramTypes == null) ? new List<SelectListItem>() : ProgramTypes.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString()),
                Selected = (ProgramTypeId == SG.Id)
            }).ToList();
            string Acode = string.Empty;
            
            IList<Distributor> distributor = _distributorDao.GetDistributorLists(AccountCode, null, null, 1, 10, ProgramTypeId);
            return View(distributor);
        }
        public PartialViewResult PartialIndex(string AccountNo, int PageIndex, string SortBy = null, string SortDirection = null, int PageSize = 2)
        {
            return PartialView(_distributorDao.GetDistributorLists(AccountNo, SortBy, SortDirection,PageIndex,10));
        }

        public ActionResult View(string EncryptedId)
        {
            if(!string.IsNullOrEmpty(EncryptedId))
            {
                Int64 Id = Security.Decrypt<Int64>(EncryptedId);
              Distributor dt=  _distributorDao.GetDistributor(Id);
                return View(dt);
            }
            return View();
        }

        public ActionResult DistributorsBudget(string EncryptedId)
        {
            if (!string.IsNullOrEmpty(EncryptedId))
            {
                string data = Security.DecodeUrlandDecrypt(EncryptedId);
                string[] verificationParams = data.Split('_');
                Guid GUID = new Guid(verificationParams[0]);               
                Int64 ProgramTypeId = Convert.ToInt64(verificationParams[1]);
                DistributorsBudget dt = _distributorDao.GetDistributorsBudget(GUID, ProgramTypeId);
                return View(dt);
            }
            return View();
        }
    }
}