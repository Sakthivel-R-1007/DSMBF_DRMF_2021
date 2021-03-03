using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Filters;
using DSMBF_DRMF.Persistence.Interfaces;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.Admin.Controllers
{
    [AuthenticationFilter(Type = "Portal Administrator,Invoice Verification")]
    public class FigureController : Controller
    {
        #region Private variables

        private IClaimFigureDao _claimFigureDao = null;
        private IDistributorDao _distributorDao = null;
        private IClaimDao _claimDao=null;

        public FigureController(IClaimDao claimDao, IClaimFigureDao claimFigureDao, IDistributorDao distributorDao)
        {
            _claimFigureDao = claimFigureDao;
            _distributorDao = distributorDao;
            _claimDao = claimDao;
        }

        #endregion

        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Add()
        {
            ViewBag.ProgramTypes = _distributorDao.GetProgramTypeList();
            return View();
        }
        [HttpPost]
        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Add(ClaimFigure CF, string InvoiceType)
        {
            if (CF != null)
            {
                UserAccount u = (UserAccount)Session["UserAccount"];

                if (_claimDao.ValidateBudgetAmount(CF.AccountCode, CF.ProgramType != null ? CF.ProgramType.Id : 0, CF.InvoiceAmount))
                {
                    if (_claimDao.ValidateAccountCode(CF.AccountCode))
                    {
                        CF.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                        CF.CreatedBy = u.Id;
                        if (_claimFigureDao.SaveClaimFigure(CF) > 0)
                        {
                            return RedirectToRoute("Claim_Index");
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Please Enter valid Account Code";
                    }
                }
                else
                {
                    ViewBag.Error = "The Distributor didn't have enough budget";
                }
            }
            else
            {
                ViewBag.Error = "Error occured.";
            }

            ViewBag.ProgramTypes = _distributorDao.GetProgramTypeList();

            return View(CF);
        }
    }

}