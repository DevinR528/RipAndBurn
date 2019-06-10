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

using Ripper;

using RipAndBurn.Rip.CDMetadata.Release;
using RipAndBurn.Rip.CDMetadata.Album;
using RipAndBurn.Rip.CDMetadata.CDInfo;

using Media = RipAndBurn.Rip.CDMetadata.Release.Media;

namespace RipAndBurn.Rip
{


    class CDRipper {
        private string _album_title = "";

        private string _cdTrayName = "";

        private CurrentCD _intrnCurrentCD;
        private Action<int> _update;

        private CDDrive driver = new CDDrive();
        private WavLib.WaveWriter wavWriter;

        private InputBox _iBox;
        private RipBurn _form;
        private FileLogger log = new FileLogger();

        public CurrentCD CDRom {
            get => this._intrnCurrentCD;
        }

        public CDRipper(string drive, RipBurn rip) {
            this._form = rip;
            this._cdTrayName = drive;
        }

        // open and close cd drive
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        protected static extern int mciSendString(string lpstrCommand,
                                                   StringBuilder lpstrReturnString,
                                                   int uReturnLength,
                                                   IntPtr hwndCallback);
        public void Open() {
            int ret = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
        }

        public void Close() {
            int ret = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
        }

        async public Task GetCD_Id() {
            this._update = (amt) => {
                if (this._form.progressBar1.Value + amt <= this._form.progressBar1.Maximum) {
                    this._form.progressBar1.Value += amt;
                }
            };

            this._form.progressBar1.Invoke(this._update, 2);
            try {
                using (var disc = DiscId.Disc.Read(this._cdTrayName, Features.Mcn | Features.Isrc)) {
                    string queryRaw = disc.SubmissionUrl.ToString();

                    this._form.progressBar1.Invoke(this._update, 2);
                    string[] queryArr = queryRaw.Split(new string[] { "id=", "&" }, StringSplitOptions.None);
                    string query = $"{queryArr[1]}?&{queryArr[2]}&{queryArr[3]}";
                    await this.Get_CD_meta(query);
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
                                        if (media.Title != null && media.Title != "") {
                                            title = media.Title;
                                        } else if (rels.Title != null && rels.Title != "") {
                                            title = rels.Title;
                                        }

                                        if (title != "") {
                                            string id = rels.Id.ToString();
                                            HttpResponseMessage res2 = await client.GetAsync($"https://musicbrainz.org/ws/2/release/{id}?inc=artist-credits+recordings&fmt=json");
                                            HttpContent resBuff2 = res2.Content;

                                            this._form.progressBar1.Invoke(this._update, 2);
                                            string content = await resBuff2.ReadAsStringAsync();
                                            TrackList albums = TrackList.FromJson(content);

                                            try {
                                                CDMetadata.Album.Media album = albums.Media.Where(cd => cd.Title == title).Single();
                                                this._album_title = this.CleanInvalidFolder(title);
                                                this._form.progressBar1.Invoke(this._update, 2);
                                                // set fields we care about
                                                this._intrnCurrentCD = CurrentCD.FromMeta(album, title);

                                                this._form.Invoke((Action)(() => {
                                                    this._form.progressLabel.Text = "Finised collecting CD info";
                                                }));
                                                return;
                                            } catch {
                                                using (InputBox box = new InputBox()) {
                                                    box.ShowDialog();
                                                    this._album_title = box.IboxTitle;
                                                }

                                                try {
                                                    CDMetadata.Album.Media album_hope = albums.Media
                                                        .Where(cd => cd.Title.Contains(this._album_title) || cd.TrackCount == media.TrackCount)
                                                        .Single();
                                                    this._form.progressBar1.Invoke(this._update, 2);
                                                    // set fields we care about
                                                    this._intrnCurrentCD = CurrentCD.FromMeta(album_hope, title);

                                                    this._form.Invoke((Action)(() => {
                                                        this._form.progressLabel.Text = "Finised collecting CD info";
                                                    }));
                                                    return;
                                                } catch {
                                                    using (InputBox box = new InputBox()) {
                                                        box.ShowDialog();
                                                        this._album_title = box.IboxTitle;
                                                    }
                                                    return;
                                                }
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

        async public Task RipCDtoTemp(string saveDir) {
            if (this.driver.Open(this._cdTrayName.ToCharArray()[0])) {
                if (this.driver.IsCDReady()) {
                    if (this.driver.Refresh()) {
                        int trks = this.driver.GetNumTracks();
                        this.driver.LockCD();

                        try {
                            this._form.progressBar1.Invoke((Action)(() => {
                                this._form.progressBar1.Value = 0;
                                this._form.progressBar1.Maximum = 100;
                                string song = this._intrnCurrentCD == null ? "" : this._intrnCurrentCD.Tracks[0].Name + " ";
                                this._form.progressLabel.Text = $"Copying track (1. {song}) to temp folder";
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
                                    this.driver.UnLockCD();
                                }
                            }
                            this._form.progressBar1.Invoke((Action)(() => {
                                this._form.progressBar1.Visible = false;
                                this._form.actionButton.Enabled = true;
                                this._form.createButton.Enabled = true;

                                this._form.progressLabel.Text = "Completed!";
                            }));
                            // encode files !!!!!!!!!!!!!!!
                            await this.EncodeFolderMp3(saveDir);
                        }
                        catch (Exception e) {
                            throw new IOException("IO Stream failed", e);
                        }
                        finally {

                            this.driver.UnLockCD();
                            // clear tmp folder no matter what
                            string open_path = Path.GetFullPath("tmp");
                            string[] files = Directory.GetFiles(open_path);

                            for (int i = 0; i < files.Length; i++) {
                                this.DeleteFile(files[i]);
                            }
                            try {
                                DriveInfo drive = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).Single();
                                this.driver.Open(drive.Name.ToCharArray()[0]);
                            }
                            catch (Exception err) {
                                this.log.Log(err);
                            }
                        }
                    }
                }
            }
            else {
                // drive could not be read
                MessageBox.Show("Could not read media try another CD.");
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
                    int num_songs = this._intrnCurrentCD == null ? 0 : this._intrnCurrentCD.Tracks.Count;
                    if (this._trackNumRawProg < num_songs || this._intrnCurrentCD == null) {
                        this._form.progressBar1.Value = 0;
                        this._form.progressBar1.Maximum = 100;
                        string song = this._intrnCurrentCD == null ? "" : this._intrnCurrentCD.Tracks[this._trackNumRawProg].Name + " ";
                        this._form.progressLabel.Text = $"Copying track ({this._trackNumRawProg + 1}. {song}) to temp folder";
                        this._trackNumRawProg++;
                    }
                }));
            }
        }

        // folder is where we get .raw tracks from
        async public Task EncodeFolderMp3(string saveLoc) {
            // hardcode because all files in here are temporary, delete after encoding
            string open_path = Path.GetFullPath("tmp");
            string[] files = Directory.GetFiles(open_path);

            if (Directory.GetDirectories(saveLoc).Where((f) => f == this._album_title).Count() > 0) {
                string[] cmp_files = Directory.GetFiles(saveLoc + this._album_title);
                if (cmp_files.Length != files.Length) {
                    this._album_title += DateTime.Now.ToShortDateString();
                } else {
                    // they are the same we just use these for burning
                    // CHECK FOR SAME INFO 
                    return;
                }
            }

            // error from open directory somewhere here or encodefilemp3
            DirectoryInfo d_info = Directory.CreateDirectory($"{saveLoc}\\{this._album_title}");

            this.log.Log($"Converting to mp3 Path: {d_info.Name}");
            string path = d_info.FullName;
            for (int i = 0; i < files.Length; i++) {
                try {
                    bool last = (files.Length - 1) == i;
                    await this.EncodeFileMp3(files[i], i, d_info.FullName, saveLoc, last);
                } catch(Exception err) {
                    this.log.Log(err);
                    throw new IOException("Encoding Error", err);
                }
            }
        }

        private long _totalInput;
        async private Task EncodeFileMp3(string trkLoca, int trkNum, string saveDir, string saveName, bool lastTrack) {
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
                            this._form.progressLabel.Text = $"Converting ({title}) to MP3 and saving to {saveName} folder";
                            this._form.progressBar1.Increment(amt);
                        }), (int)inputBytes);
                    }
                });
                await audio_in.CopyToAsync(audio_out);
                audio_in.Close(); audio_out.Close();
            }
        }

        private void DeleteFile(string file) {
            File.Delete(file);
        }
    }
}
