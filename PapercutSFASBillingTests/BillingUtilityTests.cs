using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapercutSFASBilling;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class BillingUtilityTests
    {
        [TestMethod]
        public void TestFormatAmount()
        {
            Console.WriteLine(string.Concat("1 Dollar 6 Cents:  ", new string(PapercutSFASBilling.SQLBillingServer.BillingUtility.FormatAmount(1.06))));
            Console.WriteLine(string.Concat("32 Cents:  ", new string(PapercutSFASBilling.SQLBillingServer.BillingUtility.FormatAmount(.32))));
            Console.WriteLine(string.Concat("32 Cents:  ", new string(PapercutSFASBilling.SQLBillingServer.BillingUtility.FormatAmount(0000000000000000.34))));
            
        }
    }
}
