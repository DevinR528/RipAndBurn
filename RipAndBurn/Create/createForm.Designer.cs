namespace RipAndBurn.Create {
    partial class CreateCD {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.albumListBox = new System.Windows.Forms.ListBox();
            this.songListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.burnListBox = new System.Windows.Forms.ListBox();
            this.burnButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Pick an Album";
            // 
            // albumListBox
            // 
            this.albumListBox.FormattingEnabled = true;
            this.albumListBox.Location = new System.Drawing.Point(15, 25);
            this.albumListBox.Name = "albumListBox";
            this.albumListBox.Size = new System.Drawing.Size(152, 212);
            this.albumListBox.TabIndex = 3;
            this.albumListBox.SelectedValueChanged += new System.EventHandler(this.selectedVal_onChange);
            // 
            // songListBox
            // 
            this.songListBox.FormattingEnabled = true;
            this.songListBox.Location = new System.Drawing.Point(184, 25);
            this.songListBox.Name = "songListBox";
            this.songListBox.Size = new System.Drawing.Size(150, 212);
            this.songListBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(181, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Pick a Song";
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(91, 252);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(103, 27);
            this.addButton.TabIndex = 6;
            this.addButton.Text = "Add Song";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(336, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Songs to Burn";
            // 
            // burnListBox
            // 
            this.burnListBox.FormattingEnabled = true;
            this.burnListBox.Location = new System.Drawing.Point(339, 25);
            this.burnListBox.Name = "burnListBox";
            this.burnListBox.Size = new System.Drawing.Size(150, 212);
            this.burnListBox.TabIndex = 7;
            // 
            // burnButton
            // 
            this.burnButton.Location = new System.Drawing.Point(315, 252);
            this.burnButton.Name = "burnButton";
            this.burnButton.Size = new System.Drawing.Size(103, 26);
            this.burnButton.TabIndex = 9;
            this.burnButton.Text = "Burn";
            this.burnButton.UseVisualStyleBackColor = true;
            this.burnButton.Click += new System.EventHandler(this.BurnButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(164, 318);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(202, 23);
            this.progressBar1.TabIndex = 10;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.burnerThread_doWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.burnerThread_onProgress);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.burnerThread_onComplete);
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(211, 293);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(35, 13);
            this.progressLabel.TabIndex = 11;
            this.progressLabel.Text = "label4";
            // 
            // CreateCD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 353);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.burnButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.burnListBox);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.songListBox);
            this.Controls.Add(this.albumListBox);
            this.Controls.Add(this.label1);
            this.Name = "CreateCD";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.CreateCD_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox albumListBox;
        private System.Windows.Forms.ListBox songListBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox burnListBox;
        private System.Windows.Forms.Button burnButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label progressLabel;
        public System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}