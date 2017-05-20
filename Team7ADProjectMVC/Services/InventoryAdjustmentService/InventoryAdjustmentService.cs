using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao
    public class InventoryAdjustmentService : IInventoryAdjustmentService
    {
        ProjectEntities db = new ProjectEntities();
        UtilityService uSvc = new UtilityService();
        public string findRolebyUserID(int userid)
        {
            string role = db.Employees.Find(userid).Role.Name;
            return (role);
        }
        public List<Adjustment> findSupervisorAdjustmentList()
        {
            var adjustmentlist = (from x in db.Adjustments
                                  where x.Status == "Pending Approval"
                                  || x.Status == "Approved"
                                  || x.Status == "Rejected"
                                  orderby x.AdjustmentDate
                                  select x
                                   ).OrderByDescending(x => x.AdjustmentDate).ToList();
            return (adjustmentlist);
        }
        public List<Adjustment> findManagerAdjustmentList()
        {
            var adjustmentlist = (from x in db.Adjustments
                                  where x.Status == "Pending final Approval"
                                  || x.Status == "Approved"
                                  || x.Status == "Rejected"
                                  orderby x.AdjustmentDate
                                  select x
                      ).OrderByDescending(x=>x.AdjustmentDate).ToList();
            return (adjustmentlist);
        }
        public List<Adjustment> findClerkAdjustmentList()
        {
            var adjustmentlist = (from x in db.Adjustments
                                  orderby x.AdjustmentDate
                                  select x
                                  ).OrderByDescending (x=>x.AdjustmentDate).ToList();
            return adjustmentlist;

        }

        public List<Adjustment> FindAdjustmentBySearch(List<Adjustment> searchlist, string employee, string status, string date)
        {
            if ((status == null || status == "") && (date == null || date == "") && (employee == null || employee == ""))
            {
                return searchlist;
            }
            else if ((date == null || date == "") && (employee == null || employee == ""))//only select status
            {
                var resultlist = (from x in searchlist
                                  where x.Status.Equals(status)
                                  orderby x.AdjustmentDate
                                  select x).OrderByDescending(x=>x.AdjustmentDate).ToList();
                return resultlist;
            }
            else if ((status == null || status == "") && (date == null || date == ""))//only select employee
            {
                int epyid = Int32.Parse(employee);
                var resultlist = (from x in searchlist
                                  where x.EmployeeId == epyid
                                  orderby x.AdjustmentDate
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;

            }
            else if ((status == null || status == "") && (employee == null || employee == ""))//only select date
            {
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selectedate = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var resultlist = (from x in searchlist
                                  where x.AdjustmentDate == selectedate
                                  orderby x.Status
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;
            }
            else if ((date == null || date == ""))//select employee and status
            {
                int epyid = Int32.Parse(employee);
                var resultlist = (from x in searchlist
                                  where x.EmployeeId == epyid
                                  && x.Status.Equals(status)
                                  orderby x.AdjustmentDate
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;
            }
            else if ((status == null || status == ""))//select emp and date
            {
                int epyid = Int32.Parse(employee);
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selectedate = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var resultlist = (from x in searchlist
                                  where x.AdjustmentDate == selectedate
                                  && x.EmployeeId == epyid
                                  orderby x.Status
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;
            }
            else if ((employee == null || employee == ""))//select date and status
            {
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selectedate = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var resultlist = (from x in searchlist
                                  where x.AdjustmentDate == selectedate
                                  && x.Status.Equals(status)
                                  orderby x.EmployeeId
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;
            }
            else
            {
                int epyid = Int32.Parse(employee);
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selectedate = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var resultlist = (from x in searchlist
                                  where x.AdjustmentDate == selectedate
                                  && x.Status.Equals(status)
                                  && x.EmployeeId == epyid
                                  orderby x.Status
                                  select x).OrderByDescending(x => x.AdjustmentDate).ToList();
                return resultlist;
            }
        }
        public Adjustment findAdjustmentByID(int? id)
        {
            return (db.Adjustments.Find(id));
        }
        public List<AdjustmentDetail> findDetailByAdjustment(Adjustment adjust)
        {
            return (db.AdjustmentDetails.Where(x => x.AdjustmentId == adjust.AdjustmentId).ToList());
        }
        public string findAdjustmentStatus(int? id)
        {
            return (db.Adjustments.Find(id).Status);
        }
        public decimal? caculateTotal(List<AdjustmentDetail> adjdtlist)
        {
            decimal? total = 0;
            foreach (var item in adjdtlist)
            {
                var price = Math.Abs(Convert.ToInt32(item.Quantity)) * item.Inventory.Price1;

                total += price;
            }
            return total;
        }
        public void ApproveBySupervisor(int? empid, int? adjid)
        {
            db.Adjustments.Find(adjid).SupervisorAuthorizedDate = DateTime.Today;
            db.Adjustments.Find(adjid).SupervisorId = empid;
            db.Adjustments.Find(adjid).Status = "Approved";
            var adjdtlist = db.AdjustmentDetails.Where(x => x.AdjustmentId == adjid).ToList();
            foreach (var adjdt in adjdtlist)
            {
                var ivquantity = db.Inventories.Find(adjdt.ItemNo).Quantity;
                db.Inventories.Find(adjdt.ItemNo).Quantity = ivquantity + adjdt.Quantity;
            }
            db.SaveChanges();
            SendApprovedAdjEmail(db.Adjustments.Find(adjid));
        }
        public void RejecteBySupervisor(int? empid, int? adjid)
        {
            db.Adjustments.Find(adjid).SupervisorAuthorizedDate = DateTime.Today;
            db.Adjustments.Find(adjid).SupervisorId = empid;
            db.Adjustments.Find(adjid).Status = "Rejected";
            db.SaveChanges();
            SendRejectedAdjEmail(db.Adjustments.Find(adjid));
        }
        public void ApproveByManager(int? empid, int? adjid)
        {
            db.Adjustments.Find(adjid).HeadAuthorizedDate = DateTime.Today;
            db.Adjustments.Find(adjid).HeadId = empid;
            db.Adjustments.Find(adjid).Status = "Approved";
            var adjdtlist = db.AdjustmentDetails.Where(x => x.AdjustmentId == adjid).ToList();
            foreach (var adjdt in adjdtlist)
            {
                var ivquantity = db.Inventories.Find(adjdt.ItemNo).Quantity;
                db.Inventories.Find(adjdt.ItemNo).Quantity = ivquantity + adjdt.Quantity;
            }
            db.SaveChanges();
            SendApprovedAdjEmail(db.Adjustments.Find(adjid));
        }
        public void RejectByManager(int? empid, int? adjid)
        {
            db.Adjustments.Find(adjid).HeadAuthorizedDate = DateTime.Today;
            db.Adjustments.Find(adjid).HeadId = empid;
            db.Adjustments.Find(adjid).Status = "Rejected";
            db.SaveChanges();
            SendRejectedAdjEmail(db.Adjustments.Find(adjid));
        }

        public void PendingBySupervisor(int? empid, int? adjid)
        {
            db.Adjustments.Find(adjid).SupervisorAuthorizedDate = DateTime.Today;
            db.Adjustments.Find(adjid).SupervisorId = empid;
            db.Adjustments.Find(adjid).Status = "Pending Final Approval";
            db.SaveChanges();
        }
        public void createAdjustment(Adjustment adjustment)
        {
            db.Adjustments.Add(adjustment);
            db.SaveChanges();
        }

        private void SendApprovedAdjEmail(Adjustment adj)
        {
            try //email to notify approval
            {
                string emailBody = adj.Employee.EmployeeName + ", your request for an inventory adjustment dated " + adj.AdjustmentDate.Value.Date.ToString("dd/MM/yyyy") + " is approved.";
                uSvc.SendEmail(new List<string>(new string[] { adj.Employee.Email }), "Inventory Adjustment Approved", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        private void SendRejectedAdjEmail(Adjustment adj)
        {
            try //email to notify approval
            {
                string emailBody = adj.Employee.EmployeeName + ", your request for an inventory adjustment dated " + adj.AdjustmentDate.Value.Date.ToString("dd/MM/yyyy") + " is rejected.";
                uSvc.SendEmail(new List<string>(new string[] { adj.Employee.Email }), "Inventory Adjustment Rejected", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public bool IsValidAdjustment(Adjustment adj)
        {
            foreach(AdjustmentDetail ad in adj.AdjustmentDetails)
            {
                Inventory item = db.Inventories.Where(x => x.ItemNo == ad.ItemNo).FirstOrDefault();
                if ((item.Quantity + ad.Quantity) < 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
