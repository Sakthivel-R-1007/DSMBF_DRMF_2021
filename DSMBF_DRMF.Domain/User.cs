using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Domain
{
    public class User:Entity<Int32>
    {
        public string AccountNo { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
    }
}
