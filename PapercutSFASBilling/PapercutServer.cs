using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PapercutSFASBilling
{

    public class PaperCutServer
    {
        ServerCommandProxy serverProxy;
        private List<string> papercutUsers;

        public PaperCutServer(string path, string apikey, int port)
        {
            serverProxy = new ServerCommandProxy(path, port, apikey);
            papercutUsers = new List<string>();
        }

        public int GetTotalPaperCutUsers()
        {
            // string[] printers = this.serverProxy.ListPrinters(0, 5);
            //return printers.Length; 
            return this.serverProxy.GetTotalUsers();
        }

        public bool AdjustUserBalance(string User, double Amount, string comment)
        {
            try
            {
                serverProxy.AdjustUserAccountBalance(User, Amount, comment, "");
                return true; //No exception thrown, success!
            }
            catch
            {
                return false;//Something went wrong, account not adjusted
            }
        }

        public bool RetrievePapercutUsers()
        {
            int noUsers = this.serverProxy.GetTotalUsers();
            int noUsers2 = noUsers;
            int i = 0;
            while (noUsers2 > 0)
            {
                if ((i + 1) * 1000 < noUsers)
                {
                    this.papercutUsers.AddRange(this.serverProxy.ListUserAccounts(i * 1000, (i + 1) * 1000).ToList());
                }
                else
                {
                    this.papercutUsers.AddRange(this.serverProxy.ListUserAccounts(i * 1000, noUsers).ToList());
                }
                i++;
                noUsers2 = noUsers2 - 1000;
            }
            papercutUsers = papercutUsers.Distinct<string>().ToList<string>();
            return true;
        }

        public List<string> GetPapercutUsers()
        {
            return this.papercutUsers;
        }

        //Redundant Method, removing temporary DB Backend
        public bool RetrievePapercutUsers(SQLBillingServer billingServer)
        {
            int noUsers = this.serverProxy.GetTotalUsers();
            int noUsers2 = noUsers;
            int i = 0;
            List<string> users = new List<string>();
            while (noUsers2 > 0)
            {
                if ((i + 1) * 1000 < noUsers)
                {
                    users = this.serverProxy.ListUserAccounts(i * 1000, (i + 1) * 1000).ToList();
                }
                else
                {
                    users = this.serverProxy.ListUserAccounts(i * 1000, noUsers).ToList();
                }
                i++;
                noUsers2 = noUsers2 - 1000;
                if (!billingServer.SubmitUsersToDB(users, SQLBillingServer.MAINLIST))
                {
                    return false; //Failed to submit users
                }
            }
            return true;
        }

        public List<PapercutUser> RetrievePapercutBalances(List<string> userList)
        {
            List<PapercutUser> Users = new List<PapercutUser>();
            foreach (string user in userList)
            {
                Users.Add(new PapercutUser(user, this.serverProxy.GetUserAccountBalance(user, "")));
            }
            return Users;
        }
}
    public struct PapercutUser
    {
        public string NetID;
        public double balance;

        public PapercutUser(string name, double bal)
        {
            NetID = name;
            balance = bal;
        }
    }


}
