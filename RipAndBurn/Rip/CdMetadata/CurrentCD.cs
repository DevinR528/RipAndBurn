using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RipAndBurn.Rip.CDMetadata.CDInfo {
    public class SingleTrack {
        public long? Number { get; set; }
        public string Name { get; set; }
        public long? ByteLen { get; set; }

        public SingleTrack(Album.Track trk) {
            this.Name = this.CleanInvalidFile(trk.Title);
            this.Number = trk.Position;
            this.ByteLen = trk.Length;
        }

        private string CleanInvalidFile(string file) {
            return string.Join("_", file.Split(Path.GetInvalidFileNameChars()));
        }
    }
    public class CurrentCD {
        public string Title { get; set; }
        public string Artist { get; set; }
        public long? TotalLen { get; set; }
        public List<SingleTrack> Tracks { get; set; }

        public CurrentCD() { }

        public static CurrentCD FromMeta(CDMetadata.Album.Media album, string title) {
            CurrentCD storedCd = new CurrentCD {
                Title = album.Title != "" ? album.Title : title,
                Artist = album.Tracks[0].ArtistCredit[0].Name,
                Tracks = new List<SingleTrack>(),
            };

            foreach (Album.Track trk in album.Tracks) {
                SingleTrack t = new SingleTrack(trk);
                storedCd.TotalLen += t.ByteLen;
                storedCd.Tracks.Add(t);
            }

            return storedCd;
        }
    }
}
