using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Team7ADProjectMVC.Exceptions;
using Team7ADProjectMVC.Models;

namespace Team7ADProjectMVC.Services
{
    //Author : Edwin
    public class InventoryService : IInventoryService
    {
        ProjectEntities db = new ProjectEntities();
        PushNotification fcm = new PushNotification();
        IUtilityService utilSvc = new UtilityService();

        public string FindItemIdByName(string itemName)
        {

            string itemid = db.Inventories.Where(x => x.Description == itemName).FirstOrDefault().ItemNo.ToString();
            return itemid;
        }

        public string GetItemCode(string itemDesc)
        {
            string startingLetter = itemDesc[0].ToString();
            ItemCodeGenerator result= db.ItemCodeGenerators.Find(startingLetter);
            result.itemcount++;
            db.SaveChanges();
            string fmt = "000";
            return startingLetter.ToUpper() + ((int)result.itemcount).ToString(fmt);
        }

        public Inventory FindInventoryItemById(string id)
        {
            return db.Inventories.Find(id);
        }
        public void AddItem(Inventory inventory)
        {
            db.Inventories.Add(inventory);
            db.SaveChanges();
        } 

        public List<Category> GetAllCategories()
        {
            var categories = db.Categories;
            return (categories.ToList());
        }

        public List<Inventory> GetAllInventory()
        {
            var inventories = db.Inventories;
            return (inventories.ToList());
        }

        public List<Measurement> GetAllMeasurements()
        {
            var measurements = db.Measurements;
            return (measurements.ToList());
        }

        public List<Supplier> GetAllSuppliers()
        {
            var suppliers = db.Suppliers;
            return (suppliers.ToList());
        }

        public List<Requisition> GetNotCompletedRequisitions(int departmentId)
        {
            var reqList = from req in db.Requisitions
                          where req.DepartmentId == departmentId
                          && req.RequisitionStatus != "Completed"
                          orderby req.RequisitionStatus descending
                          select req;
            return reqList.ToList();
        }

        public void UpdateInventory(Inventory inventory)
        {
            db.Entry(inventory).State = EntityState.Modified;
            db.SaveChanges();
        }

        public List<Inventory> GetInventoryListByCategory(int id)
        {
            var queryByCategory = from t in db.Inventories
                                  where t.Category.CategoryId == id
                                  orderby t.Description ascending
                                  select t;
            return (queryByCategory.ToList());
        }
        public List<StockCard> GetStockCardFor(String id)
        {
            var query = from stockCard in db.StockCards
                        where stockCard.ItemNo == id
                        orderby stockCard.Date
                        select stockCard;
            return (query.ToList());
        }

