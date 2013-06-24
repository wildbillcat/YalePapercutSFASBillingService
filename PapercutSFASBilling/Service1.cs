using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WinSCP;

namespace PapercutSFASBilling
{
    public partial class Service1 : ServiceBase
    {

        protected PaperCutServer papercutServer;
        protected SQLBillingServer billingServer;
        protected OracleServer oracleServer;
        protected ActiveDirectoryServer activeDirectoryServer;
        protected SFASSFTP FTPServer;
        protected EmailServer emailServer;
        protected string WorkingPath;
        protected string[] arguments;
        protected Timer tm;
        protected DateTime LastBilling;
        protected bool SendBillingSummary;
        static bool Vconsole = false;
        

        public Service1()
        {
            InitializeComponent();
            using (System.IO.StreamWriter Op = new System.IO.StreamWriter("C:\\debug.txt"))
            {
                Op.WriteLine("Started!");
            }
            /*
            if (!System.Diagnostics.EventLog.SourceExists("PapercutSFASBilling"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "PapercutSFASBilling", "PapercutSFASBillingLog");
            }
            eventLog1.Source = "PapercutSFASBilling";
            eventLog1.Log = "PapercutSFASBillingLog";
             */
        }

        protected override void OnStart(string[] args)
        {
                //LoadConfig(args); //Loading the configuration file takes too long, causing the service startup to fail.
                LastBilling = DateTime.Now.AddDays(-1);
                tm = new Timer(1000);
                tm.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                tm.Enabled = true;
                tm.AutoReset = true;
                //tm.Interval = 6000; // 6 Seconds
                tm.Start();
                GC.KeepAlive(tm);
                //tm.Interval = 60000; // 1 Minutes
                //tm.Interval = 300000; // 5 Minutes
                //tm.Interval = 1800000; // 30 Minutes
                using (System.IO.StreamWriter Op = new System.IO.StreamWriter("C:\\debug.txt", true))
                {
                    Op.WriteLine("Started!");
                }
                if (Vconsole)
                {
                    Console.WriteLine("On Start Ran!");
                }
        }

        public static void Main(string[] args)
        {
            Vconsole = true;
            Service1 S = new Service1();
            S.TestRun();
            Console.ReadLine();
        }
        public void TestRun()
        {
            this.LoadConfig(null);
            this.ProcessBilling();
        }

        protected void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            using (System.IO.StreamWriter Op = new System.IO.StreamWriter("C:\\debug.txt", true))
            {
                Op.WriteLine(string.Concat("Timer Ran! last Billing: ",this.LastBilling.ToString()));
            
            //Each time the timer fires, check to see if it is the next day for billing.
            if (DateTime.Now.Day != LastBilling.Day)
            {
                Op.WriteLine(string.Concat("Last Billing not today! last Billing: ", this.LastBilling.ToString()));
                tm.Stop();
                this.ProcessBilling();
                LastBilling = billingServer.GetLastBilling();
                tm.Start();
            }
            }
        }

        protected override void OnStop()
        {
             tm.Enabled = false;
             tm.Stop();
        }
            
