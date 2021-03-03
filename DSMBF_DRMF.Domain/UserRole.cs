using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSMBF_DRMF.Domain
{
    public class UserRole:Entity<Int64>
    {
      
        public string Name { get; set; }
    }
}
