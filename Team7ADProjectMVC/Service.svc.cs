using System;
using System.Collections.Generic;
using System.Linq;
using Team7ADProjectMVC.Models;
using Team7ADProjectMVC.Services;
using System.Web.Security;

namespace Team7ADProjectMVC
{
    //Author: Gogul / Linda

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service.svc or Service.svc.cs at the Solution Explorer and start debugging.
    public class Service : IService
	{
        ProjectEntities db = new ProjectEntities();

        IInventoryService invService = new InventoryService();
        IRequisitionService reqService = new RequisitionService();
        IDisbursementService disService = new DisbursementService();
        PushNotification fcm = new PushNotification();
        IDepartmentService deptSvc = new DepartmentService();
        ISupplierAndPurchaseOrderService supplierPOSvc = new SupplierAndPurchaseOrderService();

        public List<WCFMsg> DoWork()
        {
            List<WCFMsg> l = new List<WCFMsg>();
            l.Add(new WCFMsg("ok"));
            l.Add(new WCFMsg("ok2"));
            l.Add(new WCFMsg("ok3"));
            Console.Write(l.ToString());
            return l;
        }

        public List<wcfRequisitionList> RequisitionList(string deptid)
        {
            List<wcfRequisitionList> making = new List<wcfRequisitionList>();
            int departmentId = Convert.ToInt32(deptid);
            List<Requisition> reqList = invService.GetNotCompletedRequisitions(departmentId);
            
            String beforesplit = "";
            String aftersplit = "";
            Char delimiter = ' ';
         foreach(Requisition rr in reqList)
         {
             wcfRequisitionList rl = new wcfRequisitionList();
             rl.Employeename = rr.Employee.EmployeeName;
             rl.Status = rr.RequisitionStatus;
             rl.Id = rr.RequisitionId.ToString();
             beforesplit = rr.OrderedDate.ToString();
             String[] substrings = beforesplit.Split(delimiter);
             aftersplit = substrings[0];
             rl.OrderDate = aftersplit;         
             making.Add(rl);
         }
         return making.ToList();
        }

        public List<wcfRequisitionItem> getrequisitionitem(String deptId, String reqID)
        {
            List<wcfRequisitionItem> making = new List<wcfRequisitionItem>();
            int dId = Convert.ToInt32(deptId);
            int rId = Convert.ToInt32(reqID);
            List<RequisitionDetail> reqItem = deptSvc.GetRequisitionDetailByDept(dId, rId);

            foreach (RequisitionDetail rr in reqItem)
            {
                wcfRequisitionItem rl = new wcfRequisitionItem();
                rl.Itemname = rr.Inventory.Description;
                rl.Quantity = rr.Quantity.ToString();
                rl.Uom = rr.Inventory.Measurement.UnitOfMeasurement;
                making.Add(rl);
            }
            return making;
        }

        public List<wcfTodayCollectionlist> getTodayCollection(String deptid)
        {
            List<wcfTodayCollectionlist> making = new List<wcfTodayCollectionlist>();
            int dId = Convert.ToInt32(deptid);
            List<DisbursementList> list = invService.GetNotCompletedDisbursements(dId);

            List<DisbursementDetail> tempList = new List<DisbursementDetail>();
            foreach (var item in list)
            {
                if(item.DeliveryDate.Equals(DateTime.Today))
                {
                    wcfTodayCollectionlist itemTemp = new wcfTodayCollectionlist();
                    itemTemp.Collectionpt = item.Department.CollectionPoint.PlaceName;
                    itemTemp.Time = item.Department.CollectionPoint.CollectTime.ToString();
                    itemTemp.DisbursementListID = item.DisbursementListId.ToString();
                    making.Add(itemTemp);
                }
            }
            return making;
        }

        public List<wcfTodayCollectionDetail> getTodayCollectionDetail(String deptid, String disListID)
        {
            List<wcfTodayCollectionDetail> collectionDetails = new List<wcfTodayCollectionDetail>();
            int did = Convert.ToInt32(deptid);
            int disbursementListID = Convert.ToInt32(disListID);
            List<DisbursementDetail> dDetail = invService.GetNotCompletedDisbursementDetails(did, disbursementListID);

            foreach (DisbursementDetail dd in dDetail)
            {
                if(dd.PreparedQuantity>0)
                {
                    wcfTodayCollectionDetail cd = new wcfTodayCollectionDetail();
                    cd.RequestedQty = dd.PreparedQuantity.ToString();
                    cd.DisbursedQty = dd.DeliveredQuantity.ToString();
                    cd.ItemDescription = dd.Inventory.Description;
                    collectionDetails.Add(cd);
                }
               
            }
            return collectionDetails.ToList();
        }

