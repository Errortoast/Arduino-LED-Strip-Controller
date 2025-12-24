
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
            this.Tabs = new System.Windows.Forms.TabControl();
            this.Main = new System.Windows.Forms.TabPage();
            this.colorDisplay = new System.Windows.Forms.Panel();
            this.fade = new System.Windows.Forms.Button();
            this.colorPicker = new System.Windows.Forms.Button();
            this.screenSync = new System.Windows.Forms.Button();
            this.modeLabel = new System.Windows.Forms.Label();
            this.connect1 = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.comPort = new System.Windows.Forms.ComboBox();
            this.connect = new System.Windows.Forms.Button();
            this.screenSelection = new System.Windows.Forms.ComboBox();
            this.saveAndClose = new System.Windows.Forms.Button();
            this.screenSyncLabel = new System.Windows.Forms.Label();
            this.generalLabel = new System.Windows.Forms.Label();
            this.Settings = new System.Windows.Forms.TabPage();
            this.Tabs.SuspendLayout();
            this.Main.SuspendLayout();
            this.Settings.SuspendLayout();
            this.SuspendLayout();
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
            this.Main.Controls.Add(this.screenSync);
            this.Main.Controls.Add(this.modeLabel);
            this.Main.Controls.Add(this.connect1);
            this.Main.Location = new System.Drawing.Point(4, 22);
            this.Main.Name = "Main";
            this.Main.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
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
            this.fade.Location = new System.Drawing.Point(151, 7);
            this.fade.Name = "fade";
            this.fade.Size = new System.Drawing.Size(87, 23);
            this.fade.TabIndex = 8;
            this.fade.Text = "Fade";
            this.fade.UseVisualStyleBackColor = true;
            this.fade.Click += new System.EventHandler(this.fade_Click);
            // 
            // colorPicker
            // 
            this.colorPicker.Location = new System.Drawing.Point(6, 29);
            this.colorPicker.Name = "colorPicker";
            this.colorPicker.Size = new System.Drawing.Size(87, 23);
            this.colorPicker.TabIndex = 7;
            this.colorPicker.Text = "Color Picker";
            this.colorPicker.UseVisualStyleBackColor = true;
            this.colorPicker.Click += new System.EventHandler(this.colorPicker_Click);
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
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "LED Strip Controller";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
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
            // Settings
            // 
            this.Settings.Controls.Add(this.generalLabel);
            this.Settings.Controls.Add(this.screenSyncLabel);
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
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage Main;
        private System.Windows.Forms.Button connect1;
        private System.Windows.Forms.Label modeLabel;
        private System.Windows.Forms.Button screenSync;
        private System.Windows.Forms.Button fade;
        private System.Windows.Forms.Button colorPicker;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Panel colorDisplay;
        private System.Windows.Forms.TabPage Settings;
        private System.Windows.Forms.Label generalLabel;
        private System.Windows.Forms.Label screenSyncLabel;
        private System.Windows.Forms.Button saveAndClose;
        private System.Windows.Forms.ComboBox screenSelection;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.ComboBox comPort;
    }
}

