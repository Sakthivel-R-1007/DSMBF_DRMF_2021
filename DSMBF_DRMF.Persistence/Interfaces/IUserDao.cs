using DSMBF_DRMF.Domain;
using System;
using System.Collections.Generic;

namespace DSMBF_DRMF.Persistence.Interfaces
{
    public interface IUserDao
    {
        List<User> GetUserList();
        ForgotPassword SaveForgotPassword(string Email, string AccountNo, string SystemIP);
        UserAccount AuthenticateUser(UserAccount UA);
        ForgotPassword VerifyForgotPasswordUniqueId(ForgotPassword FP);
        List<string> GetLastThreeChangedPasswords(Guid GUID);
        UserAccount UpdatePassword(ForgotPassword FP);
        void SaveUserAccountLoginLog(string id, string SystemIP);
        void UpdateUserAccountLogoutLog(string id);
        int UpdateFirstTimePassword(UserAccount UA);
    }
}
