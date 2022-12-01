using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Shapes;

namespace YapiLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public enum LauncherStatus
    {
        Ready,
        Failed,
        Downloading,
        DownloadingUpdate
    }

    public partial class MainWindow : Window
    {
        private LauncherStatus CurrentStatus;

        private string RootPath;
        private string VersionFile;
        private string ProjectZip;
        private string ProjectExecutionName;

        public MainWindow()
        {
            InitializeComponent();

            RootPath = Directory.GetCurrentDirectory();
            VersionFile = System.IO.Path.Combine(RootPath, "Version.txt");
            ProjectZip = System.IO.Path.Combine(RootPath, "Build.zip");
            ProjectExecutionName = System.IO.Path.Combine(RootPath, "Build", "MyGame.exe");

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        private void StatusBtn_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(ProjectExecutionName) && CurrentStatus == LauncherStatus.Ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(ProjectExecutionName);
                startInfo.WorkingDirectory = System.IO.Path.Combine(RootPath, "Build");
                Process.Start(startInfo);

                Close();
            }
        }

        private void CheckForUpdates()
        {
            if (File.Exists(VersionFile))
            {
                string localVersion = File.ReadAllText(VersionFile);
                CurrentVersionText.Text = "Current Version:" + localVersion;

                try
                {
                    WebClient webClient = new WebClient();
                    string onlineVersion = webClient.DownloadString("Version Link File");

                    if(onlineVersion!= localVersion)
                    {
                        Console.WriteLine("New Version");
                    }
                    else
                    {
                        CurrentStatus = LauncherStatus.Ready;
                        Console.WriteLine("Ready");
                    }
                }catch(Exception exception)
                {
                    CurrentStatus = LauncherStatus.Failed;
                    MessageBox.Show($"Error Checking The Project Updates: {exception}");
                }

            }
        }

        private void InstallProjectFile()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += DownloadProgressChangedHandler;
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleteHandler);
            webClient.DownloadFileAsync(new Uri("Project Link Zip"), ProjectZip, null);
        }

        private void DownloadCompleteHandler(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = e.UserState.ToString();
                ZipFile.ExtractToDirectory(ProjectZip, RootPath, true);
                File.Delete(ProjectZip);

                File.WriteAllText(VersionFile, onlineVersion);
                CurrentVersionText.Text = "Current Version: " + onlineVersion;
                CurrentStatus = LauncherStatus.Ready;

            }
            catch(Exception exception)
            {
                CurrentStatus = LauncherStatus.Failed;
                MessageBox.Show($"Decompressing Project Failed : {exception}");
            }
        }

        private void DownloadProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }
    }
}
