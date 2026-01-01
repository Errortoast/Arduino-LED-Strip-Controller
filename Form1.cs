#region Import Packages
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO.Ports;
using ScreenCapturerNS;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using Microsoft.Win32;
using System.Threading;
#endregion

namespace Arduino_LED_Strip_Controller
{
    public partial class Form1 : Form
    {
        #region Global Variables
        private SerialPort serialPort;
        public Color averageColor;
        int downscaleFactor = 24;
        const int FrameIntervalMs = 16; // ~60 FPS
        DateTime lastScreenSyncTime = DateTime.MinValue;

        int defaultComPort = 0;
        int defaultMonitor = 0;

        private PipeServer pipeServer;

        public string currentMode = "";

        List<Color> fadeColors = new List<Color>
        {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.YellowGreen,
            Color.Green,
            Color.Turquoise,
            Color.Aqua,
            Color.Blue,
            Color.BlueViolet,
            Color.Violet
        };
        private System.Windows.Forms.Timer fadeTimer;
        private int currentColorIndex = 0;
        private int nextColorIndex = 1;
        private float fadeProgress = 0f;
        private float fadeSpeed = 0.01f;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        #region Event Handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Opening serial port");
            serialPort = new SerialPort();
            Console.WriteLine("Serial port opened");

            ScreenCapturer.OnScreenUpdated += OnScreenUpdated;
            ScreenCapturer.OnCaptureStop += (s, a) =>
            {
                currentMode = "";
            };

            Console.WriteLine("Creating .NET pipe");
            pipeServer = new PipeServer(this);
            pipeServer.StartListening();

            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, a) =>
            {
                if (currentMode == "screenSync")
                {
                    Console.WriteLine("Display settings changed – restarting capture");
                    ScreenCapturer.StopCapture();
                    ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
                }
            };

            #region file
            Console.WriteLine("Importing settings from file");
            if (!File.Exists(Application.StartupPath + "\\Settings.txt"))
            {
                var settingsFile = File.Create(Application.StartupPath + "\\Settings.txt");
                settingsFile.Close();
            }
            else
            {
                string[] lines = File.ReadAllLines(Application.StartupPath + "\\Settings.txt");
                if (new FileInfo(Application.StartupPath + "\\Settings.txt").Length != 0 && lines.Length >= 2)
                {
                    const string colonCharacter = ":";

                    string[] comFromSettingsFile = lines[0].Split(colonCharacter[0]);
                    defaultComPort = Convert.ToInt32(comFromSettingsFile[1]);

                    string[] monitorFromSettingsFile = lines[1].Split(colonCharacter[0]);
                    defaultMonitor = Convert.ToInt32(monitorFromSettingsFile[1]);
                    }

            }
            Console.WriteLine("Settings imported");
            #endregion

            #region combo boxes
            Console.WriteLine("Populating COM port list");
            string[] ports = SerialPort.GetPortNames();
            comPort.Items.AddRange(ports);
            try
            {
                comPort.SelectedIndex = defaultComPort;
            }
            catch
            {
                MessageBox.Show("Error finding COM port. Check if your device is plugged in");
            }
            Console.WriteLine("COM port list populated");

            Console.WriteLine("Populating screen list");
            foreach (var screen in Screen.AllScreens)
            {
                screenSelection.Items.Add(screen);
            }
            screenSelection.SelectedIndex = defaultMonitor;
            Console.WriteLine("Screen list populated");
            #endregion
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ScreenCapturer.StopCapture();
            stopFade();
            pipeServer?.Stop();
            sendColorToArduino(Color.FromArgb(0, 0, 0));
            if (serialPort.IsOpen) serialPort.Close();
            notifyIcon1.Dispose();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            serialPort.Close();
            serialPort.PortName = comPort.SelectedItem.ToString();
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;

