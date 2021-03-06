//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Team7ADProjectMVC
{
    using System;
    using System.Collections.Generic;
    
    public partial class Adjustment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Adjustment()
        {
            this.AdjustmentDetails = new HashSet<AdjustmentDetail>();
        }
    
        public int AdjustmentId { get; set; }
        public Nullable<System.DateTime> AdjustmentDate { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<int> SupervisorId { get; set; }
        public Nullable<System.DateTime> SupervisorAuthorizedDate { get; set; }
        public Nullable<int> HeadId { get; set; }
        public Nullable<System.DateTime> HeadAuthorizedDate { get; set; }
        public string Status { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AdjustmentDetail> AdjustmentDetails { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
