using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Management;
using System.IO;

using RipAndBurn.Rip.CDMetadata.CDInfo;
// GET RID OF
using System.Windows.Forms;



namespace RipAndBurn.Rip
{


    class CDRipper
    {

        private bool _isOpen = false;
        private bool _isReady = false;

        private DriveInfo _CDdrive;

        private Action<int> _update;

        private CDhttp _cdMeta = new CDhttp();

        public bool IsReady {
            get => _isReady;
        }

        public DriveInfo CDdrive { get => _CDdrive; set => _CDdrive = value; }

        public CDRipper() {}

        // open and close cd drive
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        protected static extern int mciSendString(string lpstrCommand,
                                                   StringBuilder lpstrReturnString,
                                                   int uReturnLength,
                                                   IntPtr hwndCallback);
        public void Open()
        {
            if (!this._isOpen) {
                int ret = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
                this._isOpen = true;
            }
        }

        public void Close()
        {
            if (this._isOpen) {
                int ret = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
                this._isOpen = false;
            }

        }


        async public Task RipCDtoTemp(CurrentCD currCD, RipBurn ripForm) {
            this._update = (amt) => ripForm.progressBar1.Value += amt;
            ripForm.progressBar1.Invoke((Action)(() => {
                ripForm.progressBar1.Value = 0;
                ripForm.progressLabel.Text = "Now Copying CD to Music";
            }));
            StreamReader input = new StreamReader(this.CDdrive.Name, true);

            StreamWriter output = new StreamWriter(this.CDdrive.Name);
        }

    }
}
