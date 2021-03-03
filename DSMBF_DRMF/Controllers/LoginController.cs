using DSMBF_DRMF.Core;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Helpers;
using DSMBF_DRMF.Persistence.Interfaces;
using DSMBF_DRMF.Service.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace DSMBF_DRMF.Controllers
{
    //[AuthenticationFilter]
    [HandleError]
    public class LoginController : Controller
    {
        // GET: Login
        #region Private variables


        private IUserDao _userDao = null;
        private IUtilityService _utilityService = null;

        public LoginController(IUserDao userDao, IUtilityService utilityService)
        {

            this._userDao = userDao;
            this._utilityService = utilityService;


        }

        #endregion

        public ActionResult Index()
        {
            string ErrorMessage = string.Empty;
            // ErrorMessage = Security.Decrypt<string>("vqQ6OqKoIlu8hrOB4uuu9A_3d_3d");
            //ErrorMessage = Security.Encrypt<string>("DRMF@2020");
            ViewBag.Error = ErrorMessage;
            Session.Abandon();
            return View();
        }

        [HttpPost]
        public ActionResult Index(UserAccount UA)
        {
            string ErrorMessage = string.Empty;
            if (!string.IsNullOrEmpty(UA.Password) && !string.IsNullOrEmpty(UA.AccountNo))
            {
                UA.Password = Security.Encrypt<string>(UA.Password);
                UA.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                UserAccount U = _userDao.AuthenticateUser(UA);
                if (U != null && U.UserRole != null)
                {
                    if (!U.IsLocked && U.Status == null)
                    {
                        Session["UserAccount"] = U;
                        string SystemIP = GetRemoteIp.GetIPAddress(HttpContext);
                        _userDao.SaveUserAccountLoginLog(U.GUID.ToString(), SystemIP);
                        if (U.IsPasswordChanged)
                        {
                            if (U.UserRole.Id == 1)
                            {
                                return RedirectToRoute("Admin_Index");
                            }
                            else if (U.UserRole.Id == 2)
                            {
                                return RedirectToRoute("Admin_Index");
                            }
                            else if (U.UserRole.Id == 3)
                            {
                                return RedirectToRoute("Admin_Index");
                            }
                            else
                            {

                                return RedirectToAction("Logout", "Login");
                            }
                        }
                        else
                        {
                            return RedirectToAction("ResetFirstTimePassword");
                        }

                    }
                    else if (U.IsLocked && U.Status == "Locked")
                    {
                        ErrorMessage = "You have made 5 unsuccessful attempts. Your account has been locked.";
                    }
                    else if (U.Status == "Invalid Password")
                    {
                        ErrorMessage = "Invalid password.";
                    }

                }
                else
                {
                    ErrorMessage = "Invalid account no or password.";
                }
            }
            ViewBag.Error = ErrorMessage;
            return View();
        }

        public ActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Forgotpassword(string Email, string AccountNo)
        {
            ForgotPassword _FP = new ForgotPassword();
            string status = string.Empty;
            if (!string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(AccountNo))
            {
                string SystemIp = GetRemoteIp.GetIPAddress(HttpContext);
                _FP = _userDao.SaveForgotPassword(Email, AccountNo, SystemIp);

                if (_FP != null)
                {

                    status= SendPasswordRecoveryEmail(_FP);
                    if (status == "Success")
                    {
                        ViewBag.Success = "Password reset link sent to your email";
                    }
                    else
                    {
                        ViewBag.Success = status;
                    }

                }
                else
                {
                    ViewBag.Error = (!string.IsNullOrEmpty(Email) ? "Email " : "Account Number ") + "does not exist in the system";

                }
            }
            else
            {
                ViewBag.Error = "Please Enter vaild " +(!string.IsNullOrEmpty(Email) ? "Email " : "Account Number ");

            }
            return View();
        }

        public ActionResult Logout()
        {

            if (Session["UserAccount"] != null)
            {
                UserAccount u = (UserAccount)Session["UserAccount"];
                _userDao.UpdateUserAccountLogoutLog(u.GUID.ToString());
            }
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public ActionResult ResetPassword(string EncDetail)
        {

            if (!string.IsNullOrEmpty(EncDetail))
            {
                string data = Security.DecodeUrlandDecrypt(EncDetail);
                string[] verificationParams = data.Split('_');
                ForgotPassword FP = _userDao.VerifyForgotPasswordUniqueId(new ForgotPassword
                {
                    UniqueId = new Guid(verificationParams[0]),
                    UserAccount = new UserAccount
                    {
                        GUID = new Guid(verificationParams[1])
                    }
                });
                if (FP == null)
                {
                    ViewBag.Error = "Invalid reset password link";
                }
                else if (FP.IsChanged)
                {
                    FP = null;
                    ViewBag.Error = "Reset password link used already.";
                }
                else if (FP.IsDeleted)
                {
                    FP = null;
                    ViewBag.Error = "Reset password link expired.";
                }
                return View(FP);
            }
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ForgotPassword FP, string ConfirmPassword, string lang)
        {

            if (FP != null && FP.UserAccount != null && (FP.UserAccount.Password == ConfirmPassword))
            {
                FP.UserAccount.Password = Security.Encrypt<string>(FP.UserAccount.Password);

                List<string> lastThreeUserPasswords = _userDao.GetLastThreeChangedPasswords(FP.UserAccount.GUID);

                if (lastThreeUserPasswords != null && lastThreeUserPasswords.Contains(FP.UserAccount.Password))
                {
                    ViewBag.Error = "Password already used.";
                }
                else
                {
                    FP.SystemIp = GetRemoteIp.GetIPAddress(HttpContext);

                    UserAccount C = _userDao.UpdatePassword(FP);

                    if (C != null)
                    {
                        //to pass routecode value to edm for translation changes
                        _utilityService.SendEmail("Reset Forgot Password", EmailTemplate.RenderViewToString<UserAccount>(this, "_EDMPasswordChangedAcknowledge", C), C.Email, false, null);
                        string successmessage = string.Empty;
                        successmessage = "Password updated successfully.";
                        ViewBag.Success = successmessage;
                    }
                    else
                    {
                        string errormessage = string.Empty;
                        errormessage = "Error Occured";
                        ViewBag.Error = errormessage;
                        FP = null;
                    }
                }
                ViewBag.Language = lang;
            }
            return View(viewName: "ResetPassword", model: FP);
        }

        [HttpGet]
        public ActionResult ResetFirstTimePassword()
        {
           
            return View(viewName: "ResetPassword", model: new ForgotPassword { UserAccount = ((UserAccount)Session["UserAccount"]) });
        }

        [HttpPost]
        public ActionResult ResetFirstTimePassword(ForgotPassword FP, string ConfirmPassword)
        {
            if (Session["UserAccount"] == null)
            {
                return RedirectToAction("Logout");
            }
            else
            {
              
          
                string Password = (FP != null && FP.UserAccount != null) ? FP.UserAccount.Password : string.Empty;

                if ((!string.IsNullOrEmpty(Password) && Password == ConfirmPassword && Password.Length >= 8 && Password.Any(char.IsDigit) && Password.Any(char.IsUpper) && Password.Any(char.IsLower)))
                {
                    UserAccount UA = new UserAccount
                    {
                        GUID = ((UserAccount)Session["UserAccount"]).GUID,
                        SystemIp = GetRemoteIp.GetIPAddress(HttpContext),
                        Password = Security.Encrypt<string>(Password)
                    };

                    List<string> lastThreeUserPasswords = _userDao.GetLastThreeChangedPasswords(UA.GUID);

                    if (lastThreeUserPasswords != null && lastThreeUserPasswords.Contains(UA.Password))
                    {
                        ViewBag.Error = "Password already used.";
                    }
                    else
                    {

                        if (_userDao.UpdateFirstTimePassword(UA) > 0)
                        {
                            UA = (UserAccount)Session["UserAccount"];
                            UA.IsPasswordChanged = true;
                            Session["UserAccount"] = UA;

                            if (UA!=null && UA.UserRole!=null && UA.UserRole.Id>0)
                            {
                                return RedirectToRoute("Admin_Index");
                            }
                            else
                            {
                                return RedirectToAction("Logout");
                            }
                        }
                    }
                }

                return View(viewName: "ResetPassword", model: new ForgotPassword { UserAccount = ((UserAccount)Session["UserAccount"]) });
            }
        }

        private string SendPasswordRecoveryEmail(ForgotPassword FP)
        {
            string status = string.Empty;
            if (FP != null && FP.UserAccount != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(RenderRazorViewToString("PasswordRecoveryEdm", FP));
                status= _utilityService.SendEmail("Password Recovery", sb.ToString(), FP.UserAccount.Email, true, null);
            }

            return status;
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}