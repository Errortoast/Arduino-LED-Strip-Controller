
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comPort = new System.Windows.Forms.ComboBox();
            this.connect = new System.Windows.Forms.Button();
            this.screenSelection = new System.Windows.Forms.ComboBox();
            this.saveAndClose = new System.Windows.Forms.Button();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.Main = new System.Windows.Forms.TabPage();
            this.colorDisplay = new System.Windows.Forms.Panel();
            this.fade = new System.Windows.Forms.Button();
            this.colorPicker = new System.Windows.Forms.Button();
            this.musicSync = new System.Windows.Forms.Button();
            this.screenSync = new System.Windows.Forms.Button();
            this.modeLabel = new System.Windows.Forms.Label();
            this.connect1 = new System.Windows.Forms.Button();
            this.Settings = new System.Windows.Forms.TabPage();
            this.blue = new System.Windows.Forms.Label();
            this.green = new System.Windows.Forms.Label();
            this.red = new System.Windows.Forms.Label();
            this.treble = new System.Windows.Forms.Label();
            this.mid = new System.Windows.Forms.Label();
            this.bass = new System.Windows.Forms.Label();
            this.blueTreble = new System.Windows.Forms.CheckBox();
            this.greenTreble = new System.Windows.Forms.CheckBox();
            this.redTreble = new System.Windows.Forms.CheckBox();
            this.blueMid = new System.Windows.Forms.CheckBox();
            this.greenMid = new System.Windows.Forms.CheckBox();
            this.redMid = new System.Windows.Forms.CheckBox();
            this.blueBass = new System.Windows.Forms.CheckBox();
            this.greenBass = new System.Windows.Forms.CheckBox();
            this.redBass = new System.Windows.Forms.CheckBox();
            this.generalLabel = new System.Windows.Forms.Label();
            this.audioSyncLabel = new System.Windows.Forms.Label();
            this.screenSyncLabel = new System.Windows.Forms.Label();
            this.useSpeakersChk = new System.Windows.Forms.CheckBox();
            this.audioOutput = new System.Windows.Forms.ComboBox();
            this.audioInput = new System.Windows.Forms.ComboBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.Tabs.SuspendLayout();
            this.Main.SuspendLayout();
            this.Settings.SuspendLayout();
            this.SuspendLayout();
            // 
            // comPort
            // 
            this.comPort.FormattingEnabled = true;
            this.comPort.Location = new System.Drawing.Point(10, 33);
            this.comPort.Name = "comPort";
            this.comPort.Size = new System.Drawing.Size(121, 21);
            this.comPort.TabIndex = 1;
            this.comPort.Text = "Select arduino COM port";
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(137, 33);
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
            this.screenSelection.Location = new System.Drawing.Point(10, 95);
            this.screenSelection.Name = "screenSelection";
            this.screenSelection.Size = new System.Drawing.Size(202, 21);
            this.screenSelection.TabIndex = 3;
            this.screenSelection.Text = "Select monitor";
            // 
            // saveAndClose
            // 
            this.saveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveAndClose.Location = new System.Drawing.Point(309, 413);
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
            this.Tabs.Size = new System.Drawing.Size(398, 468);
            this.Tabs.TabIndex = 5;
            // 
            // Main
            // 
            this.Main.Controls.Add(this.colorDisplay);
            this.Main.Controls.Add(this.fade);
            this.Main.Controls.Add(this.colorPicker);
            this.Main.Controls.Add(this.musicSync);
            this.Main.Controls.Add(this.screenSync);
            this.Main.Controls.Add(this.modeLabel);
            this.Main.Controls.Add(this.connect1);
            this.Main.Location = new System.Drawing.Point(4, 22);
            this.Main.Name = "Main";
            this.Main.Padding = new System.Windows.Forms.Padding(3);
            this.Main.Size = new System.Drawing.Size(390, 442);
            this.Main.TabIndex = 1;
            this.Main.Text = "Main";
            this.Main.UseVisualStyleBackColor = true;
            // 
            // colorDisplay
            // 
            this.colorDisplay.Location = new System.Drawing.Point(6, 66);
            this.colorDisplay.Name = "colorDisplay";
            this.colorDisplay.Size = new System.Drawing.Size(378, 33);
            this.colorDisplay.TabIndex = 9;
            // 
            // fade
            // 
            this.fade.Location = new System.Drawing.Point(9, 36);
            this.fade.Name = "fade";
            this.fade.Size = new System.Drawing.Size(87, 23);
            this.fade.TabIndex = 8;
            this.fade.Text = "Fade";
            this.fade.UseVisualStyleBackColor = true;
            this.fade.Click += new System.EventHandler(this.fade_Click);
            // 
            // colorPicker
            // 
            this.colorPicker.Location = new System.Drawing.Point(102, 36);
            this.colorPicker.Name = "colorPicker";
            this.colorPicker.Size = new System.Drawing.Size(87, 23);
            this.colorPicker.TabIndex = 7;
            this.colorPicker.Text = "Color Picker";
            this.colorPicker.UseVisualStyleBackColor = true;
            this.colorPicker.Click += new System.EventHandler(this.colorPicker_Click);
            // 
            // musicSync
            // 
            this.musicSync.Location = new System.Drawing.Point(151, 6);
            this.musicSync.Name = "musicSync";
            this.musicSync.Size = new System.Drawing.Size(87, 23);
            this.musicSync.TabIndex = 6;
            this.musicSync.Text = "Music Sync";
            this.musicSync.UseVisualStyleBackColor = true;
            this.musicSync.Click += new System.EventHandler(this.musicSync_Click);
            // 
            // screenSync
            // 
            this.screenSync.Location = new System.Drawing.Point(58, 7);
            this.screenSync.Name = "screenSync";
            this.screenSync.Size = new System.Drawing.Size(87, 23);
            this.screenSync.TabIndex = 5;
            this.screenSync.Text = "Screen Sync";
            this.screenSync.UseVisualStyleBackColor = true;
            this.screenSync.Click += new System.EventHandler(this.screenSync_Click);
            // 
            // modeLabel
            // 
            this.modeLabel.AutoSize = true;
            this.modeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modeLabel.Location = new System.Drawing.Point(6, 10);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(46, 16);
            this.modeLabel.TabIndex = 4;
            this.modeLabel.Text = "Mode:";
            // 
            // connect1
            // 
            this.connect1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connect1.Location = new System.Drawing.Point(309, 6);
            this.connect1.Name = "connect1";
            this.connect1.Size = new System.Drawing.Size(75, 23);
            this.connect1.TabIndex = 3;
            this.connect1.Text = "Connect";
            this.connect1.UseVisualStyleBackColor = true;
            this.connect1.Click += new System.EventHandler(this.connect_Click);
            // 
            // Settings
            // 
            this.Settings.Controls.Add(this.blue);
            this.Settings.Controls.Add(this.green);
            this.Settings.Controls.Add(this.red);
            this.Settings.Controls.Add(this.treble);
            this.Settings.Controls.Add(this.mid);
            this.Settings.Controls.Add(this.bass);
            this.Settings.Controls.Add(this.blueTreble);
            this.Settings.Controls.Add(this.greenTreble);
            this.Settings.Controls.Add(this.redTreble);
            this.Settings.Controls.Add(this.blueMid);
            this.Settings.Controls.Add(this.greenMid);
            this.Settings.Controls.Add(this.redMid);
            this.Settings.Controls.Add(this.blueBass);
            this.Settings.Controls.Add(this.greenBass);
            this.Settings.Controls.Add(this.redBass);
            this.Settings.Controls.Add(this.generalLabel);
            this.Settings.Controls.Add(this.audioSyncLabel);
            this.Settings.Controls.Add(this.screenSyncLabel);
            this.Settings.Controls.Add(this.useSpeakersChk);
            this.Settings.Controls.Add(this.audioOutput);
            this.Settings.Controls.Add(this.audioInput);
            this.Settings.Controls.Add(this.saveAndClose);
            this.Settings.Controls.Add(this.screenSelection);
            this.Settings.Controls.Add(this.connect);
            this.Settings.Controls.Add(this.comPort);
            this.Settings.Location = new System.Drawing.Point(4, 22);
            this.Settings.Name = "Settings";
            this.Settings.Padding = new System.Windows.Forms.Padding(3);
            this.Settings.Size = new System.Drawing.Size(390, 442);
            this.Settings.TabIndex = 2;
            this.Settings.Text = "Settings";
            this.Settings.UseVisualStyleBackColor = true;
            // 
            // blue
            // 
            this.blue.AutoSize = true;
            this.blue.Location = new System.Drawing.Point(25, 249);
            this.blue.Name = "blue";
            this.blue.Size = new System.Drawing.Size(28, 13);
            this.blue.TabIndex = 25;
            this.blue.Text = "Blue";
            // 
            // green
            // 
            this.green.AutoSize = true;
            this.green.Location = new System.Drawing.Point(17, 228);
            this.green.Name = "green";
            this.green.Size = new System.Drawing.Size(36, 13);
            this.green.TabIndex = 24;
            this.green.Text = "Green";
            // 
            // red
            // 
            this.red.AutoSize = true;
            this.red.Location = new System.Drawing.Point(26, 209);
            this.red.Name = "red";
            this.red.Size = new System.Drawing.Size(27, 13);
            this.red.TabIndex = 23;
            this.red.Text = "Red";
            // 
            // treble
            // 
            this.treble.AutoSize = true;
            this.treble.Location = new System.Drawing.Point(98, 192);
            this.treble.Name = "treble";
            this.treble.Size = new System.Drawing.Size(84, 13);
            this.treble.TabIndex = 22;
            this.treble.Text = "High Percussion";
            // 
            // mid
            // 
            this.mid.AutoSize = true;
            this.mid.Location = new System.Drawing.Point(75, 192);
            this.mid.Name = "mid";
            this.mid.Size = new System.Drawing.Size(24, 13);
            this.mid.TabIndex = 21;
            this.mid.Text = "Mid";
            // 
            // bass
            // 
            this.bass.AutoSize = true;
            this.bass.Location = new System.Drawing.Point(44, 192);
            this.bass.Name = "bass";
            this.bass.Size = new System.Drawing.Size(30, 13);
            this.bass.TabIndex = 20;
            this.bass.Text = "Bass";
            // 
            // blueTreble
            // 
            this.blueTreble.AutoSize = true;
            this.blueTreble.Location = new System.Drawing.Point(101, 248);
            this.blueTreble.Name = "blueTreble";
            this.blueTreble.Size = new System.Drawing.Size(15, 14);
            this.blueTreble.TabIndex = 19;
            this.blueTreble.UseVisualStyleBackColor = true;
            this.blueTreble.CheckedChanged += new System.EventHandler(this.blueTreble_CheckedChanged);
            // 
            // greenTreble
            // 
            this.greenTreble.AutoSize = true;
            this.greenTreble.Location = new System.Drawing.Point(101, 228);
            this.greenTreble.Name = "greenTreble";
            this.greenTreble.Size = new System.Drawing.Size(15, 14);
            this.greenTreble.TabIndex = 18;
            this.greenTreble.UseVisualStyleBackColor = true;
            this.greenTreble.CheckedChanged += new System.EventHandler(this.greenTreble_CheckedChanged);
            // 
            // redTreble
            // 
            this.redTreble.AutoSize = true;
            this.redTreble.Location = new System.Drawing.Point(101, 208);
            this.redTreble.Name = "redTreble";
            this.redTreble.Size = new System.Drawing.Size(15, 14);
            this.redTreble.TabIndex = 17;
            this.redTreble.UseVisualStyleBackColor = true;
            this.redTreble.CheckedChanged += new System.EventHandler(this.redTreble_CheckedChanged);
            // 
            // blueMid
            // 
            this.blueMid.AutoSize = true;
            this.blueMid.Location = new System.Drawing.Point(80, 248);
            this.blueMid.Name = "blueMid";
            this.blueMid.Size = new System.Drawing.Size(15, 14);
            this.blueMid.TabIndex = 16;
            this.blueMid.UseVisualStyleBackColor = true;
            this.blueMid.CheckedChanged += new System.EventHandler(this.blueMid_CheckedChanged);
            // 
            // greenMid
            // 
            this.greenMid.AutoSize = true;
            this.greenMid.Location = new System.Drawing.Point(80, 228);
            this.greenMid.Name = "greenMid";
            this.greenMid.Size = new System.Drawing.Size(15, 14);
            this.greenMid.TabIndex = 15;
            this.greenMid.UseVisualStyleBackColor = true;
            this.greenMid.CheckedChanged += new System.EventHandler(this.greenMid_CheckedChanged);
            // 
            // redMid
            // 
            this.redMid.AutoSize = true;
            this.redMid.Location = new System.Drawing.Point(80, 208);
            this.redMid.Name = "redMid";
            this.redMid.Size = new System.Drawing.Size(15, 14);
            this.redMid.TabIndex = 14;
            this.redMid.UseVisualStyleBackColor = true;
            this.redMid.CheckedChanged += new System.EventHandler(this.redMid_CheckedChanged);
            // 
            // blueBass
            // 
            this.blueBass.AutoSize = true;
            this.blueBass.Location = new System.Drawing.Point(59, 248);
            this.blueBass.Name = "blueBass";
            this.blueBass.Size = new System.Drawing.Size(15, 14);
            this.blueBass.TabIndex = 13;
            this.blueBass.UseVisualStyleBackColor = true;
            this.blueBass.CheckedChanged += new System.EventHandler(this.blueBass_CheckedChanged);
            // 
            // greenBass
            // 
            this.greenBass.AutoSize = true;
            this.greenBass.Location = new System.Drawing.Point(59, 228);
            this.greenBass.Name = "greenBass";
            this.greenBass.Size = new System.Drawing.Size(15, 14);
            this.greenBass.TabIndex = 12;
            this.greenBass.UseVisualStyleBackColor = true;
            this.greenBass.CheckedChanged += new System.EventHandler(this.greenBass_CheckedChanged);
            // 
            // redBass
            // 
            this.redBass.AutoSize = true;
            this.redBass.Location = new System.Drawing.Point(59, 208);
            this.redBass.Name = "redBass";
            this.redBass.Size = new System.Drawing.Size(15, 14);
            this.redBass.TabIndex = 11;
            this.redBass.UseVisualStyleBackColor = true;
            this.redBass.CheckedChanged += new System.EventHandler(this.redBass_CheckedChanged);
            // 
            // generalLabel
            // 
            this.generalLabel.AutoSize = true;
            this.generalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generalLabel.Location = new System.Drawing.Point(6, 10);
            this.generalLabel.Name = "generalLabel";
            this.generalLabel.Size = new System.Drawing.Size(73, 20);
            this.generalLabel.TabIndex = 10;
            this.generalLabel.Text = "General";
            // 
            // audioSyncLabel
            // 
            this.audioSyncLabel.AutoSize = true;
            this.audioSyncLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.audioSyncLabel.Location = new System.Drawing.Point(6, 131);
            this.audioSyncLabel.Name = "audioSyncLabel";
            this.audioSyncLabel.Size = new System.Drawing.Size(99, 20);
            this.audioSyncLabel.TabIndex = 9;
            this.audioSyncLabel.Text = "Audio Sync";
            // 
            // screenSyncLabel
            // 
            this.screenSyncLabel.AutoSize = true;
            this.screenSyncLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.screenSyncLabel.Location = new System.Drawing.Point(6, 72);
            this.screenSyncLabel.Name = "screenSyncLabel";
            this.screenSyncLabel.Size = new System.Drawing.Size(110, 20);
            this.screenSyncLabel.TabIndex = 8;
            this.screenSyncLabel.Text = "Screen Sync";
            // 
            // useSpeakersChk
            // 
            this.useSpeakersChk.AutoSize = true;
            this.useSpeakersChk.Location = new System.Drawing.Point(172, 156);
            this.useSpeakersChk.Name = "useSpeakersChk";
            this.useSpeakersChk.Size = new System.Drawing.Size(166, 17);
            this.useSpeakersChk.TabIndex = 7;
            this.useSpeakersChk.Text = "Use speakers as audio input?";
            this.useSpeakersChk.UseVisualStyleBackColor = true;
            this.useSpeakersChk.CheckedChanged += new System.EventHandler(this.useSpeakersChk_CheckedChanged);
            // 
            // audioOutput
            // 
            this.audioOutput.FormattingEnabled = true;
            this.audioOutput.Location = new System.Drawing.Point(10, 154);
            this.audioOutput.Name = "audioOutput";
            this.audioOutput.Size = new System.Drawing.Size(156, 21);
            this.audioOutput.TabIndex = 6;
            this.audioOutput.Text = "Select audio output device";
            // 
            // audioInput
            // 
            this.audioInput.FormattingEnabled = true;
            this.audioInput.Location = new System.Drawing.Point(15, 154);
            this.audioInput.Name = "audioInput";
            this.audioInput.Size = new System.Drawing.Size(151, 21);
            this.audioInput.TabIndex = 5;
            this.audioInput.Text = "Select audio input device";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "LED Strip Controller";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 492);
            this.Controls.Add(this.Tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Arduino LED Strip Controller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.Tabs.ResumeLayout(false);
            this.Main.ResumeLayout(false);
            this.Main.PerformLayout();
            this.Settings.ResumeLayout(false);
            this.Settings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
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
        private System.Windows.Forms.Button screenSync;
        private System.Windows.Forms.Button fade;
        private System.Windows.Forms.Button colorPicker;
        private System.Windows.Forms.Button musicSync;
        private System.Windows.Forms.CheckBox useSpeakersChk;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label blue;
        private System.Windows.Forms.Label green;
        private System.Windows.Forms.Label red;
        private System.Windows.Forms.Label treble;
        private System.Windows.Forms.Label mid;
        private System.Windows.Forms.Label bass;
        private System.Windows.Forms.CheckBox blueTreble;
        private System.Windows.Forms.CheckBox greenTreble;
        private System.Windows.Forms.CheckBox redTreble;
        private System.Windows.Forms.CheckBox blueMid;
        private System.Windows.Forms.CheckBox greenMid;
        private System.Windows.Forms.CheckBox redMid;
        private System.Windows.Forms.CheckBox blueBass;
        private System.Windows.Forms.CheckBox greenBass;
        private System.Windows.Forms.CheckBox redBass;
        private System.Windows.Forms.Label generalLabel;
        private System.Windows.Forms.Label audioSyncLabel;
        private System.Windows.Forms.Label screenSyncLabel;
        private System.Windows.Forms.Panel colorDisplay;
    }
}

