using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapercutSFASBilling;

namespace PapercutSFASBillingTests
{
    [TestClass]
    public class SFASSFTPTests
    {
        [TestMethod]
        public void UploadFiles()
        {
            List<string> billingsCompleted = new List<string>();
            SFASSFTP ftpServer = new SFASSFTP(TestingParameters.SFTPUser, TestingParameters.SFTPKeyPath, TestingParameters.SFTPServerPath, TestingParameters.SFTPPortNumber, TestingParameters.WinSCPPath, TestingParameters.SSHHostKeyFingerprint, TestingParameters.RemoteDirectory);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Concat(@"BillingSubmissions\", "TESTFILE", ".txt")))
            {
                file.Write("TESTFILE");
                //Add File Path to list of Completed Billings
                string fullPath = ((System.IO.FileStream)(file.BaseStream)).Name;
                billingsCompleted.Add(fullPath);
            }
            if (!ftpServer.UploadBillings(billingsCompleted))
            {
                Assert.Fail();
            }

        }
    }
}
