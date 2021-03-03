using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSMBF_DRMF
{
    public class GetRemoteIp
    {
        public static String GetIPAddress(HttpContextBase HttpContext)
        {
            String ip = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Request.ServerVariables["REMOTE_ADDR"];
            else
                ip = ip.Split(',')[0];

            return ip;
        }
    }
}