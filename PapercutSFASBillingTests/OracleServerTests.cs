using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PapercutSFASBilling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class OracleServerTests
    {

        [TestMethod]
        public void ConnectToOracleServer()
        {
            Console.WriteLine("Start Test: ConnectToOracleServer");
            Console.WriteLine("Creating a new Oracle Connection with the Hard Coded User Name, Password, and Connection String.");
            OracleServer Ora = new OracleServer(TestingParameters.OracleBan2User, TestingParameters.OracleBan2Password, TestingParameters.OracleBan2ServerString); //User, Password, Server
        }

        [TestMethod]
        public void GetCurrentTermCode()
        {
            Console.WriteLine("Start Test: GetCurrentTermCode");
            Console.WriteLine("Creating a new Oracle Connection with the Hard Coded User Name, Password, and Connection String.");
            OracleServer Ora = new OracleServer(TestingParameters.OracleBan2User, TestingParameters.OracleBan2Password, TestingParameters.OracleBan2ServerString); //User, Password, Server
            
            Console.WriteLine("Fetching Current Term Code...");
            string termCode = Ora.GetCurrentTermCode();
            
            if(termCode.Equals("ERROR"))
            {
                Assert.Fail("Failed to return the Term Code");
            }

            Console.WriteLine("The Current Term Code is: " + termCode);
        }

        [TestMethod]
        public void GetFunctionalUserInformationFromOracle()
        {
            string workingNetID = TestingParameters.WorkingNetID;
            string notWorkingNetID = TestingParameters.NotWorkingNetID;

            Console.WriteLine("Start Test: GetUserInformationFromOracle");
            Console.WriteLine("Creating a new Oracle Connection with the Hard Coded User Name, Password, and Connection String.");
            OracleServer Ora = new OracleServer(TestingParameters.OracleBan2User, TestingParameters.OracleBan2Password, TestingParameters.OracleBan2ServerString); //User, Password, Server
            Console.WriteLine("Fetching Current Term Code...");
            string termCode = Ora.GetCurrentTermCode();
            if(termCode.Equals("ERROR"))
            {
                Assert.Fail("Failed to return the Term Code");
            }
            Console.WriteLine("The Current Term Code is: " + termCode);
            Console.WriteLine("Now Fetching Information on working NetID: " + workingNetID);
            string[] UserData = Ora.GetUserInfo(workingNetID, termCode);

            Console.WriteLine("Length of the User Data Array:" + UserData.Length);
            if (UserData.Length != 4)
            {
                Console.WriteLine(UserData[0]);
                Assert.Fail("No Working User Data was retrieved");
                //return;
            }
            Console.WriteLine("Working User's PIDM: " + UserData[0]);
            Console.WriteLine("Working User's SPRIDEN ID: " + UserData[1]);
            Console.WriteLine("Working User's Status: " + UserData[2]);

            Console.WriteLine("Now Fetching Information on non-working NetID: " + workingNetID);
            UserData = Ora.GetUserInfo(notWorkingNetID, termCode);
            Console.WriteLine("Length of the User Data Array:" + UserData.Length);
            if (UserData.Length == 4)
            {
                Console.WriteLine("Non-Working User's PIDM: " + UserData[0]);
                Console.WriteLine("Non-Working User's SPRIDEN ID: " + UserData[1]);
                Console.WriteLine("Non-Working User's Status: " + UserData[2]);
                Assert.Fail("Working User Data was retrieved when this account should be non-functional!");
                //return;
            }
            Console.WriteLine("Non-Working User's PIDM: " + UserData[0]);
        }
    }
}
