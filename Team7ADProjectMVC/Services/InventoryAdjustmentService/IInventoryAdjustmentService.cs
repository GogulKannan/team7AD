using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team7ADProjectMVC.Services
{
    //Author : Chunxiao
    interface IInventoryAdjustmentService
    {
        string findRolebyUserID(int userid);
        List<Adjustment> findSupervisorAdjustmentList();
        List<Adjustment> findManagerAdjustmentList();
        List<Adjustment> findClerkAdjustmentList();
        List<Adjustment> FindAdjustmentBySearch(List<Adjustment> searchlist, string employee, string date, string status);
        Adjustment findAdjustmentByID(int? id);
        List<AdjustmentDetail> findDetailByAdjustment(Adjustment adjust);
        string findAdjustmentStatus(int? id);
        decimal? caculateTotal(List<AdjustmentDetail> adjdtlist);
        void ApproveBySupervisor(int? empid, int? adjid);
        void RejecteBySupervisor(int? empid, int? adjid);
        void ApproveByManager(int? empid, int? adjid);
        void RejectByManager(int? empid, int? adjid);
        void PendingBySupervisor(int? empid, int? adjid);
        void createAdjustment(Adjustment adjustment);
        bool IsValidAdjustment(Adjustment adj);

    }
}