        public List<wcfApproveRequisitions> getApproveReqList(String deptid)
        {
            List<wcfApproveRequisitions> approvalList = new List<wcfApproveRequisitions>();
            int did = Convert.ToInt32(deptid);
            List<Requisition> aList = reqService.GetAllPendingRequisitionByDept(did);

            String beforesplit = "";
            String aftersplit = "";
            Char delimiter = ' ';
            foreach (Requisition req in aList)
            {
                wcfApproveRequisitions cd = new wcfApproveRequisitions();
                cd.EmpName = req.Employee.EmployeeName.ToString();
                beforesplit = req.OrderedDate.ToString();
                String[] substrings = beforesplit.Split(delimiter);
                aftersplit = substrings[0];
                cd.ReqDate = aftersplit ; 
              
                cd.ReqID = req.RequisitionId.ToString();
                approvalList.Add(cd);
            }
            return approvalList.ToList();
        }

        public List<wcfApproveReqDetails> getApproveReqDetails(String deptId, String reqId)
        {
            List<wcfApproveReqDetails> approvalList = new List<wcfApproveReqDetails>();
            int dId = Convert.ToInt32(deptId);
            int rId = Convert.ToInt32(reqId);
            List<RequisitionDetail> aList = reqService.GetAllRequisitionDetails(dId, rId);

            foreach (RequisitionDetail req in aList)
            {
                wcfApproveReqDetails rd = new wcfApproveReqDetails();
                rd.Item = req.Inventory.Description;
                rd.Quantity = req.Quantity.ToString();
                rd.UOM = req.Inventory.Measurement.UnitOfMeasurement;
                approvalList.Add(rd);
            }
            return approvalList.ToList();
        }

        public List<String> getCollectionPoint(String deptid)
        {
            List<String> sl = new List<string>();
            int dId = Convert.ToInt32(deptid);
            List<DisbursementList> collectionLocation = disService.GetCollectionPointForDept(dId);

            String s;
            foreach (DisbursementList d in collectionLocation)
            {
               s = d.Department.CollectionPoint.PlaceName +" "+ d.Department.CollectionPoint.CollectTime;
               sl.Add(s);
            }
            return sl;
        }

        public List<wcfDisbursementList> getDisbursementList()
        {
            List<wcfDisbursementList> dList = new List<wcfDisbursementList>();
            List<DisbursementList> disburse = invService.GetProcessingDisbursements();


            String beforesplit = "";
            String aftersplit = "";
            Char delimiter = ' ';
            foreach (DisbursementList d in disburse)
            {
                wcfDisbursementList dl = new wcfDisbursementList();
                dl.DeptName = d.Department.DepartmentName;
                dl.CollectionPoint = d.Department.CollectionPoint.PlaceName;
                beforesplit = d.DeliveryDate.ToString();
                String[] substrings = beforesplit.Split(delimiter);
                aftersplit = substrings[0];
                dl.DeliveryDatetime = aftersplit + " ( " + d.Department.CollectionPoint.CollectTime.ToString()+" )"; 
                dl.RepName = d.Department.Representative.EmployeeName.ToString();
                dl.RepPhone = d.Department.Representative.PhNo.ToString();
                dl.DisListID = d.DisbursementListId.ToString();
                dList.Add(dl);
            }
            return dList;
        }

        public List<wcfDisbursementListDetail> getDisbursementListDetails(String disListID)
        {
            List<wcfDisbursementListDetail> dDetail = new List<wcfDisbursementListDetail>();
            int dId = Convert.ToInt32(disListID);
            List<DisbursementDetail> disDetail = invService.FindDisbursementDetails(dId);

            foreach (DisbursementDetail d in disDetail)
            {
                wcfDisbursementListDetail dd = new wcfDisbursementListDetail();
                dd.Ddid = d.DisbursementDetailId.ToString();
                dd.Itemid = d.ItemNo;
                dd.ItemName = d.Inventory.Description;
                dd.PreQty = d.PreparedQuantity.ToString();
                dd.DisbQty = d.DeliveredQuantity.ToString();
                dd.Remarks = d.Remark;
                dDetail.Add(dd);
            }
            return dDetail;
        }

