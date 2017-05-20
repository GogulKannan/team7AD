using System;

namespace Team7ADProjectMVC.Exceptions
{
    //Author: Edwin
    public class InventoryAndDisbursementUpdateException : Exception
    {
        public InventoryAndDisbursementUpdateException()
        {
        }

        public InventoryAndDisbursementUpdateException(string message)
        : base(message)
        {
        }
    }
}