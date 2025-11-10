using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace FlexInstaller
{
    public class FileTransferManager
    {
        private ProgressBar progBar;
        private Label lblStatus;
        private WebClient webClient;
        private bool dlComplete;
        private bool dlSuccess;

        public FileTransferManager(ProgressBar progress, Label status)
        {
            progBar = progress;
            lblStatus = status;
        }

        public async Task<bool> RetrieveFileFromCloud(string sourceUrl, string destinationPath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceUrl) || !Uri.IsWellFormedUriString(sourceUrl, UriKind.Absolute))
                {
                    lblStatus.Text = "Invalid download URL";
                    return false;
                }

                lblStatus.Text = "Connecting to download server...";
                Application.DoEvents();

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                webClient = new WebClient();
                webClient.Headers.Add("User-Agent", AppConfig.appName + " Installer v" + AppConfig.appVer);
                
                dlComplete = false;
                dlSuccess = false;

                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    progBar.Value = e.ProgressPercentage;
                    lblStatus.Text = string.Format("Downloading... {0}% ({1:F1} MB of {2:F1} MB)", 
                        e.ProgressPercentage, 
                        e.BytesReceived / 1024.0 / 1024.0,
                        e.TotalBytesToReceive / 1024.0 / 1024.0);
                    Application.DoEvents();
                };

                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    dlComplete = true;
                    if (e.Error != null)
                    {
                        lblStatus.Text = string.Format("Download failed: {0}", e.Error.Message);
                        dlSuccess = false;
                    }
                    else if (e.Cancelled)
                    {
                        lblStatus.Text = "Download was cancelled";
                        dlSuccess = false;
                    }
                    else
                    {
                        lblStatus.Text = "Download completed successfully";
                        dlSuccess = true;
                    }
                };

                webClient.DownloadFileAsync(new Uri(sourceUrl), destinationPath);

                while (!dlComplete)
                {
                    await Task.Delay(100);
                    Application.DoEvents();
                }

                webClient.Dispose();
                return dlSuccess;
            }
            catch (Exception ex)
            {
                lblStatus.Text = string.Format("Download error: {0}", ex.Message);
                if (webClient != null)
                {
                    webClient.Dispose();
                }
                return false;
            }
        }

        public static void Dispose()
        {
        }
    }
}
