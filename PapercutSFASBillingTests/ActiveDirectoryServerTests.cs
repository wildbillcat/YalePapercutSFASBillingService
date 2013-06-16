using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapercutSFASBilling;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class ActiveDirectoryServerTests
    {

        [TestMethod]
        public void NoLists()
        {
            ActiveDirectoryServer AD = new ActiveDirectoryServer("", "");
            AD.GetADuserLists();
            Console.WriteLine("Blacklist:");
            foreach (string user in AD.GetBlacklist())
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Whitelist:");
            foreach (string user in AD.GetWhitelist())
            {
                Console.WriteLine(user);
            }
        }

        [TestMethod]
        public void JustBlackList()
        {
            ActiveDirectoryServer AD = new ActiveDirectoryServer("", TestingParameters.ActiveDirectoryBlackList);
            AD.GetADuserLists();
            Console.WriteLine("Blacklist:");
            foreach (string user in AD.GetBlacklist())
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Whitelist:");
            foreach (string user in AD.GetWhitelist())
            {
                Console.WriteLine(user);
            }
        }

        [TestMethod]
        public void JustWhiteList()
        {
            ActiveDirectoryServer AD = new ActiveDirectoryServer(TestingParameters.ActiveDirectoryWhiteList, "");
            AD.GetADuserLists();
            Console.WriteLine("Blacklist:");
            foreach (string user in AD.GetBlacklist())
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Whitelist:");
            foreach (string user in AD.GetWhitelist())
            {
                Console.WriteLine(user);
            }
        }

        [TestMethod]
        public void BothLists()
        {
            ActiveDirectoryServer AD = new ActiveDirectoryServer(TestingParameters.ActiveDirectoryWhiteList, TestingParameters.ActiveDirectoryBlackList);
            AD.GetADuserLists();
            Console.WriteLine("Blacklist:");
            foreach (string user in AD.GetBlacklist())
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Whitelist:");
            foreach (string user in AD.GetWhitelist())
            {
                Console.WriteLine(user);
            }
        }
    }
}
