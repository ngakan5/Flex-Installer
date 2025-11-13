using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace FlexInstaller {
public class FileTransferManager
{
private ProgressBar barProgress;
private Label txtStatus;
private WebClient client;
private bool finished;
private bool worked;

public FileTransferManager(ProgressBar progress,Label status) {
barProgress=progress;
txtStatus=status;
}

public async Task<bool> RetrieveFileFromCloud(string webUrl,string localFile)
{
try {
if(string.IsNullOrEmpty(webUrl)||!Uri.IsWellFormedUriString(webUrl,UriKind.Absolute)) {
txtStatus.Text="Invalid download URL";
return false;
}

txtStatus.Text="Connecting to download server...";
Application.DoEvents();

ServicePointManager.SecurityProtocol=(SecurityProtocolType)3072|(SecurityProtocolType)768|(SecurityProtocolType)192;
ServicePointManager.ServerCertificateValidationCallback=delegate{return true;};

client=new WebClient();
client.Headers.Add("User-Agent",AppConfig.appName+" Installer v"+AppConfig.appVer);

finished=false;
worked=false;

client.DownloadProgressChanged+=(sender,e)=> {
barProgress.Value=e.ProgressPercentage;
txtStatus.Text=string.Format("Downloading... {0}% ({1:F1} MB of {2:F1} MB)",
e.ProgressPercentage,
e.BytesReceived/1024.0/1024.0,
e.TotalBytesToReceive/1024.0/1024.0);
Application.DoEvents();
};

client.DownloadFileCompleted+=(sender,e)=> {
finished=true;
if(e.Error!=null) {
txtStatus.Text=string.Format("Download failed: {0}",e.Error.Message);
worked=false;
} else if(e.Cancelled) {
txtStatus.Text="Download was cancelled";
worked=false;
} else {
txtStatus.Text="Download completed successfully";
worked=true;
}
};

client.DownloadFileAsync(new Uri(webUrl),localFile);

while(!finished) {
await Task.Delay(100);
Application.DoEvents();
}

client.Dispose();
return worked;
} catch(Exception ex) {
txtStatus.Text=string.Format("Download error: {0}",ex.Message);
if(client!=null) {
client.Dispose();
}
return false;
}
}

public static void Dispose() {
}
}
}
