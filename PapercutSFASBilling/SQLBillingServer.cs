using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PapercutSFASBilling
{

    public class SQLBillingServer
    {
        //Define static variables to easily determine database type
        public static int MSSQL = 1;
        public static int MYSQL = 2;

        //List Types
        public static int WHITELIST = 1;
        public static int BLACKLIST = 2;
        public static int MAINLIST = 3;

        public static string[] VALDSTATUS = new string[]{"EL", "FC", "HT", "LT", "NP"};

        private string sqlUser;
        private string sqlPass;
        private string sqlPath;
        private string sqlDatabase;
        private string sqlPrefix;
        private int sqlType;

        private string batchDetailCode;
        private string batchUserID;

        private List<PapercutUser> billableUsers;
        private List<string> billingsCompleted; //List of the Billing Batch IDs completed.

        //This constructor creates an invalid test object.
        public SQLBillingServer()
        {
            sqlUser = "test";
            sqlPass = "test";
            sqlPath = "path";
            sqlDatabase = "test";
            sqlPrefix = "test";
            sqlType = 0;
            batchDetailCode = "test";
            batchUserID = "test";
            billingsCompleted = new List<string>();
        }

        public SQLBillingServer(string user, string pass, string path, string db, string prefix, int type, string detailCode, string userID)
        {
            sqlUser = user;
            sqlPass = pass;
            sqlPath = path;
            sqlDatabase = db;
            sqlPrefix = prefix;
            sqlType = type;
            batchDetailCode = detailCode;
            batchUserID = userID;
            billingsCompleted = new List<string>(); 
        }

        //New method to generate billable users without bothering with using a Database Backend.
        public bool GenerateBillableUserList(ActiveDirectoryServer activeDirectoryServer, PaperCutServer paperCutServer)
        {
            List<string> mainList = new List<string>();
            List<string> papercutUsers; //Only used if there is a white list

            if (activeDirectoryServer.GetWhiteListLength() > 0)//If there is a white list, add white listed users that exist on PaperCut server to main list.
            {
                papercutUsers = paperCutServer.GetPapercutUsers();
                foreach (string whiteListedUser in activeDirectoryServer.GetWhitelist())
                {
                    if (papercutUsers.Contains(whiteListedUser)) //If White listed user exists in Papercut DB, add to master List.
                    {
                        mainList.Add(whiteListedUser);
                    }
                }
            }
            else //No White list, so user PaperCut Master list.
            {
                mainList = paperCutServer.GetPapercutUsers();
            }
            foreach(string blackListedUser in activeDirectoryServer.GetBlacklist())
            {
                mainList.Remove(blackListedUser);
            }
            List<PapercutUser> papercutBalances = paperCutServer.RetrievePapercutBalances(mainList);
            papercutBalances.RemoveAll(x => x.balance == 0);
            billableUsers = papercutBalances;
            return true;
        }

        public bool GenerateBilling(PaperCutServer Papercut, OracleServer Oracle)
        {
            //Normally a Billing can be completed in 1 go, however this will be set to true if an additional billing needs to be run.
            bool BillingIncomplete = false; 
            //List of any transaction errors that occur while generating billing.
            List<TransactionError> errorLog = new List<TransactionError>();
            //List of all successful transactions that are processed.
            List<BillingTransaction> transactionLedger = new List<BillingTransaction>();
            using (SqlConnection conn = new SqlConnection("Server=" + sqlPath + "; Database=" + sqlDatabase + "; User ID=" + sqlUser + "; Password=" + sqlPass + ";"))
            {
                conn.Open();
                string termCode = Oracle.GetCurrentTermCode();
                //Formatted Term Code for File
                char[] cTermCode = termCode.ToCharArray();
                //Formatted Activity Date for File
                char[] cActivityDate = DateTime.Now.ToString("MMddyyyy").ToCharArray();
                Console.WriteLine(cActivityDate);
                //Formatted Detail Code for File
                char[] cBatchDetailCode = BillingUtility.ValidDetailCode(batchDetailCode);
                //Formatted User ID for File
                char[] cBatchUserID = BillingUtility.ValidBatchUserID(batchUserID);

                //Total Cost of Billing
                double totalBilling = 0.00; //Absolute Value
                double batchTotalBalance = 0.00; //Overall Value
                /**************************************************************************************************************************************************
                 * This Section of the Method Generates the Transaction information and Validates Billable Users. 
                **************************************************************************************************************************************************/

                while(billableUsers.Count() > 0) // Iterate Through Each User Until there are no users left:
                {
                    PapercutUser user = billableUsers[0];
                    string[] oracleInfo = Oracle.GetUserInfo(user.NetID, termCode);
                    if (oracleInfo.Length == 4)//If it is has 4 values then oracle information was retrieved. Validate User:
                    {
                        if (BillingUtility.ValidStatus(oracleInfo[2]))//If it is true then the user can be billed.
                        {
                            try
                            {
                                //Where the system could have been set to use multiple transactions, it seems prudent that if a student has accrued charges in excess of 9,999,999.99 that someone should look into it, as an error is more likely.
                                if (Math.Abs(user.balance) > 9999999.99)
                                {
                                    throw new ValidationException(string.Concat("User: ", user.NetID, " has too large of balance to be billed. ($", user.balance, ")"));
                                }

                                if (totalBilling + Math.Abs(user.balance) > 9999999.99)
                                {
                                    // total billing would exceed with this user, break loop and submit billing.
                                    BillingIncomplete = true;
                                    break;
                                }

                                char[] amount = BillingUtility.FormatAmount(user.balance);
                                double billingAmount = double.Parse(new string(amount)) / 100; //This ensures that the numerical value and billed value match (Eliminating possibility of fractions of Cents.
                                char[] creditIndicator;
                                if (user.balance > 0)//If user has positive balance, a credit is sitting on their account. Set Credit indicator
                                {
                                    creditIndicator = new char[] { 'C', 'R' }; 
                                    billingAmount = billingAmount * -1;
                                }
                                else
                                {
                                    creditIndicator = new char[] { ' ', ' ' };
                                }
                                char[] userPID = BillingUtility.ValidPID(oracleInfo[0]);
                                char[] userSPRIDENID = BillingUtility.ValidSPRIDEN_ID(oracleInfo[1]);
                                transactionLedger.Add(new BillingTransaction(amount, billingAmount, creditIndicator, user.NetID, userPID, userSPRIDENID));
                                totalBilling = totalBilling + Math.Abs(billingAmount);//Absolute Sum
                                batchTotalBalance = batchTotalBalance + billingAmount;//Value Sum
                            }
                            catch (Exception e) //An exception was thrown on the 
                            {
                                //write error to error list.
                                errorLog.Add(new TransactionError(user.NetID, e.Message));
                            }
                        }
                        else
                        {
                            //User is not Billable, Status is invalid.
                            errorLog.Add(new TransactionError(user.NetID, string.Concat("User is not Billable due to invalid status. Status: ", oracleInfo[2], " : ", oracleInfo[3])));
                        }
                    }
                    else
                    {
                        //User does not exist in oracle, can not bill!
                        errorLog.Add(new TransactionError(user.NetID, string.Concat("User does not exit in Oracle in Term: ", termCode)));
                    }
                    billableUsers.Remove(user); //Done processing the user, either billed or not billable.
                }

                /**************************************************************************************************************************************************
                 * Temporary Transaction File Generation Section of Method 
                **************************************************************************************************************************************************/

                if (transactionLedger.Count() > 0) //Determines if there are any billable users. If so Generate billing, bill their PaperCut Accounts and Write the Transaction.
                {
                    try
                    {
                        //Generate New Billing ID
                        char[] cTotalBilling = BillingUtility.FormatAmount(totalBilling);
                        int billingID = this.GenerateNewBillingID(cTotalBilling, batchTotalBalance);
                        char[] BillingID = BillingUtility.FormatBatchNumber(billingID);
                        string billingTransactionSQL = string.Concat("Insert into ", this.sqlPrefix, "BillingTransactions (ActivityDate, Balance, BatchID, DetailCode, NetID, PIDM, SPRIDEN_ID, Amount, CreditIndicator, TermCode, BatchUserID) values(@ActivityDate, @Balance, @BatchID, @DetailCode, @NetID, @PIDM, @SPRIDEN_ID, @Amount, @CreditIndicator, @TermCode, @BatchUserID)");
                        SqlCommand billingTransactionQuery = new SqlCommand(billingTransactionSQL, conn);

                        double finTotalBilling = 0.00;//Absolute Sum
                        double finBatchTotalBalance = 0.00;//Value Sum

                        //Open file
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Concat(@"BillingSubmissions\", new string(BillingID), "_transactions.txt")))
                        {
                            //Now Write all detail records
                            foreach (BillingTransaction transaction in transactionLedger)
                            {
                                //If statement to credit PaperCut server, else don't bill, throw error.
                                if (Papercut.AdjustUserBalance(transaction.NetID, transaction.balance, string.Concat("Billing ID: ", new string(BillingID), ". ", cActivityDate)))
                                {
                                    billingTransactionQuery.Parameters.Clear();//Clear query for next transaction

                                    billingTransactionQuery.Parameters.AddWithValue("@Balance", transaction.balance); //Just used to Track Billing in DB
                                    billingTransactionQuery.Parameters.AddWithValue("@NetID", transaction.NetID); //Just used to Track Billing in DB

                                    file.Write(cBatchUserID);
                                    billingTransactionQuery.Parameters.AddWithValue("@BatchUserID", cBatchUserID);


                                    file.Write(BillingID);
                                    billingTransactionQuery.Parameters.AddWithValue("@BatchID", BillingID);

                                    file.Write('1'); //File only, denotes that this is a transaction entry instead of a header entry

                                    file.Write(transaction.PIDM);
                                    billingTransactionQuery.Parameters.AddWithValue("@PIDM", transaction.PIDM);

                                    file.Write(transaction.SPRIDEN_ID);
                                    billingTransactionQuery.Parameters.AddWithValue("@SPRIDEN_ID", transaction.SPRIDEN_ID);

                                    file.Write(cBatchDetailCode);
                                    billingTransactionQuery.Parameters.AddWithValue("@DetailCode", cBatchDetailCode);

                                    file.Write(cActivityDate);
                                    billingTransactionQuery.Parameters.AddWithValue("@ActivityDate", cActivityDate);

                                    file.Write(transaction.Amount);
                                    billingTransactionQuery.Parameters.AddWithValue("@Amount", transaction.Amount);

                                    file.Write(transaction.CreditIndicator);
                                    billingTransactionQuery.Parameters.AddWithValue("@CreditIndicator", transaction.CreditIndicator);

                                    file.Write(termCode);
                                    billingTransactionQuery.Parameters.AddWithValue("@TermCode", termCode);


                                    //File Specific Formatting
                                    file.WriteLine("                                       ");

                                    billingTransactionQuery.ExecuteNonQuery();//Submit Written Transaction to Database
                                   
                                    finTotalBilling = finTotalBilling + Math.Abs(transaction.balance);//Absolute Sum
                                    finBatchTotalBalance = finBatchTotalBalance + transaction.balance;//Value Sum
                                }
                                else
                                {
                                    //An error occurred trying to bill their PaperCut Account! 
                                    errorLog.Add(new TransactionError(transaction.NetID, "Error attempting to adjust User's PapercutAccount"));
                                }
                            }
                            file.Flush();
                        }//Complete writing out transactions

                        char[] finalAbsBilling = BillingUtility.FormatAmount(finTotalBilling);
                        this.UpdateBillingID(billingID, 0, finalAbsBilling, finBatchTotalBalance);
                        //Update Billing Entry with corrected information
                        /**************************************************************************************************************************************************
                         * Final Billing File Generation Section of Method 
                        **************************************************************************************************************************************************/
                        //Create Billing File
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Concat(@"BillingSubmissions\", new string(BillingID), ".txt")))
                        {
                            file.Write(cBatchUserID);
                            file.Write(BillingID);
                            file.Write('0');
                            file.Write(finalAbsBilling);
                            file.WriteLine(cActivityDate);
                            //Header Record complete

                            //Now Open Temporary file and append it to the final Billing Submission
                            using (System.IO.StreamReader temp = new System.IO.StreamReader(string.Concat(@"BillingSubmissions\", new string(BillingID), "_transactions.txt")))
                            {
                                while (!temp.EndOfStream)
                                {
                                    file.WriteLine(temp.ReadLine());
                                }
                            }
                            //Add billing to list of Completed Billings
                            string fullPath = ((System.IO.FileStream)(file.BaseStream)).Name;
                            billingsCompleted.Add(fullPath);
                        }

                        //Billing File Created, Delete temporary file
                        System.IO.File.Delete(string.Concat(@"BillingSubmissions\", new string(BillingID), "_transactions.txt"));
                    }
                    catch (Exception e)
                    {
                        //Something went Wrong with generation of Billing
                        errorLog.Add(new TransactionError("System Error: ", e.Message));
                    }


                }

                /**************************************************************************************************************************************************
                 * Final Billing File Generation Completed. Generating Error File Before Return 
                **************************************************************************************************************************************************/

                
                //Now Write out Errors: **Test if File exists, if so append, otherwise just write (or set to create if non existent and append?)
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Concat(@"BillingErrors\", new string(cActivityDate), "_Errors.txt"), true))
                {
                    foreach (TransactionError error in errorLog)
                    {
                        file.WriteLine(string.Concat(error.Username, " : ", error.Error));
                    }
                }
                
                conn.Close();
            }//End Using
            return BillingIncomplete;
        }
        

        public bool PapercutUsersBillable()
        {
            return (billableUsers.Count() > 0);
        }

        public bool SubmitUsersToDB(List<string> UserList, int table)
        {
            StringBuilder saveUsers = new StringBuilder();
            string createTable = " ";
            if (sqlType == MSSQL) //Microsoft SQL Server
            {
                saveUsers.Append("INSERT into ");
                saveUsers.Append(sqlPrefix);
                if (table == WHITELIST)
                {
                    createTable = "CREATE TABLE " + this.sqlPrefix + "temp_Whitelist ( [NetID] NVARCHAR(50) NOT NULL )";
                    saveUsers.Append("temp_WhiteList (NetID) Values");
                }
                else if (table == BLACKLIST)
                {
                    createTable = "CREATE TABLE " + this.sqlPrefix + "temp_Blacklist ( [NetID] NVARCHAR(50) NOT NULL )";
                    saveUsers.Append("temp_BlackList (NetID) Values");
                }
                else if (table == MAINLIST)
                {
                    createTable = "CREATE TABLE " +this.sqlPrefix + "temp_Mainlist ( [NetID] NVARCHAR(50) NOT NULL )";
                    saveUsers.Append("temp_MainList (NetID) Values");
                }
                foreach (string user in UserList)
                {
                    saveUsers.Append(" ('");
                    saveUsers.Append(user);
                    saveUsers.Append("'),");
                }
                string query = saveUsers.ToString();
                
                Console.WriteLine(query);
                query = query.Substring(0, query.Length - 1);
                using (SqlConnection conn = new SqlConnection("Server=" + sqlPath + "; Database=" + sqlDatabase + "; User ID=" + sqlUser + "; Password=" + sqlPass + ";"))
                {
                    conn.Open();
                    SqlCommand createTableQ = new SqlCommand(createTable, conn);
                    createTableQ.ExecuteNonQuery();
                    SqlCommand saveUsersQuery = new SqlCommand(query, conn);
                    int rows = saveUsersQuery.ExecuteNonQuery();
                    if (rows != UserList.Count()) //If number of rows not equal to number of users:
                    {
                        conn.Close();
                        return false; //Submission Failed
                    }
                    conn.Close();
                }
            }
            
            return true;
        }

        public int GenerateNewBillingID(char[] BatchTotal,double BatchTotalBalance)
        {
            int ID;

            //MS SQL Server
            string billingGen = string.Concat("Insert into ", this.sqlPrefix, "Billings (BatchStatus, BatchTotal, BatchTotalBalance) output inserted.BatchID values(@BatchStatus, @BatchTotal, @BatchTotalBalance)");
            using (SqlConnection conn = new SqlConnection("Server=" + sqlPath + "; Database=" + sqlDatabase + "; User ID=" + sqlUser + "; Password=" + sqlPass + ";"))
            {
                conn.Open();
                SqlCommand generateBilling = new SqlCommand(billingGen, conn);
                generateBilling.Parameters.AddWithValue("@BatchStatus", 1);
                generateBilling.Parameters.AddWithValue("@BatchTotal", BatchTotal);
                generateBilling.Parameters.AddWithValue("@BatchTotalBalance", BatchTotalBalance);
                generateBilling.ExecuteNonQuery();
                ID = int.Parse(generateBilling.ExecuteScalar().ToString());
                conn.Close();
            }
            return ID;
        }

        public void UpdateBillingID(int batchID, int batchStatus, char[] batchTotal, double batchTotalBalance)
        {
            try
            {
                string UpdateBilling = string.Concat("UPDATE ", this.sqlPrefix, "Billings SET BatchStatus = @BatchStatus, BatchTotal = @BatchTotal, BatchTotalBalance = @BatchTotalBalance WHERE BatchID = @BatchID");
                using (SqlConnection conn = new SqlConnection("Server=" + sqlPath + "; Database=" + sqlDatabase + "; User ID=" + sqlUser + "; Password=" + sqlPass + ";"))
                {
                    conn.Open();
                    SqlCommand updateBilling = new SqlCommand(UpdateBilling, conn);
                    updateBilling.Parameters.AddWithValue("@BatchStatus", batchStatus);
                    updateBilling.Parameters.AddWithValue("@BatchTotal", batchTotal);
                    updateBilling.Parameters.AddWithValue("@BatchTotalBalance", batchTotalBalance);
                    updateBilling.Parameters.AddWithValue("@BatchID", batchID);
                    updateBilling.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                //ERROR WITH QUERY!!!!
                Console.Error.WriteLine(e.Message);
                throw new Exception(string.Concat("Billing ID not Updated! Error: ", e.Message));
            }
        }

        public List<string> GetCompletedBillings()
        {
            return billingsCompleted;
        }

        private static class BillingUtility
        {
            public static char[] FormatAmount(double amount)
                {
                    string amount2 = (Math.Abs(amount*100)).ToString("0.00");
                    string cAmount = amount2.Substring(0, amount2.IndexOf('.')); //Find the Absolute Value, then convert it to a string
                    cAmount.Remove(cAmount.Length - 3, 1); //Removes the "." from the number
                    if(cAmount.Length>9)//If the length is greater than 10 characters, then the bill is too much to be billed in a single batch transaction
                    {
                        throw new ValidationException(string.Concat("Amount user is to be billed is too great for the system. Amount: ", cAmount.ToString()));
                    }
                    while (cAmount.Length < 9) //Loop zero fills array to proper length.
                    {
                        cAmount = string.Concat("0", cAmount);
                    }
                    return cAmount.ToCharArray();
                }

            public static char[] FormatBatchNumber(int batchID)
            {
                string temp = batchID.ToString();
                if (temp.Length > 5)
                {
                    throw new ValidationException(string.Concat("The Batch ID Returned by the database is over 5 characters! ", temp));
                }
                while (temp.Length < 5)
                {
                    temp = string.Concat("0", temp);
                }
                return temp.ToCharArray();
            }

            public static bool ValidStatus(string status)
            {
                for (int i = 0; i < SQLBillingServer.VALDSTATUS.Length; i++)
                {
                    if (status.Equals(SQLBillingServer.VALDSTATUS[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static char[] ValidDetailCode(string detailCode)
            {
                if (detailCode.Length > 4)
                {
                    throw new ValidationException(string.Concat("The detail code is greater than 4 characters: ", detailCode));
                }
                while(detailCode.Length < 4)
                {
                    detailCode = string.Concat(detailCode, " "); //add spaces to get the detail code to 4 characters.
                }
                return detailCode.ToCharArray();
            }

            public static char[] ValidBatchUserID(string batchUserID)
            {
                if (batchUserID.Length > 8)
                {
                    throw new ValidationException(string.Concat("The BatchUserID is too many characters (Limit 8): ", batchUserID));
                }
                while(batchUserID.Length < 8)
                {
                    batchUserID = string.Concat(batchUserID, " ");
                }
                return batchUserID.ToCharArray();
            }

            public static char[] ValidPID(string pid)
            {
                if (pid.Length > 8)
                {
                    throw new ValidationException(string.Concat("The PID number is too long! PID:", pid));
                }
                while (pid.Length < 8)
                {
                    pid = string.Concat(" ", pid);
                }
                return pid.ToCharArray();
            }

            public static char[] ValidSPRIDEN_ID(string spriden_id)
            {
                if (spriden_id.Length > 9)
                {
                    throw new ValidationException(string.Concat("The PID number is too long! PID:", spriden_id));
                }
                while (spriden_id.Length < 9)
                {
                    spriden_id = string.Concat(" ", spriden_id);
                }
                return spriden_id.ToCharArray();
            }
        }
    }

    public struct BillingTransaction
    {
        /// <summary>
        /// The User's university netID
        /// </summary>
        public string NetID;
        /// <summary>
        /// The User's Balance from PaperCut
        /// </summary>
        public double balance;
        /// <summary>
        /// The Banner PID Number (8 Char)
        /// </summary>
        public char[] PIDM;
        /// <summary>
        /// The Spriden_ID of User (9 Char) (Blanks if unavailable)
        /// </summary>
        public char[] SPRIDEN_ID;
        /// <summary>
        /// The Char array of the amount (9 Char)
        /// </summary>
        public char[] Amount;
        /// <summary>
        /// Defines if Transaction is a credit. IE: Positive papercut balance. Use "CR" , else "  ". (2 char)
        /// </summary>
        public char[] CreditIndicator;

        /// <summary>
        /// Constructor
        /// </summary>
        public BillingTransaction(char[] amount, double Balance, char[] creditIndicator, string netID, char[] PIDM, char[] SPRIDEN_ID)
        {
            this.Amount = amount;
            this.balance = Balance;
            this.CreditIndicator = creditIndicator;
            this.NetID = netID;
            this.PIDM = PIDM;
            this.SPRIDEN_ID = SPRIDEN_ID;
        }
    }
}
