using System;
using System.Windows.Forms;

namespace RipAndBurn {
    public partial class InputBox : Form {

        private string _title = "";

        public string IboxTitle {
            get => this._title;
        }

        public InputBox() {
            InitializeComponent();
        }

        public InputBox(string label) {
            InitializeComponent();
            this.label1.Text = label;
        }

        private void NameButton_Click(object sender, EventArgs e) {
            if (this.nameTextBox.Text != "") {
                this._title = this.nameTextBox.Text;
                this.Close();
            } else {
                this.label1.Text = "Oops add an Album title below.";
                this.nameTextBox.Focus();
            }
        }

        private void onForm_Closing(object sender, FormClosingEventArgs e) {
            if (this._title == "") {
                e.Cancel = true;
            }
        }
    }
}
