using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Team7ADProjectMVC.Models;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao & Sandi
    public class RequisitionService : IRequisitionService
    {
        ProjectEntities db = new ProjectEntities();
        PushNotification notify = new PushNotification();
        UtilityService uSvc = new UtilityService();

        public List<Requisition> ListAllRequisitionByDept(int? deptId)
        {

            return (db.Requisitions.Where(x=>x.DepartmentId==deptId).OrderByDescending(x=>x.OrderedDate).ToList());
        }
        public List<Requisition> ListAllRequisition()
        {

            return (db.Requisitions.ToList());
        }
        public List<Requisition> GetAllPendingRequisitionByDept(int? depId)
        {
            var queryByStatus = from t in db.Requisitions 
                                  where t.RequisitionStatus == "Pending Approval" && t.DepartmentId == depId
                                 orderby t.RequisitionId ascending
                                  select t;
            return (queryByStatus.ToList());
 
        }
      
        public Requisition FindById(int? requisitionId)
        {
            return db.Requisitions.Find(requisitionId);
        }
        
        public void  UpdateApproveStatus(Requisition requisition, string comments,int? approvedbyId)
        {

            requisition.RequisitionStatus = "Approved";
            requisition.Comment = comments;
            requisition.ApprovedDate = DateTime.Today.Date;
            requisition.ApprovedBy = approvedbyId;
            
            db.Entry(requisition).State = EntityState.Modified;
            db.SaveChanges();

            string reqListId = requisition.RequisitionId.ToString();
            notify.NewRequisitonMade(reqListId);
            try
            {
                string emailBody = requisition.Employee.EmployeeName + ", your requisition dated " + requisition.OrderedDate.Value.Date.ToString("dd/MM/yyyy") + " has been approved. Please go to http://" + uSvc.GetBaseUrl() + "/Stationery/Requisition/" + requisition.RequisitionId + " for more information.";
                uSvc.SendEmail(new List<string>(new string[] { requisition.Employee.Email }), "Approved Requisition", emailBody);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            
        }
        public void UpdateRejectStatus(Requisition requisition, string comments, int? approvedbyId)
        {

            requisition.Comment = comments;
            requisition.ApprovedDate = DateTime.Today.Date;
            requisition.RequisitionStatus = "Rejected";
            requisition.ApprovedBy = approvedbyId;

            db.Entry(requisition).State = EntityState.Modified;
            db.SaveChanges();

            try
            {
                string emailBody = requisition.Employee.EmployeeName + ", your requisition dated " + requisition.OrderedDate.Value.Date.ToString("dd/MM/yyyy") + " has been rejected. Please go to http://" + uSvc.GetBaseUrl() + "/Stationery/Requisition/" + requisition.RequisitionId + " for more information.";
                uSvc.SendEmail(new List<string>(new string[] { requisition.Employee.Email }), "Rejected Requisition", emailBody);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
         public List<Requisition> getDataForPagination(string searchString)
        {
            var queryByStatus= db.Requisitions.Where(s => (s.Employee.EmployeeName.Contains(searchString)
                                       || s.OrderedDate.ToString().Contains(searchString)));
            return (queryByStatus.ToList());
        }

        public List<RequisitionDetail> GetAllRequisitionDetails(int dId, int rId)
        {
            var aList = from a in db.RequisitionDetails
                        where a.RequisitionId == rId
                        && a.Requisition.DepartmentId == dId
                        && a.Requisition.RequisitionStatus == "Pending Approval"
                        orderby a.Inventory.Description ascending
                        select a;
            return aList.ToList();
        }
        public List<RequisitionDetail> GetAllRequisitionDetails()
        {
            return db.RequisitionDetails.ToList();
        }

        public void CreateRequisition(Requisition r)
        {
            db.Requisitions.Add(r);
            db.SaveChanges();
        }

        public void UpdateRequisition(Requisition requisition, Requisition req, int idd, int eid, int? deid)
        {

            requisition.RequisitionId = idd;
            req.RequisitionStatus = "Pending Approval";
            req.EmployeeId = eid;
            req.DepartmentId = deid;
            req.OrderedDate = DateTime.Today;

        }

        public string CreateRequisition(Requisition requisition, int employeeId)
        {
            requisition.Employee = db.Employees.Find(employeeId);
            db.Requisitions.Add(requisition);
            db.SaveChanges();
            return requisition.RequisitionId.ToString();
        }
    }
}