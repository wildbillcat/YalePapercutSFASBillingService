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
            Console.WriteLine(string.Concat("1 Cent:  ", new string(PapercutSFASBilling.SQLBillingServer.BillingUtility.FormatAmount(00.01))));
            char[] amount = SQLBillingServer.BillingUtility.FormatAmount(.009);
            Console.WriteLine(new string(amount));
            double billingAmount = double.Parse(new string(amount)) / 100;
            Console.WriteLine(billingAmount);
        }
    }
}
