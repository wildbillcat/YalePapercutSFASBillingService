using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PapercutSFASBilling
{
    public partial class Service1 : ServiceBase
    {

        private PaperCutServer papercutServer;
        private SQLBillingServer billingServer;
        private OracleServer oracleServer;
        private ActiveDirectoryServer activeDirectoryServer;
        private SFASSFTP FTPServer;
        private string WorkingPath;
        private string[] arguments;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LoadConfig(args);
        }

        

        protected override void OnStop()
        {

        }

        public void TestConfig(string[] args)
        {
            LoadConfig(args);
        }

        public void TestBilling(string[] args)
        {
            arguments = args;
            ProcessBilling();
        }
            
        private void LoadConfig(string[] args)
        {
            //Test for Billing and Error Folders
            if (!System.IO.Directory.Exists("BillingErrors"))
            {
                System.IO.Directory.CreateDirectory("BillingErrors");
            }
            if (!System.IO.Directory.Exists("BillingSubmissions"))
            {
                System.IO.Directory.CreateDirectory("BillingSubmissions");
            }
            bool test = false;
            //parse args
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/p") || args[i].Equals("-p"))
                {
                    Console.WriteLine("Array space" + i + ": " + args[i]);
                    i++;
                    WorkingPath = args[i];
                }
                else if (args[i].Equals("test"))
                {
                    test = true;
                }
                
            }
            if (!WorkingPath[WorkingPath.Length - 1].Equals('\\'))
            {
                if (test)
                {
                    Console.WriteLine(string.Concat("No \\ detected at end of line, appending: ", WorkingPath));
                }
                WorkingPath = string.Concat(WorkingPath, @"\");
                if (test)
                {
                    Console.WriteLine(string.Concat("Appended Path: ", WorkingPath));
                }
            }

            int counter = 0;
            string line;


            //Papercut Server Variables
            string paperCutPath = "";
            string apiKey = "";
            int paperCutPort = 0;

            //SQL Billing Server
            string sqlUser = "";
            string sqlPass = "";
            string sqlPath = "";
            string sqlDatabase = "";
            string sqlPrefix = "";
            int sqlType = 0;

            //Oracle Server
            string oracleUser = "";
            string oraclePass = "";
            string oraclePath = "";

            //Active Directory Server
            string whiteList = "";
            string blackList = "";

            //Billing Configuration
            string batchUserID = "";
            string batchDetailCode = "";

            //SFTP Configuration
            string SFTPUser = "";
            string SFTPKeyPath = "";
            string SFTPServerPath = "";
            int SFTPPortNumber = 22;
            string SFTPRemoteDirectory = "";
            string WinSCPPath = "";
            string SSHHostKeyFingerprint = "";


            // Read the file line by line and parse out configuration information.
            if (test)
            {
                Console.WriteLine("Working Path: " + WorkingPath);
            }
            System.IO.StreamReader file = new System.IO.StreamReader(string.Concat(WorkingPath, "Config.txt"));
            while ((line = file.ReadLine()) != null)
            {
                if(test)
                {
                    Console.WriteLine("Line Read: " + line);
                }
                int endofline = line.Length;
                int last = line.IndexOf("=");
                counter++;
                if (last != -1)
                {
                    string setting = line.Substring(0, last - 1).Trim();
                    Console.WriteLine("Setting: " + setting);
                    string value = line.Substring(last + 1, endofline - (last + 1)).Trim();
                    Console.WriteLine("Value: " + value);
                    Console.WriteLine(" ");

                    if (setting.Equals("PapercutPath"))
                    {
                        paperCutPath = value;
                    }
                    else if (setting.Equals("PapercutAPIKey"))
                    {
                        apiKey = value;
                    }
                    else if (setting.Equals("PapercutPort"))
                    {
                        paperCutPort = int.Parse(value);
                    }
                    else if (setting.Equals("sqlBillingUser"))
                    {
                        sqlUser = value;
                    }
                    else if (setting.Equals("sqlBillingPassword"))
                    {
                        sqlPass = value;
                    }
                    else if (setting.Equals("sqlBillingPath"))
                    {
                        sqlPath = value;
                    }
                    else if (setting.Equals("sqlBillingDatabase"))
                    {
                        sqlDatabase = value;
                    }
                    else if (setting.Equals("sqlBillingPrefix"))
                    {
                        sqlPrefix = value;
                    }
                    else if (setting.Equals("sqlBillingType"))
                    {
                        if (value.Equals("MSSQL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sqlType = SQLBillingServer.MSSQL;
                        }
                        else if (value.Equals("MYSQL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sqlType = SQLBillingServer.MYSQL;
                        }
                        else
                        {
                            //Unrecognized database type
                        }

                    }
                    else if (setting.Equals("oracleUser"))
                    {
                        oracleUser = value;
                    }
                    else if (setting.Equals("oraclePassword"))
                    {
                        oraclePass = value;
                    }
                    else if (setting.Equals("oraclePath"))
                    {
                        oraclePath = value;
                    }
                    else if (setting.Equals("ActiveDirectoryWhiteList"))
                    {
                        whiteList = value;
                    }
                    else if (setting.Equals("ActiveDirectoryBlackList"))
                    {
                        blackList = value;
                    }
                    else if (setting.Equals("BatchUserID"))
                    {
                        batchUserID = value;
                    }
                    else if (setting.Equals("BatchDetailCode"))
                    {
                        batchDetailCode = value;
                    }
                    else if (setting.Equals("SFTPUser"))
                    {
                        SFTPUser = value;
                    }
                    else if (setting.Equals("SFTPKeyPath"))
                    {
                        SFTPKeyPath = value;
                    }
                    else if (setting.Equals("SFTPServerPath"))
                    {
                        SFTPServerPath = value;
                    }
                    else if (setting.Equals("SFTPPortNumber"))
                    {
                        SFTPPortNumber = int.Parse(value);
                    }
                    else if (setting.Equals("SFTPRemoteDirectory"))
                    {
                        SFTPRemoteDirectory = value;
                    }
                    else if (setting.Equals("WinSCPPath"))
                    {
                        WinSCPPath = value;
                    }
                    else if (setting.Equals("SSHHostKeyFingerprint"))
                    {
                        SSHHostKeyFingerprint = value;
                    }
                    else
                    {
                        //unrecognized line. Do nothing
                    }
                    ////////
                }
            }

            file.Close();
            ///// pretending that the config file was done correctly!

            this.papercutServer = new PaperCutServer(paperCutPath, apiKey, paperCutPort);
            this.billingServer = new SQLBillingServer(sqlUser, sqlPass, sqlPath, sqlDatabase, sqlPrefix, sqlType, batchDetailCode, batchUserID);
            this.oracleServer = new OracleServer(oracleUser, oraclePass, oraclePath);
            this.activeDirectoryServer = new ActiveDirectoryServer(whiteList, blackList);
            this.FTPServer = new SFASSFTP(SFTPUser, SFTPKeyPath, SFTPServerPath, SFTPPortNumber, WinSCPPath, SSHHostKeyFingerprint, SFTPRemoteDirectory);
            if (test)
            {
                Console.WriteLine("Testing Oracle Connection:");
                Console.WriteLine("Retrieving Term Code from Oracle: " + this.oracleServer.GetCurrentTermCode());
                Console.WriteLine(" ");
                Console.WriteLine("Testing PaperCut Connection: ");
                Console.WriteLine("Total Number of PaperCut Users: " + this.papercutServer.GetTotalPaperCutUsers());
                Console.WriteLine(" ");
                Console.WriteLine("Testing Active Directory Connection:");
                this.activeDirectoryServer.TestActiveDirectoryConnection();
                Console.WriteLine("Testing Billing Server Connection:");
                Console.WriteLine("Test Must Be Made, not Doing anything Presently");
              
                if (papercutServer.RetrievePapercutUsers(billingServer))
                {
                    Console.WriteLine("Retrieved PaperCut User list and imported to table!");
                }
            }
        }

        protected void ProcessBilling()
        {
            LoadConfig(arguments); //Before Each billing Cycle, refresh configuration file and Items.
            //billingServer.ClearTemporaryTables(); This method should be unnecessary now that comparison workload is service side instead of sql side.

            //Pulls list of Papercut users from the database
            if (!papercutServer.RetrievePapercutUsers())
            {
                //Write Failure to Log
                return;
            }

            //Pull Blacklist and Whitelist of Users from AD
            if (!activeDirectoryServer.GetADuserLists())
            {
                //Write Failure to Log
                return;
            }

            billingServer.GenerateBillableUserList(activeDirectoryServer, papercutServer);

            if (!billingServer.PapercutUsersBillable()) //if there aren't billable users, return
            {
                return;
            }
            //There are billable users, submit the batch of users for billing! Runs a billing until the billing has been run across all other users.
            while (this.billingServer.GenerateBilling(papercutServer, oracleServer));

            //Now that File(s) are generated, retrieve list of the billings and submit them.
            this.FTPServer.UploadBillings(billingServer.GetCompletedBillings());

            //When Method is complete, force garbage collection to scrap all resources
            GC.Collect();  
        }
    }
}
