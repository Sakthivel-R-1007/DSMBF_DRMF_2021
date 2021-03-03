using System.Web;

namespace DSMBF_DRMF.Domain
{
    public class File:Entity<long>
    {
        public HttpPostedFileBase[] files { get; set; }
    }
}
