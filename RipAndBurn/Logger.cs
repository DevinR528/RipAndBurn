using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RipAndBurn {
    public abstract class Logger {
        public abstract void Log(Exception err);
    }

    public class FileLogger : Logger {
        public string filePath = "log.txt";

        public FileLogger() {
            if (!File.Exists(this.filePath))
                File.Create(filePath).Close();
        }
        public override void Log(Exception err) {
            using (StreamWriter streamWriter = new StreamWriter(this.filePath)) {
                string msg = $"[{DateTime.Now.ToShortDateString()}] at [{DateTime.Now.ToShortTimeString()}] {err.TargetSite}"
                    + Environment.NewLine + "\t" + $"Source: {err.Source}"
                    + Environment.NewLine + "\t" + $"Message: {err.Message}"
                    + Environment.NewLine + "\t" + $"Trace: {err.StackTrace}"
                    + Environment.NewLine + "\t" + $"Inner: {err.InnerException}" + Environment.NewLine;
                streamWriter.Write(msg);
                streamWriter.Close();
            }
        }
    }
}
