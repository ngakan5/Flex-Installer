using System;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;
using System.Diagnostics;

namespace FlexInstaller
{
    static class ApplicationEntry
    {
        [STAThread]
        static void Main(string[] args)
        {
            string exeName = Path.GetFileName(Application.ExecutablePath).ToLower();
            bool isUninstaller = exeName.Contains("uninstall") || (args.Length > 0 && args[0] == "/uninstall");

            if (isUninstaller)
            {
                if (AppConfig.requireAdmin && !IsRunningAsAdministrator())
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = Environment.CurrentDirectory;
                        startInfo.FileName = Application.ExecutablePath;
                        startInfo.Verb = "runas";
                        if (args.Length > 0)
                        {
                            startInfo.Arguments = string.Join(" ", args);
                        }
                        Process.Start(startInfo);
                        Environment.Exit(0);
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("This uninstaller requires administrator privileges.", "Administrator Rights Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Environment.Exit(1);
                        return;
                    }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UninstallerForm());
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new InstallerMainWindow());
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
