using ClosedXML.Excel;
using DSMBF_DRMF.Core;
using DSMBF_DRMF.Domain;
using DSMBF_DRMF.Filters;
using DSMBF_DRMF.Helpers;
using DSMBF_DRMF.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace DSMBF_DRMF.Areas.Admin.Controllers
{
    [AuthenticationFilter(Type = "Portal Administrator,Invoice Verification")]
    public class ReportController : Controller
    {
        #region Private variables


        private IReportDao _reportDao = null;


        public ReportController(IReportDao reportDao)
        {

            this._reportDao = reportDao;



        }

        #endregion
        // GET: PortalAdministrator/Report
        public ActionResult Index()
        {
            IList<ProgramType> ProgramType = null;
            ProgramType = _reportDao.GetProgramTypeList();
            ViewBag.ProgramTypeList = (ProgramType == null) ? new List<SelectListItem>() : ProgramType.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();

            IList<Report> Reports = null;
            Reports = _reportDao.GetReportList();
            ViewBag.ReportsList = (Reports == null) ? new List<SelectListItem>() : Reports.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();


            IList<Claim> InvoicePeriods = null;
            //InvoicePeriods = _reportDao.GetInvoicePeriodList();
            ViewBag.InvoicePeriodList = (InvoicePeriods == null) ? new List<SelectListItem>() : InvoicePeriods.Select(SG => new SelectListItem
            {
                Text = SG.InvoicePeriod,
                Value = SG.InvoicePeriod
            }).ToList();

            return View();
        }
        [HttpPost]
        public ActionResult Index(string Reports, string ProgramType = null, string Quarter = null, string InvoicePeriod = null)
        {
            int ReportId = 0;
            if (!string.IsNullOrEmpty(Reports) || !string.IsNullOrEmpty(ProgramType))
            {
                string data = Core.Security.DecodeUrlandDecrypt(Reports);
                ReportId = Convert.ToInt32(data);
                string Program = "0";
                if (!string.IsNullOrEmpty(ProgramType))
                {
                    Program = Core.Security.DecodeUrlandDecrypt(ProgramType);
                }
                int ProgramTypeId = Convert.ToInt32(Program);
                if (ReportId > 0)
                {
                    ExportToExcel(ReportId, Quarter, InvoicePeriod, ProgramTypeId);
                }
            }

            IList<ProgramType> ProgramTypes = null;
            ProgramTypes = _reportDao.GetProgramTypeList();
            ViewBag.ProgramTypeList = (ProgramTypes == null) ? new List<SelectListItem>() : ProgramTypes.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();

            IList<Report> Report = null;
            Report = _reportDao.GetReportList();
            ViewBag.ReportsList = (Reports == null) ? new List<SelectListItem>() : Report.Select(SG => new SelectListItem
            {
                Text = SG.Name,
                Value = Security.Encrypt<string>(SG.Id.ToString())
            }).ToList();

            IList<Claim> InvoicePeriods = null;
            InvoicePeriods = _reportDao.GetInvoicePeriodList(ReportId == 7);
            ViewBag.InvoicePeriodList = (InvoicePeriods == null) ? new List<SelectListItem>() : InvoicePeriods.Select(SG => new SelectListItem
            {
                Text = SG.InvoicePeriod,
                Value = SG.InvoicePeriod
            }).ToList();
            return View();
        }

        public JsonResult InvoicePeriod(string EncDetail)
        {
            IList<Claim> invoicePeriods = null;

            if (!string.IsNullOrEmpty(EncDetail))
            {
                long reportId = Security.Decrypt<long>(EncDetail);
                invoicePeriods = _reportDao.GetInvoicePeriodList(reportId == 7);
            }

            return Json(invoicePeriods == null ? new List<string>() : invoicePeriods.Select(s => s.InvoicePeriod).ToList(), JsonRequestBehavior.AllowGet);
        }

        public void ExportToExcel(int ReportId, string Quarter, string InvoicePeriod, int ProgramTypeId = 0)
        {
            UserAccount u = (UserAccount)Session["UserAccount"];
            DataTable list = new DataTable();


            list = GetData(ReportId, ProgramTypeId, Quarter, InvoicePeriod);
            string ReportName1 = "";
            switch (ProgramTypeId)
            {
                case 1:
                    ReportName1 = "(PV DRMF)";
                    break;
                case 2:
                    ReportName1 = "(CV DRMF)";
                    break;
                case 3:
                    ReportName1 = "";
                    break;

            }
            string ReportName = "";
            switch (ReportId)
            {
                case 1:
                    ReportName = "Distributor_Budget" + ReportName1;
                    break;
                case 2:
                    ReportName = "Claim" + ReportName1;
                    break;
                case 3:
                    ReportName = "Spending" + ReportName1;
                    break;
                case 4:
                    ReportName = "Payment_Manager" + ReportName1;
                    break;
                case 5:
                    ReportName = "Distributor" + ReportName1;
                    break;
                case 6:
                    ReportName = "Program_Owner_Payment" + ReportName1;
                    break;
                case 7:
                    ReportName = "Claim_Validation" + ReportName1;
                    break;

            }
            DataRow dr1 = list.NewRow();
            if (list != null && list.Columns != null && list.Columns.Count > 0 && ReportId != 7)
            {
                foreach (DataColumn col in list.Columns)
                {
                    list.Columns[col.ColumnName].ColumnName = !string.IsNullOrEmpty(KeywordDictionary.FindEnglishText(col.ColumnName)) ? KeywordDictionary.FindEnglishText(col.ColumnName) : col.ColumnName;

                }
            }
            if (ReportId > 0 && ReportId == 2)
            {
                int i = 0;
                foreach (DataRow dr in list.Rows)
                {
                    list.Rows[i][6] = (list.Rows[i][6] == DBNull.Value) ? null : Convert.ToDateTime(list.Rows[i][6].ToString()).ToString("dd/MM/yyyy");
                    i++;
                }
            }
            //if (ReportId > 0 && ReportId == 4)
            //{
            //    int i = 0;
            //    foreach (DataRow dr in list.Rows)
            //    {
            //        list.Rows[i][4] = (list.Rows[i][4] == DBNull.Value) ? null : Convert.ToDateTime(list.Rows[i][4].ToString()).ToString("dd/MM/yyyy");
            //        i++;
            //    }
            //}
            //if (ReportId > 0 && ReportId == 1)
            //{
            //    int i = 0;
            //    foreach (DataRow dr in list.Rows)
            //    {
            //        list.Rows[i][4] = (list.Rows[i][4] == DBNull.Value) ? null : Convert.ToDecimal(list.Rows[i][4].ToString()).ToString("0.00##");
            //        i++;
            //    }
            //}
            list.Rows.InsertAt(dr1, 0);
            list.Rows[0].Delete();
            list.AcceptChanges();
            //var grid = new System.Web.UI.WebControls.GridView();
            //grid.ApplyStyle(new Style() { BorderStyle = BorderStyle.Solid });
            //grid.Font.Name = "Calibri";
            //grid.Font.Size = 11;
            //grid.DataSource = list;
            //grid.DataBind();
            //Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportName + "_" + DateTime.Now.ToString() + "Reports.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            var workbook = new XLWorkbook();
            string range = "A1:B2";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            var worksheet = workbook.AddWorksheet(list, ReportName);
            worksheet.Row(1).InsertRowsAbove(1);
            worksheet.Range(range).Row(1);
            worksheet.Range(range).Row(1).Merge();
            //if (ReportId > 0 && ReportId == 4)
            //{
            //    string range1 = (ProgramTypeId > 0 && ProgramTypeId == 2 ? "F1:I1" : "F1:J1");
            //    string range2 = (ProgramTypeId > 0 && ProgramTypeId == 2 ? "J1:M1" : "K1:O1");
            //    worksheet.Range(range1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //    worksheet.Range(range1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //    worksheet.Range(range1).Style.Font.FontSize = 11;
            //    worksheet.Range(range1).Style.Font.Bold = true;
            //    worksheet.Range(range1).Style.Fill.BackgroundColor = XLColor.LightGray;
            //    worksheet.Range(range1).Row(1).Merge().Value = "(RMB，不含增值税）";
            //    worksheet.Range(range2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //    worksheet.Range(range2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //    worksheet.Range(range2).Style.Font.FontSize = 11;
            //    worksheet.Range(range2).Style.Font.Bold = true;
            //    worksheet.Range(range2).Style.Fill.BackgroundColor = XLColor.Gray;
            //    worksheet.Range(range2).Row(1).Merge().Value = "(RMB，税额 ）";
            //}
            using (var ms = new MemoryStream())
            {
                workbook.SaveAs(ms);
                ms.WriteTo(Response.OutputStream);
                ms.Close();
            }
            Response.Flush();
            Response.End();



            //Response.ContentEncoding = System.Text.Encoding.Unicode;
            //Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            //StringWriter sw = new StringWriter();
            //HtmlTextWriter htw = new HtmlTextWriter(sw);
            //grid.RenderControl(htw);
            //Response.Write(sw.ToString());
            //Response.End();
        }
        public DataTable GetData(int ReportId, int ProgramTypeId, string Quarter, string InvoicePeriod)
        {
            DataTable list = new DataTable();
            UserAccount u = (UserAccount)Session["UserAccount"];

            switch (ReportId)
            {
                case 1:
                    list = DistributorBudgetReportDataTable(_reportDao.GetDistributorBudgetReports(ProgramTypeId), ProgramTypeId);
                    break;
                case 2:
                    list = ToDataTable(_reportDao.GetClaimReports(Quarter, ProgramTypeId));
                    break;
                case 3:
                    list = ToDataTable(_reportDao.GetSpendingReports(ProgramTypeId));
                    break;
                case 4:
                    list = RemoveDataTable(_reportDao.GetPaymentReports(Quarter, InvoicePeriod, ProgramTypeId), ProgramTypeId, Quarter, InvoicePeriod.ToUpper());
                    break;
                case 5:
                    list = ToDataTable(_reportDao.GetDistributortReports(ProgramTypeId));
                    break;

                case 6:
                    list = MonthlyDataTable(_reportDao.GetMonthlyPaymentReports(InvoicePeriod, ProgramTypeId), ProgramTypeId);
                    break;
                case 7:
                    list = ToDataTable(_reportDao.GetMonthlyClaimReport(InvoicePeriod, ProgramTypeId));
                    break;

            }

            return list;
        }

        private DataTable DistributorBudgetReportDataTable<T>(List<T> items, long ProgramTypeId)
        {
            DataTable dataTable = ToDataTable<T>(items);
            if (ProgramTypeId > 0 && ProgramTypeId == 1)
            {
                dataTable.Columns.Remove("EMCH_Jan_Mar_G_TotalBudget");
                dataTable.Columns.Remove("EMCH_Jan_Mar_G_BudgetUtilized");
                dataTable.Columns.Remove("EMCH_Jan_Mar_G_BudgetRemaining");
                dataTable.Columns.Remove("EMCH_Apr_Jun_LO_TotalBudget");
                dataTable.Columns.Remove("EMCH_Apr_Jun_LO_BudgetUtilized");
                dataTable.Columns.Remove("EMCH_Apr_Jun_LO_BudgetRemaining");
                dataTable.Columns.Remove("EMCH_Apr_Jun_G_TotalBudget");
                dataTable.Columns.Remove("EMCH_Apr_Jun_G_BudgetUtilized");
                dataTable.Columns.Remove("EMCH_Apr_Jun_G_BudgetRemaining");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_LO_TotalBudget");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_LO_BudgetUtilized");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_LO_BudgetRemaining");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_CR_TotalBudget");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_CR_BudgetUtilized");
                dataTable.Columns.Remove("EMCSS_Apr_Jun_CR_BudgetRemaining");

                dataTable.Columns.Remove("EMCH_July_Dec_LO_TotalBudget");
                dataTable.Columns.Remove("EMCH_July_Dec_LO_BudgetUtilized");
                dataTable.Columns.Remove("EMCH_July_Dec_LO_BudgetRemaining");
                dataTable.Columns.Remove("EMCH_July_Dec_G_TotalBudget");
                dataTable.Columns.Remove("EMCH_July_Dec_G_BudgetUtilized");
                dataTable.Columns.Remove("EMCH_July_Dec_G_BudgetRemaining");
                dataTable.Columns.Remove("EMCSS_July_Dec_LO_TotalBudget");
                dataTable.Columns.Remove("EMCSS_July_Dec_LO_BudgetUtilized");
                dataTable.Columns.Remove("EMCSS_July_Dec_LO_BudgetRemaining");
                dataTable.Columns.Remove("EMCSS_July_Dec_CR_TotalBudget");
                dataTable.Columns.Remove("EMCSS_July_Dec_CR_BudgetUtilized");
                dataTable.Columns.Remove("EMCSS_July_Dec_CR_BudgetRemaining");

                // dataTable.Columns[""].SetOrdinal(0);

                dataTable.Columns["Area"].SetOrdinal(0);
                dataTable.Columns["AccountCode"].SetOrdinal(1);
                dataTable.Columns["CompanyName_EN"].SetOrdinal(2);
                dataTable.Columns["CompanyName_SC"].SetOrdinal(3);
                dataTable.Columns["ProgramType"].SetOrdinal(4);
                dataTable.Columns["FullYearTotalBudget"].SetOrdinal(5);
                dataTable.Columns["EMCSS_Jan_Mar_LO_TotalBudget"].SetOrdinal(6);
                dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetUtilized"].SetOrdinal(7);
                dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetRemaining"].SetOrdinal(8);
                dataTable.Columns["EMCSS_Jan_Mar_CR_TotalBudget"].SetOrdinal(9);
                dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetUtilized"].SetOrdinal(10);
                dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetRemaining"].SetOrdinal(11);
                dataTable.Columns["EMCH_Jan_Mar_LO_TotalBudget"].SetOrdinal(12);
                dataTable.Columns["EMCH_Jan_Mar_LO_BudgetUtilized"].SetOrdinal(13);
                dataTable.Columns["EMCH_Jan_Mar_LO_BudgetRemaining"].SetOrdinal(14);
                dataTable.Columns["FullYearTotalBudgetUtilized"].SetOrdinal(15);
                dataTable.Columns["FullYearTotalBudgetRemaining"].SetOrdinal(16);
            }


            return dataTable;
        }

        private DataTable RemoveDataTable<T>(List<T> items, long ProgramTypeId, string Quarter, string InvoicePeriod)
        {
            DataTable dataTable = ToDataTable<T>(items);

            string QuarterString = "";
            switch (Quarter)
            {
                case "Q1":
                    QuarterString = "1Q";
                    break;
                case "Q2":
                    QuarterString = "2Q";
                    break;
            }


            dataTable.Columns["AccountCode"].ColumnName = "Account Code";
            dataTable.Columns["CompanyName_EN"].ColumnName = "Distributor Company Name (EN)";
            dataTable.Columns["CompanyName_SC"].ColumnName = "Distributor Company Name (SC)";
            int ordinalIndex = 0;
            if (string.IsNullOrEmpty(InvoicePeriod))
            {
                dataTable.Columns["TotalAmount"].ColumnName = QuarterString + " send total amount (RMB)";
                dataTable.Columns.Remove("Month");
                ordinalIndex = -1;
            }
            else
            {
                dataTable.Columns["Month"].ColumnName = "Month";
                dataTable.Columns["TotalAmount"].ColumnName = "Total Amount by Month(RMB)";
            }

            dataTable.Columns["EMCH_Jan_Mar_LO_BudgetDetected"].ColumnName = "EMCH Jan-Mar 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH Jan-Mar 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_G_BudgetDetected"].ColumnName = "EMCH Jan-Mar 13% 润滑脂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH Jan-Mar 13 % 润滑脂 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_LO_BudgetDetected"].ColumnName = "EMCH Apr-Jun 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH Apr-Jun 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_G_BudgetDetected"].ColumnName = "EMCH Apr-Jun 13% 润滑脂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH Apr-Jun 13 % 润滑脂 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetDetected"].ColumnName = "EMCSS Jan-Mar 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Jan-Mar 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetDetected"].ColumnName = "EMCSS Jan-Mar 13% 化学试剂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Jan-Mar 13% 化学试剂 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_LO_BudgetDetected"].ColumnName = "EMCSS Apr-Jun 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Apr-Jun 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_CR_BudgetDetected"].ColumnName = "EMCSS Apr-Jun 13% 化学试剂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Apr-Jun 13% 化学试剂 -" + QuarterString + " (RMB)";

            dataTable.Columns["EMCH_July_Dec_LO_BudgetDetected"].ColumnName = "EMCH July-Dec 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_July_Dec_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH July-Dec 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCH_July_Dec_G_BudgetDetected"].ColumnName = "EMCH July-Dec 13% 润滑脂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCH_July_Dec_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH July-Dec 13 % 润滑脂 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_July_Dec_LO_BudgetDetected"].ColumnName = "EMCSS July-Dec 13% 润滑油 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_July_Dec_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS July-Dec 13 % 润滑油 -" + QuarterString + " (RMB)";
            dataTable.Columns["EMCSS_July_Dec_CR_BudgetDetected"].ColumnName = "EMCSS July-Dec 13% 化学试剂 -" + QuarterString + " (未税) (RMB)";
            dataTable.Columns["EMCSS_July_Dec_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS July-Dec 13 % 化学试剂 -" + QuarterString + " (RMB)";

            if (ProgramTypeId > 0 && ProgramTypeId == 1)
            {
                dataTable.Columns.Remove("EMCH Jan-Mar 13% 润滑脂 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH Jan-Mar 13 % 润滑脂 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCH Apr-Jun 13% 润滑油 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH Apr-Jun 13 % 润滑油 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCH Apr-Jun 13% 润滑脂 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH Apr-Jun 13 % 润滑脂 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCSS Apr-Jun 13% 润滑油 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS Apr-Jun 13 % 润滑油 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCSS Apr-Jun 13% 化学试剂 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS Apr-Jun 13% 化学试剂 -" + QuarterString + " (RMB)");

                dataTable.Columns.Remove("EMCH July-Dec 13% 润滑油 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH July-Dec 13 % 润滑油 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCH July-Dec 13% 润滑脂 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH July-Dec 13 % 润滑脂 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCSS July-Dec 13% 润滑油 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS July-Dec 13 % 润滑油 -" + QuarterString + " (RMB)");
                dataTable.Columns.Remove("EMCSS July-Dec 13% 化学试剂 -" + QuarterString + " (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS July-Dec 13 % 化学试剂 -" + QuarterString + " (RMB)");

                // dataTable.Columns[""].SetOrdinal(0);

                dataTable.Columns["Account Code"].SetOrdinal(0);
                dataTable.Columns["Distributor Company Name (EN)"].SetOrdinal(1);
                dataTable.Columns["Distributor Company Name (SC)"].SetOrdinal(2);

                string totalTitle = QuarterString + " send total amount (RMB)";
                if (string.IsNullOrEmpty(Quarter))
                {
                    dataTable.Columns["Month"].SetOrdinal(3);
                    totalTitle = "Total Amount by Month(RMB)";
                }

                dataTable.Columns[totalTitle].SetOrdinal(4 + ordinalIndex);
                dataTable.Columns["EMCSS Jan-Mar 13% 润滑油 -" + QuarterString + " (未税) (RMB)"].SetOrdinal(5 + ordinalIndex);
                dataTable.Columns["TAX - EMCSS Jan-Mar 13 % 润滑油 -" + QuarterString + " (RMB)"].SetOrdinal(6 + ordinalIndex);
                dataTable.Columns["EMCSS Jan-Mar 13% 化学试剂 -" + QuarterString + " (未税) (RMB)"].SetOrdinal(7 + ordinalIndex);
                dataTable.Columns["TAX - EMCSS Jan-Mar 13% 化学试剂 -" + QuarterString + " (RMB)"].SetOrdinal(8 + ordinalIndex);
                dataTable.Columns["EMCH Jan-Mar 13% 润滑油 -" + QuarterString + " (未税) (RMB)"].SetOrdinal(9 + ordinalIndex);
                dataTable.Columns["TAX - EMCH Jan-Mar 13 % 润滑油 -" + QuarterString + " (RMB)"].SetOrdinal(10 + ordinalIndex);

            }

            return dataTable;
        }

        private DataTable MonthlyDataTable<T>(List<T> items, long ProgramTypeId)
        {
            DataTable dataTable = ToDataTable<T>(items);

            dataTable.Columns["AccountCode"].ColumnName = "Account Code";
            dataTable.Columns["CompanyName_EN"].ColumnName = "Distributor Company Name (EN)";
            dataTable.Columns["CompanyName_SC"].ColumnName = "Distributor Company Name (SC)";
            dataTable.Columns["InvoicePeriod"].ColumnName = "Period";
            dataTable.Columns["TotalAmount"].ColumnName = "Total amount (RMB)";

            dataTable.Columns["EMCH_Jan_Mar_LO_BudgetDetected"].ColumnName = "EMCH Jan-Mar 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH Jan-Mar 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_G_BudgetDetected"].ColumnName = "EMCH Jan-Mar 13% 润滑脂 - (未税) (RMB)";
            dataTable.Columns["EMCH_Jan_Mar_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH Jan-Mar 13 % 润滑脂 - (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_LO_BudgetDetected"].ColumnName = "EMCH Apr-Jun 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH Apr-Jun 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_G_BudgetDetected"].ColumnName = "EMCH Apr-Jun 13% 润滑脂 - (未税) (RMB)";
            dataTable.Columns["EMCH_Apr_Jun_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH Apr-Jun 13 % 润滑脂 - (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetDetected"].ColumnName = "EMCSS Jan-Mar 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Jan-Mar 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetDetected"].ColumnName = "EMCSS Jan-Mar 13% 化学试剂 - (未税) (RMB)";
            dataTable.Columns["EMCSS_Jan_Mar_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Jan-Mar 13% 化学试剂 - (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_LO_BudgetDetected"].ColumnName = "EMCSS Apr-Jun 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Apr-Jun 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_CR_BudgetDetected"].ColumnName = "EMCSS Apr-Jun 13% 化学试剂 - (未税) (RMB)";
            dataTable.Columns["EMCSS_Apr_Jun_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS Apr-Jun 13% 化学试剂 - (RMB)";

            dataTable.Columns["EMCH_July_Dec_LO_BudgetDetected"].ColumnName = "EMCH July-Dec 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCH_July_Dec_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCH July-Dec 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCH_July_Dec_G_BudgetDetected"].ColumnName = "EMCH July-Dec 13% 润滑脂 - (未税) (RMB)";
            dataTable.Columns["EMCH_July_Dec_G_BudgetDetectedTax"].ColumnName = "TAX - EMCH July-Dec 13 % 润滑脂 - (RMB)";
            dataTable.Columns["EMCSS_July_Dec_LO_BudgetDetected"].ColumnName = "EMCSS July-Dec 13% 润滑油 - (未税) (RMB)";
            dataTable.Columns["EMCSS_July_Dec_LO_BudgetDetectedTax"].ColumnName = "TAX - EMCSS July-Dec 13 % 润滑油 - (RMB)";
            dataTable.Columns["EMCSS_July_Dec_CR_BudgetDetected"].ColumnName = "EMCSS July-Dec 13% 化学试剂 - (未税) (RMB)";
            dataTable.Columns["EMCSS_July_Dec_CR_BudgetDetectedTax"].ColumnName = "TAX - EMCSS July-Dec 13 % 化学试剂 - (RMB)";

            if (ProgramTypeId == 1)
            {
                //dataTable.Columns.Remove("EMCH Jan-Mar 16% 润滑脂 - (未税) (RMB)");
                //dataTable.Columns.Remove("TAX - EMCH Jan-Mar 16 % 润滑脂 - (RMB)");
                dataTable.Columns.Remove("EMCH Apr-Jun 13% 润滑油 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH Apr-Jun 13 % 润滑油 - (RMB)");
                dataTable.Columns.Remove("EMCH Apr-Jun 13% 润滑脂 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH Apr-Jun 13 % 润滑脂 - (RMB)");
                dataTable.Columns.Remove("EMCSS Apr-Jun 13% 润滑油 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS Apr-Jun 13 % 润滑油 - (RMB)");
                dataTable.Columns.Remove("EMCSS Apr-Jun 13% 化学试剂 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS Apr-Jun 13% 化学试剂 - (RMB)");

                dataTable.Columns.Remove("EMCH July-Dec 13% 润滑油 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH July-Dec 13 % 润滑油 - (RMB)");
                dataTable.Columns.Remove("EMCH July-Dec 13% 润滑脂 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCH July-Dec 13 % 润滑脂 - (RMB)");
                dataTable.Columns.Remove("EMCSS July-Dec 13% 润滑油 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS July-Dec 13 % 润滑油 - (RMB)");
                dataTable.Columns.Remove("EMCSS July-Dec 13% 化学试剂 - (未税) (RMB)");
                dataTable.Columns.Remove("TAX - EMCSS July-Dec 13 % 化学试剂 - (RMB)");


                // dataTable.Columns[""].SetOrdinal(0);

                dataTable.Columns["Account Code"].SetOrdinal(0);
                dataTable.Columns["Distributor Company Name (EN)"].SetOrdinal(1);
                dataTable.Columns["Distributor Company Name (SC)"].SetOrdinal(2);
                dataTable.Columns["Period"].SetOrdinal(3);
                dataTable.Columns["Total amount (RMB)"].SetOrdinal(4);
                dataTable.Columns["EMCSS Jan-Mar 13% 润滑油 - (未税) (RMB)"].SetOrdinal(5);
                dataTable.Columns["TAX - EMCSS Jan-Mar 13 % 润滑油 - (RMB)"].SetOrdinal(6);
                dataTable.Columns["EMCSS Jan-Mar 13% 化学试剂 - (未税) (RMB)"].SetOrdinal(7);
                dataTable.Columns["TAX - EMCSS Jan-Mar 13% 化学试剂 - (RMB)"].SetOrdinal(8);
                dataTable.Columns["EMCH Jan-Mar 13% 润滑油 - (未税) (RMB)"].SetOrdinal(9);
                dataTable.Columns["TAX - EMCH Jan-Mar 13 % 润滑油 - (RMB)"].SetOrdinal(10);

            }


            return dataTable;
        }

        public DataTable RenameSpendingReport(DataTable dt)
        {
            return dt;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

    }
}