using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Management;
using System.IO;

// Rip Folder
using RipAndBurn.Rip;

namespace RipAndBurn
{
    public partial class RipBurn : Form
    {

        private CDRipper _cdRip;
        private CDhttp _cdMetaInfo = new CDhttp();

        private ManagementEventWatcher _watcher;

        public RipBurn() { InitializeComponent(); }

        private void Form1_Load(object sender, EventArgs e) {
            this.outLabel.Text = "Click Open or just wait if a CD with music on it is in the CD drive";
            this.progressLabel.Text = "";
            this.progressBar1.Visible = false;


            this._cdRip = new CDRipper();

            WqlEventQuery q = new WqlEventQuery();
            q.EventClassName = "__InstanceModificationEvent";
            q.WithinInterval = new TimeSpan(0, 0, 2);
            q.Condition = @"TargetInstance ISA 'Win32_LogicalDisk'";

            ConnectionOptions opt = new ConnectionOptions();
            opt.EnablePrivileges = true;
            opt.Authority = null;
            opt.Authentication = AuthenticationLevel.Default;
            ManagementScope scope = new ManagementScope("\\root\\CIMV2", opt);

            this._watcher = new ManagementEventWatcher(scope, q);
            this._watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived_CD_Door);
            this._watcher.Start();
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
            this._cdRip.Open();
            this._watcher.Start();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this._cdRip.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            
        }

        // EVENTS **************************************************************

        async private void watcher_EventArrived_CD_Door(object sender, EventArrivedEventArgs e) {
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom))
                if (drive.Name == "D:\\") {
                    if (drive.IsReady == true) {
                        this._watcher.Stop();
                        Action<int> startPbar = (total) => {
                            this.StartProgBar(total);
                            this.progressLabel.Text = "Fetching CD info...";
                            this.outLabel.Text = "I notice there is a CD I will start copying it";
                        };
                        this.progressBar1.Invoke(startPbar, 12);

                        this._cdMetaInfo.GetCD_Id(drive.Name, this);
                        await this._cdMetaInfo.GetReleaseData(this);
                        await this._cdRip.RipCDtoTemp(this._cdMetaInfo.CDRom, this);
                    }
                    else {
                        Console.Error.WriteLine("Error");
                    }
                }
        }
        // Closes Form
        private void ripper_onFormClose(object sender, FormClosingEventArgs e)
        {
            this._cdRip.Close();
        }

        // MINE ***************************************************************
        private void StartProgBar(int numSections) {
            this.progressBar1.Visible = true;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = numSections;
            this.progressBar1.Value = 0;
            this.progressBar1.Step = 1;
        }

        private void ResetWatch_Progress() {
            this.outLabel.Text = "Click Open or just wait if a CD with music on it is in the CD drive";
            this._watcher.Start();
            this.progressBar1.Visible = false;
        }
    }
}
