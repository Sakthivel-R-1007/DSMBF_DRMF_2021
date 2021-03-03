using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSMBF_DRMF.Domain
{
    public class Entity<TIdentity>
    {
        public TIdentity Id { get; set; }
        public string EncryptedId { get; set; }
        public TIdentity CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public TIdentity ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public TIdentity DeletedBy { get; set; }
        public DateTime DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string SystemIp { get; set; }
    }
}
