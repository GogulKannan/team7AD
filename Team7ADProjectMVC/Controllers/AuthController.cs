using System;
using System.Web.Mvc;
using System.Web.Security;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Controllers
{
    //Author : Zhan Seng
    public class AuthController : Controller
    {
        IDepartmentService deptSvc;
        public AuthController()
        {
            deptSvc = new DepartmentService();
        }

        // GET: Auth
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                String redirectUrl="";             
                int userId = Int32.Parse(User.Identity.Name);
                Employee emp = deptSvc.FindById(userId);
                Session["user"] = emp;
                

                if (emp.Role.Name == "Store Clerk" || emp.Role.Name == "Store Representative" || emp.Role.Name == "Store Supervisor")
                {
                    redirectUrl="~/Store/ViewRequisitions";
                }
                if (emp.Role.Name == "Department Head" || emp.Role.Name == "Store Manager")
                {
                    redirectUrl="~/Head/ApproveRequisition";
                }
                if (emp.Role.Name == "Employee" || emp.Role.Name == "Representative")
                {
                    redirectUrl= "~/Stationery/EmployeeRequisition";
                }
                if (deptSvc.IsDelegate(emp))
                {
                    deptSvc.SetDelegatePermissions(emp);
                    Session["user"] = emp;
                    //return Redirect(Url.Content("~/Head/ApproveRequisition")); //If delegated, do not redirect to Make Requisition use case
                    redirectUrl = "~/Head/ApproveRequisition";
                }

                if (Session["ReturnUrl"] != null)
                {
                    redirectUrl = "~" + Session["ReturnUrl"].ToString();
                    Session.Remove("ReturnUrl");
                }
                return Redirect(Url.Content(redirectUrl));
            }

            else return Redirect(Url.Content("~/Login.aspx"));

        }

        public ActionResult Logout()
        {
            Session["user"] = new Employee();
            FormsAuthentication.SignOut();
            return Redirect(Url.Content("~/Login.aspx"));
        }

    }
}