        private void LoadConfig(string[] args)
        {
            bool test = false;
            //Read Text file that points service at the Billing Directory
            using(System.IO.StreamReader dirPath = new System.IO.StreamReader("BillingDirectory.txt")){
                WorkingPath = dirPath.ReadLine();
            }
            //Parse Path
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
            //Test for Billing and Error Folders
            if (!System.IO.Directory.Exists(string.Concat(WorkingPath, "BillingErrors")))
            {
                System.IO.Directory.CreateDirectory(string.Concat(WorkingPath, "BillingErrors"));
            }
            if (!System.IO.Directory.Exists(string.Concat(WorkingPath, "BillingSubmissions")))
            {
                System.IO.Directory.CreateDirectory(string.Concat(WorkingPath, "BillingSubmissions"));
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
            string SFTPPassword = "";
            string SFTPKeyPath = "";
            string SFTPServerPath = "";
            int SFTPPortNumber = 22;
            string SFTPRemoteDirectory = "";
            string WinSCPPath = "";
            string SSHHostKeyFingerprint = "";
            Protocol FileProtocol = Protocol.Sftp;

            //SMTP Server Configuration
            string SMTPServer = "";
            int SMTPPort = 25;
            bool SSLEnabled = false;
            string SMTPUser = "";
            string SMTPPassword = "";
            string EmailFrom = "";
            string EmailTo = "";
            


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
/////////////////////PaperCut Server///////////////////////////////////////////////////////
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
////////////////////SQL Billing Server/////////////////////////////////////////////////////
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
////////////////////Oracle Server///////////////////////////////////////////
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
////////////////////Active Directory///////////////////////////////////////
                    else if (setting.Equals("ActiveDirectoryWhiteList"))
                    {
                        whiteList = value;
                    }
                    else if (setting.Equals("ActiveDirectoryBlackList"))
                    {
                        blackList = value;
                    }
////////////////////Batch Configuration Information////////////////////////
                    else if (setting.Equals("BatchUserID"))
                    {
                        batchUserID = value;
                    }
                    else if (setting.Equals("BatchDetailCode"))
                    {
                        batchDetailCode = value;
                    }
////////////////////SFTP Server Configuration//////////////////////////////
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
                    else if (setting.Equals("RemoteDirectory"))
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
                    else if (setting.Equals("SFTPPassword"))
                    {
                        SFTPPassword = value;
                    }
                    else if (setting.Equals("FileProtocol"))
                    {
                        if (value.Equals("SFTP", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileProtocol = Protocol.Sftp;
                        }
                        else if (value.Equals("FTP", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileProtocol = Protocol.Ftp;
                        }
                        else if (value.Equals("SCP", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileProtocol = Protocol.Scp;
                        }
                        else
                        {
                            //Unrecognized database type
                        }
                    }
                    ////////////////////Email Server Configuration////////////////////////////////
                    else if (setting.Equals("SMTPServer"))
                    {
                        SMTPServer = value;
                    }
                    else if (setting.Equals("SMTPPort"))
                    {
                        SMTPPort = int.Parse(value);
                    }
                    else if (setting.Equals("SSLEnabled"))
                    {
                        SSLEnabled = bool.Parse(value);
                    }
                    else if (setting.Equals("SMTPUser"))
                    {
                        SMTPUser = value;
                    }
                    else if (setting.Equals("SMTPPassword"))
                    {
                        SMTPPassword = value;
                    }
                    else if (setting.Equals("EmailFrom"))
                    {
                        EmailFrom = value;
                    }
                    else if (setting.Equals("EmailTo"))
                    {
                        EmailTo = value;
                    }
                    else if (setting.Equals("SendBillingSummary"))
                    {
                        SendBillingSummary = bool.Parse(value);
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
            this.billingServer = new SQLBillingServer(sqlUser, sqlPass, sqlPath, sqlDatabase, sqlPrefix, sqlType, batchDetailCode, batchUserID, WorkingPath);
            this.oracleServer = new OracleServer(oracleUser, oraclePass, oraclePath);
            this.activeDirectoryServer = new ActiveDirectoryServer(whiteList, blackList);
            if (FileProtocol.Equals(Protocol.Ftp))
            {
                this.FTPServer = new SFASSFTP(SFTPUser, SFTPPassword, SFTPServerPath, SFTPPortNumber, WinSCPPath, SFTPRemoteDirectory, FileProtocol);
            }
            else
            {
                this.FTPServer = new SFASSFTP(SFTPUser, SFTPPassword, SFTPKeyPath, SFTPServerPath, SFTPPortNumber, WinSCPPath, SSHHostKeyFingerprint, SFTPRemoteDirectory, FileProtocol);
            }
            this.emailServer = new EmailServer(EmailFrom, EmailTo, SMTPServer, SMTPPort, SSLEnabled, SMTPUser, SMTPPassword);
            this.LastBilling = billingServer.GetLastBilling();
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
              
                if (papercutServer.RetrievePapercutUsers())
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
                emailServer.sendMessage(string.Concat("Billing Failed! Could not pull Papercut Users! ", DateTime.Now.ToString("MM/dd/yyyy")));
                return;
            }

            //Pull Blacklist and Whitelist of Users from AD
            if (!activeDirectoryServer.GetADuserLists())
            {
                //Write Failure to Log
                emailServer.sendMessage(string.Concat("Billing Failed! Could not pull Active Directory Users! ", DateTime.Now.ToString("MM/dd/yyyy")));
                return;
            }

            billingServer.GenerateBillableUserList(activeDirectoryServer, papercutServer);

            if (!billingServer.PapercutUsersBillable()) //if there aren't billable users, return
            {
                LastBilling = DateTime.Now;
                emailServer.sendMessage(string.Concat("Billing Not Run, there are no billable users! ", LastBilling.ToString("MM/dd/yyyy")));
                return;
            }
            //There are billable users, submit the batch of users for billing! Runs a billing until the billing has been run across all other users.
            while (this.billingServer.GenerateBilling(papercutServer, oracleServer));

            //Now that File(s) are generated, retrieve list of the billings and submit them.
            if (this.FTPServer.UploadBillings(billingServer.GetCompletedBillings()))
            {
                //update Billing Status
                foreach(char[] bill in billingServer.GetCompletedBillingIDs()){
                    billingServer.UpdateBillingStatus(int.Parse(new string(bill)), 6);
                }
            }
            else
            {
                emailServer.sendMessage(string.Concat("Billing Run, but File not submitted. Error with Upload.", DateTime.Now.ToString("MM/dd/yyyy")));
            }

            //Send Summary e-mail
            if (SendBillingSummary)
            {
                emailServer.SendSummaryEmail(billingServer, billingServer.GetCompletedBillingIDs());
            }

            //When Method is complete, force garbage collection to scrap all resources
            GC.Collect();  
        }
    }
}
