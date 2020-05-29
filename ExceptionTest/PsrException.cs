using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;

namespace ExceptionTest {
    public class PsrException
    {
        // Original credits: https://stackoverflow.com/questions/24960786/integrate-psr-in-a-vb-net-application

        private int _timeSpent;
        private bool _psrRunning;
        private const string MAILHOST = "yourserver.tld";
        private const string FROM = "who@domain.tld";
        private const string TO = "exceptions@domain.tld";

        public string HandleException()
        {
            Version osVersion = Environment.OSVersion.Version;
            Version win7Version = Version.Parse("6.1.7600");
            string psrFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Exception_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip"; 
            const int I32_TIMEOUT = 600000; // 10 Minutes
            const int I32_SLEEP_INTERVAL = 200; // 0,2 Seconds

            _timeSpent = 0;
            _psrRunning = false;
            try
            {
                // Check Windows version
                if (osVersion.CompareTo(win7Version) < 0)
                {
                    MessageBox.Show("You must have Windows 7 or newer", "Unusable function", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return "Err: OS too old";
                }
                string psrPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\psr.exe ";

                // Check if psr.exe exists (should always be true, but who knows...)
                if (File.Exists(psrPath) == false)
                {
                    MessageBox.Show("Could not find file 'psr.exe' in folder " + Environment.GetFolderPath(Environment.SpecialFolder.System), "PSR.EXE not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "Err: PSR not found";

                }

                MessageBox.Show("This application crashed, please use this tool to tell us what you did and what happened. After that please close this program.", "Application Crash", MessageBoxButtons.OK);

                // Start PSR and check event when finished
                Process psr = new Process();
                psr.StartInfo.FileName = psrPath;
                psr.StartInfo.CreateNoWindow = true;
                psr.StartInfo.Arguments = " /start /maxsc 50 /exitonsave 1 /output " + psrFile;
                psr.EnableRaisingEvents = true;
                psr.Start();

                _psrRunning = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error executing PSR." + ex.Message, "Retry.");
                return "Err: PSR exception " + ex.Message;
            }
            // Wait for "Exited" event for the time specified in i32Timeout (10 minutes)
            while (_psrRunning)
            {
                Process[] pname = Process.GetProcessesByName("psr");

                _timeSpent += I32_SLEEP_INTERVAL;
                if (_timeSpent > I32_TIMEOUT)
                {
                    MessageBox.Show("Timeout (10 minutes). We stop tracking", "Operation not completed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    if (pname.Length != 0) {
                        pname[0].Kill();
                    }
                    return "Err: Timeout";
                }

                // Process exited
                if (pname.Length == 0) {
                    _psrRunning = false;
                }

                System.Threading.Thread.Sleep(I32_SLEEP_INTERVAL);
            }
            MessageBox.Show("Thanks, your steps have been recorded and now we are sending them by mail", "Operation completed", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            string exitMessage = string.Empty;
            try
            {
                
                MailMessage mailToBeSent = new MailMessage(FROM, TO, "Exception occured - file attached", "Check the .zip file attached");
                if (!File.Exists(psrFile))
                {
                    MessageBox.Show("Il file " + psrFile + " was NOT found." + Environment.NewLine + "Check folder " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + " & and send it to support manually", "PSR file not found.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    exitMessage = "Err: PDR file not found";
                    return exitMessage;
                }
                Attachment psrAttachment = new Attachment(psrFile);
                mailToBeSent.Attachments.Add(psrAttachment);
                SmtpClient mailClient = new SmtpClient(MAILHOST);
                mailClient.UseDefaultCredentials = true;
                mailClient.Send(mailToBeSent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending mail " + ex.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                exitMessage = "Err: mail NOT sent";
            }
            return exitMessage;
        }


    }
}

