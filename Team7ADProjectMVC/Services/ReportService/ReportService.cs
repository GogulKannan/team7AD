using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Team7ADProjectMVC.Models;

namespace Team7ADProjectMVC.Services
{
    //Author : Zhan Seng
    public class ReportService : IReportService
    {
        public List<string> GetYearValues()
        {
            List<string> yrs = new List<string>();
            int y = DateTime.Now.Year;
            for (int i = y; i > y - 8; i--)
            {
                yrs.Add(i.ToString());
            }
            return yrs;
        }

        public List<string> GetMonthValues()
        {
            List<string> mths = new List<string>();
            for (int i = 1; i < 13; i++)
            {
                mths.Add(i.ToString());
            }
            return mths;
        }

        public List<YrMth> GetListOfYrMthFromUI(string yr1, string mth1, string yr2, string mth2, string yr3, string mth3)
        {
            List <YrMth> list= new List<YrMth>();
            list.Add(new YrMth(Int32.Parse(yr1), Int32.Parse(mth1)));
            if (yr2.Length > 0 && mth2.Length > 0)
            {
                list.Add(new YrMth(Int32.Parse(yr2), Int32.Parse(mth2)));
            }
            if (yr3.Length > 0 && mth3.Length > 0)
            {
                list.Add(new YrMth(Int32.Parse(yr3), Int32.Parse(mth3)));
            }
            return list;
        }

        public DataView GetDataForDisbAnalysis(List<YrMth> yrMthList, List<string> depts, string categorySelected)
        {
            DataSet1TableAdapters.disbAnalysisTableAdapter da = new DataSet1TableAdapters.disbAnalysisTableAdapter();
            DataSet1.disbAnalysisDataTable dt = new DataSet1.disbAnalysisDataTable();
            da.Fill(dt);
            List<DataSet1.disbAnalysisRow> filteredList = dt.Where(x => x.CategoryName == categorySelected && depts.Contains(x.DepartmentName)).ToList<DataSet1.disbAnalysisRow>();
            DataSet1.disbAnalysisDataTable filteredDT = new DataSet1.disbAnalysisDataTable();
            foreach (DataSet1.disbAnalysisRow r in filteredList)
            {
                filteredDT.ImportRow(r);
            }
            EnumerableRowCollection<Team7ADProjectMVC.DataSet1.disbAnalysisRow> query;
            if (yrMthList.Count == 3)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[1].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[1].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[2].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[2].yr
                        select row;
            }
            else if (yrMthList.Count == 2)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[1].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[1].yr
                        select row;
            }
            else
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        select row;
            }
            DataView data = query.AsDataView();
            return data;
        }

        public DataView GetDataForSupplierAnalysis(List<YrMth> yrMthList, string categorySelected)
        {
            DataSet1.PurchaseAnalysisDataTable dt = new DataSet1.PurchaseAnalysisDataTable();
            DataSet1TableAdapters.PurchaseAnalysisTableAdapter da = new DataSet1TableAdapters.PurchaseAnalysisTableAdapter();
            da.Fill(dt);
            List<DataSet1.PurchaseAnalysisRow> filteredList = dt.Where(x => x.CategoryName == categorySelected).ToList<DataSet1.PurchaseAnalysisRow>();
            DataSet1.PurchaseAnalysisDataTable filteredDT = new DataSet1.PurchaseAnalysisDataTable();
            foreach (DataSet1.PurchaseAnalysisRow r in filteredList)
            {
                filteredDT.ImportRow(r);
            }

            EnumerableRowCollection<Team7ADProjectMVC.DataSet1.PurchaseAnalysisRow> query;
            if (yrMthList.Count==3)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("AuthorizedDate").Month == yrMthList[0].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("AuthorizedDate").Month == yrMthList[1].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[1].yr
                        || row.Field<DateTime>("AuthorizedDate").Month == yrMthList[2].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[2].yr
                        select row;
            }
            else if (yrMthList.Count == 2)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("AuthorizedDate").Month == yrMthList[0].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("AuthorizedDate").Month == yrMthList[1].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[1].yr
                        select row;
            }
            else
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("AuthorizedDate").Month == yrMthList[0].mth && row.Field<DateTime>("AuthorizedDate").Year == yrMthList[0].yr
                        select row;
            }

            DataView data = query.AsDataView();
            return data;
        }


        public DataView GetDataForStocklist()
        {
            DataSet1.StocklistDataTable dt = new DataSet1.StocklistDataTable();
            DataSet1TableAdapters.StocklistTableAdapter da = new DataSet1TableAdapters.StocklistTableAdapter();
            da.Fill(dt);
            return dt.AsDataView();
        }

        public DataView GetDataForCostAnalysis(List<YrMth> yrMthList, List<string> depts, List<string> categoriesSelected)
        {
            DataSet1TableAdapters.CostAnalysisTableAdapter da = new DataSet1TableAdapters.CostAnalysisTableAdapter();
            DataSet1.CostAnalysisDataTable dt = new DataSet1.CostAnalysisDataTable();
            da.Fill(dt);
            List<DataSet1.CostAnalysisRow> filteredList;
            if (categoriesSelected.Contains("All"))
            {
                filteredList = dt.Where(x => depts.Contains(x.DepartmentName)).ToList<DataSet1.CostAnalysisRow>();
            }
            else filteredList = dt.Where(x => categoriesSelected.Contains(x.CategoryName) && depts.Contains(x.DepartmentName)).ToList<DataSet1.CostAnalysisRow>();

            DataSet1.CostAnalysisDataTable filteredDT = new DataSet1.CostAnalysisDataTable();
            foreach (DataSet1.CostAnalysisRow r in filteredList)
            {
                filteredDT.ImportRow(r);
            }
            EnumerableRowCollection<Team7ADProjectMVC.DataSet1.CostAnalysisRow> query;
            if (yrMthList.Count == 3)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[1].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[1].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[2].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[2].yr
                        select row;
            }
            else if (yrMthList.Count == 2)
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        || row.Field<DateTime>("DeliveryDate").Month == yrMthList[1].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[1].yr
                        select row;
            }
            else
            {
                query = from row in filteredDT.AsEnumerable()
                        where row.Field<DateTime>("DeliveryDate").Month == yrMthList[0].mth && row.Field<DateTime>("DeliveryDate").Year == yrMthList[0].yr
                        select row;
            }
            DataView data = query.AsDataView();
            return data;
        }

    }
}