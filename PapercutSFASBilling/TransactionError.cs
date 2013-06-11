using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapercutSFASBilling
{
    public struct TransactionError
    {

        public string Username;
        public string Error;

        public TransactionError(string Username, string Error)
        {
            this.Username = Username;
            this.Error = Error;
        }
    }
}

