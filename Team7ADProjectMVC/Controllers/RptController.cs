using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Team7ADProjectMVC.Models;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author : Zhan Seng
    public class RptController : Controller
    {
        private IReportService rptSvc;
        private IInventoryService invSvc;
        private IDepartmentService deptSvc;
        public RptController()
        {
            rptSvc = new ReportService();
            invSvc = new InventoryService();
            deptSvc = new DepartmentService();
        }
        
        // GET: Rpt
        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult Index()
        {

            ViewBag.Departments = deptSvc.ListAllDepartments();
            ViewBag.Categories = invSvc.GetAllCategories();
            ViewBag.Months = rptSvc.GetMonthValues();
            ViewBag.Years = rptSvc.GetYearValues();

            return View("ItemDeptRpt");
        }

        [HttpPost]
        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult Index(FormCollection form)
        {

            List<YrMth> yrMthList = rptSvc.GetListOfYrMthFromUI(Request.Form["Year"], Request.Form["Month"], Request.Form["Year2"], Request.Form["Month2"], Request.Form["Year3"], Request.Form["Month3"]);
            List<string> depts = Request.Form["Departments"].Split(',').ToList<string>();
            String categorySelected = Request.Form["Categories"];
            DataView data = rptSvc.GetDataForDisbAnalysis(yrMthList, depts, categorySelected);
            Session["rptData"] = data;
            Session["rptPath"] = "~/Reports/CrystalReport1.rpt";
            return Redirect("ReportViewer.aspx");

        }

        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult ItemSupplier()
        {
            ViewBag.Categories = invSvc.GetAllCategories();
            ViewBag.Months = rptSvc.GetMonthValues();
            ViewBag.Years = rptSvc.GetYearValues();

            return View("ItemSupplierRpt");
        }

        [HttpPost]
        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult ItemSupplier(FormCollection f)
        {
            String categorySelected = Request.Form["Categories"];
            List<YrMth> yrMthList = rptSvc.GetListOfYrMthFromUI(Request.Form["Year"], Request.Form["Month"], Request.Form["Year2"], Request.Form["Month2"], Request.Form["Year3"], Request.Form["Month3"]);

            DataView data = rptSvc.GetDataForSupplierAnalysis(yrMthList, categorySelected);
            Session["rptData"] = data;
            Session["rptPath"] = "~/Reports/CrystalReport2.rpt";
            return Redirect("/ReportViewer.aspx");

        }

        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult Stocklist()
        {

            DataView data = rptSvc.GetDataForStocklist();
            Session["rptData"] = data;
            Session["rptPath"] = "~/Reports/CrystalReport3.rpt";
            return Redirect("/ReportViewer.aspx");

        }

        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult CostByDept()
        {

            ViewBag.Departments = deptSvc.ListAllDepartments();
            ViewBag.Categories = invSvc.GetAllCategories();
            ViewBag.Months = rptSvc.GetMonthValues();
            ViewBag.Years = rptSvc.GetYearValues();

            return View("CostDeptRpt");
        }

        [HttpPost]
        [AuthorisePermissions(Permission = "ViewReports")]
        public ActionResult CostByDept(FormCollection form)
        {

            List<YrMth> yrMthList = rptSvc.GetListOfYrMthFromUI(Request.Form["Year"], Request.Form["Month"], Request.Form["Year2"], Request.Form["Month2"], Request.Form["Year3"], Request.Form["Month3"]);
            List<string> depts = Request.Form["Departments"].Split(',').ToList<string>();
            List<string> categoriesSelected = Request.Form["Categories"].Split(',').ToList<string>();
            DataView data = rptSvc.GetDataForCostAnalysis(yrMthList, depts, categoriesSelected);
            Session["rptData"] = data;
            Session["rptPath"] = "~/Reports/CrystalReport4.rpt";
            return Redirect("/ReportViewer.aspx");

        }


    }
}