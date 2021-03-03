using System;
using System.Collections.Generic;

namespace DSMBF_DRMF.Domain
{
    public class UserAccount : Entity<Int64>
    {
       
        public Guid GUID { get; set; }
        public string AccountNo { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public string Password { get; set; }
        public UserRole UserRole { get; set; }
        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public bool IsPasswordExpired { get; set; }
        public bool IsPasswordChanged { get; set; }
        public DateTime PasswordChangedDate { get; set;}


    }
}
