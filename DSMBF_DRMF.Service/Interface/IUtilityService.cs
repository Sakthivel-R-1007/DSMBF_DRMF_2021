using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSMBF_DRMF.Service.Interface
{
    public interface IUtilityService
    {
        string SendEmail(string Subject, string Content, string To, bool ISBCC, string FromEmail, string CCEmail = "");
    }
}
