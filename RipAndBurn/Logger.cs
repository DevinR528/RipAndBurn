using System;
using System.IO;

namespace RipAndBurn {
    public abstract class Logger {
        public abstract void Log(Exception err);
        public abstract void Log(string msg);
        public abstract void Log(Exception err, string msg);
    }

    public class FileLogger : Logger {
        public string filePath = "log.txt";

        public FileLogger() {
            if (!File.Exists(this.filePath))
                File.Create(filePath).Close();
        }
        public override void Log(Exception err) {
            string msg = $"[{DateTime.Now.ToShortDateString()}] at [{DateTime.Now.ToShortTimeString()}] {err.TargetSite}"
                    + Environment.NewLine + "\t" + $"Source: {err.Source}"
                    + Environment.NewLine + "\t" + $"Message: {err.Message}"
                    + Environment.NewLine + "\t" + $"Trace: {err.StackTrace}"
                    + Environment.NewLine + "\t" + $"Inner: {err.InnerException}" + Environment.NewLine + Environment.NewLine;

            using (StreamWriter fst = File.AppendText(this.filePath)) {
                lock(fst) {
                    fst.Write(msg);
                    fst.Close();
                }
            }
        }

        public override void Log(string msg) {
            using (StreamWriter fst = File.AppendText(this.filePath)) {
                string dEOL = Environment.NewLine + Environment.NewLine;
                lock(fst) {
                    fst.Write($"[{DateTime.Now.ToShortDateString()}] at [{DateTime.Now.ToShortTimeString()}]\r\n\t {msg+dEOL}");
                    fst.Close();
                }
            }
        }

        public override void Log(Exception err, string msg) {
            string report = $"[{DateTime.Now.ToShortDateString()}] at [{DateTime.Now.ToShortTimeString()}] {msg}"
                    + Environment.NewLine + "\t" + $"Source: {err.Source}"
                    + Environment.NewLine + "\t" + $"Message: {err.Message}"
                    + Environment.NewLine + "\t" + $"Trace: {err.StackTrace}"
                    + Environment.NewLine + "\t" + $"Inner: {err.InnerException}" + Environment.NewLine + Environment.NewLine;

            using (StreamWriter fst = File.AppendText(this.filePath)) {
                lock (fst) {
                    fst.Write(report);
                    fst.Close();
                }
            }
        }
    }
}
