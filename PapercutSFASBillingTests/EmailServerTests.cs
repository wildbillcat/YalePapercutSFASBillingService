using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PapercutSFASBilling;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class EmailServerTests
    {
        [TestMethod]
        public void TestEmailSummary()
        {
            List<char[]> billID = new List<char[]>();
            billID.Add("00001".ToCharArray());
            SQLBillingServer billingServer = new SQLBillingServer(TestingParameters.SQLBillingServeruser, TestingParameters.SQLBillingServerpass, TestingParameters.SQLBillingServerpath, TestingParameters.SQLBillingServerdb, TestingParameters.SQLBillingServerprefix, TestingParameters.SQLBillingServertype, TestingParameters.SQLBillingServerdetailCode, TestingParameters.SQLBillingServeruserID);
            EmailServer mailMan = new EmailServer(TestingParameters.fromAddress, TestingParameters.recipientAddress, TestingParameters.smtpServer, TestingParameters.smtpPort, TestingParameters.sslEnable, TestingParameters.smtpUser, TestingParameters.smtpPassword);
            mailMan.SendSummaryEmail(billingServer, billID);
        }
    }
}
