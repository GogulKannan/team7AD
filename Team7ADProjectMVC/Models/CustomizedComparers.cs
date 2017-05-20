using System.Collections.Generic;

namespace Team7ADProjectMVC.Models
{

    //Author : Edwin
    public class CustomizedComparers : IComparer<Requisition>
    {
        public int Compare(Requisition x, Requisition y)
        {
            if (x.DepartmentId.Equals(y.DepartmentId))
            {
                return 0;
            }
            else if (x.DepartmentId < y.DepartmentId)
            {
                return -1;
            }
            else
            {
                return +1;
            }
        }
    }

    public class DisbursementListComparer : IComparer<DisbursementDetail>
    {
        public int Compare(DisbursementDetail x, DisbursementDetail y)
        {
            return (x.ItemNo.CompareTo(y.ItemNo));
        }
    }
}