using Dapper;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Persistence.Implementations
{
   public class UserDao: IUserDao
    {
        #region Private variables and constructors

        protected readonly IDbConnectionFactory factory;

        public UserDao(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        #endregion
        public List<User> GetUserList()
        {
            List<User> DisplayUserList = new List<User>();
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[SP_SelectUsers_SP]";
                DynamicParameters param = new DynamicParameters();
                DisplayUserList = conn.Query<User>(SQL, param, commandType: CommandType.StoredProcedure).ToList();
                conn.Close();
            }
            return DisplayUserList;
        }

        public UserAccount AuthenticateUser(UserAccount UA)
        {
            UserAccount _UA = new UserAccount();
            DynamicParameters param = new DynamicParameters();
            param.Add("@AccountNo", UA.AccountNo, dbType: DbType.String);
            param.Add("@Password", UA.Password, DbType.String);
            param.Add("@SystemIP", UA.SystemIp, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_AuthenticateUser]";
                _UA = conn.Query<UserAccount,UserRole, UserAccount>(SQL, (user, userrole) =>
                {
                    if (user != null)
                    {
                        user.UserRole = userrole;

                    }
                    return user;
                }, param, commandType: CommandType.StoredProcedure).FirstOrDefault<UserAccount>();
            }
            return _UA;
        }

        public ForgotPassword SaveForgotPassword(string Email, string AccountNo, string SystemIP)
        {
            ForgotPassword result = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@Email", Email, dbType: DbType.String);
            param.Add("@AccountNo", AccountNo, DbType.String);
            param.Add("@SystemIP", SystemIP, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_SaveForgotPassword]";
                result = conn.Query<ForgotPassword, UserAccount,UserRole,ForgotPassword>(sql: SQL,
                    param: param,
                    map: (FP,UR,R) =>
                    {
                        if (FP != null)
                        {
                            FP.UserAccount = UR;
                            if (FP.UserAccount != null)
                            {
                                FP.UserAccount.UserRole = R;
                            }

                        }
                        return FP;
                    },
            commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            return result;
        }

        public ForgotPassword VerifyForgotPasswordUniqueId(ForgotPassword FP)
        {
            ForgotPassword result = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@UniqueId", FP.UniqueId, dbType: DbType.Guid);
            param.Add("@GUID", FP.UserAccount.GUID, dbType: DbType.Guid);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_VerifyForgotPasswordUniqueId]";
                result = conn.Query<ForgotPassword, UserAccount, ForgotPassword>(sql: SQL,
                    param: param,
                    map: (FPD, AU) =>
                    {
                        if (FPD != null)
                        {
                            FPD.UserAccount = AU;
                        }
                        return FPD;
                    },
            commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            return result;
        }

        public List<string> GetLastThreeChangedPasswords(Guid GUID)
        {
            List<string> result = null;

            DynamicParameters param = new DynamicParameters();
            param.Add("@GUID", GUID, dbType: DbType.Guid);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_GetLastThreeChangedPasswords]";
                result = conn.Query<string>(sql: SQL,
                    param: param,
                    commandType: CommandType.StoredProcedure).ToList();
            }
            return result;
        }
        public UserAccount UpdatePassword(ForgotPassword FP)
        {
            UserAccount result = null;
            DynamicParameters param = new DynamicParameters();
            param.Add("@UniqueId", FP.UniqueId, dbType: DbType.Guid);
            param.Add("@SystemIP", FP.SystemIp, dbType: DbType.String);
            param.Add("@GUID", FP.UserAccount.GUID, dbType: DbType.Guid);
            param.Add("@Password", FP.UserAccount.Password, dbType: DbType.String);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_UpdatePassword]";
                result = conn.Query<UserAccount, UserRole, UserAccount>(sql: SQL,
                    param: param, map: (UA, UR) =>
                    {
                        if (UA != null)
                        {
                            UA.UserRole = UR;
                        }
                        return UA;
                    },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            return result;
        }

        public void SaveUserAccountLoginLog(string id, string SystemIP)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@GUID", id, DbType.String);
            param.Add("@SystemIP", SystemIP, DbType.String);
            long result = 0;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_SaveUserAccountLoginLog]";
                result = conn.Execute(sql: SQL, param: param, commandType: CommandType.StoredProcedure);
                conn.Close();
            }
        }
        public void UpdateUserAccountLogoutLog(string id)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@GUID", id, DbType.String);
            long result = 0;
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_UpdateUserAccountLogoutLog]";
                result = conn.Execute(sql: SQL, param: param, commandType: CommandType.StoredProcedure);
                conn.Close();
            }
        }

        public int UpdateFirstTimePassword(UserAccount UA)
        {
            int result = 0;
            DynamicParameters param = new DynamicParameters();
            param.Add("@NewPassword", UA.Password, dbType: DbType.String);
            param.Add("@SystemIP", UA.SystemIp, dbType: DbType.String);
            param.Add("@GUID", UA.GUID, dbType: DbType.Guid);
            using (IDbConnection conn = factory.GetConnection())
            {
                conn.Open();
                string SQL = @"[USP_UpdateFirstTimePassword]";
                result = conn.Execute(SQL, param, commandType: CommandType.StoredProcedure);
            }
            return result;
        }

    }




}
