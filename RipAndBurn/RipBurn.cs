using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

using System.Management;
using System.IO;

// Rip Folder
using RipAndBurn.Rip;
using RipAndBurn.Burn;
using IMAPI2.Interop;
using System.Net.Http;

namespace RipAndBurn
{

    struct Args {
        public string path;
        public string drive;
    }
    public partial class RipBurn : Form
    {
        private bool _isBurning;
        public bool _isRipping;

        private string _driveName;

        private CDRipper _cdRip;

        private ManagementEventWatcher _watcher;

        public RipBurn() { InitializeComponent(); }

        private void Form1_Load(object sender, EventArgs e) {
            this.outLabel.Text = "Click Open or just wait if a CD with music on it is in the CD drive";
            this.progressLabel.Text = "";
            this.progressBar1.Visible = false;


            this._cdRip = new CDRipper();

            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;

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
            // NEED
            this._watcher.Start();
        }

        private void ActionButton_Click(object sender, EventArgs e) {
            this._cdRip.Open();
            this._watcher.Start();
        }

        private void BurnButton_Click(object sender, EventArgs e) {
            this.progressBar1.Value = 0;
            if (this._driveName != null) {
                Args args = new Args { drive = this._driveName, path = null };
                this.backgroundWorker1.RunWorkerAsync(args);
            } else {
                // change to Music after dev
                this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
                DialogResult dr = this.folderBrowserDialog1.ShowDialog();

                if (dr == DialogResult.OK) {
                    string path = this.folderBrowserDialog1.SelectedPath;

                    foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom)) {
                        if (drive.IsReady) {
                            Args args = new Args { drive = drive.Name, path = path };

                            this.progressBar1.Visible = true;
                            this.progressBar1.Maximum = 100;

                            this._isBurning = true;
                            this.backgroundWorker1.RunWorkerAsync(args);
                            return;
                        } else {
                            MessageBox.Show("Make sure there is a blank CD in the drive.");
                        }
                    }
                }
            }
        }

        private void burnerThread_doWork(object sender, DoWorkEventArgs e) {
            Args arg = (Args)e.Argument;
            new Burner(arg.drive, arg.path, this._cdRip.CDRom, this);
        }

        private void burnerThread_onProgress(object sender, ProgressChangedEventArgs e) {
            var burnData = (BurnData)e.UserState;

            if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM) {
                this.progressLabel.Text = burnData.statusMessage;
            }
            else if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING) {
                switch (burnData.currentAction) {
                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA:
                        this.progressLabel.Text = "Validating current media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA:
                        this.progressLabel.Text = "Formatting media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:
                        // FIXME
                        this.progressLabel.Text = "Writing songs to CD...";

                        long writtenSectors = burnData.lastWrittenLba - burnData.startLba;
                        if (writtenSectors > 0 && burnData.sectorCount > 0) {
                            var percent = (int)((100 * writtenSectors) / burnData.sectorCount);
                            this.progressBar1.Value = percent;
                        }
                        else {
                            this.progressBar1.Value = 0;
                        }
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:
                        this.progressLabel.Text = "Finalizing writing...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:
                        this.progressLabel.Text = "Completed!";
                        break;


                }
            }
        }

        private void burnerThread_onComplete(object sender, RunWorkerCompletedEventArgs e) {
            this._isBurning = false;
            if (e.Error != null) {

                if (e.Error is InvalidCastException) {
                    MessageBox.Show("CD seems to have something on it already try a new disc.");
                } else {
                    MessageBox.Show("We had a problem writing to the disc, unfortunately you may have to start over.");
                }
            } else {
                this.outLabel.Text = "You did it, Rip and Burn Completed!!!!";
                this._cdRip.Open();
            }
            this.progressBar1.Visible = false;
            this.progressLabel.Text = "";
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            
        }

        // EVENTS **************************************************************

        // the event hanler can use async method to awaitmethod calls
        async private void watcher_EventArrived_CD_Door(object sender, EventArrivedEventArgs e) {
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom)) {
                if (drive.IsReady == true) {
                    this._driveName = drive.Name;

                    this._isRipping = true;
                    this._watcher.Stop();
                    Action<int> startPbar = (total) => {
                        this.StartProgBar(total);
                        this.progressLabel.Text = "Fetching CD info...";
                        this.outLabel.Text = "I notice there is a CD I will start copying it";
                    };
                    this.progressBar1.Invoke(startPbar, 12);
                    
                    try {
                        // *************************************            clearly this is terible
                        string query = this._cdRip.GetCD_Id(this._driveName, this);

                        await this._cdRip.Get_CD_meta(query);
                        char dChar = drive.Name.ToCharArray(0, 1)[0];

                        await this._cdRip.RipCDtoTemp(dChar);
                        //stop watcher until burn is complete
                        this._isRipping = false;
                    } catch (IOException err) {
                        MessageBox.Show("Something went wrong with the Ripping of the CD try again." 
                            + err.ToString());
                    } catch (HttpRequestException err) {
                        MessageBox.Show("Something went wrong, check your internet connection."
                            + err.ToString());
                    } catch(DiscId.DiscIdException dex) {
                        MessageBox.Show("Something went wrong, this disc can not be read."
                            + dex.ToString());
                    }
                    // READY TO BURN
                    //this._cdRip.Open();
                } else {
                    this._watcher.Stop();
                    MessageBox.Show("No CD found to rip. Now you must click Start button.");
                }
            }
        }
        // Closes Form
        private void ripper_onFormClose(object sender, FormClosingEventArgs e)
        {
            this._cdRip.Close();
            if (this._isBurning || this._isRipping) {
                e.Cancel = true;
                // show message
            }
        }

        // MINE ***************************************************************
        private void StartProgBar(int numSections) {
            this.progressBar1.Visible = true;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = numSections;
            this.progressBar1.Value = 0;
            this.progressBar1.Step = 1;
        }
    }
}
