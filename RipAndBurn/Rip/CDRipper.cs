using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.IO;
using System.Windows.Forms;

using NAudio.Wave;
using NAudio.Lame;
using DiscId;

using RipAndBurn.Rip.CDMetadata.Release;
using RipAndBurn.Rip.CDMetadata.Album;
using RipAndBurn.Rip.CDMetadata.CDInfo;
using Media = RipAndBurn.Rip.CDMetadata.Release.Media;
using Ripper;


namespace RipAndBurn.Rip
{


    class CDRipper {

        private bool _isOpen = false;
        private string _album_title = "";

        private CurrentCD _intrnCurrentCD;
        private Action<int> _update;

        private CDDrive driver = new CDDrive();
        private WavLib.WaveWriter wavWriter;

        private InputBox _iBox;
        private RipBurn _form;

        public CurrentCD CDRom {
            get => this._intrnCurrentCD;
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

        async public Task Get_CD_meta(string diskQuery) {

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

                                        string title = "";
                                        if (rels.Title != null) {
                                            title = rels.Title;
                                        } else if (media.Title != null) {
                                            title = media.Title;
                                        }

                                        if (title != "") {
                                            string id = rels.Id.ToString();
                                            HttpResponseMessage res2 = await client.GetAsync($"https://musicbrainz.org/ws/2/release/{id}?inc=artist-credits+recordings&fmt=json");
                                            HttpContent resBuff2 = res2.Content;

                                            this._form.progressBar1.Invoke(this._update, 2);
                                            string content = await resBuff2.ReadAsStringAsync();
                                            TrackList albums = TrackList.FromJson(content);

                                            foreach (CDMetadata.Album.Media album in albums.Media
                                                .Where((album) => album.TrackCount == media.TrackCount))
                                            {
                                                this._album_title = this.CleanInvalidFolder(title);
                                                this._form.progressBar1.Invoke(this._update, 2);
                                                // set fields we care about
                                                this._intrnCurrentCD = CurrentCD.FromMeta(album, title);

                                                this._form.Invoke((Action)(() => {
                                                    this._form.progressLabel.Text = "Finised collecting CD info";
                                                }));
                                                return;
                                            }
                                        } else {
                                            // no title, ask user for album title
                                            using (InputBox box = new InputBox()) {
                                                box.ShowDialog();
                                                this._album_title = box.IboxTitle;
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // no matching ids 
                    MessageBox.Show("Can not find a CD that matches the one inserted, we'll burn without song names.");
                    // check title
                    using (InputBox box = new InputBox()) {
                        box.ShowDialog();
                        this._album_title = box.IboxTitle;
                    }
                    return;
                } else {
                    // status not 200
                    MessageBox.Show("Probably not connected to the internet, we'll burn without song names.");
                    // check title
                    using (InputBox box = new InputBox()) {
                        box.ShowDialog();
                        this._album_title = box.IboxTitle;
                    }
                    return;
                }
            } catch (Exception err) {
                throw new HttpRequestException("UhOh", err);
            }
        }

        private string CleanInvalidFolder(string title) {
            char[] a = Path.GetInvalidPathChars();
            char[] b = Path.GetInvalidFileNameChars();

            char[] invalid = new char[a.Length + b.Length];
            a.CopyTo(invalid, 0);
            b.CopyTo(invalid, a.Length);

            return string.Join("", title.Split(invalid));
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
                                string song = this._intrnCurrentCD == null ? "" : this._intrnCurrentCD.Tracks[0].Name + " ";
                                this._form.progressLabel.Text = $"Copying track (1. {song}) to Music";
                            }));

