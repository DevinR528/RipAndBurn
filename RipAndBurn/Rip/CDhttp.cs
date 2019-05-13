using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using DiscId;

using RipAndBurn.Rip.CDMetadata.Release;
using RipAndBurn.Rip.CDMetadata.Album;
using RipAndBurn.Rip.CDMetadata.CDInfo;
// GET RID OF
using System.Windows.Forms;

using System.ComponentModel;
using Media = RipAndBurn.Rip.CDMetadata.Release.Media;

namespace RipAndBurn.Rip
{
    class CDhttp
    {

        private string _discID = "";

        private CurrentCD _currentCD;

        private Action<int> _update;

        private InputBox _iBox;
        public CurrentCD CDRom {
            get => this._currentCD;
        }
        public InputBox IBox {
            get => this._iBox;
        }

        public CDhttp()
        {
            
        }

        public int GetCD_Id(string drive, RipBurn ripperForm) {
            this._update = (amt) => ripperForm.progressBar1.Value += amt;

            ripperForm.progressBar1.Invoke(this._update, 2);
            try {
                using (var disc = DiscId.Disc.Read(drive, Features.Mcn | Features.Isrc)) {
                    string queryRaw = disc.SubmissionUrl.ToString();

                    ripperForm.progressBar1.Invoke(this._update, 2);
                    string[] queryArr = queryRaw.Split('?', '=', '&');
                    string query = $"{queryArr[2]}?{queryArr[5]}={queryArr[6]}";
                    this._discID = query;
                    return 0;
                }
            }
            catch (DiscIdException ex) {
                throw ex;
            }
        }


        async public Task GetReleaseData(RipBurn ripperForm) {

            ripperForm.progressBar1.Invoke(this._update, 2);
            try {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("User-Agent", "Grandpa's Cd Copier/1.0.0 ( devin.ragotzy@gmail.com )");
                HttpResponseMessage res = await client.GetAsync($"https://musicbrainz.org/ws/2/discid/{this._discID}&fmt=json");
                
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
                                                .Where((album) => album.Title == title)) {
                                                ripperForm.progressBar1.Invoke(this._update, 2);
                                                this._currentCD = CurrentCD.FromMeta(album);

                                                ripperForm.Invoke((Action)(() => {
                                                    ripperForm.progressLabel.Text = "Finised collecting CD info";
                                                }));
                                                this.IBox.Dispose();
                                                return;
                                            }
                                        } else {
                                            // Cancel hit
                                            MessageBox.Show("Error");
                                        }
                                    } else {
                                        // cant find info so continue on
                                        MessageBox.Show("error");
                                    }
                                } else {
                                    // no matching sectors
                                }
                            }
                        }
                    }
                } else {
                    // status not 200
                    MessageBox.Show("Probably not connected to the internet, if so we'll try again");
                }
            } catch (Exception err) {
                throw err;
            }
        }




    }
}
