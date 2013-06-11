using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.DataAccess.Client;
using System.Data.Common;
using System.Data;

namespace PapercutSFASBilling
{
    public class OracleServer
    {
        private string constr;
        private string ProviderName = "Oracle.DataAccess.Client";
        private DbProviderFactory factory;

        public OracleServer(string user, string pass, string path)
        {
            constr = "User Id=" + user + "; Password=" + pass + "; Data Source=" + path + ";";
            factory = DbProviderFactories.GetFactory(ProviderName);
        }

        /// <summary>
        /// Returns the Current TermCode
        /// </summary>
        public string GetCurrentTermCode()
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = constr;
                    conn.Open();

                    OracleCommand cmd = (OracleCommand)factory.CreateCommand();
                    cmd.Connection = (OracleConnection)conn;

                    
                    cmd.CommandText = "select current_term_code from syvctrm_with_su";

                    OracleDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        string termcode = reader.GetString(0);
                        return termcode;
                    }
                    else
                    {
                        Console.WriteLine("Error! No rows returned for the TermCode in Oracle!");
                        return "ERROR";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return "ERROR";
                }
            }
        }

        /// <summary>
        /// Array of strings, Cell 0 = PIDM, Cell 1 = SPRIDEN_ID, Cell 2 = Student Status
        /// </summary>
        /// <param name="NetID">User's NetID</param>
        /// <param name="TermCode">Term Code for the billing</param>
        public string[] GetUserInfo(string NetID, string TermCode)
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = constr;
                    conn.Open();

                    OracleCommand cmd = (OracleCommand)factory.CreateCommand();
                    cmd.Connection = (OracleConnection)conn;

                    //Build Query to get student information
                    string sqlQuery = string.Concat("SELECT syvyids_pidm, syvyids_spriden_id, stvests_code, stvests_desc FROM syvyids, sfbetrm, stvests WHERE (syvyids_pidm = sfbetrm_pidm) AND (sfbetrm_term_code = '", TermCode, "') AND (sfbetrm_ests_code = stvests_code) AND (syvyids_netid = '", NetID, "')");
                    //Console.WriteLine("Query: " + sqlQuery);
                    cmd.CommandText = sqlQuery;

                    OracleDataReader reader = cmd.ExecuteReader();
                    
                    if (reader.HasRows)
                    {
                        string[] userinfo = new string[4] { reader.GetInt32(0).ToString(), reader.GetString(1), reader.GetString(2), reader.GetString(3) };
                        return userinfo;
                    }
                    else
                    {
                        Console.WriteLine("Error! No rows returned for the User in Oracle!");
                        return new string[]{"No Rows in Oracle"};
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return new string[]{"ERROR"};
                }
            }
        }


    }

}