                            string tmp = Path.GetFullPath("tmp");
                            for (int i = 1; i <= trks; i++) {

                                string name = string.Format("{0}\\track{1:00}.raw", tmp, i);
                                Stream file_st = new FileStream(name, FileMode.Create, FileAccess.Write);
                                WavLib.WaveFormat format = new WavLib.WaveFormat(44100, 16, 2);

                                try {
                                    WaveFormat fmt = new NAudio.Wave.WaveFormat();

                                    this.wavWriter = new WavLib.WaveWriter(file_st, format, this.driver.TrackSize(i));
                                    // drives reading of cd data
                                    this.driver.ReadTrack(i,
                                        // callback to add bytes to wavewriter then stream to mp3 encode
                                        new CdDataReadEventHandler(WriteWaveData),
                                        new CdReadProgressEventHandler(CdReadProgress));
                                }
                                catch (Exception err) {
                                    throw new IOException("Write failed", err);
                                }
                                finally {
                                    file_st.Close();
                                }
                            }
                            this._form.progressBar1.Invoke((Action)(() => {
                                this._form.progressBar1.Visible = false;
                                this._form.progressLabel.Text = "Completed!";
                            }));
                            // encode files !!!!!!!!!!!!!!!
                            await this.EncodeFolderMp3();
                        }
                        catch (Exception e) {
                            throw new IOException("IO Stream failed", e);
                        }
                        finally {
                            this.driver.UnLockCD();
                        }
                    }
                }
            }
            else {
                // drive could not be read
                MessageBox.Show("Could not read media try another CD, I'm not sure if a DVD will work.");
            }
        }

        private void WriteWaveData(object sender, DataReadEventArgs ea) {
            if (this.wavWriter != null) {
                // acutally writes cd bytes to stream obj
                this.wavWriter.Write(ea.Data, 0, (int)ea.DataSize);
            }
        }

        private int _trackNumRawProg = 1;
        private void CdReadProgress(object sender, ReadProgressEventArgs ea) {
            ulong percent = ((ulong)ea.BytesRead * 100) / ea.Bytes2Read;
            if (percent < 100) {
                this._form.progressBar1.Invoke((Action<int>)((amt) => this._form.progressBar1.Value = amt), (int)percent);
            } else {
                this._form.progressBar1.Invoke((Action)(() => {
                    if (this._trackNumRawProg < this._intrnCurrentCD.Tracks.Count) {
                        this._form.progressBar1.Value = 0;
                        this._form.progressBar1.Maximum = 100;
                        string song = this._intrnCurrentCD == null ? "" : this._intrnCurrentCD.Tracks[this._trackNumRawProg].Name + " ";
                        this._form.progressLabel.Text = $"Copying track ({this._trackNumRawProg}. {song}) to Music";
                        this._trackNumRawProg++;
                    }
                }));
            }
        }

        // folder is where to get raw from
        async private Task EncodeFolderMp3 () {
            DialogResult dr = MessageBox.Show("Save to default Music folder", "Save Folder", MessageBoxButtons.YesNo);

            string saveLoc = "";
            if (dr == DialogResult.Yes) {
                saveLoc = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            } else {
                this._form.Invoke((Action)(() => {
                    this._form.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
                    DialogResult folderDR = this._form.folderBrowserDialog1.ShowDialog();

                    if (folderDR == DialogResult.OK) {
                        saveLoc = this._form.folderBrowserDialog1.SelectedPath;
                    }
                }));
            }

            // hardcode because all files in here are temporary, delete after encoding
            string open_path = Path.GetFullPath("tmp");
            string[] files = Directory.GetFiles(open_path);

            if (Directory.GetDirectories(saveLoc).Where((f) => f == this._album_title).Count() > 0) {
                string[] cmp_files = Directory.GetFiles(saveLoc+this._album_title);
                if (cmp_files.Length != files.Length) {
                    this._album_title += DateTime.Now.ToShortDateString();
                } else {
                    // they are the same we just use these for burning
                    // CHECK FOR SAME INFO 
                    return;
                }
            }

            DirectoryInfo d_info = Directory.CreateDirectory($"{saveLoc}\\{this._album_title}");
            for (int i = 0; i < files.Length; i++) {
                try {
                    bool last = (files.Length - 1) == i;
                    await this.EncodeFileMp3(files[i], i, d_info.FullName, last);
                } catch(Exception err) {
                    throw new IOException("Encoding Error", err);
                }
            }
        }

        private long _totalInput;
        async private Task EncodeFileMp3(string trkLoca, int trkNum, string saveDir, bool lastTrack) {
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
                    this._form.progressBar1.Visible = true;
                    this._form.progressBar1.Value = 0;
                    this._form.progressBar1.Maximum = (int)this._totalInput;
                }));
                audio_out.OnProgress += ((object writer, long inputBytes, long outputBytes, bool finished) => {
                    if (finished) {
                        this._form.progressBar1.Invoke((Action)(() => {
                            this._form.progressBar1.Value = this._form.progressBar1.Maximum;
                            if (lastTrack) {
                                this._form.progressBar1.Visible = false;
                                this._form.progressLabel.Text = "";
                                this._form.outLabel.Text = "Finished Ripping Click Burn to continue";
                            }
                        }));
                    }
                    else {
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
