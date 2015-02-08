using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGHelper
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
