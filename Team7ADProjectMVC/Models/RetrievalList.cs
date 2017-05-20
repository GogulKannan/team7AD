using System.Collections.Generic;

namespace Team7ADProjectMVC.Models
{
    //Author : Edwin
    public class RetrievalList
    {
        public int? retrievalId { get; set; }
        public List<Requisition> requisitionList { get; set; }
 
        public List<RetrievalListItems> itemsToRetrieve { get; set; }

        public RetrievalList()
        {
            retrievalId = null;
        }
        public RetrievalList(int retrievalListId)
        {
            retrievalId = retrievalListId;
        }
    }

    public class RetrievalListItems
    {
        public string itemNo { get; set;}
        public int requiredQuantity { get; set; }
        public int collectedQuantity { get; set; }
        public string binNo { get; set; }
        public string description { get; set; }
        public bool collectionStatus { get; set; }
    }

    public class RetrievalListItemsComparer : IComparer<RetrievalListItems>
    {
        public int Compare(RetrievalListItems x, RetrievalListItems y)
        {

            if (x.itemNo.Equals(y.itemNo))
            {
                return 0;
            }
            else return (x.itemNo.CompareTo(y.itemNo));
        }

    }
}