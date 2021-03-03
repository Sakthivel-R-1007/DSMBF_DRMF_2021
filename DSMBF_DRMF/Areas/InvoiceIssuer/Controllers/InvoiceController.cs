using DSMBF_DRMF.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.InvoiceIssuer.Controllers
{
    [AuthenticationFilter(Type = "Invoice Issuer")]
    public class InvoiceController : Controller
    {
        // GET: InvoiceIssuer/Invoice
        public ActionResult Index()
        {
            return View();
        }
    }
}