            try
            {
                serialPort.Open();
            }
            catch
            {
                MessageBox.Show("There was an error when establishing a connection with " + comPort.SelectedItem.ToString() + ". Check if the COM port is being used by another program");
            }
        }

        private void saveAndClose_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Writing settings to file");
            using (StreamWriter writer = new StreamWriter(Application.StartupPath + "\\Settings.txt"))
            {
                writer.Flush();
                writer.WriteLine("Default COM port:" + comPort.SelectedIndex.ToString());
                writer.WriteLine("Default monitor:" + screenSelection.SelectedIndex.ToString());
            }
            Console.WriteLine("Settings written to file");
        }

        public void screenSync_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            ScreenCapturer.StopCapture();
            stopFade();
            currentMode = "screenSync";
            Console.WriteLine("Starting screen sync");
            ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Console.WriteLine("System suspending…");
                stopFade();
                ScreenCapturer.StopCapture();

                var sw = Stopwatch.StartNew();
                while (ScreenCapturer.IsActive && sw.ElapsedMilliseconds < 2000)
                {
                    Thread.Sleep(10);
                }
                if (ScreenCapturer.IsActive)
                    Debug.WriteLine("⚠️ Warning: capture thread didn't exit in time.");

                serialPort?.Close();
            }
        }

        public void fade_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            stopFade();
            ScreenCapturer.StopCapture();
            currentMode = "fade";
            Console.WriteLine("Starting color fade");
            startFade();
        }

        private void colorPicker_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            stopFade();
            ScreenCapturer.StopCapture();
            currentMode = "color";
            Console.WriteLine("Opening color dialog");
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("Sending color to arduino");
                    sendColorToArduino(colorDialog.Color);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F13))
            {
                Console.WriteLine("Stopping other processes");
                stopFade();
                ScreenCapturer.StopCapture();
                currentMode = "color";
                sendColorToArduino(Color.FromArgb(0, 0, 0));
            }
            else if (keyData == (Keys.Control | Keys.F14))
            {
                Console.WriteLine("Stopping other processes");
                stopFade();
                ScreenCapturer.StopCapture();
                currentMode = "fade";
                Console.WriteLine("Starting color fade");
                startFade();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region utility
        Color LerpColor(Color from, Color to, float t)
        {
            int r = (int)(from.R + (to.R - from.R) * t);
            int g = (int)(from.G + (to.G - from.G) * t);
            int b = (int)(from.B + (to.B - from.B) * t);
            return Color.FromArgb(r, g, b);
        }

        public dynamic Clamp(dynamic value, dynamic min, dynamic max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public void sendColorToArduino(Color inputColor)
        {
            Color color = Color.FromArgb(Convert.ToInt32(inputColor.R), Convert.ToInt32(inputColor.G), Convert.ToInt32(inputColor.B));
            if (serialPort.IsOpen)
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        serialPort.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}");
                    }
                    catch (Exception)
                    {

                    }
                });
            }
            Debug.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}");
            colorDisplay.BackColor = color;
        }

        public Color HexToColor(string hex)
        {
            // Remove the '#' if present
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            // Parse the R, G, B values
            int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(r, g, b);
        }
        #endregion

        #region screen
        private Color GetDownscaledAverageColor(Bitmap source, int factor)
        {
            var data = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            long rSum = 0, gSum = 0, bSum = 0;
            long wSum = 0;

            unsafe
            {
                byte* scan0 = (byte*)data.Scan0;
                int stride = data.Stride;

                for (int y = 0; y < source.Height; y += factor)
                {
                    byte* row = scan0 + y * stride;
                    for (int x = 0; x < source.Width; x += factor)
                    {
                        byte* px = row + x * 4;
                        int b = px[0], g = px[1], r = px[2];
                        int brightness = (r + g + b) / 3;

                        int w = (brightness < 20) ? 1 : 1000;

                        rSum += r * w;
                        gSum += g * w;
                        bSum += b * w;
                        wSum += w;
                    }
                }
            }

            source.UnlockBits(data);

            if (wSum <= 0)
                return Color.Black;

            return Color.FromArgb(
                (int)(rSum / wSum),
                (int)(gSum / wSum),
                (int)(bSum / wSum));
        }

        void OnScreenUpdated(object sender, OnScreenUpdatedEventArgs e)
        {
            averageColor = GetDownscaledAverageColor(e.Bitmap, downscaleFactor);
            sendColorToArduino(averageColor);
            e.Bitmap.Dispose();
        }
        #endregion

        #region fade
        public void startFade()
        {
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 16;
            fadeTimer.Tick += FadeTimer_Tick;
            fadeTimer.Start();
        }

        public void stopFade()
        {
            if (fadeTimer != null)
            {
                fadeTimer.Stop();
            }
        }

        void FadeTimer_Tick(object sender, EventArgs e)
        {
            fadeProgress += fadeSpeed;

            if (fadeProgress >= 1.0f)
            {
                fadeProgress = 0.0f;
                currentColorIndex = nextColorIndex;
                nextColorIndex = (nextColorIndex + 1) % fadeColors.Count;
            }

            // Blend between current and next
            Color currentFadeColor = LerpColor(
                fadeColors[currentColorIndex],
                fadeColors[nextColorIndex],
                fadeProgress
            );
            sendColorToArduino(currentFadeColor);
        }
        #endregion
    }
    public class PipeServer
    {
        private const string PipeName = "ArduinoLEDStripStreamDeckPluginComms";
        private Form1 form;
        private CancellationTokenSource cts;

        public PipeServer(Form1 formInstance)
        {
            this.form = formInstance;
        }

        public void StartListening()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        try
                        {
                            server.WaitForConnection();

                            byte[] buffer = new byte[256];
                            int bytesRead = server.Read(buffer, 0, buffer.Length);
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            HandleMessage(message);
                        }
                        catch
                        {
                        }
                    }
                }
            }, token);
        }

        public void Stop()
        {
            cts?.Cancel();
        }

        private void HandleMessage(string message)
        {
            form.BeginInvoke(new Action(() =>
            {
                Debug.WriteLine(message);
                if (message == "screensync")
                {
                    form.screenSync_Click(null, EventArgs.Empty);
                }
                else if (message == "fade")
                {
                    form.fade_Click(null, EventArgs.Empty);
                }
                else if (message.Split("|"[0])[0] == "color")
                {
                    Console.WriteLine("Stopping other processes");
                    ScreenCapturer.StopCapture();
                    form.stopFade();
                    form.currentMode = "color";
                    Console.WriteLine("Sending color");
                    form.averageColor = form.HexToColor(message.Split("|"[0])[1]);
                    form.sendColorToArduino(form.averageColor);
                }
            }));
        }
    }
}