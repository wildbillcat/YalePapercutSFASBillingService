using System;
using System.ServiceProcess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapercutSFASBilling;


namespace PapercutSFASBillingTests
{
    [TestClass]
    public class Service1Tests
    {
        
        [TestMethod]
        public void RunTestBilling()
        {
            //PaperCutServerTests pcut = new PaperCutServerTests();
            //pcut.ChargeWorkingUser();
            BillingManager bill = new BillingManager(true);
            bill.RunBilling();
        }

        [TestMethod]
        public void RunTestBilling2()
        {
            BillingManager.Main();
        }
    }
}
