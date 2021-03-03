using System.Web;

namespace DSMBF_DRMF.Domain
{
    public class InvoiceDocument:Entity<long>
    {
        public string Name { get; set; }
        public string GUID { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public bool ReAttach { get; set; }
        public HttpPostedFileBase Document { get; set; }
    }
}
