using System;
using System.Collections.Generic;
using Team7ADProjectMVC.Models;

namespace Team7ADProjectMVC.Services
{
    //Author : Edwin
    interface IInventoryService
    {
        string FindItemIdByName(string itemName);
        string GetItemCode(string itemDesc);
        List<Inventory> GetAllInventory();

        List<Category> GetAllCategories();

        Inventory FindInventoryItemById(String id);

        List<Measurement> GetAllMeasurements();

        List<Supplier> GetAllSuppliers();

        void AddItem(Inventory inventory);

        void UpdateInventory(Inventory inventory);

        List<Inventory> GetInventoryListByCategory(int id);
        //Takes category ID, returns list of inventory

        List<StockCard> GetStockCardFor(String id);

        List<Requisition> GetOutStandingRequisitions();

        RetrievalList GetRetrievalList();

        void PopulateRetrievalList();

        void PopulateRetrievalListItems();

        void ClearRetrievalList();

        void AutoAllocateDisbursementsByOrderOfRequisition(DateTime? deliveryDate);

        List<DisbursementDetail> GenerateListForManualAllocation();

        int GetLastRetrievalListId();

        List<Requisition> GetRequisitionsSummedByDept(int currentRetrievalListId);

        void ManuallyAllocateDisbursements(int[] departmentId, int[] preparedQuantity, int[] adjustedQuantity, int[] disbursementListId, int[] disbursementDetailId, string[] itemNo);
        void UpdateDisbursementListDetails(int disbursementListId, string[] itemNo, int[] originalPreparedQty, int[] adjustedQuantity, string[] remarks);
        void UpdateInventoryQuantity(string itemNo, int collectedQuantity);
        List<Requisition> GetNotCompletedRequisitions(int departmentId);
        void UpdateCollectionInfo(RetrievalList rList, int collectedQuantity, string itemNo);
        List<DisbursementList> GetNotCompletedDisbursements(int dId);
        List<DisbursementDetail> GetNotCompletedDisbursementDetails(int did, int disbursementListID);
        List<DisbursementList> GetProcessingDisbursements();
        List<DisbursementDetail> FindDisbursementDetails(int dId);
        void UpdateDisbursementDate(DateTime deliveryDate, int disbursementListId);
    }
}
