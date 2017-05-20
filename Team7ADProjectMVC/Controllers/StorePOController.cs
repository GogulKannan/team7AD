using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Team7ADProjectMVC.Exceptions;
using Team7ADProjectMVC.Models;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author : Edwin
    public class StorePOController : Controller
    {
        private IInventoryService inventorySvc;
        private IDepartmentService deptSvc;
        private ISupplierAndPurchaseOrderService supplierAndPOSvc;
        private IUtilityService utilSvc;
        
        public StorePOController()
        {
            inventorySvc = new InventoryService();
            deptSvc = new DepartmentService();
            supplierAndPOSvc = new SupplierAndPurchaseOrderService();
            utilSvc = new UtilityService();
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder")]
        public ActionResult GeneratePO()
        {
            List<Inventory> itemsToResupply = supplierAndPOSvc.GetAllItemsToResupply();
            return View(itemsToResupply);
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder")]
        public ActionResult GeneratePurchaseOrders(string[] itemNo, int[] supplier, int?[] orderQuantity)
        {
            Employee currentEmployee = (Employee)Session["User"];
            supplierAndPOSvc.GeneratePurchaseOrders(currentEmployee,itemNo, supplier, orderQuantity);
            Session["inventoryToResupply"] = new List<Inventory>();
            List<Employee> storeManagement = deptSvc.GetStoreManagerAndSupervisor();
            try //email to Store Manager and Supervisor
            {
                string emailBody = storeManagement[0].EmployeeName + " and " + storeManagement[1].EmployeeName + ", you have new pending purchase orders for approval. Please go to http://"+ utilSvc.GetBaseUrl() + "StorePO/PurchaseOrderSummary to approve them.";
                utilSvc.SendEmail(new List<string>(new string[] { storeManagement[0].Email, storeManagement[1].Email }), "New Purchase Orders for Approval", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return RedirectToAction("PurchaseOrderSummary");
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder,ApprovePurchaseOrder")]
        public ActionResult PurchaseOrderSummary()
        {
            List<PurchaseOrder> poList = supplierAndPOSvc.GetAllPOOrderByApproval();
            poList.OrderByDescending(x => x.OrderDate);
            
            return View(poList);
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder,ApprovePurchaseOrder")]
        public ActionResult SearchPurchaseOrderSummary(string orderStatus, string dateOrderedString, string dateApprovedString)
        {
            DateTime? dateOrdered = null;
            DateTime? dateApproved = null;
            int resultCount = 0;
            if (dateOrderedString != null && dateOrderedString.Count() > 1)
            {
                dateOrdered = utilSvc.GetDateTimeFromPicker(dateOrderedString);
            }
            if (dateApprovedString != null && dateApprovedString.Count() > 1)
            {
                dateApproved = utilSvc.GetDateTimeFromPicker(dateApprovedString);
            }

            List <PurchaseOrder> poList = supplierAndPOSvc.SearchPurchaseOrders(orderStatus, dateOrdered, dateApproved, out resultCount);
            ViewBag.ResultCount = resultCount;
            return View("PurchaseOrderSummary", poList);
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder,ApprovePurchaseOrder")]
        public ActionResult DeliveryDetails(int id)
        {
            List<DeliveryDetail> deliveryDetailsList = supplierAndPOSvc.GetDeliveryDetailsByDeliveryId(id);
            Delivery delivery = supplierAndPOSvc.FindDeliveryById(id);
            ViewBag.DeliveryDetailsList = deliveryDetailsList;
            return View("ViewReceiveOrder",delivery);
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "MakePurchaseOrder,ApprovePurchaseOrder")]
        public ActionResult PurchaseOrder(int id, int? page)
        {
            PurchaseOrder purchaseOrder = supplierAndPOSvc.FindPOById(id);
            ViewBag.PurchaseOrder = purchaseOrder;

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(purchaseOrder.PurchaseDetails.ToPagedList(pageNumber, pageSize));
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "ApprovePurchaseOrder")]
        public ActionResult ApprovePO(int poNumber, string approve)
        {
            if(approve=="Approve")
            {
                approve = "Approved";
            } else
            {
                approve = "Rejected";
            }
            Employee currentEmployee = (Employee)Session["User"];
            supplierAndPOSvc.ApprovePurchaseOrder(currentEmployee, poNumber, approve);
            return RedirectToAction("PurchaseOrderSummary");
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult ListDeliveries()
        {
            List<Delivery> allDeliveries = supplierAndPOSvc.GetAllDeliveries();
            return View(allDeliveries);
        }
        // seq diagram done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult AcceptDelivery(int deliveryId, string deliveryRefNo, string dateDelivered, int[] deliveryDetailId, string[] itemNo, int[] quantity, string[] remarks)
        {
            Employee currentEmployee = (Employee)Session["User"];
            supplierAndPOSvc.ReceiveDelivery(currentEmployee, deliveryId, deliveryRefNo, dateDelivered, deliveryDetailId, itemNo, quantity, remarks);

            return RedirectToAction("ListDeliveries");
        }

        [AuthorisePermissions(Permission = "MakePurchaseOrder")]
        public ActionResult CreateAdhocPurchaseOrder()
        {
            Employee currentEmployee = (Employee)Session["User"];
            ViewBag.InventoryItems = inventorySvc.GetAllInventory();
            List<Inventory> itemsToResupply = (List<Inventory>)Session["inventoryToResupply"];
            if (TempData["doc"] != null)
            {
                ViewBag.Error = TempData["doc"];
            }
                return View(itemsToResupply);
        }

        [AuthorisePermissions(Permission = "MakePurchaseOrder")]
        public ActionResult AddAdhocPOItem(string inventoryId)
        {
            Inventory item = inventorySvc.FindInventoryItemById(inventoryId);
            List<Inventory> itemsToResupply = (List<Inventory>)Session["inventoryToResupply"];
            try
            {
                var q = (from x in itemsToResupply
                         where x.ItemNo == inventoryId
                         select x).FirstOrDefault();
                if (q != null)
                {
                    throw new RequisitionAndPOCreationException("Duplicate items are not allowed.");
                }
            }
            catch (RequisitionAndPOCreationException e)
            {
                TempData["doc"] = e;
                return RedirectToAction("CreateAdhocPurchaseOrder");
            }
            itemsToResupply.Add(item);
            return RedirectToAction("CreateAdhocPurchaseOrder");
        }

        [AuthorisePermissions(Permission = "MakePurchaseOrder")]
        public ActionResult DeleteItem(string inventoryId)
        {
            List<Inventory> itemsToResupply = (List<Inventory>)Session["inventoryToResupply"];
            itemsToResupply.RemoveAll(x => x.ItemNo == inventoryId);
            Session["inventoryToResupply"] = itemsToResupply;
            return RedirectToAction("CreateAdhocPurchaseOrder");
        }

    }

}