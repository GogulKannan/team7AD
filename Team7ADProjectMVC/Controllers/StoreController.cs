using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Team7ADProjectMVC.Exceptions;
using Team7ADProjectMVC.Models;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author : Edwin
    public class StoreController : Controller
    {
        private IInventoryService inventorySvc;
        private IDisbursementService disbursementSvc;
        private IDepartmentService deptSvc;
        private ISupplierAndPurchaseOrderService supplierAndPOSvc;
        private IUtilityService utilSvc;

        public StoreController()
        {
            inventorySvc = new InventoryService();
            disbursementSvc = new DisbursementService();
            deptSvc = new DepartmentService();
            supplierAndPOSvc = new SupplierAndPurchaseOrderService();
            utilSvc = new UtilityService();
        }

        //**************** INVENTORY ********************

        //Seq Diagram Done + Design Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult Inventory(int? page, int? id) 
        {
            List<Inventory> inventories;
            try
            {
                inventories = inventorySvc.GetInventoryListByCategory((int)id);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                inventories = inventorySvc.GetAllInventory();
            }

            ViewBag.Cat = inventorySvc.GetAllCategories().ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View("ViewInventory",inventories.ToPagedList(pageNumber,pageSize));
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult InventoryItem(String id)
        {
            Inventory inventory = inventorySvc.FindInventoryItemById(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.inv = inventory;
            ViewBag.sCard = inventorySvc.GetStockCardFor(id);
            return View("ViewStockCard",inventory);
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult RetrievalList()
        {
            RetrievalList rList = inventorySvc.GetRetrievalList();
            if (TempData["doc"] != null)
            {
                ViewBag.Error = TempData["doc"];
            }
            DateTime suggestedDeliveryDate = DateTime.Today.AddDays(utilSvc.DaysToAdd(DateTime.Today.DayOfWeek, DayOfWeek.Friday));
            ViewBag.SuggestedDeliveryDate = suggestedDeliveryDate;
            ViewBag.RList = rList;
            return View("ViewRetrievalList");
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult MarkAsCollected(int collectedQuantity, string itemNo)
        {
            RetrievalList rList = inventorySvc.GetRetrievalList();
            inventorySvc.UpdateCollectionInfo(rList, collectedQuantity, itemNo);
            
            return RedirectToAction("RetrievalList");
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult New()
        {
            ViewBag.CategoryId = new SelectList(inventorySvc.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.MeasurementId = new SelectList(inventorySvc.GetAllMeasurements(), "MeasurementId", "UnitOfMeasurement");
            ViewBag.SupplierId1 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode");
            ViewBag.SupplierId2 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode");
            ViewBag.SupplierId3 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode");
            return View("NewStockCard");
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New([Bind(Include = "ItemNo,CategoryId,Description,ReorderLevel,ReorderQuantity,MeasurementId,Quantity,HoldQuantity,SupplierId1,Price1,SupplierId2,Price2,SupplierId3,Price3,BinNo")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                if ((inventory.SupplierId1 != inventory.SupplierId2) && (inventory.SupplierId1 != inventory.SupplierId3) && (inventory.SupplierId2 != inventory.SupplierId3))
                {
                    inventory.ItemNo = inventorySvc.GetItemCode(inventory.Description);
                    inventorySvc.AddItem(inventory);
                    return RedirectToAction("Inventory");
                }
                else
                {
                    ViewBag.Error = "Please ensure that all three suppliers are different.";
                }
            }
            else
            {
                ViewBag.Error = "Please ensure that all three suppliers are different.";
            }

            ViewBag.CategoryId = new SelectList(inventorySvc.GetAllCategories(), "CategoryId", "CategoryName", inventory.CategoryId);
            ViewBag.MeasurementId = new SelectList(inventorySvc.GetAllMeasurements(), "MeasurementId", "UnitOfMeasurement", inventory.MeasurementId);
            ViewBag.SupplierId1 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId1);
            ViewBag.SupplierId2 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId2);
            ViewBag.SupplierId3 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId3);
            return View("NewStockCard");
        }
        //Seq Diagram Done  + Design Done
        // GET: Inventories/Edit/5
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult Edit(string id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = inventorySvc.FindInventoryItemById(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(inventorySvc.GetAllCategories(), "CategoryId", "CategoryName", inventory.CategoryId);
            ViewBag.MeasurementId = new SelectList(inventorySvc.GetAllMeasurements(), "MeasurementId", "UnitOfMeasurement", inventory.MeasurementId);
            ViewBag.SupplierId1 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId1);
            ViewBag.SupplierId2 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId2);
            ViewBag.SupplierId3 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId3);
            ViewBag.inv = inventory;
            return View("UpdateStockCard",inventory);
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItemNo,CategoryId,Description,ReorderLevel,ReorderQuantity,MeasurementId,Quantity,HoldQuantity,SupplierId1,Price1,SupplierId2,Price2,SupplierId3,Price3,BinNo")] Inventory inventory) 
        {
            if (ModelState.IsValid)
            {
                inventorySvc.UpdateInventory(inventory);
                return RedirectToAction("Inventory");
            }
            ViewBag.CategoryId = new SelectList(inventorySvc.GetAllCategories(), "CategoryId", "CategoryName", inventory.CategoryId);
            ViewBag.MeasurementId = new SelectList(inventorySvc.GetAllMeasurements(), "MeasurementId", "UnitOfMeasurement", inventory.MeasurementId);
            ViewBag.SupplierId1 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId1);
            ViewBag.SupplierId2 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId2);
            ViewBag.SupplierId3 = new SelectList(inventorySvc.GetAllSuppliers(), "SupplierId", "SupplierCode", inventory.SupplierId3);
            ViewBag.inv = inventory;
            return View("UpdateStockCard",inventory);
        }
        //Seq Diagram Done + Design Done
        public ActionResult Search(int id, int? page) 
        {
            var inventories = inventorySvc.GetInventoryListByCategory(id);
            ViewBag.Cat = inventorySvc.GetAllCategories().ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View("ViewInventory", inventories.ToPagedList(pageNumber,pageSize));
        }

        //************** DISBURSEMENTS **************
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult ViewDisbursements(int? page, int? id, String status)
        {
            List<DisbursementList> disbursementList;
            try
            {
                disbursementList = disbursementSvc.GetDisbursementsBySearchCriteria(id, status);
            }
            catch (Exception e)
            {
                disbursementList = disbursementSvc.GetAllDisbursements();
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            disbursementList = disbursementList.OrderByDescending(x => x.DeliveryDate).ToList();
            ViewBag.Id = id;
            ViewBag.Status = status;
            ViewBag.Departments = deptSvc.ListAllDepartments();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(disbursementList.ToPagedList(pageNumber, pageSize));
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult ViewDisbursement(int id)
        {
            DisbursementList dl = disbursementSvc.GetDisbursementById(id);
            ViewBag.disbursementListInfo = dl;
            ViewBag.Representative = deptSvc.FindEmployeeById((int)dl.Department.RepresentativeId);
            return View(dl);
        }
        //Seq Diagram Done
        public ActionResult SearchDisbursements(int? id, String status)
        { 
            ViewBag.Departments = deptSvc.ListAllDepartments();

            return View("ViewDisbursements", disbursementSvc.GetDisbursementsBySearchCriteria(id, status));
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult UpdateDisbursement(int disbursementListId, string[] itemNo, int[] originalPreparedQty, int[] adjustedQuantity, string[] remarks)
        {
            inventorySvc.UpdateDisbursementListDetails(disbursementListId, itemNo, originalPreparedQty, adjustedQuantity, remarks);
            return RedirectToAction("ViewDisbursements");
        }

        public ActionResult UpdateDateOfDelivery(string deliveryDateString, int disbursementListId)
        {
            DateTime deliveryDate = utilSvc.GetDateTimeFromPicker(deliveryDateString);
            inventorySvc.UpdateDisbursementDate(deliveryDate, disbursementListId);
            return RedirectToAction("ViewDisbursement/" + disbursementListId);
        }
        // ********************* MAINTAIN *******************
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult SupplierList()
        {
            return View(supplierAndPOSvc.GetAllSuppliers());
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult Supplier(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = supplierAndPOSvc.FindSupplierById(id);
            List<Inventory> listOfItemsFromSupplier = supplierAndPOSvc.FindInventoryItemsBySupplier(id);
            ViewBag.SupplierItems = listOfItemsFromSupplier;
            ViewBag.SupplierId = supplier.SupplierId;
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Supplier([Bind(Include = "SupplierId,SupplierCode,SupplierName,ContactName,PhNo,FaxNo,Address,GstRegistrationNo")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {

                supplierAndPOSvc.UpdateSupplier(supplier);
                return RedirectToAction("SupplierList");
            }
            return View("Supplier",supplier);
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        public ActionResult AddSupplier()
        {
            return View("Supplier");
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "InventoryManagement")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSupplier([Bind(Include = "SupplierId,SupplierCode,SupplierName,ContactName,PhNo,FaxNo,Address,GstRegistrationNo")] Supplier supplier)
        {
                supplierAndPOSvc.AddNewSupplier(supplier);
                return RedirectToAction("SupplierList");
        }


        //****************** Outstanding Requisitions ***************
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult ViewRequisitions()
        {
            RetrievalList rList = inventorySvc.GetRetrievalList();
            ViewBag.rList = rList;
            return View(inventorySvc.GetOutStandingRequisitions());
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult GenerateRetrievalList()
        {   
            inventorySvc.PopulateRetrievalList();
            inventorySvc.PopulateRetrievalListItems();
            return RedirectToAction("ViewRequisitions");
        }
        //Seq Diagram Done  + Design Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult ClearRetrievalList()
        {
            inventorySvc.ClearRetrievalList();
            return RedirectToAction("ViewRequisitions");
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult DisburseItems(string disbursementDateString)
        {
            DateTime deliveryDate = utilSvc.GetDateTimeFromPicker(disbursementDateString);
            try
            {
                inventorySvc.AutoAllocateDisbursementsByOrderOfRequisition(deliveryDate);
            }
            catch (InventoryAndDisbursementUpdateException e)
            {
                TempData["doc"] = e;
                return RedirectToAction("RetrievalList");
            }
            return RedirectToAction("ReallocateDisbursements");
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult ReallocateDisbursements()
        {
            List<DisbursementDetail> reallocationList = inventorySvc.GenerateListForManualAllocation();
            DisbursementListComparer comparer = new DisbursementListComparer(); //sort by item no
            reallocationList.Sort(comparer);
            int currentRetrievalListId = inventorySvc.GetLastRetrievalListId();
            List<Requisition> summedListByDepartment = inventorySvc.GetRequisitionsSummedByDept(currentRetrievalListId);
            ViewBag.MaxQuantityOfEachItem = summedListByDepartment;
            if (TempData["PrepQtyException"] != null)
            {
                ViewBag.PrepQtyException = TempData["PrepQtyException"].ToString();
            }  

            return View(reallocationList);
        }
        //Seq Diagram Done
        [AuthorisePermissions(Permission = "Disbursement")]
        public ActionResult Reallocate(int[] departmentId, int[] preparedQuantity,int [] disbursementListId, int[] disbursementDetailId, string[] itemNo, int[] adjustedQuantity)
        {
            try
            {
                inventorySvc.ManuallyAllocateDisbursements(departmentId, preparedQuantity, adjustedQuantity, disbursementListId, disbursementDetailId, itemNo);
            }
            catch (InventoryAndDisbursementUpdateException e)
            {
                TempData["PrepQtyException"] = e;
            }

            return RedirectToAction("ReallocateDisbursements");
        }
    }
}