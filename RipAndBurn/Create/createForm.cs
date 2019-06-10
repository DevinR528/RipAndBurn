using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RipAndBurn.Burn;
using IMAPI2.Interop;
using Ripper;

using RipAndBurn;
using RipAndBurn.Burn;


namespace RipAndBurn.Create {
    struct Args {
        public List<string> paths;
        public string drive;
    }
    public partial class CreateCD : Form {

        private string music_path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        private List<string> keepers = new List<string>();

        private CDDrive driver = new CDDrive();

        private List<string> temp_song_path = new List<string>();

        private FileLogger log = new FileLogger();

        public CreateCD() {
            InitializeComponent();
        }

        private void CreateCD_Load(object sender, EventArgs e) {
            this.backgroundWorker1.WorkerReportsProgress = true;

            foreach (string album in Directory.GetDirectories(music_path)) {
                this.albumListBox.Items.Add(Path.GetFileName(album));
            }
            
        }

        private void selectedVal_onChange(object sender, EventArgs e) {
            temp_song_path.Clear();
            this.songListBox.Items.Clear();
            foreach (string path in Directory.EnumerateFiles(music_path + "\\" + (string)this.albumListBox.SelectedItem)) {
                temp_song_path.Add(path);
                string song = Path.GetFileNameWithoutExtension(path);
                this.songListBox.Items.Add(song);
            }
        }

        private void AddButton_Click(object sender, EventArgs e) {
            if (this.songListBox.SelectedIndex > -1) {
                // add to screen
                this.burnListBox.Items.Add(this.songListBox.SelectedItem);

                // add to list of file paths
                keepers.Add(temp_song_path[this.songListBox.SelectedIndex]);
            }
        }

        private void BurnButton_Click(object sender, EventArgs e) {
            this.log.Log("Burn from Create CD");

            if (this.burnListBox.Items.Count < 5) {
                DialogResult r = MessageBox.Show("Are you sure there are only a few songs on this cd.", "BURN", MessageBoxButtons.YesNo);
                if (r == DialogResult.Yes) {
                    try {
                        this.burnButton.Enabled = false;
                        // this wont work if there are more than one cd drive
                        DriveInfo drive = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).Single();
                        Args args = new Args { drive = drive.Name, paths = this.keepers };

                        this.progressBar1.Visible = true;
                        this.progressBar1.Maximum = 100;

                        this.backgroundWorker1.RunWorkerAsync(args);
                    }
                    catch (Exception err) {
                        this.log.Log(err);
                        MessageBox.Show("Found a non blank disc");
                    }
                }
            } else {
                try {
                    this.burnButton.Enabled = false;
                    // this wont work if there are more than one cd drive
                    DriveInfo drive = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).Single();
                    Args args = new Args { drive = drive.Name, paths = this.keepers };

                    this.progressBar1.Visible = true;
                    this.progressBar1.Maximum = 100;

                    this.backgroundWorker1.RunWorkerAsync(args);
                }
                catch (Exception err) {
                    this.log.Log(err);
                    MessageBox.Show("Found a non blank disc");
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void burnerThread_doWork(object sender, DoWorkEventArgs e) {
            Args arg = (Args)e.Argument;
            new Burner(arg.drive, arg.paths, this);
        }

        private void burnerThread_onProgress(object sender, ProgressChangedEventArgs e) {
            var burnData = (BurnData)e.UserState;

            this.StartProgBar(100);

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
            this.driver.UnLockCD();
            this.keepers.Clear();
            this.burnListBox.Items.Clear();
            this.burnButton.Enabled = true;
            if (e.Error != null) {

                if (e.Error is InvalidCastException) {
                    MessageBox.Show("CD seems to have something on it already try a new disc.");
                    this.log.Log(e.Error);
                }
                else {
                    MessageBox.Show("We had a problem writing to the disc, you may have to burn again.");
                    this.log.Log(e.Error);
                }
            }
            else {
                MessageBox.Show("You finished burning time to exit");
                this.Close();
            }
        }

        private void StartProgBar(int numSections) {
            this.progressBar1.Visible = true;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = numSections;
            this.progressBar1.Value = 0;
            this.progressBar1.Step = 1;
        }
    }
}
