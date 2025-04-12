
namespace Arduino_LED_Strip_Controller
{
    partial class Form1
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
            this.modeSelect = new System.Windows.Forms.ComboBox();
            this.comPort = new System.Windows.Forms.ComboBox();
            this.connect = new System.Windows.Forms.Button();
            this.screenSelection = new System.Windows.Forms.ComboBox();
            this.saveAndClose = new System.Windows.Forms.Button();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.Main = new System.Windows.Forms.TabPage();
            this.modeLabel = new System.Windows.Forms.Label();
            this.connect1 = new System.Windows.Forms.Button();
            this.Settings = new System.Windows.Forms.TabPage();
            this.audioOutput = new System.Windows.Forms.ComboBox();
            this.audioInput = new System.Windows.Forms.ComboBox();
            this.Tabs.SuspendLayout();
            this.Main.SuspendLayout();
            this.Settings.SuspendLayout();
            this.SuspendLayout();
            // 
            // modeSelect
            // 
            this.modeSelect.FormattingEnabled = true;
            this.modeSelect.Items.AddRange(new object[] {
            "Screen Sync",
            "Music Sync"});
            this.modeSelect.Location = new System.Drawing.Point(58, 6);
            this.modeSelect.Name = "modeSelect";
            this.modeSelect.Size = new System.Drawing.Size(147, 21);
            this.modeSelect.TabIndex = 0;
            this.modeSelect.SelectedIndexChanged += new System.EventHandler(this.modeSelect_SelectedIndexChanged);
            // 
            // comPort
            // 
            this.comPort.FormattingEnabled = true;
            this.comPort.Location = new System.Drawing.Point(6, 33);
            this.comPort.Name = "comPort";
            this.comPort.Size = new System.Drawing.Size(121, 21);
            this.comPort.TabIndex = 1;
            this.comPort.Text = "Select arduino COM port";
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(133, 33);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 2;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // screenSelection
            // 
            this.screenSelection.FormattingEnabled = true;
            this.screenSelection.Location = new System.Drawing.Point(6, 6);
            this.screenSelection.Name = "screenSelection";
            this.screenSelection.Size = new System.Drawing.Size(202, 21);
            this.screenSelection.TabIndex = 3;
            this.screenSelection.Text = "Select monitor";
            // 
            // saveAndClose
            // 
            this.saveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveAndClose.Location = new System.Drawing.Point(815, 524);
            this.saveAndClose.Name = "saveAndClose";
            this.saveAndClose.Size = new System.Drawing.Size(75, 23);
            this.saveAndClose.TabIndex = 4;
            this.saveAndClose.Text = "Save";
            this.saveAndClose.UseVisualStyleBackColor = true;
            this.saveAndClose.Click += new System.EventHandler(this.saveAndClose_Click);
            // 
            // Tabs
            // 
            this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Tabs.Controls.Add(this.Main);
            this.Tabs.Controls.Add(this.Settings);
            this.Tabs.Location = new System.Drawing.Point(12, 12);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(904, 579);
            this.Tabs.TabIndex = 5;
            // 
            // Main
            // 
            this.Main.Controls.Add(this.modeLabel);
            this.Main.Controls.Add(this.connect1);
            this.Main.Controls.Add(this.modeSelect);
            this.Main.Location = new System.Drawing.Point(4, 22);
            this.Main.Name = "Main";
            this.Main.Padding = new System.Windows.Forms.Padding(3);
            this.Main.Size = new System.Drawing.Size(896, 553);
            this.Main.TabIndex = 1;
            this.Main.Text = "Main";
            this.Main.UseVisualStyleBackColor = true;
            // 
            // modeLabel
            // 
            this.modeLabel.AutoSize = true;
            this.modeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modeLabel.Location = new System.Drawing.Point(6, 7);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(46, 16);
            this.modeLabel.TabIndex = 4;
            this.modeLabel.Text = "Mode:";
            // 
            // connect1
            // 
            this.connect1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connect1.Location = new System.Drawing.Point(815, 6);
            this.connect1.Name = "connect1";
            this.connect1.Size = new System.Drawing.Size(75, 23);
            this.connect1.TabIndex = 3;
            this.connect1.Text = "Connect";
            this.connect1.UseVisualStyleBackColor = true;
            this.connect1.Click += new System.EventHandler(this.connect_Click);
            // 
            // Settings
            // 
            this.Settings.Controls.Add(this.audioOutput);
            this.Settings.Controls.Add(this.audioInput);
            this.Settings.Controls.Add(this.saveAndClose);
            this.Settings.Controls.Add(this.screenSelection);
            this.Settings.Controls.Add(this.connect);
            this.Settings.Controls.Add(this.comPort);
            this.Settings.Location = new System.Drawing.Point(4, 22);
            this.Settings.Name = "Settings";
            this.Settings.Padding = new System.Windows.Forms.Padding(3);
            this.Settings.Size = new System.Drawing.Size(896, 553);
            this.Settings.TabIndex = 2;
            this.Settings.Text = "Settings";
            this.Settings.UseVisualStyleBackColor = true;
            // 
            // audioOutput
            // 
            this.audioOutput.FormattingEnabled = true;
            this.audioOutput.Location = new System.Drawing.Point(6, 87);
            this.audioOutput.Name = "audioOutput";
            this.audioOutput.Size = new System.Drawing.Size(151, 21);
            this.audioOutput.TabIndex = 6;
            this.audioOutput.Text = "Select audio output device";
            // 
            // audioInput
            // 
            this.audioInput.FormattingEnabled = true;
            this.audioInput.Location = new System.Drawing.Point(6, 60);
            this.audioInput.Name = "audioInput";
            this.audioInput.Size = new System.Drawing.Size(151, 21);
            this.audioInput.TabIndex = 5;
            this.audioInput.Text = "Select audio input device";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 603);
            this.Controls.Add(this.Tabs);
            this.Name = "Form1";
            this.Text = "Arduino LED Strip Controller";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Tabs.ResumeLayout(false);
            this.Main.ResumeLayout(false);
            this.Main.PerformLayout();
            this.Settings.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox modeSelect;
        private System.Windows.Forms.ComboBox comPort;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.ComboBox screenSelection;
        private System.Windows.Forms.Button saveAndClose;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage Main;
        private System.Windows.Forms.Button connect1;
        private System.Windows.Forms.TabPage Settings;
        private System.Windows.Forms.Label modeLabel;
        private System.Windows.Forms.ComboBox audioOutput;
        private System.Windows.Forms.ComboBox audioInput;
    }
}

