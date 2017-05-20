using System;

namespace Team7ADProjectMVC.Exceptions
{
    //Author: Edwin
    public class RequisitionAndPOCreationException : Exception
    {
        public RequisitionAndPOCreationException()
        {
        }

        public RequisitionAndPOCreationException(string message)
        : base(message)
        {
        }
    }
}