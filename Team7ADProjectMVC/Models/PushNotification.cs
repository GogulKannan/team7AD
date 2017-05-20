using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Team7ADProjectMVC.Services;

namespace Team7ADProjectMVC.Models
{

    //Author: Gogul / Linda
    public class PushNotification
    {
        //IDepartmentService deptSvc;
        ProjectEntities db = new ProjectEntities();
        UtilityService uSvc = new UtilityService();
        public PushNotification()
        {
            // TODO: Add constructor logic here
            //deptSvc = new DepartmentService();
        }

        public bool Successful
        {
            get;
            set;
        }

        public string Response
        {
            get;
            set;
        }
        public Exception Error
        {
            get;
            set;
        }

        public PushNotification PushFCMNotification(string title, string message, string token, List<String> myData)
        {

            PushNotification result = new PushNotification();
            try
            {
                result.Successful = true;
                result.Error = null;

                string SERVER_API_KEY = "AAAAjD8Iv20:APA91bG3BW0rQSG9WRbpf0SCSboeMOlwm9xyTZF3AsPNbj97wlM7resjGzdjUUQuhvytRdWsvoEKcwq4vKqMeM2uBQRLBj84tWxSWeX87XV1-p_DRBtBUrlxvt_Qq1tDrwFDo_9a9A5t";
                var SENDER_ID = "602352959341";
                WebRequest tRequest;
                tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

                tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

                var data = new
                     {
                         //single device
                         to = token,

                         notification = new
                          {
                              title = title,
                              body = message,
                          },
                         data = new
                          {
                              intent = myData[0],
                              pageHeader = myData[1],
                              id = myData[2],
                              extraDetail = myData[3],
                          }
                     };

                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.ContentLength = byteArray.Length;

                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse tResponse = tRequest.GetResponse();

                dataStream = tResponse.GetResponseStream();

                StreamReader tReader = new StreamReader(dataStream);

                String sResponseFromServer = tReader.ReadToEnd();
                result.Response = sResponseFromServer;

                tReader.Close();
                dataStream.Close();
                tResponse.Close();
            }
            catch (Exception e)
            {
                result.Successful = false;
                result.Response = null;
                result.Error = e;
            }
            return result;
        }

        public void PushFCMNotificationToStoreClerk(string title, string message, List<String> myData)
        {
            List<Employee> Allemp = db.Employees.Where(p => p.RoleId == 1).ToList();

            foreach (Employee e in Allemp)
            {
                if (e.Token == null)
                {
                    Notification n = new Notification();
                    n.EmployeeId = e.EmployeeId;
                    n.Title = title;
                    n.Body = message;
                    n.Intent = myData[0];
                    n.PageHeader = myData[1];
                    n.PageId = myData[2];
                    n.ExtraDetail = myData[3];
                    db.Notifications.Add(n);
                    db.SaveChanges();

                }
                else
                {
                    PushFCMNotification(title, message, e.Token, myData);
                }
            }
        }

