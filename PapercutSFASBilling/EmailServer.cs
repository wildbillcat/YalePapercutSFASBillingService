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

        public bool SendSummaryEmail(SQLBillingServer Billing)
        {
            string message = "Billing(s) have been run. Summary: \n ";
            foreach(char[] bill in Billing.GetCompletedBillingIDs())
            {
                message = string.Concat(message, "Billing ID: ", new string(bill), " = ", Billing.GetBillingTotal(bill), "\n");
            }
            MailMessage msg = new MailMessage();
            msg.To.Add(recipientAddress); 
            msg.From = new MailAddress(fromAddress); 
            msg.Subject = string.Concat("Billing on ", DateTime.Now.ToString("MM/dd/yyyy")); 
            msg.Body = message; 

            SmtpClient client = new SmtpClient(smtpServer, smtpPort);
            client.Credentials = cred; 
            client.EnableSsl = enableSsl; 
            client.Send(msg);

            return true;
        }
    }
}
