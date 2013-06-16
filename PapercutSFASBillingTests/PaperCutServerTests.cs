using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapercutSFASBilling;
using System.Collections.Generic;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class PaperCutServerTests
    {
        [TestMethod]
        public void ChargeWorkingUser()
        {
            PaperCutServer papercut = new PaperCutServer(TestingParameters.PaperCutPath, TestingParameters.PaperCutAPIKey, TestingParameters.PaperCutPort);
            papercut.AdjustUserBalance(TestingParameters.WorkingNetID, -60, "PaperCutServerTests->ChargeWorkingUser");
            Console.WriteLine(String.Concat("User Charged $60: ", TestingParameters.WorkingNetID));
        }

        [TestMethod]
        public void CreditWorkingUser()
        {
            PaperCutServer papercut = new PaperCutServer(TestingParameters.PaperCutPath, TestingParameters.PaperCutAPIKey, TestingParameters.PaperCutPort);
            papercut.AdjustUserBalance(TestingParameters.WorkingNetID, 60, "PaperCutServerTests->ChargeWorkingUser");
            Console.WriteLine(String.Concat("User Credited $60: ", TestingParameters.WorkingNetID));
        }

        [TestMethod]
        public void TotalNumberUsers()
        {
            PaperCutServer papercut = new PaperCutServer(TestingParameters.PaperCutPath, TestingParameters.PaperCutAPIKey, TestingParameters.PaperCutPort);
            Console.WriteLine(String.Concat("Total Number of Users In PaperCut: ", papercut.GetTotalPaperCutUsers()));
        }

        [TestMethod]
        public void RetrievePaperCutUsers()
        {
            PaperCutServer papercut = new PaperCutServer(TestingParameters.PaperCutPath, TestingParameters.PaperCutAPIKey, TestingParameters.PaperCutPort);
            papercut.RetrievePapercutUsers();
            Console.WriteLine("PaperCut Users Retrieved");
        }

        [TestMethod]
        public void GetUserBalance()
        {
            PaperCutServer papercut = new PaperCutServer(TestingParameters.PaperCutPath, TestingParameters.PaperCutAPIKey, TestingParameters.PaperCutPort);
            List<string> users = new List<string>();
            users.Add(TestingParameters.WorkingNetID);
            List<PapercutUser> Balances = papercut.RetrievePapercutBalances(users);
            foreach (PapercutUser p in Balances)
            {
                Console.WriteLine(String.Concat("User: ", p.NetID, " Balance: ",p.balance));
            }
        }       
    }
}
