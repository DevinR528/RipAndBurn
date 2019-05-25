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
// internal cd driver
using Ripper;
using System.Collections.Generic;

namespace RipAndBurn
{

    struct Args {
        public string path;
        public string drive;
    }
    public partial class RipBurn : Form {

        FileLogger log = new FileLogger();

        private bool _isBurning;
        public bool _isRipping;

        public Action<string> ResetProgBar;

        private string _driveName;

        private CDDrive driver = new CDDrive();

        private CDRipper _cdRip;

        private ManagementEventWatcher _watcher;

        public RipBurn() { InitializeComponent(); }

        private void Form1_Load(object sender, EventArgs e) {
            this.outLabel.Text = "Click 'Start Rip' to start copying";
            this.progressLabel.Text = "";
            this.progressBar1.Visible = false;


            this._cdRip = new CDRipper();

            this.backgroundWorker1.WorkerReportsProgress = true;

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

            this.ResetProgBar = (msg) => {
                this.progressBar1.Value = 0;
                this.progressBar1.Visible = false;
                this.outLabel.Text = msg;
                this.progressLabel.Text = "";
            };
            // NEED
            //this._watcher.Start();
        }

        private void ActionButton_Click(object sender, EventArgs e) {
            this._cdRip.Open();
            this._watcher.Start();
        }

        private void BurnButton_Click(object sender, EventArgs e) {
            this.progressBar1.Value = 0;
            if (this._driveName != null) {
                this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyMusic;
                DialogResult dr = this.folderBrowserDialog1.ShowDialog();

                if (dr == DialogResult.OK) {
                    string path = this.folderBrowserDialog1.SelectedPath;
                    Args args = new Args { drive = this._driveName, path = path };
                    this.backgroundWorker1.RunWorkerAsync(args);
                }
            } else {
                // no rip so find folder
                this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult dr = this.folderBrowserDialog1.ShowDialog();

                if (dr == DialogResult.OK) {
                    try {
                        this.burnButton.Enabled = false;
                        string path = this.folderBrowserDialog1.SelectedPath;
                        // this wont work if there are more than one cd drive
                        DriveInfo drive = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).Single();
                        Args args = new Args { drive = drive.Name, path = path };

                        this.progressBar1.Visible = true;
                        this.progressBar1.Maximum = 100;

                        this._isBurning = true;
                        this.backgroundWorker1.RunWorkerAsync(args);
                    }
                    catch (Exception err) {
                        this.log.Log(err);
                        MessageBox.Show("Found a non blank disc");
                    }
                    return;
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
            this.driver.UnLockCD();
            this.burnButton.Enabled = true;
            if (e.Error != null) {

                if (e.Error is InvalidCastException) {
                    MessageBox.Show("CD seems to have something on it already try a new disc.");
                    this.log.Log(e.Error);
                } else {
                    MessageBox.Show("We had a problem writing to the disc, you may have to burn again.");
                    this.log.Log(e.Error);
                }
            } else {
                this.outLabel.Text = "You did it, Rip and Burn Completed!!!!";
                this._cdRip.Open();
                this.progressBar1.Visible = false;
                this.progressLabel.Text = "";
            }
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
                        this.actionButton.Enabled = false;
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

                        string saveLoc = "";
                        this.Invoke((Action)(() => {
                            DialogResult dr = MessageBox.Show("Save to default Music folder", "Save Folder", MessageBoxButtons.YesNo);

                            if (dr == DialogResult.Yes) {
                                saveLoc = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            }
                            else {
                                this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
                                DialogResult folderDR = this.folderBrowserDialog1.ShowDialog();

                                if (folderDR == DialogResult.OK) {
                                    saveLoc = this.folderBrowserDialog1.SelectedPath;
                                }
                            }

                        }));
                        await this._cdRip.RipCDtoTemp(dChar, saveLoc);
                        this._isRipping = false;
                    } catch (IOException err) {
                        this.log.Log(err);
                        MessageBox.Show("Something went wrong with the Ripping of the CD try again.");
                        this.Invoke(this.ResetProgBar, "Click Start Rip to Try again");

                    } catch (HttpRequestException err) {
                        this.log.Log(err);
                        string msg = "Something went wrong, most likely the program could not connect to the internet."
                            + "check your internet connection or simply try again, you know computers are funny.";
                        MessageBox.Show(msg);
                        this.Invoke(this.ResetProgBar, "Click Start Rip to Try again");

                    } catch (DiscId.DiscIdException dex) {
                        this.log.Log(dex);
                        MessageBox.Show("Something went wrong, this disc can not be read.");
                        this.Invoke(this.ResetProgBar, "Click Start Rip to Try again");
                    } finally {
                        this.driver.UnLockCD();
                        this._cdRip.Open();
                    }
                    // READY TO BURN
                } else {
                    this._watcher.Stop();
                    MessageBox.Show("No CD found to rip. Now you must Click Start Rip button.");
                }
            }
        }
        // Closes Form
        private void ripper_onFormClose(object sender, FormClosingEventArgs e) {
            
            this.driver.UnLockCD();
            if (this._isBurning || this._isRipping) {
                DialogResult res = MessageBox.Show("You are in the middle of something!! Are you sure", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.No) {
                    e.Cancel = true;
                } else {
                    // clear tmp folder no matter what
                    string open_path = Path.GetFullPath("tmp");
                    string[] files = Directory.GetFiles(open_path);

                    for (int i = 0; i < files.Length; i++) {
                        File.Delete(files[i]);
                    }

                    this.driver.Close();
                }
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

        private void CreateButton_Click(object sender, EventArgs e) {

        }
    }
}
