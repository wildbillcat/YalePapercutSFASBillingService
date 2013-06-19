using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace PapercutSFASBilling
{
    public class SFASSFTP
    {
        
        SessionOptions sessionOptions;
        string remoteDirectory;
        string executablePath;
        
        //SFTP/SCP Constructor
        public SFASSFTP(string SFTPUser, string SFTPPassword, string SFTPKeyPath, string SFTPServerPath, int SFTPPortNumber, string WinSCPPath, string SSHHostKeyFingerprint, string RemoteDirectory, Protocol FileProtocol)
        {
            sessionOptions = new SessionOptions
            {
                Protocol = FileProtocol,
                HostName = SFTPServerPath,
                UserName = SFTPUser,
                Password = SFTPPassword,
                PortNumber = SFTPPortNumber,
                SshHostKeyFingerprint = SSHHostKeyFingerprint,
                SshPrivateKeyPath = SFTPKeyPath
            };
            remoteDirectory = RemoteDirectory;
            executablePath = WinSCPPath;
        }

        //FTP Constructor
        public SFASSFTP(string SFTPUser, string SFTPPassword, string SFTPServerPath, int SFTPPortNumber, string WinSCPPath, string RemoteDirectory, Protocol FileProtocol)
        {
            sessionOptions = new SessionOptions
            {
                Protocol = FileProtocol,
                HostName = SFTPServerPath,
                UserName = SFTPUser,
                Password = SFTPPassword,
                PortNumber = SFTPPortNumber
            };
            remoteDirectory = RemoteDirectory;
            executablePath = WinSCPPath;
        }

        public bool UploadBillings(List<string> billingFiles)
        {
            try
            {
                using (Session session = new Session())
                {
                    //Set the Executable Path
                    session.ExecutablePath = executablePath;

                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    foreach (string file in billingFiles)
                    {
                        transferResult = session.PutFiles(file, remoteDirectory, false, transferOptions);
                        transferResult.Check();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        

    }
}
