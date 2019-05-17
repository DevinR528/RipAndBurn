using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.IO;

using NAudio.Wave;
using NAudio.Lame;
using DiscId;

using RipAndBurn.Rip.CDMetadata.Release;
using RipAndBurn.Rip.CDMetadata.Album;
using RipAndBurn.Rip.CDMetadata.CDInfo;
using Media = RipAndBurn.Rip.CDMetadata.Release.Media;
using Ripper;
// GET RID OF
using System.Windows.Forms;



namespace RipAndBurn.Rip
{


    class CDRipper {

        private bool _isOpen = false;

        private CurrentCD _intrnCurrentCD;
        private Action<int> _update;

        private CDDrive driver = new CDDrive();

        private WavLib.WaveWriter wavWriter;

        private InputBox _iBox;
        private RipBurn _form;
        public CurrentCD CDRom {
            get => this._intrnCurrentCD;
        }
        public InputBox IBox {
            get => this._iBox;
        }

        public CDRipper() { }

        // open and close cd drive
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        protected static extern int mciSendString(string lpstrCommand,
                                                   StringBuilder lpstrReturnString,
                                                   int uReturnLength,
                                                   IntPtr hwndCallback);
        public void Open() {
            if (!this._isOpen) {
                int ret = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
                this._isOpen = true;
            }
        }

        public void Close() {
            if (this._isOpen) {
                int ret = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
                this._isOpen = false;
            }

        }

        public string GetCD_Id(string drive, RipBurn rip) {
            this._form = rip;
            this._update = (amt) => this._form.progressBar1.Value += amt;

            this._form.progressBar1.Invoke(this._update, 2);
            try {
                using (var disc = DiscId.Disc.Read(drive, Features.Mcn | Features.Isrc)) {
                    string queryRaw = disc.SubmissionUrl.ToString();

                    this._form.progressBar1.Invoke(this._update, 2);
                    string[] queryArr = queryRaw.Split(new string[] { "id=", "&" }, StringSplitOptions.None);
                    string query = $"{queryArr[1]}?&{queryArr[2]}&{queryArr[3]}";
                    return query;
                }
            }
            catch (DiscIdException ex) {
                throw ex;
            }
        }

        async public Task GetReleaseData(string diskQuery) {

            this._form.progressBar1.Invoke(this._update, 2);
            try {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("User-Agent", "Grandpa's Cd Copier/1.0.0 ( devin.ragotzy@gmail.com )");
                HttpResponseMessage res = await client.GetAsync($"https://musicbrainz.org/ws/2/discid/{diskQuery}&fmt=json");

                if (res.IsSuccessStatusCode) {
                    HttpContent resBuff = res.Content;

                    string body = await resBuff.ReadAsStringAsync();
                    AllReleases r = AllReleases.FromJson(body);

                    this._form.progressBar1.Invoke(this._update, 2);
                    // {83644aee-b687-4e1c-b5e6-5f21faee0eb1}
                    foreach (Release rels in r.Releases) {
                        foreach (Media media in rels.Media) {
                            foreach (AllReleases metaInfo in media.Discs) {
                                if (metaInfo.Sectors == r.Sectors) {
                                    if (rels.Id != null) {
                                        // check title
                                        this._iBox = new InputBox(rels.Media);
                                        DialogResult dRes = this._iBox.ShowDialog();
                                        string title = this._iBox.GetTitle();

                                        if (dRes == DialogResult.OK) {
                                            string id = rels.Id.ToString();
                                            HttpResponseMessage res2 = await client.GetAsync($"https://musicbrainz.org/ws/2/release/{id}?inc=artist-credits+recordings&fmt=json");
                                            HttpContent resBuff2 = res2.Content;

                                            this._form.progressBar1.Invoke(this._update, 2);
                                            string content = await resBuff2.ReadAsStringAsync();
                                            TrackList albums = TrackList.FromJson(content);

                                            foreach (CDMetadata.Album.Media album in albums.Media
                                                .Where((album) => album.Title == title))
                                            {

                                                this._form.progressBar1.Invoke(this._update, 2);
                                                // set fields we care about
                                                this._intrnCurrentCD = CurrentCD.FromMeta(album);

                                                this._form.Invoke((Action)(() => {
                                                    this._form.progressLabel.Text = "Finised collecting CD info";
                                                }));
                                                this.IBox.Dispose();
                                                return;
                                            }
                                        }
                                        else {
                                            // Cancel hit
                                            return;
                                        }
                                    } else {
                                        // cant find info so continue on
                                        MessageBox.Show("Can not find a CD that matches the one inserted");
                                        return;
                                    }
                                } else {
                                    // no matching ids 
                                    MessageBox.Show("Can not find a CD that matches the one inserted");
                                    return;
                                }
                            }
                        }
                    }
                } else {
                    // status not 200
                    MessageBox.Show("Probably not connected to the internet, we'll burn without song names.");
                    return;
                }
            }
            catch (Exception err) {
                throw new HttpRequestException("");
            }
        }

