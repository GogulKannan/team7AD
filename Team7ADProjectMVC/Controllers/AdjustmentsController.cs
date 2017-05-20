using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Team7ADProjectMVC.Models;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author : Chunxiao
    public class AdjustmentsController : Controller
    {
        private IInventoryAdjustmentService ivadjustsvc;
        private IDepartmentService deptSvc;
        private IInventoryService invSvc;
        private UtilityService uSvc;

        public AdjustmentsController()
        {
            ivadjustsvc = new InventoryAdjustmentService();
            deptSvc = new DepartmentService();
            invSvc = new InventoryService();
            uSvc = new UtilityService();
        }

        // GET: Adjustments
        [AuthorisePermissions(Permission = "MakeAdjustment,ApproveAdjustment")]
        public ActionResult ViewAdjustment(int? page)
        {
            Employee user = (Employee)Session["user"];

            string role = ivadjustsvc.findRolebyUserID(user.EmployeeId);
            List<Employee> empList = deptSvc.GetEverySingleEmployeeInDepartment(user.DepartmentId);
            ViewBag.employee = new SelectList(empList, "EmployeeId", "EmployeeName");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (role == "Store Supervisor")
            {
                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Approval"},
                    new SelectListItem {Text ="Approved" },
                    new SelectListItem {Text ="Rejected" },
                };

                ViewBag.status = statuslist;
                var adjustmentlist = ivadjustsvc.findSupervisorAdjustmentList();
                TempData["list"] = adjustmentlist;
                return View(adjustmentlist.ToPagedList(pageNumber, pageSize));
            }

            if (role == "Store Manager")
            {
                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Final Approval" },
                    new SelectListItem {Text ="Approved"},
                    new SelectListItem {Text ="Rejected"},
                };

                ViewBag.status = statuslist;
                var adjustmentlist = ivadjustsvc.findManagerAdjustmentList();
                TempData["list"] = adjustmentlist;
                return View(adjustmentlist.ToPagedList(pageNumber, pageSize));
            }
            if (role == "Store Clerk")
            {

                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Approval"},
                    new SelectListItem {Text ="Pending Final Approval" },
                    new SelectListItem {Text ="Approved"},
                    new SelectListItem {Text ="Rejected"},

                };

                ViewBag.status = statuslist;
                var adjustmentlist = ivadjustsvc.findClerkAdjustmentList();
                TempData["list"] = adjustmentlist;
                return View(adjustmentlist.ToPagedList(pageNumber, pageSize));

            }



            return View();
        }

        [AuthorisePermissions(Permission = "MakeAdjustment,ApproveAdjustment")]
        public ActionResult SearchAdjustment(string employee, string status, string date, int? page)
        {

            Employee user = ((Employee)Session["user"]);

            string role = ivadjustsvc.findRolebyUserID(user.EmployeeId);
            List<Employee> empList = deptSvc.GetEverySingleEmployeeInDepartment(user.DepartmentId);
            ViewBag.employee = new SelectList(empList, "EmployeeId", "EmployeeName");
            var adjustmentlist = (List<Adjustment>)TempData.Peek("list");
            var result = ivadjustsvc.FindAdjustmentBySearch(adjustmentlist, employee, status, date);
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (role == "Store Supervisor")
            {
                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Approval"},
                    new SelectListItem {Text ="Approved" },
                    new SelectListItem {Text ="Rejected" },
                };

                ViewBag.status = statuslist;
                return View("ViewAdjustment", result.ToPagedList(pageNumber, pageSize));
            }

            if (role == "Store Manager")
            {
                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Final Approval" },
                    new SelectListItem {Text ="Approved"},
                    new SelectListItem {Text ="Rejected"},
                };

                ViewBag.status = statuslist;
                return View("ViewAdjustment", result.ToPagedList(pageNumber, pageSize));
            }
            if (role == "Store Clerk")
            {
                List<SelectListItem> statuslist = new List<SelectListItem>()
                {
                    new SelectListItem {Text ="Pending Approval"},
                    new SelectListItem {Text ="Pending Final Approval" },
                    new SelectListItem {Text ="Approved"},
                    new SelectListItem {Text ="Rejected"},

                };

                ViewBag.status = statuslist;
                return View("ViewAdjustment", result.ToPagedList(pageNumber, pageSize));

            }
            return View();


        }

        [AuthorisePermissions(Permission = "MakeAdjustment,ApproveAdjustment")]
        public ActionResult ViewAdjustmentDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Adjustment adjustment = ivadjustsvc.findAdjustmentByID(id);
            if (adjustment == null)
            {
                return HttpNotFound();
            }
            List<AdjustmentDetail> dtlist = ivadjustsvc.findDetailByAdjustment(adjustment);
            decimal? total = ivadjustsvc.caculateTotal(dtlist);
            ViewBag.Adjid = id;
            ViewBag.status = ivadjustsvc.findAdjustmentStatus(id);
            ViewBag.sum = total;
            return View(dtlist);
        }

        [AuthorisePermissions(Permission = "ApproveAdjustment")]
        [HttpPost, ActionName("ViewAdjustmentDetail")]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveAdjustment(int id, string result)
        {
            Employee user = (Employee)Session["user"];
            string role = ivadjustsvc.findRolebyUserID(user.EmployeeId);
            if (role == "Store Supervisor")
            {
                if (result == "Approve")
                {
                    ivadjustsvc.ApproveBySupervisor(user.EmployeeId, id);

                    return RedirectToAction("ViewAdjustment");
                }
                else if (result == "Reject")
                {
                    ivadjustsvc.RejecteBySupervisor(user.EmployeeId, id);
                    return RedirectToAction("ViewAdjustment");
                }
                else
                {
                    ivadjustsvc.PendingBySupervisor(user.EmployeeId, id);
                    try //email to notify manager of approval
                    {
                        List<Employee> storeManagement = deptSvc.GetStoreManagerAndSupervisor();
                        string emailBody = storeManagement.Where(x => x.RoleId == 6).First().EmployeeName + ", you have a new pending inventory adjustment for approval. Please go to http://" + uSvc.GetBaseUrl() + "/Adjustments/ViewAdjustmentDetail/" + id + " to approve the adjustment.";
                        uSvc.SendEmail(new List<string>(new string[] { storeManagement.Where(x => x.RoleId == 6).First().Email }), "New Inventory Adjustment Pending Approval", emailBody);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }

                    return RedirectToAction("ViewAdjustment");
                }

            }
            if (role == "Store Manager")
            {
                if (result == "Approve")
                {
                    ivadjustsvc.ApproveByManager(user .EmployeeId, id);
                    return RedirectToAction("ViewAdjustment");

                }
                if (result == "Reject")
                {
                    ivadjustsvc.RejectByManager(user.EmployeeId, id);
                    return RedirectToAction("ViewAdjustment");
                }

            }
            return View();
        }


        [AuthorisePermissions(Permission = "MakeAdjustment")]
        [HttpGet]
        public ActionResult Create()                          //create new adjustment
        {
            Employee currentEmployee = (Employee)Session["User"];
            var adjust = new Adjustment
            {
                AdjustmentDate = DateTime.Today,
                EmployeeId = currentEmployee.EmployeeId,
                Status = Convert.ToString("Pending Approval")
            };
            var adjustdetail = new AdjustmentDetail();
            ViewBag.ItemNo = new SelectList(invSvc.GetAllInventory(), "ItemNo", "Description");

            adjust.AdjustmentDetails.Add(adjustdetail);
            return View(adjust);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorisePermissions(Permission = "MakeAdjustment")]
        public ActionResult Create([Bind] Adjustment adjust)
        {
            if (ModelState.IsValid)
            {
                if (!ivadjustsvc.IsValidAdjustment(adjust))//check if adjustment quantity entered greater than inventory quantity 
                {
                    ViewBag.ErrorMsg = "Inventory quantity cannot be less than 0. Please check quantity for adjustments.";
                    ViewBag.ItemNo = new SelectList(invSvc.GetAllInventory(), "ItemNo", "Description");
                    return View(adjust);
                }
                ivadjustsvc.createAdjustment(adjust);
                try //email to notify supervisor of approval
                {
                    List<Employee> storeManagement = deptSvc.GetStoreManagerAndSupervisor();
                    string emailBody = storeManagement.Where(x => x.RoleId == 5).First().EmployeeName + ", you have a new pending inventory adjustment for approval. Please go to http://" + uSvc.GetBaseUrl() + "/Adjustments/ViewAdjustmentDetail/" + adjust.AdjustmentId + " to approve the adjustment.";
                    uSvc.SendEmail(new List<string>(new string[] { storeManagement.Where(x => x.RoleId == 5).First().Email }), "New Inventory Adjustment Pending Approval", emailBody);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                return RedirectToAction("ViewAdjustment");
            }

            ViewBag.ItemNo = new SelectList(invSvc.GetAllInventory(), "ItemNo", "Description");
            return View(adjust);
        }


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [AuthorisePermissions(Permission = "MakeAdjustment")]
        public ActionResult AddDetail()      //click 'Add new item button' to add new adjustment details
        {
            Adjustment currentAdjustment = (Adjustment)Session["adjustment"];
            Session["adjustment"] = new Adjustment();
            var adjustdetail = new AdjustmentDetail();
            ViewBag.ItemNo = new SelectList(invSvc.GetAllInventory(), "ItemNo", "Description");
            currentAdjustment.AdjustmentDetails.Add(adjustdetail);
            return View(currentAdjustment);
        }

    }
}