        public List<wcfStockReorder> getStockReorder()
        {
            List<wcfStockReorder> soList = new List<wcfStockReorder>();
            List<Inventory> reOrders = supplierPOSvc.GetAllItemsToResupply();

            foreach (Inventory i in reOrders)
            {
                wcfStockReorder inv = new wcfStockReorder();
                inv.ItemName = "#" + i.ItemNo+" "+ i.Description;
                inv.ActualQty = i.Quantity.ToString();
                inv.ReorderLevel = i.ReorderLevel.ToString();
                inv.ReorderQty = i.ReorderQuantity.ToString();
                inv.Supplier1 = i.Supplier.SupplierName;
                inv.S1Phone = i.Supplier.PhNo.ToString();
                inv.S1Price = "$ " + i.Price1.ToString();
                inv.Supplier2 = i.Supplier1.SupplierName;
                inv.S2Phone = i.Supplier1.PhNo.ToString();
                inv.S2Price = "$ " + i.Price2.ToString();
                inv.Supplier3 = i.Supplier2.SupplierName;
                inv.S3Phone = i.Supplier2.PhNo.ToString();
                inv.S3Price = "$ " + i.Price3.ToString();

                soList.Add(inv);
            }
            return soList;
        }

        public List<wcfRetrivalList> getRetrivalList()
        {
            List<wcfRetrivalList> retrialList = new List<wcfRetrivalList>();
            RetrievalList reList = new RetrievalList();
            reList = invService.GetRetrievalList();
            int? rid =reList.retrievalId;
            List<RetrievalListItems> itemsToR = reList.itemsToRetrieve;
   
            foreach (RetrievalListItems r in itemsToR)
            {
                wcfRetrivalList rl = new wcfRetrivalList();
                rl.ItemNo = r.itemNo;
                rl.ItemName = r.description;
                rl.BinNo = r.binNo;
                rl.RequestedQty = r.requiredQuantity.ToString();
                rl.RetrievedQty = r.collectedQuantity.ToString();
             
                String st = "";
                if(r.collectionStatus.ToString().Equals("False"))
                {
                    st = "Not Collected";
                }
                else
                {
                    st = "Collected";
                }
                rl.Status = st;
                retrialList.Add(rl);
            }
            return retrialList;
        }