        public void CheckForStockReorder()
        {
            var checkForStockReorder = (from x in db.Inventories
                                        where x.Quantity < x.ReorderLevel
                                        select x).ToList();

            List<String> myData = new List<string>();
            myData.Add("StockCard");
            myData.Add("StockCard");
            myData.Add("0");
            myData.Add("0");

            if (checkForStockReorder != null)
            {
                PushFCMNotificationToStoreClerk("Low Stock Alert", "Please review stock cards.", myData);
                try //Email to all clerks for low stocks
                {
                    List<string> clerkEmails = new List<string>();
                    List<Employee> clerks = db.Employees.Where(x => x.Role.Name == "Store Clerk").ToList();
                    foreach(Employee emp in clerks)
                    {
                        clerkEmails.Add(emp.Email);
                    }
                    string emailBody = "There are stocks which are running low. Please go to http://localhost:23130/StorePO/GeneratePO to generate PO for stocks which are running low.";
                    uSvc.SendEmail(clerkEmails, "Stocks Running Low", emailBody);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        public void CollectionPointChanged(int deptid)
        {

            Department wcfItem = db.Departments.Where(p => p.DepartmentId == deptid).First();
            String cpointName = wcfItem.CollectionPoint.PlaceName;
            String deptname = wcfItem.DepartmentName;
            List<String> myData = new List<string>();
            myData.Add("DisbursementList");
            myData.Add(deptname);
            myData.Add(cpointName);
            myData.Add("0");

            PushFCMNotificationToStoreClerk(deptname + " Collection", "Changed to: " + cpointName, myData);
        }

        public void RepAcceptRequisition(String DisListID)
        {
            int dlid = Convert.ToInt32(DisListID);
            DisbursementList wcfItem = db.DisbursementLists.Where(p => p.DisbursementListId == dlid).First();
            string deptName = wcfItem.Department.DepartmentName;
            List<String> myData = new List<string>();
            myData.Add("reqaccepted");
            myData.Add(deptName);
            myData.Add("0");
            myData.Add("0");
            String title = deptName;
            String message = "Accepted Disbursement";

            PushFCMNotificationToStoreClerk(title, message, myData);
        }

        public void NewRequisitonMade(string reqListID)
        {
            int reqID = Convert.ToInt32(reqListID);
            Requisition wcfItem = db.Requisitions.Where(p => p.RequisitionId == reqID).First();
            string deptName = wcfItem.Employee.Department.DepartmentName;
            List<String> myData = new List<string>();
            myData.Add("UnfulfilledRequisitions");
            myData.Add("UnfulfilledRequisitions List");
            myData.Add("0");
            myData.Add("0");

            PushFCMNotificationToStoreClerk("New Requisition", "From: " + deptName, myData);
        }

        public void NotificationForHeadOnCreate(String EmpID)
        {
            String  title="New Requisition";
            int eid = Convert.ToInt32(EmpID);
            Employee wcfItem = db.Employees.Where(p => p.EmployeeId == eid).First();
            int deptId = (int)wcfItem.DepartmentId;
            Employee head = db.Employees.Where(W => W.DepartmentId == deptId).Where(x => x.RoleId == 2).First();
            string empName = wcfItem.EmployeeName;
            string token = head.Token;
            List<String> myData = new List<string>();
            myData.Add("ApproveRequisition");
            myData.Add("Approve Requisition");
            myData.Add("0");
            myData.Add("0");
            String message="From: " + empName;

            List<Employee> Allemp = db.Employees.Where(p => p.RoleId == 2).Where(p => p.DepartmentId == deptId).ToList();
            foreach (Employee e in Allemp)
            {
                if (e.Token == null)
                {
                    Notification n = new Notification();
                    n.EmployeeId = e.EmployeeId;
                    n.Title = title;
                    n.Body = message;
                    n.Intent = myData[0];
                    n.PageHeader = myData[1];
                    n.PageId = myData[2];
                    n.ExtraDetail = myData[3];
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
                else
                {
                    PushFCMNotification(title, message, e.Token, myData);
                }
            }
        }

        public void PushNotificationForRep(string title, string message, List<String> myData,int deptID)
        {
            List<Employee> Allemp = db.Employees.Where(p => p.RoleId == 4).Where(p => p.DepartmentId == deptID).ToList();
            foreach (Employee e in Allemp)
            {
                if (e.Token == null)
                {
                    Notification n = new Notification();
                    n.EmployeeId = e.EmployeeId;
                    n.Title = title;
                    n.Body = message;
                    n.Intent = myData[0];
                    n.PageHeader = myData[1];
                    n.PageId = myData[2];
                    n.ExtraDetail = myData[3];
                    db.Notifications.Add(n);
                    db.SaveChanges();
                }
                else
                {
                    PushFCMNotification(title, message, e.Token, myData);
                }
            }
        }
    }
}