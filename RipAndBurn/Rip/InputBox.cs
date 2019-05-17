using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RipAndBurn.Rip.CDMetadata.Release;
using Media = RipAndBurn.Rip.CDMetadata.Release.Media;

namespace RipAndBurn {
    public partial class InputBox : Form {

        private List<Media> _albums;
        private Media _album;

        public InputBox(List<Media> cd) {
            InitializeComponent();
            this._albums = cd;
        }

        public Media Album {
            get => this._album;
        }

        public string GetTitle() => this._album.Title;

        private void InputBox_Load(object sender, EventArgs e) {
            foreach(Media album in this._albums) {
                // work on this make sure we dont have to do more json nav
                this.nameComboBox.Items.Add(album.Title);
            }
        }
        private void NameButton_Click(object sender, EventArgs e) {
            if (this.nameComboBox.Text != "") {
                try {
                    this._album = this._albums.Where((cd) => cd.Title == this.nameComboBox.Text).Single();
                } catch (Exception err) {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private void CancelButton_Click(object sender, EventArgs e) {

        }
    }
}
