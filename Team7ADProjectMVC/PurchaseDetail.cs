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
    
    public partial class PurchaseDetail
    {
        public int PurchaseDetailId { get; set; }
        public Nullable<int> PurchaseOrderId { get; set; }
        public string ItemNo { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> SupplierId { get; set; }
    
        public virtual Inventory Inventory { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