        public List<Requisition> GetOutStandingRequisitions()
            {
            var query = from rq in db.Requisitions
                        where rq.RequisitionStatus != "Pending Approval"
                        && rq.RequisitionStatus != "Rejected"
                        && rq.RequisitionStatus != "Processing"
                        && rq.RequisitionStatus != "Completed"
                        orderby rq.ApprovedDate
                        select rq;

            List<Requisition> temp = query.ToList();
            System.Web.HttpContext.Current.Application.Lock();
            RetrievalList rList = (RetrievalList)System.Web.HttpContext.Current.Application["RetrievalList"];
            System.Web.HttpContext.Current.Application.UnLock();

            try
            {
                if (temp.Count != 0 && rList.requisitionList.Count != 0)
                {
                    try
                    {
                        foreach (var item in rList.requisitionList)
                        {
                            temp.RemoveAll(x => x.RequisitionId == item.RequisitionId);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Expected" + e.ToString());
            }
            return (temp);

        }

        public RetrievalList GetRetrievalList()
        {
            System.Web.HttpContext.Current.Application.Lock();
            RetrievalList rList = (RetrievalList)System.Web.HttpContext.Current.Application["RetrievalList"];


            if (rList.retrievalId == null)
            {
                var query = from rt in db.Retrievals
                            orderby rt.RetrievalId
                            select rt;
                rList.retrievalId = (query.ToList()).Last().RetrievalId + 1;
                Retrieval tempRetrieval = new Retrieval();
                tempRetrieval.RetrievalId = (int)rList.retrievalId;
                tempRetrieval.RetrievalDate = DateTime.Today;
                db.Retrievals.Add(tempRetrieval);
                db.SaveChanges();
                System.Web.HttpContext.Current.Application["RetrievalList"] = rList;
            }

            System.Web.HttpContext.Current.Application.UnLock();
            return rList;
        }

        public void PopulateRetrievalList()
        {
            System.Web.HttpContext.Current.Application.Lock();
            RetrievalList rList = (RetrievalList)System.Web.HttpContext.Current.Application["RetrievalList"];
            if (rList.requisitionList == null)
            {
                rList.requisitionList = GetOutStandingRequisitions();
            }

            System.Web.HttpContext.Current.Application["RetrievalList"] = rList;
            System.Web.HttpContext.Current.Application.UnLock();
        }
        public void PopulateRetrievalListItems()
        {
            System.Web.HttpContext.Current.Application.Lock();
            RetrievalList rList = (RetrievalList)System.Web.HttpContext.Current.Application["RetrievalList"];
            if (rList.itemsToRetrieve == null)
            {
                rList.itemsToRetrieve = new List<RetrievalListItems>();

                List<RetrievalListItems> unconsolidatedList = new List<RetrievalListItems>();

                foreach (Requisition requisition in rList.requisitionList)
                {
                    foreach (RequisitionDetail reqDetails in requisition.RequisitionDetails)
                    {
                        int invBalance = (int)FindInventoryItemById(reqDetails.ItemNo).Quantity;
                        if (reqDetails.OutstandingQuantity > 0 && invBalance > 0)
                        {
                            RetrievalListItems newItem = new RetrievalListItems();
                            newItem.itemNo = reqDetails.ItemNo;
                            newItem.requiredQuantity = (int)reqDetails.OutstandingQuantity;
                            newItem.binNo = reqDetails.Inventory.BinNo;
                            newItem.description = reqDetails.Inventory.Description;
                            newItem.collectionStatus = false;
                            unconsolidatedList.Add(newItem);
                        }
                    }
                }
                RetrievalListItemsComparer comparer = new RetrievalListItemsComparer();
                unconsolidatedList.Sort(comparer);

                int i = 0;
                foreach (var item in unconsolidatedList)
                {
                    if (i == 0)
                    {
                        rList.itemsToRetrieve.Add(item);
                        i++;
                    }
                    else if (item.itemNo.Equals(rList.itemsToRetrieve[i - 1].itemNo))
                    {
                        rList.itemsToRetrieve[i - 1].requiredQuantity += item.requiredQuantity;
                    }
                    else
                    {
                        rList.itemsToRetrieve.Add(item);
                        i++;
                    }
                }

                foreach (var item in rList.itemsToRetrieve)
                {
                    int invBal = (int)FindInventoryItemById(item.itemNo).Quantity;
                    if (item.requiredQuantity > invBal)
                    {
                        item.requiredQuantity = invBal;
                    }
                }
            }

            HttpContext.Current.Application["RetrievalList"] = rList;
            HttpContext.Current.Application.UnLock();
        }

        public void ClearRetrievalList()
        {
            System.Web.HttpContext.Current.Application["RetrievalList"] = new RetrievalList();
        }

        public void AutoAllocateDisbursementsByOrderOfRequisition(DateTime? deliveryDate)
        {
            System.Web.HttpContext.Current.Application.Lock();
            RetrievalList retrievalList = (RetrievalList)System.Web.HttpContext.Current.Application["RetrievalList"];

            foreach (var itemsCollected in retrievalList.itemsToRetrieve)
            {
                UpdateInventoryQuantity(itemsCollected.itemNo, itemsCollected.collectedQuantity);
            }
            try
            {
                fcm.CheckForStockReorder();

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
          

            List < Requisition > requisitionListFromRList = retrievalList.requisitionList;
            DisbursementList dList = new DisbursementList();
            List<DisbursementDetail> tempDisbursementDetailList = new List<DisbursementDetail>();

            int? currentDisbursementListId = null;

            foreach (Requisition requisition in requisitionListFromRList)
            {
                var q = (from x in db.DisbursementLists
                         where x.RetrievalId == retrievalList.retrievalId
                         && x.DepartmentId == requisition.DepartmentId
                         select x).FirstOrDefault();
                if (q == null) // if its first time entering loop, create new disbursementlist for dept
                {
                    currentDisbursementListId = CreateNewDisbursementListForDepartment(dList, requisition, retrievalList, currentDisbursementListId, deliveryDate);

                    foreach (RequisitionDetail reqDetails in requisition.RequisitionDetails)
                    {
                        AddDisbursementDetailToTempList(currentDisbursementListId, reqDetails, retrievalList, tempDisbursementDetailList);
                    }
                }
                else if (q.DepartmentId == requisition.DepartmentId)
                {
                    foreach (RequisitionDetail reqDetails in requisition.RequisitionDetails)
                    {
                        currentDisbursementListId = q.DisbursementListId;
                        AddDisbursementDetailToTempList(currentDisbursementListId, reqDetails, retrievalList, tempDisbursementDetailList);
                    }
                }
            }
            SaveDisbursementDetailsIntoDB(tempDisbursementDetailList);

            foreach (Requisition r in requisitionListFromRList)
            {
                Requisition temp = db.Requisitions.Find(r.RequisitionId);
                temp.RetrievalId = retrievalList.retrievalId;
                temp.RequisitionStatus = "Processing";
                db.Entry(temp).State = EntityState.Modified;
                db.SaveChanges();
            }

            ClearRetrievalList();
            HttpContext.Current.Application.UnLock();
        }

        
        public void UpdateInventoryQuantity(string itemNo, int modifiedQuantity) 
        {
            Inventory i = db.Inventories.Find(itemNo);
            i.Quantity -= modifiedQuantity;
            if (i.Quantity >= 0)
            {
                db.Entry(i).State = EntityState.Modified;
                db.SaveChanges();
       
            }
            else
            {
                throw new InventoryAndDisbursementUpdateException("The quantity of '" + i.Description + "' (Item code: "+ itemNo +") collected was more than the available quantity (Bal: " + i.Quantity+ "). Please try again.");
                //Shouldn't happen because html5 validation will check first
            }
        }

        //supplementary method (not declared in interface)
        private void SaveDisbursementDetailsIntoDB(List<DisbursementDetail> tempDisbursementDetailList)
        {
            var q = tempDisbursementDetailList
                    .GroupBy(ac => new
                    {
                        ac.DisbursementListId,
                        ac.ItemNo,
                    })
                    .Select(ac => new DisbursementDetail
                    {
                        DisbursementListId = (int)ac.Key.DisbursementListId,
                        ItemNo = ac.Key.ItemNo,
                        PreparedQuantity = ac.Sum(acs => acs.PreparedQuantity),
                        DeliveredQuantity = ac.Sum(acs => acs.DeliveredQuantity)
                    });


            foreach (DisbursementDetail newDisbursementDetail in q.ToList())
            {
                db.Set(typeof(DisbursementDetail)).Attach(newDisbursementDetail);
                db.DisbursementDetails.Add(newDisbursementDetail);
                db.SaveChanges();
            }
        }
        
        //supplementary method (not declared in interface)
        private void AddDisbursementDetailToTempList(int? currentDisbursementListId, RequisitionDetail reqDetails, RetrievalList retrievalList, List<DisbursementDetail> tempDisbursementDetailList)
        {
            DisbursementDetail newDisbursementDetail = new DisbursementDetail();
            newDisbursementDetail.DisbursementListId = currentDisbursementListId;
            newDisbursementDetail.ItemNo = reqDetails.ItemNo;

            var x = (from y in retrievalList.itemsToRetrieve
                     where y.itemNo == newDisbursementDetail.ItemNo
                     select y).SingleOrDefault();
            if (x != null)
            {
                if (x.collectedQuantity >= reqDetails.OutstandingQuantity && x.collectedQuantity != 0)
                {
                    newDisbursementDetail.PreparedQuantity = reqDetails.OutstandingQuantity;
                    newDisbursementDetail.DeliveredQuantity = newDisbursementDetail.PreparedQuantity;
                    x.collectedQuantity = x.collectedQuantity - (int)reqDetails.OutstandingQuantity;
                }
                else
                {
                    newDisbursementDetail.PreparedQuantity = x.collectedQuantity;
                    newDisbursementDetail.DeliveredQuantity = newDisbursementDetail.PreparedQuantity;
                    x.collectedQuantity = x.collectedQuantity - (int)newDisbursementDetail.PreparedQuantity;
                }
                
                tempDisbursementDetailList.Add(newDisbursementDetail);
            }
        }
        private int? CreateNewDisbursementListForDepartment(DisbursementList dList, Requisition requisition, RetrievalList retrievalList, int? currentDisbursementListId, DateTime? deliverydate)
        {
            
            Department d = db.Departments.Find(requisition.DepartmentId);
            dList.DepartmentId = d.DepartmentId;
            dList.RetrievalId = retrievalList.retrievalId;
            dList.Status = "Processing";
            if (deliverydate != null)
            {
                dList.DeliveryDate = deliverydate;
            }
            else
            {
                dList.DeliveryDate = DateTime.Today.AddDays(utilSvc.DaysToAdd(DateTime.Today.DayOfWeek, DayOfWeek.Friday));
            }
            db.Set(typeof(DisbursementList)).Attach(dList);
            db.DisbursementLists.Add(dList);
            db.SaveChanges(); // creates new disbursementlist

            currentDisbursementListId = db.DisbursementLists
                                        .OrderByDescending(x => x.DisbursementListId)
                                        .FirstOrDefault().DisbursementListId; //returns created disbursementlist Id

            try //email to notify manager of approval
            {        
                string emailBody = dList.Department.Representative.EmployeeName + ", you have a disbursement scheduled for collection on " +dList.DeliveryDate+ " "+ dList.Department.CollectionPoint.CollectTime+ " at "+dList.Department.CollectionPoint.PlaceName+ ". Please click on http://"+ utilSvc.GetBaseUrl() + "/Representative/ViewDisbursementDetail/"+dList.DisbursementListId+" to view details for confirmation.";
                utilSvc.SendEmail(new List<string>(new string[] { dList.Department.Representative.Email}), "New Disbursement Scheduled for Collection", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return currentDisbursementListId;
        }

        public List<DisbursementDetail> GenerateListForManualAllocation()
        {
            int lastRetrievalListId = db.Retrievals
                                        .OrderByDescending(x => x.RetrievalId)
                                        .FirstOrDefault().RetrievalId;

            var currentDisbursement = (from x in db.DisbursementLists
                                       where x.RetrievalId == lastRetrievalListId
                                       select x).ToList();
            while (currentDisbursement == null || currentDisbursement.Count() <1)
            {
                lastRetrievalListId = lastRetrievalListId - 1;
                currentDisbursement = (from x in db.DisbursementLists
                                       where x.RetrievalId == lastRetrievalListId
                                       select x).ToList();
            }
            List<DisbursementDetail> tempDisbursementDetailList = new List<DisbursementDetail>();
            List<DisbursementDetail> returnDisbursementDetailList = new List<DisbursementDetail>();
            foreach (var x in currentDisbursement)
            {
                if (x.Status != "Completed")
                {
                    foreach (var y in x.DisbursementDetails)
                    {
                        tempDisbursementDetailList.Add(y);
                    }
                }
            }

            var consolidatedDisbursementList = tempDisbursementDetailList
                                                .GroupBy(ac => new
                                                {
                                                    ac.ItemNo
                                                })
                                                .Select(ac => new DisbursementDetail
                                                {
                                                    ItemNo = ac.Key.ItemNo,
                                                    PreparedQuantity = ac.Sum(acs => acs.PreparedQuantity)
                                                });

            List<RequisitionDetail> tempRequisitionDetailList = new List<RequisitionDetail>();

            var test = (from x in db.Requisitions
                        where x.RetrievalId == lastRetrievalListId
                        select x).ToList();

            foreach (var item in test)
            {
                foreach (var item2 in item.RequisitionDetails)
                {
                    tempRequisitionDetailList.Add(item2);
                }
            }

            var consolidatedRequisitionList = tempRequisitionDetailList
                                                .GroupBy(ac => new
                                                {
                                                    ac.ItemNo
                                                })
                                                .Select(ac => new RequisitionDetail
                                                {
                                                    ItemNo = ac.Key.ItemNo,
                                                    OutstandingQuantity = ac.Sum(acs => acs.OutstandingQuantity),
                                                });


            foreach (var item in consolidatedDisbursementList)
            {
                var t = (from x in consolidatedRequisitionList
                         where item.ItemNo == x.ItemNo
                         select x).FirstOrDefault();
                if (t.OutstandingQuantity == item.PreparedQuantity)
                {
                    //ignore item
                    Console.WriteLine("Equal");
                }
                else if (t.OutstandingQuantity != item.PreparedQuantity)
                {
                    //do something
                    Console.WriteLine("Not Equal");
                    var x = (from y in tempDisbursementDetailList
                             where y.ItemNo == item.ItemNo
                             select y).ToList();
                    foreach (var i2 in x)
                    {
                        returnDisbursementDetailList.Add(i2);
                    }
                }
            }
            List<string> itemNoList = new List<string>();
            foreach (var item in returnDisbursementDetailList)
            {
                var q = (from x in returnDisbursementDetailList
                         where item.ItemNo == x.ItemNo
                         select x).ToList();
                if (q.Count ==1)
                {
                    itemNoList.Add(item.ItemNo);
                }
            }
            foreach (var item in itemNoList)
            {
                returnDisbursementDetailList.RemoveAll(x => x.ItemNo == item);
            }

            return returnDisbursementDetailList;
        }

        public int GetLastRetrievalListId()
        {
            int currentRetrievalListId = db.Retrievals
                                        .OrderByDescending(x => x.RetrievalId)
                                        .FirstOrDefault().RetrievalId;
            return currentRetrievalListId;
        }

        public List<Requisition> GetRequisitionsSummedByDept(int currentRetrievalListId)
        {
            List<Requisition> returnList = new List<Requisition>();
            //get current retrieval list
            var q = (from x in db.Requisitions
                    where x.RetrievalId == currentRetrievalListId
                    select x).ToList();

            while(q == null || q.Count <1)
            {
                currentRetrievalListId = currentRetrievalListId - 1;
                q = (from x in db.Requisitions
                         where x.RetrievalId == currentRetrievalListId
                         select x).ToList();
            }
            HashSet<Department> test = new HashSet<Department>();

            foreach (var x in q.ToList())
            {
                Department d = db.Departments.Find(x.DepartmentId);
                test.Add(d);
            }

            foreach (Department d in test)
            {
                var q2 = from x in db.RequisitionDetails
                         where x.Requisition.RetrievalId == currentRetrievalListId
                         && x.Requisition.DepartmentId == d.DepartmentId
                         select x;

                var pp = q2.ToList();

                var q3 = pp
                        .GroupBy(ac => new
                        {
                            ac.ItemNo,
                        })
                        .Select(ac => new RequisitionDetail
                        {
                            ItemNo = ac.Key.ItemNo,
                            OutstandingQuantity = ac.Sum(acs => acs.OutstandingQuantity),
                        });
                Requisition req = new Requisition();
                req.RequisitionDetails = q3.ToList();
                req.DepartmentId = d.DepartmentId;
                returnList.Add(req);
            }
            return returnList;
        }

        public void ManuallyAllocateDisbursements(int[] departmentId, int[] preparedQuantity, int[] adjustedQuantity, int[] disbursementListId, int[] disbursementDetailId, string[] itemNo)
        {

            int deptId;

            int adjustedQty;
            int disburseListId;
            int disburseDetailId;
            string itemNumber;

            bool error=false;

            if (CheckIfInputQtyExceedsCollectedQty(itemNo, preparedQuantity, adjustedQuantity))
            {
                for (int i = 0; i < departmentId.Length; i++)
                {
                    deptId = departmentId[i];
                    adjustedQty = adjustedQuantity[i];
                    disburseListId = disbursementListId[i];
                    disburseDetailId = disbursementDetailId[i];
                    itemNumber = itemNo[i];

                    var q = (from x in db.DisbursementDetails
                             where x.DisbursementList.DepartmentId == deptId
                             && x.DisbursementListId == disburseListId
                             && x.DisbursementDetailId == disburseDetailId
                             && x.ItemNo == itemNumber
                             select x).SingleOrDefault();
                    q.PreparedQuantity = adjustedQty;
                    db.Entry(q).State = EntityState.Modified;
                    db.SaveChanges();
                }
            } else
            {
                error = true;
            }
            if (error)
            {
                throw new InventoryAndDisbursementUpdateException("Adjusted quantity exceeds collected quantity.");
            }

        }

        public bool CheckIfInputQtyExceedsCollectedQty(string[] itemNo, int[] preparedQuantity, int[] adjustedQuantity)
        {
            bool status = false;
            string itemNumber = null;
            int prepQty = 0;
            int adjQty = 0;
            for (int i = 0; i < itemNo.Count(); i++)
            {
                if (itemNumber == null)
                {
                    itemNumber = itemNo[i];
                    prepQty = preparedQuantity[i];
                    adjQty = adjustedQuantity[i];
                }else if (itemNumber == itemNo[i])
                {
                    prepQty += preparedQuantity[i];
                    adjQty += adjustedQuantity[i];
                    if (adjQty == prepQty)
                    {
                        status = true;
                    }
                    else
                    {
                        status = false;
                    }
                }
                else if (itemNumber != itemNo[i])
                {
                    if (adjQty == prepQty)
                    {
                        status = true;
                    }else
                    {
                        status = false;
                    }
                }
            }
            return status;
        }

        public void UpdateDisbursementListDetails(int disbursementListId, string[] itemNo, int[] originalPreparedQty, int[] adjustedQuantity, string[] remarks)
        {
            for (int i =0; i < itemNo.Count();i++)
            {
                var tempItemNo = itemNo[i];
                var disbursementDetail = (from x in db.DisbursementDetails
                                         where x.DisbursementListId == disbursementListId
                                         && x.ItemNo == tempItemNo
                                          select x).FirstOrDefault();
                if(originalPreparedQty[i] >= adjustedQuantity[i] && disbursementDetail.DisbursementList.Status != "Completed")
                {
                    int updateAmount = adjustedQuantity[i]- (int)disbursementDetail.DeliveredQuantity;
                    UpdateInventoryQuantity(tempItemNo, updateAmount);
                    disbursementDetail.DeliveredQuantity = adjustedQuantity[i];
                    disbursementDetail.Remark = remarks[i];
                    db.Entry(disbursementDetail).State = EntityState.Modified;
                    db.SaveChanges();
               
                } else if (disbursementDetail.DisbursementList.Status == "Completed")
                {
                    throw new InventoryAndDisbursementUpdateException("The disbursement has been completed. Unable to make further changes.");
                    //This should not happen, because the submit button is hidden when status = completed
                }
                else 
                {
                    throw new InventoryAndDisbursementUpdateException("Prepared quantity is greater than adjusted quantity"); 
                    //This should not happen, because html5 validation is in use
                }

            }
        }

        public void UpdateCollectionInfo(RetrievalList rList, int collectedQuantity, string itemNo)
        {
            foreach (var item in rList.itemsToRetrieve)
            {
                if (item.itemNo.Equals(itemNo))
                {
                    item.collectedQuantity = collectedQuantity;
                    item.collectionStatus = true;
                }
            }
        }

        public List<DisbursementList> GetNotCompletedDisbursements(int dId)
        {
            var r = from x in db.DisbursementLists
                    where x.DepartmentId == dId
                    && x.Status != "Completed"
                    orderby x.Status
                    select x;
            return r.ToList();
        }

        public List<DisbursementDetail> GetNotCompletedDisbursementDetails(int did, int disbursementListID)
        {
            var dDetail = from r in db.DisbursementDetails
                          where r.DisbursementList.DepartmentId == did
                          && r.DisbursementListId == disbursementListID
                          orderby r.Inventory.Description ascending
                          select r;
            return dDetail.ToList();
        }

        public List<DisbursementList> GetProcessingDisbursements()
        {
            var disburse = from d in db.DisbursementLists
                           where d.Status.Equals("Processing")
                           orderby d.DeliveryDate ascending
                           select d;
            return disburse.ToList();
        }

        public List<DisbursementDetail> FindDisbursementDetails(int dId)
        {
            var disDetail = from dd in db.DisbursementDetails
                            where dd.DisbursementListId == dId
                            orderby dd.Inventory.Description ascending
                            select dd;
            return disDetail.ToList();
        }

        public void UpdateDisbursementDate(DateTime deliveryDate, int disbursementListId)
        {
            DisbursementList disbursementList = db.DisbursementLists.Find(disbursementListId);
            DateTime previousDate = disbursementList.DeliveryDate.Value;
            disbursementList.DeliveryDate = deliveryDate;
            db.Entry(disbursementList).State = EntityState.Modified;
            db.SaveChanges();
            try
            {
                string emailBody=disbursementList.Department.Representative.EmployeeName+", your disbursement scheduled to be collected on " + previousDate.ToString("dd/MM/yyyy")+" has been rescheduled to " + disbursementList.DeliveryDate.Value.Date.ToString("dd/MM/yyyy")+ ". Please click on http://"+ utilSvc.GetBaseUrl() + "//Representative/ViewDisbursementDetail/" + disbursementList.DisbursementListId + " for more information.";
                utilSvc.SendEmail(new List<string>(new string[] { disbursementList.Department.Representative.Email }), "Disbursement Rescheduled for Collection", emailBody);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}