using DSMBF_DRMF.Persistence;
using DSMBF_DRMF.Persistence.Implementations;
using DSMBF_DRMF.Persistence.Interfaces;
using DSMBF_DRMF.Service.Implementation;
using DSMBF_DRMF.Service.Interface;
using System.Web.Mvc;
using Unity;
using Unity.Injection;
using Unity.Mvc5;

namespace DSMBF_DRMF
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            // container.RegisterType<IUserDao, UserDao>();
             container.RegisterType<IUserDao, UserDao>();
            container.RegisterType<IDistributorDao, DistributorDao>();
            container.RegisterType<IClaimDao, ClaimDao>();
            container.RegisterType<IReportDao, ReportDao>();
            container.RegisterType<IUtilityService, UtilityService>();
            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IDbConnectionFactory, DbConnectionFactory>(new InjectionConstructor("DefaultDb"));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}