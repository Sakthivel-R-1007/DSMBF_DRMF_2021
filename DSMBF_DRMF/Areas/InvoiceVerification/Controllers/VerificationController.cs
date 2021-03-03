using DSMBF_DRMF.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.InvoiceVerification.Controllers
{
    [AuthenticationFilter(Type = "Invoice Verification")]
    public class VerificationController : Controller
    {
        // GET: InvoiceVerification/Verification
        public ActionResult Index()
        {
            return View();
        }
    }
}