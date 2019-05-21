namespace RipAndBurn
{
    partial class RipBurn
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RipBurn));
            this.actionButton = new System.Windows.Forms.Button();
            this.outLabel = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.burnButton = new System.Windows.Forms.Button();
            this.progressLabel = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // actionButton
            // 
            this.actionButton.Location = new System.Drawing.Point(199, 192);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(102, 38);
            this.actionButton.TabIndex = 0;
            this.actionButton.Text = "Click to Start";
            this.actionButton.UseVisualStyleBackColor = true;
            this.actionButton.Click += new System.EventHandler(this.ActionButton_Click);
            // 
            // outLabel
            // 
            this.outLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outLabel.Location = new System.Drawing.Point(37, 160);
            this.outLabel.Name = "outLabel";
            this.outLabel.Size = new System.Drawing.Size(431, 25);
            this.outLabel.TabIndex = 1;
            this.outLabel.Text = "label1";
            this.outLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(28, 140);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(481, 17);
            this.progressBar1.TabIndex = 2;
            // 
            // burnButton
            // 
            this.burnButton.Location = new System.Drawing.Point(199, 254);
            this.burnButton.Name = "burnButton";
            this.burnButton.Size = new System.Drawing.Size(102, 23);
            this.burnButton.TabIndex = 3;
            this.burnButton.Text = "Burn";
            this.burnButton.UseVisualStyleBackColor = true;
            this.burnButton.Click += new System.EventHandler(this.BurnButton_Click);
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(228, 124);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(35, 13);
            this.progressLabel.TabIndex = 4;
            this.progressLabel.Text = "label1";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.burnerThread_doWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.burnerThread_onProgress);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.burnerThread_onComplete);
            // 
            // RipBurn
            // 
            this.AcceptButton = this.actionButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 297);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.burnButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.outLabel);
            this.Controls.Add(this.actionButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RipBurn";
            this.Text = "Rip and Burn a CD Grandpa!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ripper_onFormClose);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button actionButton;
        private System.Windows.Forms.Button burnButton;
        public System.Windows.Forms.ProgressBar progressBar1;
        public System.Windows.Forms.Label progressLabel;
        public System.Windows.Forms.SaveFileDialog saveFileDialog1;
        public System.ComponentModel.BackgroundWorker backgroundWorker1;
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        public System.Windows.Forms.Label outLabel;
    }
}