        async public Task RipCDtoTemp(char driveChar) {
            if (this.driver.Open(driveChar)) {
                if (this.driver.IsCDReady()) {
                    if (this.driver.Refresh()) {
                        int trks = this.driver.GetNumTracks();
                        this.driver.LockCD();

                        try {
                            this._form.progressBar1.Invoke((Action)(() => {
                                this._form.progressBar1.Value = 0;
                                this._form.progressBar1.Maximum = 100;
                                this._form.progressLabel.Text = $"Copying CD to temp folder (1. {this._intrnCurrentCD.Tracks[0].Name})";
                            }));
                            for (int i = 1; i <= trks; i++) {

                                WavLib.WaveFormat format = new WavLib.WaveFormat(44100, 16, 2);
                                string name = string.Format("test\\track{0:00}.raw", i);

                                Stream fileStrm = new FileStream(name, FileMode.Create, FileAccess.Write);
                                try {
                                    // this writes the cd data to the file stream object
                                    this.wavWriter = new WavLib.WaveWriter(fileStrm, format, this.driver.TrackSize(i));
                                    // drives reading of cd data
                                    if (this.driver.ReadTrack(i, new CdDataReadEventHandler(WriteWaveData), new CdReadProgressEventHandler(CdReadProgress)) > 0) {

                                    }
                                } catch {
                                    throw new IOException("Write failed");
                                } finally {
                                    fileStrm.Close();
                                }
                            }
                            // encode files !!!!!!!!!!!!!!!
                            await this.EncodeFolderMp3();
                        } catch {
                            throw new IOException("IO Stream failed");
                        }
                        finally {
                            this.driver.UnLockCD();
                        }
                    }
                }
            } else {
                // drive could not be read
                MessageBox.Show("Could not read media try another CD, I'm not sure if a DVD will work but try.");
            }
        }

        private int _trackNum = 1;
        private void CdReadProgress(object sender, ReadProgressEventArgs ea) {
            ulong percent = ((ulong)ea.BytesRead * 100) / ea.Bytes2Read;
            if (percent < 100) {
                this._form.progressBar1.Invoke((Action<int>)((amt) => this._form.progressBar1.Value = amt), (int)percent);
            } else {
                this._form.progressBar1.Invoke((Action)(() => {
                    if (this._trackNum < this._intrnCurrentCD.Tracks.Count) {
                        this._form.progressBar1.Value = 0;
                        this._form.progressBar1.Maximum = 100;
                        string song = this._intrnCurrentCD.Tracks[this._trackNum].Name;
                        this._form.progressLabel.Text = $"Copying {this._trackNum + 1}. {song} to temp folder";
                        this._trackNum++;
                    }
                }));
            }
        }


        public void WriteWaveData(object sender, DataReadEventArgs ea) {
            if (this.wavWriter != null) {
                this.wavWriter.Write(ea.Data, 0, (int)ea.DataSize);
            }
        }

        // folder is where to get raw from
        async private Task EncodeFolderMp3 () {
            string homeDir = Environment.GetEnvironmentVariable("%HOMEDRIVE%%HOMEPATH%");
            string music_path = Path.GetFullPath("Music");
            DialogResult dr = MessageBox.Show("Save to default Music folder", "Save Folder", MessageBoxButtons.YesNo);

            string saveLoc = "";
            if (dr == DialogResult.Yes) {
                saveLoc = Path.GetFullPath("Music");
            } else {
                this._form.Invoke((Action)(() => {
                    this._form.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
                    DialogResult folderDR = this._form.folderBrowserDialog1.ShowDialog();

                    if (folderDR == DialogResult.OK) {
                        saveLoc = this._form.folderBrowserDialog1.SelectedPath;
                    }
                }));
            }

            // hardcode because all files in here are temporary delete after encoding
            string open_path = Path.GetFullPath("tmp");
            string[] files = Directory.GetFiles(open_path);

            for (int i = 0; i < files.Length; i++) {
                try {
                    await this.EncodeFileMp3(files[i], i, saveLoc);
                } catch {
                    throw new IOException("Encoding Error");
                }
            }
        }

        private long _totalInput;
        async private Task EncodeFileMp3(string trkLoca, int trkNum, string saveDir) {
            ID3TagData tag = null;

            if (this._intrnCurrentCD != null) {
                tag = new ID3TagData {
                    Artist = this._intrnCurrentCD.Artist,
                    Album = this._intrnCurrentCD.Title,
                    Title = this._intrnCurrentCD.Tracks[trkNum].Name,
                    Track = (trkNum + 1).ToString(),
                };
            }
            string title = tag == null ? Path.GetFileNameWithoutExtension(trkLoca) : tag.Title;
            string path = $"{saveDir}\\{title}.mp3";

            using (AudioFileReader audio_in = new AudioFileReader(trkLoca))
                //***************************                        Saved here
            using (LameMP3FileWriter audio_out = new LameMP3FileWriter(path, audio_in.WaveFormat, LAMEPreset.V3, tag)) {
                audio_out.MinProgressTime = 250;
                this._totalInput += audio_in.Length;
                this._form.progressBar1.Invoke((Action)(() => {
                    this._form.progressBar1.Value = 0;
                    this._form.progressBar1.Maximum = (int)this._totalInput;
                }));
                audio_out.OnProgress += ((object writer, long inputBytes, long outputBytes, bool finished) => {
                    if (finished) {
                        this._form.progressBar1.Invoke((Action)(() => {
                            this._form.progressLabel.Text = $"All set now we can start burning";
                            this._form.progressBar1.Value = this._form.progressBar1.Maximum;
                        }));
                    } else {
                        this._form.progressBar1.Invoke((Action<int>)((amt) => {
                            this._form.progressLabel.Text = $"Converting ({title}) to MP3";
                            this._form.progressBar1.Increment(amt);
                        }), (int)inputBytes);
                    }
                });
                await audio_in.CopyToAsync(audio_out);
                this.DeleteFile(trkLoca);
            }
        }

        private void DeleteFile(string file) {
            File.Delete(file);
        }
    }
}
