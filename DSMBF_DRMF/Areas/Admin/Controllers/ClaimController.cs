using DSMBF_DRMF.Core;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Filters;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.Admin.Controllers
{
    [AuthenticationFilter(Type = "Portal Administrator,Invoice Verification")]
    public class ClaimController : Controller
    {
        #region Private variables


        private IClaimDao _claimDao = null;
        private IDistributorDao _distributorDao = null;

        public ClaimController(IClaimDao claimDao, IDistributorDao distributorDao)
        {
            _claimDao = claimDao;
            _distributorDao = distributorDao;
        }

        #endregion

        public ActionResult Index()
        {
            IList<ProgramType> ProgramType = null;
            ProgramType = _distributorDao.GetProgramTypeList();
            ViewBag.CategoryList = (ProgramType == null) ? new List<SelectListItem>() : ProgramType.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "服务费",
                Value = Security.Encrypt<string>("1")
            });
            listItems.Add(new SelectListItem
            {
                Text = "礼品费用",
                Value = Security.Encrypt<string>("2")

            });
            ViewBag.InvoiceTypeList = listItems;
            string Acode = string.Empty;
            IList<Claim> claims = _claimDao.GetClaimLists(Acode, null, null, 1, 10, 0,null,null,null,0,null);
            return View(claims);
        }
        [HttpPost]
        public ActionResult Index(string AccountCode, string Category = null, string InvoiceDate = null,string Company=null, string ProgramType = null,string Invoice=null,string InvoiceType=null,string Company_SC=null, string Quarter = null)
        {
            IList<ProgramType> ProgramTypes = null;
            Int64 ProgramTypeId = 0;
            Int64 InvoiceTypeId = 0;
            if (!string.IsNullOrEmpty(Category))
            {
                ProgramTypeId = Security.Decrypt<Int64>(Category);

            }
            if (!string.IsNullOrEmpty(InvoiceType))
            {
                InvoiceTypeId = Security.Decrypt<Int64>(InvoiceType);

            }
            ProgramTypes = _distributorDao.GetProgramTypeList();
            ViewBag.CategoryList = (ProgramTypes == null) ? new List<SelectListItem>() : ProgramTypes.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString()),
                Selected = (ProgramTypeId == SG.Id)
            }).ToList();
            string Acode = string.Empty;
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "服务费",
                Value = Security.Encrypt<string>("1"),
               
            });
            listItems.Add(new SelectListItem
            {
                Text = "礼品费用",
                Value = Security.Encrypt<string>("2")

            });
            ViewBag.InvoiceTypeList = (listItems == null) ? new List<SelectListItem>() : listItems.Select(SG => new SelectListItem
            {
                Text = SG.Text,
                Value =SG.Value.ToString(),
                Selected = (InvoiceType == SG.Value.ToString())
            }).ToList();
            IList<Claim> claims = _claimDao.GetClaimLists(AccountCode, null, null, 1, 10, ProgramTypeId, InvoiceDate,Company,Invoice,InvoiceTypeId,Company_SC,Quarter);
            return View(claims);
        }
        public PartialViewResult PartialIndex(string AccountNo, int PageIndex, string SortBy = null, string SortDirection = null, int PageSize = 2, string Category = null, string InvoiceDate = null, string Company = null, string ProgramType = null, string Invoice = null, string InvoiceType = null, string Company_SC = null,string Quarter=null)
        {
            Int64 CategoryId = 0;
            Int64 InvoiceTypeId = 0;
            if (!string.IsNullOrEmpty(Category))
            {
                CategoryId = Security.Decrypt<Int64>(Category);

            }
            if (!string.IsNullOrEmpty(InvoiceType))
            {
                InvoiceTypeId = Security.Decrypt<Int64>(InvoiceType);

            }
            return PartialView(_claimDao.GetClaimLists(AccountNo, SortBy, SortDirection, PageIndex, 10, CategoryId, InvoiceDate,Company,Invoice,InvoiceTypeId,Company_SC, Quarter));
        }
        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Add()
        {
            // ViewBag.CategoryList = _claimDao.GetCategoryList(); 
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "服务费",
                Value = Security.Encrypt<string>("1")
            });
            listItems.Add(new SelectListItem
            {
                Text = "礼品费用",
                Value = Security.Encrypt<string>("2")
               
            });
            ViewBag.InvoiceTypeList = listItems;
            ViewBag.ProgramTypes = _distributorDao.GetProgramTypeList();
            return View();
        }
        [HttpPost]
        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Add(Claim C,string InvoiceType, string Quarter)
        {
            if (C != null && C.files != null)
            {
                UserAccount u = (UserAccount)Session["UserAccount"];
                bool Amountresult = _claimDao.ValidateBudgetAmount(C != null ? C.AccountCode : "", C.ProgramType != null ? C.ProgramType.Id : 0, C.InvoiceAmount);
                if (Amountresult)
                {
                    bool result = _claimDao.ValidateAccountCode(C != null ? C.AccountCode : "");
                    if (result)
                    {
                        C.InvoiceDocuments = new List<InvoiceDocument>();
                        foreach (HttpPostedFileBase d in C.files)
                        {
                            InvoiceDocument IVD = new InvoiceDocument();
                            if (d != null && d.FileName != null)
                            {
                                IVD.Name = d.FileName;
                                IVD.GUID = Guid.NewGuid().ToString();
                                IVD.Extension = System.IO.Path.GetExtension(IVD.Name);
                                IVD.ReAttach = true;
                                string location = Path.Combine(Server.MapPath("~/Resources/InvoiceDocuments/"), IVD.GUID + IVD.Extension);
                                d.SaveAs(location);
                            }
                            C.InvoiceDocuments.Add(IVD);
                        }

                        C.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                        C.CreatedBy = u.Id;
                        C.Quarter= Quarter;
                        C.InvoiceTypeId = string.IsNullOrEmpty(InvoiceType) ? 0 : Security.Decrypt<Int64>(InvoiceType);
                        long Id = _claimDao.SaveClaim(C);
                        if (Id > 0)
                        {
                            return RedirectToRoute("Claim_Index");
                        }
                        else
                        {
                            ViewBag.Error = "Incorrect Date";
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Please Enter valid Account Code";
                        // ModelState.AddModelError("Id", "Please Enter valid Account Code");
                    }
                }
                else
                {
                    ViewBag.Error = "The Distributor didn't have enough budget";
                    //ModelState.AddModelError("Id", "The Distributor didn't have enough budget");
                }
            }
            else
            {

                ViewBag.Error = "Incorrect Date";
                //ModelState.AddModelError("Id", "Incorrect Date");
            }
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "服务费",
                Value = Security.Encrypt<string>("1")
            });
            listItems.Add(new SelectListItem
            {
                Text = "礼品费用",
                Value = Security.Encrypt<string>("2")

            });
            ViewBag.InvoiceTypeList = listItems;
            ViewBag.ProgramTypes = _distributorDao.GetProgramTypeList();
            return View();
        }
        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Edit(string EncDetail)
        {
            if (!string.IsNullOrEmpty(EncDetail))
            {
                Claim C = _claimDao.GetClaim(Security.DecodeUrlandDecrypt(EncDetail));
                C.EncryptedId = Security.EncryptandEncodeUrl(C.Id.ToString());
                //ViewBag.CategoryList = _claimDao.GetCategoryList();
                ViewBag.ProgramTypes = _distributorDao.GetProgramTypeList();
                List<SelectListItem> listItems = new List<SelectListItem>();
                listItems.Add(new SelectListItem
                {
                    Text = "服务费",
                    Value = Security.Encrypt<string>("1")
                });
                listItems.Add(new SelectListItem
                {
                    Text = "礼品费用",
                    Value = Security.Encrypt<string>("2")

                });
                ViewBag.InvoiceTypeList = (listItems == null) ? new List<SelectListItem>() : listItems.Select(SG => new SelectListItem
                {
                    Text = SG.Text,
                    Value = SG.Value.ToString(),
                    Selected = (Security.Encrypt<string>(Convert.ToString((C.InvoiceTypeId>0?C.InvoiceTypeId:0))) == SG.Value.ToString())
                }).ToList();


                return View(viewName: "Add", model: C);
            }
            return View();
        }
        [HttpPost]
        [AuthenticationFilter(Type = "Portal Administrator")]
        public ActionResult Edit(Claim C,string InvoiceType, string Quarter)
        {
            if (C != null && C.files != null)
            {
                UserAccount u = (UserAccount)Session["UserAccount"];

                bool result = _claimDao.ValidateAccountCode(C != null ? C.AccountCode : "");
                if (result)
                {
                    C.Id = Convert.ToInt64(Security.DecodeUrlandDecrypt(C.EncryptedId));
                    //C.InvoiceDocuments = new List<InvoiceDocument>();
                    foreach (HttpPostedFileBase d in C.files)
                    {
                        InvoiceDocument IVD = new InvoiceDocument();
                        if (d != null && d.FileName != null)
                        {
                            IVD.Name = d.FileName;
                            IVD.GUID = Guid.NewGuid().ToString("N");
                            IVD.Extension = System.IO.Path.GetExtension(IVD.Name);
                            IVD.ReAttach = true;
                            string location = Path.Combine(Server.MapPath("~/Resources/InvoiceDocuments/"), IVD.GUID + IVD.Extension);
                            d.SaveAs(location);
                        }
                        C.InvoiceDocuments.Add(IVD);
                    }
                    C.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                    C.CreatedBy = u.Id;
                    C.Quarter = Quarter;
                    C.InvoiceTypeId = string.IsNullOrEmpty(InvoiceType) ? 0 : Security.Decrypt<Int64>(InvoiceType);
                    long Id = _claimDao.SaveClaim(C);
                    if (Id > 0)
                    {
                        return RedirectToRoute("Claim_Index");
                    }
                    else
                    {
                        ViewBag.Error = "Incorrect Date";
                    }
                }
                else
                {
                    ViewBag.Error = "Please Enter valid Account Code";
                }
            }
            return View();
        }
        [ActionName("View")]
        public ActionResult view(string EncDetail)
        {
            return View(_claimDao.GetClaim(Security.DecodeUrlandDecrypt(EncDetail)));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string EncDetail = null)
        {
            if (!string.IsNullOrEmpty(EncDetail))
            {
                Claim C = new Claim();
                C.Id = Convert.ToInt64(Security.DecodeUrlandDecrypt(EncDetail));
                C.ModifiedBy = ((UserAccount)Session["UserAccount"]).Id;
                C.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                _claimDao.DeleteClaim(C);
            }
            return RedirectToRoute("Claim_View", new { EncDetail = EncDetail });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Paid(string EncDetail = null)
        {
            if (!string.IsNullOrEmpty(EncDetail))
            {
                Claim C = new Claim();
                C.Id = Convert.ToInt64(Security.DecodeUrlandDecrypt(EncDetail));
                C.ModifiedBy = ((UserAccount)Session["UserAccount"]).Id;
                C.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                _claimDao.PaidClaim(C);
            }
            return RedirectToRoute("Claim_View", new { EncDetail = EncDetail });
        }
        [HttpPost]
        public JsonResult AccountCode(string Prefix)
        {
            IList<Claim> AccountCodeList = null;
            AccountCodeList = _claimDao.GetAccountCodeList(Prefix);
            return Json(AccountCodeList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CheckProgramType(string AccountCode, string EncDetail)
        {
            return Json(_claimDao.CheckProgramType(AccountCode), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProgramTypeList(string AccountCode)
        {
            return Json(_claimDao.GetProgramTypeList(AccountCode), JsonRequestBehavior.AllowGet);
        }

    }
}