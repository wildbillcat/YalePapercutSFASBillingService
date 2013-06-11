using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace PapercutSFASBilling
{
    class ActiveDirectoryServer
    {
        private string[] blackListedGroups;
        private string[] whiteListedGroups;
        private int BlackListLength;
        private int WhiteListLength;
        private bool test;

        public ActiveDirectoryServer(string whiteList, string blackList)
        {
            this.whiteListedGroups = this.ParseList(whiteList);
            this.blackListedGroups = this.ParseList(blackList);
        }

        private string[] ParseList(string list)
        {
            string[] pList;
            if (list.Length > 0)
            {
                char[] delimiters = { ',' };
                pList = list.Split(delimiters);
                return pList;
            }
            pList = new string[0];
            return pList;
        }

        public void TestActiveDirectoryConnection()
        {
            this.test = true;
            GetADuserLists(new SQLBillingServer());
        }

        public bool GetADuserLists(SQLBillingServer billingServer)
        {
            List<string> whiteList = new List<string>();
            List<string> blackList = new List<string>();

            for (int i = 0; i < whiteListedGroups.Length; i++) //Fetches all members from the whitelist
            {
                if (test)
                {
                    Console.WriteLine("Fetching White List Group: " + whiteListedGroups[i]);
                }
                whiteList = GetGroupList(whiteListedGroups[i], whiteList);
                if (test)
                {
                    Console.WriteLine("White List Members: " + whiteList.Count());
                }

            }
            for (int i = 0; i < blackListedGroups.Length; i++) //Fetches all members from the blacklist
            {
                if (test)
                {
                    Console.WriteLine("Fetching Black List Group :" + blackListedGroups[i]);
                } 
                blackList = GetGroupList(blackListedGroups[i], blackList);
                if (test)
                {
                    Console.WriteLine("Black List Members: " + blackList.Count());
                }
            }
            if (!test)
            {
                WhiteListLength = whiteList.Count();
                if (this.WhiteListLength != 0 && !billingServer.SubmitUsersToDB(whiteList, SQLBillingServer.WHITELIST))
                {
                    return false;
                }
                BlackListLength = blackList.Count();
                if (whiteList.Count != 0 && billingServer.SubmitUsersToDB(blackList, SQLBillingServer.BLACKLIST))
                {
                    return false;
                }
            }
            return true; //Made it to end of function!
        }

        public int GetWhiteListLength()
        {
            return WhiteListLength;
        }

        public int GetBlackListLength()
        {
            return BlackListLength;
        }

        private List<string> GetGroupList(string groupName, List<string> NetIDs)
        {
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            // find the group in question
            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupName);
            // if found....
            if (group != null)
            {
                foreach (Principal p in group.GetMembers(true))
                {
                    if (test)
                    {
                        Console.WriteLine(string.Concat("Member: ", p.DisplayName, " - ", p.GetType()));
                    }
                    NetIDs.Add(p.DisplayName);
                }
                group.Dispose();
            }
            NetIDs = NetIDs.Distinct().ToList();
            ctx.Dispose();
            return NetIDs;
        }
    
    
    }
}
