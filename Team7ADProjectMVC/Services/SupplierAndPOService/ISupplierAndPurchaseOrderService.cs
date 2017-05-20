using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team7ADProjectMVC.Services
{
    //Author : Edwin
    interface ISupplierAndPurchaseOrderService
    {
        List<Supplier> GetAllSuppliers();
        Supplier FindSupplierById(int? id);
        List<Inventory> FindInventoryItemsBySupplier(int? id);
        void UpdateSupplier(Supplier supplier);
        void AddNewSupplier(Supplier supplier);
        List<Inventory> GetAllItemsToResupply();
        void GeneratePurchaseOrders(Employee employee, string[] itemNo, int[] supplier, int?[] orderQuantity);
        List<PurchaseOrder> GetAllPOOrderByApproval();
        List<PurchaseOrder> SearchPurchaseOrders(string orderStatus, DateTime? dateOrdered, DateTime? dateApproved, out int resultCount);
        PurchaseOrder FindPOById(int id);
        void ApprovePurchaseOrder(Employee employee, int poNumber, string approve);
        List<Delivery> GetAllDeliveries();
        Delivery FindDeliveryById(int id);
        List<DeliveryDetail> GetDeliveryDetailsByDeliveryId(int id);
        void ReceiveDelivery(Employee employee, int deliveryId, string deliveryRefNo, string dateDelivered, int[] deliveryDetailId, string[] itemNo, int[] quantity, string[] remarks);
    }
}