        public void markascollected(String collected,String itemNo)
        {
            try
            {
                int collectedid = Convert.ToInt32(collected);
                RetrievalList rList = invService.GetRetrievalList();
                invService.UpdateCollectionInfo(rList, collectedid, itemNo);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public String getallocate(String deliverydate)
        {
            String rt = "false";
            try
            {
              
                String[] e = deliverydate.Split('/');
                DateTime edate = new DateTime(Int32.Parse(e[2]), Int32.Parse(e[0]), Int32.Parse(e[1]));
                invService.AutoAllocateDisbursementsByOrderOfRequisition(edate);
                rt = "True";
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                rt = "false";
            }
            return rt;
        }


        public string makePermissionstring(String s)
        {
            if(s.Equals("True"))
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        public wcflogin getlogin(String userid , String password, String token)
        {
            wcflogin dDetail = new wcflogin();
            try
            {
               
                int empid = Convert.ToInt32(userid);
                bool result = Membership.ValidateUser(userid, password);
                if (result == true)
                {
                    Employee emp = deptSvc.FindEmployeeById(empid);
                    if(emp.RoleId<=4)
                    {
                        emp.Token = token;
                        deptSvc.UpdateEmployee(emp);

                        if (deptSvc.IsDelegate(emp))
                        {
                            deptSvc.SetDelegatePermissions(emp);
                        }

                        dDetail.Role = emp.Role.Name;
                        dDetail.Deptid = emp.DepartmentId.ToString();
                        dDetail.Userid = userid;
                        dDetail.EmpName = emp.EmployeeName;
                        dDetail.Authenticate = "true";
                        Role makePerm = emp.Role;
                        dDetail.Permission = makePermissionstring(makePerm.ViewRequisition.ToString()) + "-" + makePermissionstring(makePerm.ApproveRequisition.ToString()) + "-" +
                            makePermissionstring(makePerm.ChangeCollectionPoint.ToString()) + "-" + makePermissionstring(makePerm.ViewCollectionDetails.ToString());

                        PushOldNotification(empid, token);
                    }
                    else
                    {
                        dDetail.Authenticate = "false";
                    }
                   
                }
                else
                {
                    dDetail.Authenticate = "false";
                }
                return dDetail;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                dDetail.Authenticate = "false";
                return dDetail;
            }
        }

        public String updatelocation(String deptid, String collectionptid)
        {
            try {
                int dId = Convert.ToInt32(deptid);
                int cpoint = Convert.ToInt32(collectionptid);
                Department wcfItem = deptSvc.FindDeptById(dId);
                deptSvc.changeDeptCp(wcfItem, cpoint);                
                return "true";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }

        public void updatedqun(wcfDisbursementListDetail c )
        {
            int dId = Convert.ToInt32(c.Ddid);
            int dId1 = Convert.ToInt32(c.DisbQty);
            int math;
            DisbursementDetail dd = disService.UpdateDisbursementStatus(dId, dId1, c.Remarks);            
            math = dId1 - (int)dd.DeliveredQuantity;
            invService.UpdateInventoryQuantity(dd.ItemNo, math);
        }

        public string approveReq(String reqId,String  deptheadID)
        {
            string result = "False";
            try {
                int rId = Convert.ToInt32(reqId);
                int headid = Convert.ToInt32(deptheadID);
                Requisition r = reqService.FindById(rId);
                reqService.UpdateApproveStatus(r, null, headid);
                result = "True";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                result = "False";
            }
            return result;
        }

        public string rejectReq(String reqId, String remarks, String deptheadID)
        {
            string result = "False";
            try
            {
                int rId = Convert.ToInt32(reqId);
                int headid = Convert.ToInt32(deptheadID);
                Requisition r = reqService.FindById(rId);
                reqService.UpdateRejectStatus(r, remarks, headid);
                result = "True";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                result = "False";
            }
            return result;
        }

        public List<wcfStoreRequisitions> getStoreRequistions()
        {
            List<wcfStoreRequisitions> storeReq = new List<wcfStoreRequisitions>();
            List<Requisition> reqList = invService.GetOutStandingRequisitions();

            String beforesplit = "";
            String aftersplit = "";
            Char delimiter = ' ';
            foreach (Requisition req in reqList)
            {
                wcfStoreRequisitions rl = new wcfStoreRequisitions();
                beforesplit = req.Employee.Department.DepartmentName;
                String[] substrings = beforesplit.Split(delimiter);
                aftersplit = substrings[0];
                rl.DeptName = aftersplit;

                beforesplit = req.ApprovedDate.ToString();
                String[] substrings1 = beforesplit.Split(delimiter);
                aftersplit = substrings1[0];
                rl.ApprovalDate = aftersplit;

                rl.ReqStatus = req.RequisitionStatus;
                storeReq.Add(rl);
            }
                return storeReq;
        }

        public String wcfBtnReqList()
        {
            RetrievalList rList = invService.GetRetrievalList();
              String result="";
                if(rList.requisitionList == null)
                {
                    result= "generate";
                }
                else
                result = "view";
                return result;
            }

        public String wcfGenetateBtnOK()
        {
            try
            {
                invService.PopulateRetrievalList();
                invService.PopulateRetrievalListItems();
                return "true";
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }

        public String wcfClearListBtnOK()
        {
            try
            {
                invService.ClearRetrievalList();
                return "true";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }
        public String wcfAcceptCollection(String DisListId)
        {
            try
            {
                int disId = Convert.ToInt32(DisListId);
                disService.ConfirmDisbursement(disId); 
                return "true";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }

        public String wcfSendForConfirmation(String DisbListId)
        {
            try
            {
            int dId = Convert.ToInt32(DisbListId);

            DisbursementList disb = disService.GetDisbursementById(dId);
            int deptit= (int)disb.DepartmentId;
            string deptName = disb.Department.DepartmentName;

            String name =disb.Department.CollectionPoint.PlaceName;
            List<String> myData = new List<string>();
            myData.Add("ReceiveRequisition");
            myData.Add(name);
            myData.Add(DisbListId);
            myData.Add(disb.Department.CollectionPoint.CollectTime.ToString());

            fcm.PushNotificationForRep("Accept Delivery", "Please Confirm Delivery", myData,deptit);

                return "true";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }

        public String wcfLogout(String userID)
        {
            try
            {
                int Uid = Convert.ToInt32(userID);
                Employee emp = db.Employees.Where(W => W.EmployeeId == Uid).First();
                emp.Token = null;
                db.SaveChanges();
                return "true";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return "false";
            }
        }

        public void PushOldNotification(int  EmpID,String token)
        {
            var notification= from n in db.Notifications where n.EmployeeId == EmpID select n;
            if (notification != null)
            {
                foreach (Notification n in notification)
                {
                    List<String> myData = new List<string>();
                    myData.Add(n.Intent);
                    myData.Add(n.PageHeader);
                    myData.Add(n.PageId);
                    myData.Add(n.ExtraDetail);
                    fcm.PushFCMNotification(n.Title, n.Body, token, myData);
                    DeleteOldNotifications(n.NotificationId);                    
                }
                db.SaveChanges();
            }
        }

        public void DeleteOldNotifications (int notID)
        {
            var report = (from d in db.Notifications
                          where d.NotificationId == notID
                          select d).Single();

            db.Notifications.Remove(report);
        }
    }
}
