using System;

namespace BgHelper
{
    // Custom Exceptions
    public class BgHelperTimeoutException : Exception
    {
        public BgHelperTimeoutException(string message)
            : base(message)
        {
        }
    }
}
