using System;
using System.Linq;
using System.Web.Mvc;
using Team7ADProjectMVC.Models;
using PagedList;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author: Sandi

    public class HeadController : Controller
    {

        private IRequisitionService reqsvc;
        public static int count = 0;
        private IDepartmentService depsvc;

  

        Employee user;
        int? depIdofLoginUser;
        int? depHeadId;


        public HeadController()
        {
            reqsvc = new RequisitionService();
            depsvc = new DepartmentService();
        }

        //----------------------------View/Approve/Reject Requisition Part----------------------------------------start here
        [AuthorisePermissions(Permission = "ApproveRequisition")]
        public ActionResult ApproveRequisition(string currentFilter, string searchString, int? page)
        {

            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;
            
            var requisitions = reqsvc.GetAllPendingRequisitionByDept(depIdofLoginUser);


            if (searchString != null)

                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;

                int pageSize = 3;
                int pageNumber = (page ?? 1);

                if (!String.IsNullOrEmpty(searchString))
                {

                    var q = reqsvc.getDataForPagination(searchString);
                    requisitions = q.ToList();
                }

                Employee userName = (Employee)Session["User"];

                requisitions.RemoveAll(x => x.DepartmentId != userName.DepartmentId);
                requisitions.RemoveAll(x => x.RequisitionStatus != "Pending Approval");
                ViewBag.req = requisitions.ToList();

                return View("ListAllRequisition", requisitions.ToPagedList(pageNumber, pageSize));
  

        }



        [AuthorisePermissions(Permission = "ApproveRequisition")]
        public ActionResult EmployeeRequisition(int? id)
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Requisition r = reqsvc.FindById(id);//Retrieve the selected requisition by requisition id
            if (r == null)
            {
                return HttpNotFound();
            }
            ViewBag.r = r;

            return View("Approve", r);
        }

        [AuthorisePermissions(Permission = "ApproveRequisition")]
        public ActionResult MarkAsCollected(int? rid, string textcomments, string status)
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Requisition r = reqsvc.FindById(rid);

            if (textcomments == null || textcomments.Length < 1)
            {
                textcomments = "N/A";

            }
            if (status.Equals("Approve"))
            {
                reqsvc.UpdateApproveStatus(r, textcomments,depHeadId);

                return RedirectToAction("ApproveRequisition");
            }
            else
            {
                reqsvc.UpdateRejectStatus(r, textcomments, depHeadId);
               
                return RedirectToAction("ApproveRequisition");
            }

        }

        //----------------------------View/Approve/Reject Requisition Part-------------------------end here


        //----------------------------Delegation Part----------------------------------------------start here
        [AuthorisePermissions(Permission = "DelegateRole")]
        public ActionResult show()
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Delegate delegatedEmployee = depsvc.getDelegatedEmployee(depIdofLoginUser);//Check delegate database whether there is delegated employee or not

            if (delegatedEmployee == null)
            {
                string[] startdate = DateTime.Today.ToString().Split(' ');
                string[] enddate = DateTime.Today.AddDays(7).ToString().Split(' ');
                string[] sd = startdate[0].Split('/');
                string[] ed = enddate[0].Split('/');

                ViewBag.autoStartdate = sd[1] + "/" + sd[0] + "/" + sd[2];
                ViewBag.autoEnddate = ed[1] + "/" + ed[0] + "/" + ed[2];
                ViewBag.empList = depsvc.GetAllEmployeebyDepId(depIdofLoginUser);
                ViewBag.endDate = DateTime.Today.AddDays(7);//to auto populate the end date 7 days after the start date(current date) when the page is load
                return View("DelegateRole");

            }
            return RedirectToAction("fill");//If there is someone already delegated,to show the details 

        }

        [AuthorisePermissions(Permission = "DelegateRole")]
        public ActionResult ManageDelegation(int? empId, string status, string startDate, string endDate, int? DelegateId)

        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Employee emp = depsvc.FindById(empId);
            Delegate d = depsvc.FinddelegaterecordById(DelegateId);


            if (status.Equals("Delegate"))
            {


                if (startDate.Equals("") && !(endDate.Equals("")))
                {
                    String[] e = endDate.Split('/');
                    DateTime edate = new DateTime(Int32.Parse(e[2]), Int32.Parse(e[1]), Int32.Parse(e[0]));
                    DateTime sdate = DateTime.Today;

                    depsvc.manageDelegate(emp, sdate, edate, depHeadId);
                    return RedirectToAction("fill");//to populate data after processing to the database
                }
                else if (startDate.Equals("") && (endDate.Equals("")))
                {
                    DateTime edate = DateTime.Today;
                    DateTime sdate = DateTime.Today;

                    depsvc.manageDelegate(emp, sdate, edate, depHeadId);
                    return RedirectToAction("fill");
                }
                else
                {
                    String[] s = startDate.Split('/');
                    DateTime sdate = new DateTime(Int32.Parse(s[2]), Int32.Parse(s[1]), Int32.Parse(s[0]));

                    String[] e = endDate.Split('/');
                    DateTime edate = new DateTime(Int32.Parse(e[2]), Int32.Parse(e[1]), Int32.Parse(e[0]));

                    depsvc.manageDelegate(emp, sdate, edate, depHeadId);

                    return RedirectToAction("fill");
                }

            }
            //update-----------------------------------------------------------------------------------------------------------------------  
            else if (status.Equals("Update"))
            {
                if (startDate.Equals("") && !(endDate.Equals("")))//if click  button after updating only enddate
                {
                    String[] e = endDate.Split('/');
                    DateTime edate = new DateTime(Int32.Parse(e[2]), Int32.Parse(e[1]), Int32.Parse(e[0]));
                    ViewBag.s1 = d.StartDate;

                    depsvc.updateDelegate( d, ViewBag.s1, edate, depHeadId);

                    return RedirectToAction("show");
                }
                else if (endDate.Equals("") && !(startDate.Equals("")))//if click  button after updating only start date
                {
                    String[] s = startDate.Split('/');
                    DateTime sdate = new DateTime(Int32.Parse(s[2]), Int32.Parse(s[1]), Int32.Parse(s[0]));
                    ViewBag.e1 = d.EndDate;

                    depsvc.updateDelegate( d, sdate, ViewBag.e1, depHeadId);

                    return RedirectToAction("show");
                }
                else if (endDate.Equals("") && (startDate.Equals("")))//if click  button without updating new start date and end date
                {
                    ViewBag.s1 = d.StartDate;
                    ViewBag.e1 = d.EndDate;

                    depsvc.updateDelegate( d, ViewBag.s1, ViewBag.e1, depHeadId);

                    return RedirectToAction("show");
                }
                else
                {
                    String[] s = startDate.Split('/');
                    DateTime sdate = new DateTime(Int32.Parse(s[2]), Int32.Parse(s[1]), Int32.Parse(s[0]));
                    String[] e = endDate.Split('/');
                    DateTime edate = new DateTime(Int32.Parse(e[2]), Int32.Parse(e[1]), Int32.Parse(e[0]));
                    depsvc.updateDelegate(d, sdate, edate, depHeadId);

                    return RedirectToAction("show");
                }
            }
            //terminate-----------------------------------------------------------------------------------------------------------
            depsvc.TerminateDelegate(d);
            return RedirectToAction("show");

        }

        [AuthorisePermissions(Permission = "DelegateRole")]
        public ActionResult fill()//to show the delegated data from database
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Delegate d = depsvc.getDelegatedEmployee(depIdofLoginUser);
            Employee e = depsvc.FindById(d.EmployeeId);

            string[] startdate = d.StartDate.ToString().Split(' ');
            string[] enddate = d.EndDate.ToString().Split(' ');
            string[] sd = startdate[0].Split('/');
            string[] ed = enddate[0].Split('/');

            ViewBag.s1 = sd[1] + "/" + sd[0] + "/" + sd[2];
            ViewBag.s2 = d.StartDate;
            ViewBag.e1 = ed[1] + "/" + ed[0] + "/" + ed[2];
            ViewBag.e2 = d.EndDate;

            ViewBag.delegateId = d.DelegateId;
            ViewBag.emp = e.EmployeeName;
            ViewBag.empid = e.EmployeeId;
            return View("Terminate");
        }



        //----------------------------Delegation Part----------------------------------end here
        [AuthorisePermissions(Permission = "ChangeRepresentative")]
        public ActionResult ChangeRepresentive()
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Employee currentRep = depsvc.GetCurrentRep(depIdofLoginUser);
            ViewBag.currentRep = currentRep;
            var emplist = depsvc.GetAllEmployee(depIdofLoginUser, currentRep.EmployeeId);
            ViewBag.employeeList = emplist.ToList();
            return View("ChangeDepartmentRepresentative", emplist);

        }

        [AuthorisePermissions(Permission = "ChangeRepresentative")]
        public ActionResult ManageCangeRep(int? empId)
        {
            user = (Employee)Session["user"];
            depIdofLoginUser = user.DepartmentId;
            depHeadId = user.EmployeeId;

            Employee currentRep = depsvc.GetCurrentRep(depIdofLoginUser);
            Employee newRep = depsvc.GetEmpbyId(empId);//find new rep
            depsvc.ChangeRep(currentRep, newRep);
            return RedirectToAction("ChangeRepresentive");

        }
    }
}
