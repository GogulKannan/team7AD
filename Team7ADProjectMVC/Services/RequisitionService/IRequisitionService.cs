using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao && Sandi
    public interface IRequisitionService
    {
        List<Requisition> ListAllRequisitionByDept(int? deptId);
        List<Requisition> ListAllRequisition();
        List<Requisition> GetAllPendingRequisitionByDept(int? depId);       
        Requisition FindById(int? requisitionId);    
        void UpdateApproveStatus(Requisition requisition,string comments,int? approvedbyId);
        void UpdateRejectStatus(Requisition requisition, string comments, int? approvedbyId);
        List<Requisition> getDataForPagination(string searchString);
        List<RequisitionDetail> GetAllRequisitionDetails(int dId, int rId);
        List<RequisitionDetail> GetAllRequisitionDetails();
        void CreateRequisition(Requisition r);
        void UpdateRequisition(Requisition requisition, Requisition req, int idd, int eid, int? deid);
        string CreateRequisition(Requisition requisition, int employeeId);
    }
}