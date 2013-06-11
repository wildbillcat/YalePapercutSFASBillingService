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
        public void RunTestConfiguration()
        {
            Service1 service = new Service1();
            string[] arguments = new string[3] { "-p", @"E:\Development\Visual Studio 2012\Projects\PapercutSFASBilling\PapercutSFASBillingTests\TestConfigs\Test1", "test" };
            service.TestConfig(arguments);
        }

        [TestMethod]
        public void RunTestBilling()
        {
            Service1 services = new Service1();
            string[] arguments = new string[2] { "-p", @"E:\Development\Visual Studio 2012\Projects\PapercutSFASBilling\PapercutSFASBillingTests\TestConfigs\Test1"};
            services.TestBilling(arguments);
        }
    }
}
