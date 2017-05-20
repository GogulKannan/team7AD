using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Team7ADProjectMVC
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //DataSet1.PurchaseAnalysisDataTable dt = new DataSet1.PurchaseAnalysisDataTable();
            //DataSet1TableAdapters.PurchaseAnalysisTableAdapter da = new DataSet1TableAdapters.PurchaseAnalysisTableAdapter();
            //da.Fill(dt);
            //ReportDocument cr = new ReportDocument();
            //cr.Load(Server.MapPath("~/Reports/CrystalReport2.rpt"));
            //cr.SetDataSource((DataTable)dt);
            //CrystalReportViewer1.ReportSource = cr;
           
            ReportDocument cr = new ReportDocument();
            cr.Load(Server.MapPath(Session["rptPath"].ToString()));
            cr.SetDataSource(Session["rptData"]);
            CrystalReportViewer1.ReportSource = cr;
        }
    }
}