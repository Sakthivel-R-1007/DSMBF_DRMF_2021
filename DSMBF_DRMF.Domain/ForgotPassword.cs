using System;

namespace DSMBF_DRMF.Domain
{
    public class ForgotPassword:Entity<Int64>
    {
        public Guid UniqueId { get; set; }
        public UserAccount UserAccount { get; set; }
        public bool IsChanged { get; set; }
    }
}
