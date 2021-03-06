﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ProjectEntities : DbContext
    {
        public ProjectEntities()
            : base("name=ProjectEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Adjustment> Adjustments { get; set; }
        public virtual DbSet<AdjustmentDetail> AdjustmentDetails { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CollectionPoint> CollectionPoints { get; set; }
        public virtual DbSet<Delegate> Delegates { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<DeliveryDetail> DeliveryDetails { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DisbursementDetail> DisbursementDetails { get; set; }
        public virtual DbSet<DisbursementList> DisbursementLists { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<ItemCodeGenerator> ItemCodeGenerators { get; set; }
        public virtual DbSet<Measurement> Measurements { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<Requisition> Requisitions { get; set; }
        public virtual DbSet<RequisitionDetail> RequisitionDetails { get; set; }
        public virtual DbSet<Retrieval> Retrievals { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<disbAnalysi> disbAnalysis { get; set; }
        public virtual DbSet<StockCard> StockCards { get; set; }
    }
}
