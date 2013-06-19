using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace PapercutSFASBilling
{
    public class EmailServer
    {
        NetworkCredential cred;
        string fromAddress;
        string smtpServer;
        int smtpPort = 25;
        bool enableSsl = true;
        string recipientAddress;

        public EmailServer(string fromAddress, string recipientAddress, string smtpServer, int smtpPort, bool sslEnable, string smtpUser, string smtpPassword)
        {
            cred = new NetworkCredential(smtpUser, smtpPassword);
            this.fromAddress = fromAddress;
            this.recipientAddress = recipientAddress;
            this.smtpServer = smtpServer;
            this.smtpPort = smtpPort;
            this.enableSsl = sslEnable;
        }

        public bool SendSummaryEmail(SQLBillingServer Billing, List<char[]> BillingIDs)
        {
            StringBuilder mess = new StringBuilder();
            foreach(char[] bill in BillingIDs)
            {
                mess.Append("Billing ID :");
                mess.Append(bill);
                mess.Append(" Transaction Total = ");
                mess.Append(Billing.GetBillingTotal(bill));
                mess.Append(" Billing Total = ");
                mess.Append(Billing.GetBillingTotalB(bill));
                mess.Append("\n");
            }
            mess.Append("\n");
            mess.Append("\n");
            mess.Append("Today's Errors: \n");
            try{
                using(System.IO.StreamReader temp = new System.IO.StreamReader(Billing.GetErrorPath()))
                {
                    while (!temp.EndOfStream){
                        mess.Append(temp.ReadLine());
                        mess.Append("\n");
                    }
                }
            }catch(Exception e){
                //File not found!
                //There were no errors
                Console.WriteLine(e.Message);
            }
            MailMessage msg = new MailMessage();
            msg.To.Add(recipientAddress); 
            msg.From = new MailAddress(fromAddress); 
            msg.Subject = string.Concat("Billing on ", DateTime.Now.ToString("MM/dd/yyyy")); 
            msg.Body = mess.ToString(); 

            SmtpClient client = new SmtpClient(smtpServer, smtpPort);
            client.Credentials = cred; 
            client.EnableSsl = enableSsl; 
            client.Send(msg);

            return true;
        }

        public void sendMessage(string Message)
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(recipientAddress);
            msg.From = new MailAddress(fromAddress);
            msg.Subject = string.Concat("Billing on ", DateTime.Now.ToString("MM/dd/yyyy"));
            msg.Body = Message;

            SmtpClient client = new SmtpClient(smtpServer, smtpPort);
            client.Credentials = cred;
            client.EnableSsl = enableSsl;
            client.Send(msg);
        }
    }
}
