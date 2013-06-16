using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace PapercutSFASBilling
{
    class SFASSFTP
    {
        
        SessionOptions sessionOptions;
        string remoteDirectory;
        string executablePath;
        
        public SFASSFTP(string SFTPUser, string SFTPKeyPath, string SFTPServerPath, int SFTPPortNumber, string WinSCPPath, string SSHHostKeyFingerprint, string RemoteDirectory)
        {
            sessionOptions = new SessionOptions {
                Protocol = Protocol.Sftp,
                HostName = SFTPServerPath,
                UserName = SFTPUser,
                PortNumber = SFTPPortNumber,
                SshHostKeyFingerprint = SSHHostKeyFingerprint,
                SshPrivateKeyPath = SFTPKeyPath
            };
            remoteDirectory = RemoteDirectory;
            executablePath = WinSCPPath;
        }

        public bool UploadBillings(string[] billingFiles)
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
