using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Management;
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

        private InputBox _iBox;
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
            
            //if (!this._isOpen) {
            //    int ret = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
            //    this._isOpen = true;
            //}
        }

        public void Close() {
            if (this._isOpen) {
                int ret = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
                this._isOpen = false;
            }

        }

        public string GetCD_Id(string drive, RipBurn ripperForm) {
            this._update = (amt) => ripperForm.progressBar1.Value += amt;

            ripperForm.progressBar1.Invoke(this._update, 2);
            try {
                using (var disc = DiscId.Disc.Read(drive, Features.Mcn | Features.Isrc)) {
                    string queryRaw = disc.SubmissionUrl.ToString();

                    ripperForm.progressBar1.Invoke(this._update, 2);
                    string[] queryArr = queryRaw.Split(new string[] { "id=", "&" }, StringSplitOptions.None);
                    string query = $"{queryArr[1]}?&{queryArr[2]}&{queryArr[3]}";
                    return query;
                }
            }
            catch (DiscIdException ex) {
                throw ex;
            }
        }

        async public Task GetReleaseData(RipBurn ripperForm, string diskQuery) {

            ripperForm.progressBar1.Invoke(this._update, 2);
            try {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("User-Agent", "Grandpa's Cd Copier/1.0.0 ( devin.ragotzy@gmail.com )");
                HttpResponseMessage res = await client.GetAsync($"https://musicbrainz.org/ws/2/discid/{diskQuery}&fmt=json");

                if (res.IsSuccessStatusCode) {
                    HttpContent resBuff = res.Content;

                    string body = await resBuff.ReadAsStringAsync();
                    AllReleases r = AllReleases.FromJson(body);

                    ripperForm.progressBar1.Invoke(this._update, 2);
                    // {83644aee-b687-4e1c-b5e6-5f21faee0eb1}
                    foreach (Release rels in r.Releases) {
                        foreach (Media media in rels.Media) {
                            foreach (AllReleases metaInfo in media.Discs) {
                                if (metaInfo.Sectors == r.Sectors) {
                                    if (rels.Id != null) {
                                        // title
                                        this._iBox = new InputBox(rels.Media);
                                        DialogResult dRes = this._iBox.ShowDialog();
                                        string title = this._iBox.GetTitle();

                                        if (dRes == DialogResult.OK) {
                                            string id = rels.Id.ToString();
                                            HttpResponseMessage res2 = await client.GetAsync($"https://musicbrainz.org/ws/2/release/{id}?inc=artist-credits+recordings&fmt=json");
                                            HttpContent resBuff2 = res2.Content;

                                            ripperForm.progressBar1.Invoke(this._update, 2);
                                            string content = await resBuff2.ReadAsStringAsync();
                                            TrackList albums = TrackList.FromJson(content);

                                            foreach (CDMetadata.Album.Media album in albums.Media
                                                .Where((album) => album.Title == title))
                                            {

                                                ripperForm.progressBar1.Invoke(this._update, 2);
                                                // set fields we care about
                                                this._intrnCurrentCD = CurrentCD.FromMeta(album);

                                                ripperForm.Invoke((Action)(() => {
                                                    ripperForm.progressLabel.Text = "Finised collecting CD info";
                                                }));
                                                this.IBox.Dispose();
                                                return;
                                            }
                                        }
                                        else {
                                            // Cancel hit
                                            MessageBox.Show("Error");
                                        }
                                    }
                                    else {
                                        // cant find info so continue on
                                        MessageBox.Show("error");
                                    }
                                }
                                else {
                                    // no matching sectors
                                }
                            }
                        }
                    }
                } else {
                    // status not 200
                    MessageBox.Show("Probably not connected to the internet, if so we'll try again");
                }
            }
            catch (Exception err) {
                throw err;
            }
        }

        async public Task RipCDtoTemp(CurrentCD currCD, char driveChar, RipBurn ripForm) {
            this._update = (amt) => ripForm.progressBar1.Value += amt;
            ripForm.progressBar1.Invoke((Action)(() => {
                ripForm.progressBar1.Value = 0;
                ripForm.progressBar1.Maximum = currCD.Tracks.Count * 10;
                ripForm.progressLabel.Text = "Now Copying CD to Music folder";
            }));
            if (this.driver.Open(driveChar)) {
                if (this.driver.IsCDReady()) {
                    if (this.driver.Refresh()) {
                        int trks = this.driver.GetNumTracks();
                        byte[] buff = new byte[4096];
                        this.driver.LockCD();

                        try {
                            for (int i = 1; i <= trks; i++) {
                                Stream Strm = new FileStream(string.Format("track{0:00}.raw", i),
                                                                FileMode.Create, FileAccess.Write);
                                try {
                                    uint Size = 0;
                                    while (this.driver.ReadTrack(i, buff, ref Size, null) > 0)
                                    //If there is no error and datawas read 
                                    //successfully a positive number is returned
                                    {
                                        Strm.Write(buff, 0, (int)Size);
                                    }
                                }
                                finally {
                                    Strm.Close();
                                }
                            }
                            
                        } catch (Exception err) {
                            MessageBox.Show(err.ToString());
                        } finally {
                            this.driver.UnLockCD();
                        }
                    }
                }
            }
        }
    }
}

//ID3TagData tag = new ID3TagData {
//    Artist = currCD.Artist,
//    Album = currCD.Title,
//    Title = currCD.Tracks[i].Name,
//    Track = (i + 1).ToString(),
//};
//using (AudioFileReader audIn = new AudioFileReader(dir))
//using (LameMP3FileWriter audOut = new LameMP3FileWriter($"test\\{tag.Title}.mp3", audIn.WaveFormat, LAMEPreset.V3, tag)) {
//    audOut.MinProgressTime = 250;
//    long input_length = audIn.Length;
//    audOut.OnProgress += ((object writer, long inputBytes, long outputBytes, bool finished) => {
//        ripForm.progressLabel.Text = $"Now Copying CD to Music folder song: {tag.Title}";
//        int to_add = 10;
//        ripForm.progressBar1.Invoke(this._update, to_add);
//    });
//    await audIn.CopyToAsync(audOut);
//}