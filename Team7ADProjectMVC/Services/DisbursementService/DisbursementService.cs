using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Team7ADProjectMVC.Models;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao
    public class DisbursementService : IDisbursementService
    {
        ProjectEntities db = new ProjectEntities();
        PushNotification notify = new PushNotification();
        UtilityService uSvc = new UtilityService();

        public List<DisbursementList> GetAllDisbursements()
        {
            var disbursementList = from d in db.DisbursementLists
                                   select d;

            return (disbursementList.ToList());
        }


        public DisbursementList GetDisbursementById(int? id)
        {

            return db.DisbursementLists.Find(id);
        }
        public List<DisbursementList> GetDisbursementByDeptId(int? id)
        {
            var disbursementList = (from d in db.DisbursementLists
                                   where d.DepartmentId ==id
                                   orderby d.DeliveryDate
                                   select d).OrderByDescending(x => x.DeliveryDate);

            return (disbursementList.ToList());
        }

        public List<DisbursementList> GetDisbursementsBySearchCriteria(int? departmentId, string status)
        {
            if ((status == null || status == "") && departmentId == null)
            {
                return (db.DisbursementLists.ToList());
            }
            else if (status == null || status == "")
            {
                var queryResults = from d in db.DisbursementLists
                                   where d.DepartmentId == departmentId
                                   orderby d.DeliveryDate
                                   select d;
                return (queryResults.ToList());
            }
            else if (departmentId == null)
            {
                var queryResults = from d in db.DisbursementLists
                                   where d.Status == status
                                   orderby d.DeliveryDate
                                   select d;
                return (queryResults.ToList());
            }
            else
            {
                var queryResults = from d in db.DisbursementLists
                                   where d.DepartmentId == departmentId
                                   && d.Status == status
                                   orderby d.DeliveryDate
                                   select d;
                return (queryResults.ToList());
            }
        }

        public void UpdateDisbursementList(DisbursementList disbursementList)
        {
            db.Entry(disbursementList).State = EntityState.Modified;
            db.SaveChanges();
        }
        public List<DisbursementList> FindDisbursementsBySearch(List<DisbursementList> disbursementlist,string date, string status)
        {
            if ((status == null || status == "") && (date == null || date == ""))
            {
                return (disbursementlist);
            }
            else if (status == null || status == "")
            {
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selected = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var queryResults = from d in disbursementlist
                                   where d.DeliveryDate == selected
                                   orderby d.Status
                                   select d;
                return (queryResults.ToList());
            }
            else if (date == null || date == "")
            {
                var queryResults = (from d in disbursementlist
                                   where d.Status.Equals(status)
                                   orderby d.DeliveryDate
                                   select d).OrderByDescending (x=>x.DeliveryDate);
                return (queryResults.ToList());
            }
            else
            {
                List<String> datesplit = date.Split('/').ToList<String>();
                DateTime selected = new DateTime(Int32.Parse((datesplit[2])), Int32.Parse((datesplit[1])), Int32.Parse((datesplit[0])));
                var queryResults = from d in disbursementlist
                                   where d.Status.Equals(status)
                                   && d.DeliveryDate == selected
                                   orderby d.DeliveryDate
                                   select d;
                return (queryResults.ToList());
            }
        }

        public List<DisbursementDetail> GetdisbursementdetailById(int? id)
        {
            var disbursementDetails = db.DisbursementDetails.Where(model => model.DisbursementListId == id).Include(d => d.DisbursementList);
            return (disbursementDetails.ToList());
        }
        public string findCpnameByDisburse(int? id)
        {
            return (db.DisbursementLists.Find(id).Department.CollectionPoint.PlaceName);
        }
        public string findCptimeByDisburse(int? id)
        {
            return (db.DisbursementLists.Find(id).Department.CollectionPoint.CollectTime.ToString());
        }
        public string findDisbursenmentStatus(int? id)
        {
            return (db.DisbursementLists.Find(id).Status);
        }
        public void ConfirmDisbursement(int? disburseid)
        {
            var deptid = db.DisbursementLists.Find(disburseid).DepartmentId;
            int rid = db.DisbursementLists.Find(disburseid).Retrieval.RetrievalId;
            List<Requisition> requisitionlist = db.Requisitions.Where(model => model.RetrievalId == rid).ToList();

            List<RequisitionDetail> rdlist = (from x in db.RequisitionDetails
                                              where x.Requisition.RetrievalId == rid
                                              && x.Requisition.DepartmentId == deptid
                                              select x).ToList();

            var itlist = (from x in rdlist

                          group x by x.ItemNo into g
                          select new
                          {
                              ItemNo = g.Key,
                              OutstandingQuantity = g.Sum(x => x.OutstandingQuantity)
                          }).ToList();

            foreach (var total in itlist)

            {
                if(db.DisbursementLists.Find(disburseid).DisbursementDetails.Where(x=>x.ItemNo==total.ItemNo).ToList().Count==0)
                {
                    continue;
                }
                var deliveryquantity = db.DisbursementLists.Find(disburseid).DisbursementDetails.Single(model => model.ItemNo == total.ItemNo).DeliveredQuantity;

                var samelist = (from x in rdlist
                                where x.ItemNo == total.ItemNo
                                orderby x.Requisition.RequisitionId
                                select x
                                ).ToList();
                  
                if (total.OutstandingQuantity <= deliveryquantity)
                {
                    foreach (var item in samelist)
                    {
                        db.RequisitionDetails.Find(item.RequisitionDetailId).OutstandingQuantity = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < samelist.Count(); i++)
                    {

                        if (samelist[i].OutstandingQuantity > deliveryquantity)
                        {
                            db.RequisitionDetails.Find(samelist[i].RequisitionDetailId).OutstandingQuantity -= deliveryquantity;
                            break;
                        }
                        else
                        {
                            deliveryquantity -= samelist[i].OutstandingQuantity;
                            db.RequisitionDetails.Find(samelist[i].RequisitionDetailId).OutstandingQuantity = 0;

                        }
                    }
                }
            }
            db.DisbursementLists.Find(disburseid).Status = "Completed";

            foreach (var item in requisitionlist)
            {
                item.RequisitionStatus = "Completed";
            }

            foreach (var item in rdlist)
            {
                if (item.OutstandingQuantity != 0)
                {
                    item.Requisition.RequisitionStatus = "Outstanding";
                    break;
                }
            }

            db.SaveChanges();

            foreach(Requisition r in requisitionlist.Where(x=>x.DepartmentId==deptid))
            {
                try
                {
                    if (r.RequisitionStatus == "Completed")
                    {
                        string emailBody = r.Employee.EmployeeName + ", your requisition dated " + r.OrderedDate.Value.Date.ToString("dd/MM/yyyy") + " has been fulfilled and delivered. Please see http://" + uSvc.GetBaseUrl() + "/Stationery/Requisition/" + r.RequisitionId + " for more details.";
                        uSvc.SendEmail(new List<string>(new string[] { r.Employee.Email }), "Requisition Fulfilled", emailBody);
                    }
                    else
                    {
                        string emailBody = r.Employee.EmployeeName + ", your requisition dated " + r.OrderedDate.Value.Date.ToString("dd/MM/yyyy") + " has been processed with outstanding items remaining. Outstanding items will be disbursed in the next disbursement cycle when they are in stock. Please see http://" + uSvc.GetBaseUrl() + "/Stationery/Requisition/" + r.RequisitionId + " for more details.";
                        uSvc.SendEmail(new List<string>(new string[] { r.Employee.Email }), "Requisition Processed", emailBody);
                    }
                } catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
            string disbID = disburseid.ToString(); 
            notify.RepAcceptRequisition(disbID);
        }

        public List<DisbursementList> GetCollectionPointForDept(int dId)
        {
            var collectionLocation = from c in db.DisbursementLists
                                     where c.DepartmentId == dId
                                     select c;
            return collectionLocation.ToList();
        }

        public DisbursementDetail UpdateDisbursementStatus(int disbursementDetailId, int updatedDeliveryQuantity, string remarks)
        {
            DisbursementDetail dd = db.DisbursementDetails.Where(p => p.DisbursementDetailId == disbursementDetailId).First();
            IInventoryService invSvc = new InventoryService();
            int updateAmount = updatedDeliveryQuantity - (int)dd.DeliveredQuantity;
            invSvc.UpdateInventoryQuantity(dd.ItemNo, updateAmount);
            dd.DeliveredQuantity = updatedDeliveryQuantity;
            dd.Remark = remarks;
            db.SaveChanges();
            return dd;
        }
    }
}