using System.Web.Optimization;

namespace DSMBF_DRMF.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            #region css
            bundles.Add(new StyleBundle("~/layoutcss").Include(
                  "~/Content/css/style.css",
                  "~/Content/css/responsive-style.css"
                 ));

            bundles.Add(new StyleBundle("~/popupcss").Include(
                    "~/Content/css/popup.css"
                ));

            bundles.Add(new StyleBundle("~/claimaddcss").Include(
               "~/Content/css/jquery-ui.css",
               "~/Content/js/jqueryCalendar/jqueryCalendar.css"
              ));

            bundles.Add(new StyleBundle("~/claimindexcss").Include(
             "~/Content/css/flexslider.css",
             "~/Content/js/jqueryCalendar/jqueryCalendar.css"
            ));


            bundles.Add(new StyleBundle("~/claimviewcss").Include(
            "~/Content/css/style.css",
            "~/Content/css/responsive-style.css",
            "~/Content/css/popup.css"
           ));
            bundles.Add(new StyleBundle("~/distributorcss").Include(
           "~/Content/css/flexslider.css"
          ));

            #endregion

            #region js

            bundles.Add(new ScriptBundle("~/layoutjs").Include(
                   "~/Content/js/jquery-1.8.0.min.js",
                    "~/Content/js/common.js"
                    ));

            #region Login

            bundles.Add(new ScriptBundle("~/loginjs").Include(
                 "~/Content/js/vendor/jquery.validate.js",
                 "~/Content/js/Admin/login-index.js"
                ));
            #endregion

            #region ForgotPassword

            bundles.Add(new ScriptBundle("~/forgotpasswordjs").Include(
              "~/Content/js/vendor/jquery.validate.js",
              "~/Content/js/Admin/login-forgotpassword.js"
             ));
            #endregion

            #region ResetPassword

            bundles.Add(new ScriptBundle("~/resetpasswordjs").Include(
              "~/Content/js/vendor/jquery.validate.js",
              "~/Content/js/Admin/login-resetpassword.js"
             ));
            #endregion

            #region Claim add
            bundles.Add(new ScriptBundle("~/claim-addjs").Include(
               "~/Content/js/jqueryCalendar/jquery-ui-1.8.15.custom.min.js",
               "~/Content/js/vendor/jquery-1.12.4.js",
                "~/Content/js/vendor/jquery-ui-1.12.1.js",
                 "~/Content/js/vendor/jquery.validate.min.js",
              "~/Content/js/vendor/jquery.alphanum.js",
               "~/Content/js/vendor/jquery.validate.additional-methods.js",
                "~/Content/js/Admin/admin-claim-add.js"
              ));
            #endregion

            #region Figure add
            bundles.Add(new ScriptBundle("~/figure-addjs").Include(
               "~/Content/js/jqueryCalendar/jquery-ui-1.8.15.custom.min.js",
               "~/Content/js/vendor/jquery-1.12.4.js",
                "~/Content/js/vendor/jquery-ui-1.12.1.js",
                 "~/Content/js/vendor/jquery.validate.min.js",
              "~/Content/js/vendor/jquery.alphanum.js",
               "~/Content/js/vendor/jquery.validate.additional-methods.js",
                "~/Content/js/Admin/admin-figure-add.js"
              ));
            #endregion


            #region Claim index
            bundles.Add(new ScriptBundle("~/claim-indexjs").Include(
               "~/Content/js/jqueryCalendar/jquery-ui-1.8.15.custom.min.js",
               "~/Content/js/vendor/jquery-1.12.4.js",
                "~/Content/js/vendor/jquery-ui-1.12.1.js",
                 "~/Content/js/vendor/jquery.validate.min.js",
              "~/Content/js/vendor/jquery.alphanum.js",
               "~/Content/js/vendor/jquery.simplePagination.js",
                "~/Content/js/Admin/admin-claim-index.js"
              ));
            #endregion

            #region Claim View
            bundles.Add(new ScriptBundle("~/claim-viewjs").Include(
               "~/Content/js/jquery-1.9.1.min.js",
               "~/Content/js/common.js",
                "~/Content/js/ga.js",
                 "~/Content/js/bootstrap/bootstrap.js",
              "~/Content/js/Admin/admin-claim-view.js"
              ));
            #endregion


            #region home index
            bundles.Add(new ScriptBundle("~/home-indexjs").Include(
              "~/Content/js/slider_responsive/jquery.flexslider.js",
                "~/Content/js/slider_responsive/shCore.js",
               "~/Content/js/slider_responsive/shBrushXml.js",
                "~/Content/js/slider_responsive/shBrushJScript.js",
                 "~/Content/js/slider_responsive/jquery.easing.js",
              "~/Content/js/slider_responsive/jquery.mousewheel.js",
               "~/Content/js/slider_responsive/demo.js"
              ));
            #endregion


            bundles.Add(new ScriptBundle("~/admin-distributor-indexjs").Include(
              "~/Content/js/vendor/jquery.validate.js",
              "~/Content/js/vendor/jquery.simplePagination.js",
             "~/Content/js/Admin/admin-distributor-index.js"
            ));
            #endregion

        }
    }
}