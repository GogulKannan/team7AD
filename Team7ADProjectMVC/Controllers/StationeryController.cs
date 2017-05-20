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
    public class StationeryController : Controller
    {
        IInventoryService invService;
        IDepartmentService deptService;
        IRequisitionService reqService;
        IUtilityService uSvc;
        PushNotification pushSvc;
        public StationeryController()
        {
            invService = new InventoryService();
            deptService = new DepartmentService();
            reqService = new RequisitionService();
            uSvc = new UtilityService();
            pushSvc = new PushNotification();
        }
        // GET: Stationery
        [AuthorisePermissions(Permission = "ViewRequisition")]
        public ActionResult DepartmentRequisitions(int? page, int? employeeId, string dateOrderedString, string status)
        {
            Employee currentEmployee = (Employee)Session["User"];
            ViewBag.Employees = deptService.GetEverySingleEmployeeInDepartment(currentEmployee.DepartmentId);
            bool canCreate = true;

            if (currentEmployee.Role.Name == "Department Head")
            {
                canCreate = false;
            }
            else
            {
                canCreate = !(deptService.IsDelegate(currentEmployee));
            }

            ViewBag.canCreate = canCreate;

            List<Requisition> resultList = reqService.ListAllRequisitionByDept(currentEmployee.DepartmentId);

            if(employeeId != null)
            {
                resultList.RemoveAll(x => x.EmployeeId != employeeId);
            }
            if (dateOrderedString != null && dateOrderedString.Length >1)
            {
                DateTime dateOrdered = uSvc.GetDateTimeFromPicker(dateOrderedString);
                resultList.RemoveAll(x => x.OrderedDate != dateOrdered);
            }
            if (status != null && status.Length > 2)
            {
                resultList.RemoveAll(x => x.RequisitionStatus != status);
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(resultList.ToPagedList(pageNumber, pageSize));
        }

        [AuthorisePermissions(Permission = "MakeRequisition")]
        public ActionResult EmployeeRequisition()
        {
            Employee currentEmployee = (Employee)Session["User"];
            var requisition = new Requisition
            {
                OrderedDate = DateTime.Today,
                EmployeeId = currentEmployee.EmployeeId,
                DepartmentId = currentEmployee.DepartmentId,
                RequisitionStatus = Convert.ToString("Pending Approval")
            };
            var requisitionDetail = new RequisitionDetail();
            ViewBag.ItemNo = new SelectList(invService.GetAllInventory(), "ItemNo", "Description");

            requisition.RequisitionDetails.Add(requisitionDetail);
            return View(requisition);
        }

        [AuthorisePermissions(Permission = "MakeRequisition")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeRequisition([Bind] Requisition requisition)
        {
            Employee currentEmployee = (Employee)Session["User"];
            
            try
            {
                if (ModelState.IsValid)
                {
                    var q = (from x in requisition.RequisitionDetails
                            orderby x.ItemNo
                            select x).ToList();

                    for (int i = 0; i< q.Count()-1; i++)
                    {
                        if(q[i].ItemNo == q[i+1].ItemNo)
                        {
                            throw new RequisitionAndPOCreationException("Please ensure the items are not duplicated.");
                        }

                    }
                    if(requisition.RequisitionDetails == null || requisition.RequisitionDetails.Count == 0)
                    {
                        throw new RequisitionAndPOCreationException("Please ensure there are items added in the requisition.");
                    }

                    foreach (var item in requisition.RequisitionDetails)
                    {
                        if(item.Quantity ==null || item.Quantity <=0)
                        {
                            throw new RequisitionAndPOCreationException("Please ensure the item quantity is greater than zero.");
                        }
                        item.OutstandingQuantity = item.Quantity;
                    }
                    
                    reqService.CreateRequisition(requisition, currentEmployee.EmployeeId);
                    pushSvc.NotificationForHeadOnCreate(currentEmployee.EmployeeId.ToString());

                    try
                    {
                        string emailBody = requisition.Employee.Department.Head.EmployeeName + ", You have a pending requisition from " + requisition.Employee.EmployeeName + ". Please go to http://" + uSvc.GetBaseUrl() + "/Head/EmployeeRequisition/" + requisition.RequisitionId+" to approve the request.";
                        Delegate delegateRecord = deptService.getDelegatedEmployee(requisition.DepartmentId);
                        if (delegateRecord != null) //if there is a delegate for the department, email will be addressed to the delegate and CC to the head.
                        {
                            emailBody = delegateRecord.Employee.EmployeeName + ", You have a pending requisition from " + requisition.Employee.EmployeeName + ". Please go to http://" + uSvc.GetBaseUrl() + "/Head/EmployeeRequisition/" + requisition.RequisitionId + " to approve the request.";
                            uSvc.SendEmail(new List<string>(new string[] { delegateRecord.Employee.Email }), "New Requisition Pending Approval", emailBody, new List<string>(new string[] { requisition.Employee.Department.Head.Email }));
                        } 
                        else uSvc.SendEmail(new List<string>(new string[] { requisition.Employee.Department.Head.Email }), "New Requisition Pending Approval", emailBody);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                    return RedirectToAction("DepartmentRequisitions");
                }
            }
            catch (RequisitionAndPOCreationException e)
            {
                ViewBag.Error = e.Message.ToString();
            }
            
            ViewBag.ItemNo = new SelectList(invService.GetAllInventory(), "ItemNo", "Description");
            return View(requisition);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [AuthorisePermissions(Permission = "MakeRequisition")]
        public ActionResult AddDetail()
        {
            Requisition currentRequisition = (Requisition)Session["requisition"];
            Session["requisition"] = new Requisition();
            var requisitionDetail = new RequisitionDetail();
            ViewBag.ItemNo = new SelectList(invService.GetAllInventory(), "ItemNo", "Description");
            currentRequisition.RequisitionDetails.Add(requisitionDetail);
            return View(currentRequisition);
        }

        [AuthorisePermissions(Permission = "ViewRequisition")]
        public ActionResult Requisition(int id)
        {
            return View(reqService.FindById(id));
        }
    }
}