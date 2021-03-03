using DSMBF_DRMF.Filters;
using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.Admin.Controllers
{
    [AuthenticationFilter(Type = "Portal Administrator,Invoice Verification")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}