using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PapercutSFASBilling
{
    [Serializable()]
    public class ValidationException : Exception, ISerializable
    {

        public ValidationException() : base() 
        { 
            
        }
        public ValidationException(string message) : base(message) 
        {
            
        }
        public ValidationException(string message, System.Exception inner) : base(message, inner)  
        { 
            
        }
        public ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 

        }
        
    }
}

