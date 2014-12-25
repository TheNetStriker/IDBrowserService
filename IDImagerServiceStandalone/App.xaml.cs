using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IDBrowserServiceStandalone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.ServiceModel.ServiceHost hostIDBrowserService;
        private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon taskbarIcon;
        private System.Drawing.Icon ApplicationIcon;

        public App()
        {
            try
            {
                System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();

                System.Windows.Controls.MenuItem btnQuit = new System.Windows.Controls.MenuItem();
                btnQuit.Name = "btnQuit";
                btnQuit.Header = "Quit";
                btnQuit.IsCheckable = false;
                btnQuit.Click += btnQuit_Click;

                System.Windows.Controls.ContextMenu cntxRemoteExecuteServer = new System.Windows.Controls.ContextMenu();
                cntxRemoteExecuteServer.Items.Add(btnQuit);
                cntxRemoteExecuteServer.Name = "cntxIDBrowserServiceStandalone";

                using (System.IO.Stream file = thisExe.GetManifestResourceStream("IDBrowserServiceStandalone.Images.Antenna.ico"))
                {
                    ApplicationIcon = new System.Drawing.Icon(file);
                }

                taskbarIcon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();
                taskbarIcon.Icon = ApplicationIcon;
                taskbarIcon.ContextMenu = cntxRemoteExecuteServer;

                hostIDBrowserService = new System.ServiceModel.ServiceHost(typeof(IDBrowserServiceCode.Service));
                hostIDBrowserService.Open();

                taskbarIcon.ShowBalloonTip("Server started", System.String.Format("IDBrowserService started on: {0}", hostIDBrowserService.BaseAddresses[0]), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
            catch (Exception e)
            {
                if (taskbarIcon != null) { taskbarIcon.ShowBalloonTip("Error", e.ToString(), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error); };
            }
        }

        private void btnQuit_Click(System.Object sender, System.EventArgs e)
        {
            taskbarIcon.Dispose();

            if ((hostIDBrowserService != null))
            {
                if (hostIDBrowserService.State == System.ServiceModel.CommunicationState.Opened)
                {
                    hostIDBrowserService.Close();
                }
            }
            Application.Current.Shutdown();
        }
    }
}
