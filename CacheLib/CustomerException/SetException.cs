using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLib.CustomerException
{
    public class SetException : Exception
    {
        public SetException()
        { 
        }

        public SetException(string message) : base(message)
        {
        }

        public SetException(string message, Exception innerException) : base(message, innerException)
        { 
        }

    }
}
