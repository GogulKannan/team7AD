using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao
    interface IDisbursementService
    {
        List<DisbursementList> GetAllDisbursements();
        List<DisbursementList> GetDisbursementByDeptId(int? id);
        List<DisbursementList> GetDisbursementsBySearchCriteria(int? departmentId, String status);
        //Search for disbursements by department and status
        DisbursementList GetDisbursementById(int? id);
        void UpdateDisbursementList(DisbursementList disbursementList);
        List<DisbursementDetail> GetdisbursementdetailById(int? id);
        string findCpnameByDisburse(int? id);
        string findCptimeByDisburse(int? id);
        List<DisbursementList> FindDisbursementsBySearch(List<DisbursementList> disbursementlist,string date, string status);
        string findDisbursenmentStatus(int? id);
        void ConfirmDisbursement(int? id);
        List<DisbursementList> GetCollectionPointForDept(int dId);
        DisbursementDetail UpdateDisbursementStatus(int dId, int dId1, string remarks);
    }
}