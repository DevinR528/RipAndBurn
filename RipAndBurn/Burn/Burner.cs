using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using IMAPI2.Interop;
using IMAPI2.MediaItem;

using RipAndBurn.Rip.CDMetadata.CDInfo;
using Istream = System.Runtime.InteropServices.ComTypes.IStream;


namespace RipAndBurn.Burn {

    class Burner {

        private string _driveName;

        private List<string> _fileLoc;

        private string _title;

        private dynamic _form;

        private FileLogger log = new FileLogger();

        private BurnData _bData = new BurnData();

        private MsftFileSystemImage _fsImage;

        private MsftDiscFormat2Data _cdFormat;


        public Burner(string drive, string path, RipBurn form) {
            this._driveName = drive;

            this._fileLoc = Directory.GetFiles(path).ToList();
            this._title = path.Split('\\').Last();
            
            this._form = form;

            this.log.Log("Start Burn");
            this.StartBurn();
        }

        public Burner(string drive, List<string> paths, Create.CreateCD form) {
            this._driveName = drive;

            this._fileLoc = paths;
            this._title = "Mix";

            this._form = form;

            this.log.Log("Start Burn");
            this.StartBurn();
        }

        public void StartBurn() {
            this._cdFormat = new MsftDiscFormat2Data();

            MsftDiscMaster2 discMaster = new MsftDiscMaster2();

            if (!discMaster.IsSupportedEnvironment)
                throw new NotSupportedException("Disc type not supported");

            foreach (string uniq_id in discMaster) {
                MsftDiscRecorder2 discRecorder2 = new MsftDiscRecorder2();
                discRecorder2.InitializeDiscRecorder(uniq_id);

                this._bData.uniqueRecorderId = discRecorder2.ActiveDiscRecorder;

                if (this._driveName == (string)discRecorder2.VolumePathNames[0]) {
                    try {
                        this._cdFormat.Recorder = discRecorder2;

                        // creates mut filesys using disc recorder
                        System.Runtime.InteropServices.ComTypes.IStream fileSystem;
                        if (this.CreateFs(discRecorder2, out fileSystem)) {
                            // keeps track of progress and other info
                            MsftDiscFormat2Data disc_fmt_data = new MsftDiscFormat2Data {
                                Recorder = discRecorder2,
                                ClientName = "Burner",
                                ForceMediaToBeClosed = true,
                            };

                            disc_fmt_data.Update += discWrite_onProgress;
                            this.log.Log("Start Image Write");
                            try {
                                // write to disc
                                disc_fmt_data.Write(fileSystem);
                            }
                            catch (Exception err) {
                                this.log.Log(err);
                                throw new BurnException("Write failed", err);
                            }
                            finally {
                                if (this._cdFormat != null) {
                                    Marshal.ReleaseComObject(this._cdFormat);
                                }
                                if (this._fsImage != null) {
                                    Marshal.ReleaseComObject(this._fsImage);
                                }
                            }
                        }
                    }
                    catch (COMException err) {
                        this.log.Log(err);
                        throw new FormatException("Unable to create image on media, check there is a blank CD in drive", err);
                    }
                    catch (Exception err) {
                        Type e = err.GetType();
                        this.log.Log(err, e.Name);
                        throw new UnreachException("HMMMM NOT COOL", err);
                    } finally {
                        if (this._cdFormat != null) {
                            Marshal.ReleaseComObject(this._cdFormat);
                        }

                        if (this._fsImage != null) {
                            Marshal.ReleaseComObject(this._fsImage);
                        }
                    }
                }
            }
            
        }

        private bool CreateFs(IDiscRecorder2 disc, out Istream ds) {
            //cdr 2, cdrom 1 cdrw 3, disk 12
            this._fsImage = new MsftFileSystemImage();
            this._fsImage.ChooseImageDefaults(disc);
            // ********************************* Universal disc format                      iso standard format
            this._fsImage.FileSystemsToCreate = FsiFileSystems.FsiFileSystemUDF | FsiFileSystems.FsiFileSystemISO9660;
            this._fsImage.VolumeName = this._title;

            this._fsImage.Update += createFs_onProgress;

            if (!this._cdFormat.MediaHeuristicallyBlank) {
                this._fsImage.MultisessionInterfaces = this._cdFormat.MultisessionInterfaces;
                this._fsImage.ImportFileSystem();
            }

            Int64 freeMediaBlocks = this._fsImage.FreeMediaBlocks;
            long _totalDiscSize = 2048 * freeMediaBlocks;

            IFsiDirectoryItem rootItem = this._fsImage.Root;

            try {
                foreach (string path in this._fileLoc) {
                    string full = Path.GetFullPath(path);
                    var fileItem = new FileItem(full);
                    fileItem.AddToFileSystem(rootItem);
                }
                this.log.Log("Create FS Image");
                ds = this._fsImage.CreateResultImage().ImageStream;
            } catch (Exception err) {
                this.log.Log(err);
                throw new BadImageFormatException("Could not create image or file items on image", err);
            }
            return true;
        }

        private void discWrite_onProgress(
            [In, MarshalAs(UnmanagedType.IDispatch)] object sender,
            [In, MarshalAs(UnmanagedType.IDispatch)] object progress)
        {
            var format2Data = (IDiscFormat2Data)sender;
            var eventArgs = (IDiscFormat2DataEventArgs)progress;

            this._bData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            // IDiscFormat2DataEventArgs Interface
            this._bData.elapsedTime = eventArgs.ElapsedTime;
            this._bData.remainingTime = eventArgs.RemainingTime;
            this._bData.totalTime = eventArgs.TotalTime;

            // IWriteEngine2EventArgs Interface
            this._bData.currentAction = eventArgs.CurrentAction;
            this._bData.startLba = eventArgs.StartLba;
            this._bData.sectorCount = eventArgs.SectorCount;
            this._bData.lastReadLba = eventArgs.LastReadLba;
            this._bData.lastWrittenLba = eventArgs.LastWrittenLba;
            this._bData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            this._bData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            this._bData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            this._form.backgroundWorker1.ReportProgress(0, this._bData);
        }

        private void createFs_onProgress([In, MarshalAs(UnmanagedType.IDispatch)] object sender,
            [In, MarshalAs(UnmanagedType.BStr)]string currentFile, [In] int copiedSectors, [In] int totalSectors) {
            var percentProgress = 0;
            if (copiedSectors > 0 && totalSectors > 0) {
                percentProgress = (copiedSectors * 100) / totalSectors;
            }

            if (!string.IsNullOrEmpty(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                this._bData.statusMessage = $"Adding {fileInfo.Name} to disc";

                this._bData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM;
                this._form.backgroundWorker1.ReportProgress(percentProgress, this._bData);
            }
        }
    }
}
