using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO.Ports;
using ScreenCapturerNS;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Dsp;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using Microsoft.Win32;
using System.Threading;

namespace Arduino_LED_Strip_Controller
{
    public partial class Form1 : Form
    {
        #region Global Variables
        #region audio
        private List<MMDevice> renderDevices = new List<MMDevice>();
        private List<MMDevice> inputDevices = new List<MMDevice>();

        private MMDeviceEnumerator deviceEnumerator;

        private const int FFT_SIZE = 2048; // Must be a power of 2
        private const int SampleRate = 44100; // Should match your audio format

        // FFT buffers
        private NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[FFT_SIZE];

        // Smoothing state for bass, mid, treble
        private double smoothedBass = 0;
        private double smoothedMid = 0;
        private double smoothedTreble = 0;
        private const double SMOOTHING_ALPHA = 0.1; // 0 < alpha < 1

        private int bassFrequency = 100; // in Hz
        private int midFrequency = 3000;

        private WasapiLoopbackCapture loopbackCapture;
        private WaveInEvent waveIn;
        #endregion

        private SerialPort serialPort;
        public Color averageColor;
        int downscaleFactor = 6;

        int defaultComPort = 0;
        int defaultMonitor = 0;
        int defaultAudioInput = 0;
        int defaultAudioOutput = 0;
        bool useSpeakers = true;
        bool[] audioColorChannels = { true, false, false, false, true, false, false, false, true };

        private PipeServer pipeServer;

        public string currentMode = "";
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
                if (new FileInfo(Application.StartupPath + "\\Settings.txt").Length !=0 && lines.Length >= 6)
                {
                    const string colonCharacter = ":";

                    string[] comFromSettingsFile = lines[0].Split(colonCharacter[0]);
                    defaultComPort = Convert.ToInt32(comFromSettingsFile[1]);

                    string[] monitorFromSettingsFile = lines[1].Split(colonCharacter[0]);
                    defaultMonitor = Convert.ToInt32(monitorFromSettingsFile[1]);

                    string[] audioInputFromSettingsFile = lines[2].Split(colonCharacter[0]);
                    defaultAudioInput = Convert.ToInt32(audioInputFromSettingsFile[1]);

                    string[] audioOutputFromSettingsFile = lines[3].Split(colonCharacter[0]);
                    defaultAudioOutput = Convert.ToInt32(audioOutputFromSettingsFile[1]);

                    string[] useSpeakersFromSettingsFile = lines[4].Split(colonCharacter[0]);
                    useSpeakers = useSpeakersFromSettingsFile[1] == "True" ? true : false;

                    string[] audioColorChannelsFromSettingsFile = lines[5].Split(colonCharacter[0]);
                    audioColorChannels = new bool[] {audioColorChannelsFromSettingsFile[1]=="True"?true:false, audioColorChannelsFromSettingsFile[2] == "True" ? true : false, audioColorChannelsFromSettingsFile[3] == "True" ? true : false, audioColorChannelsFromSettingsFile[4] == "True" ? true : false, audioColorChannelsFromSettingsFile[5] == "True" ? true : false, audioColorChannelsFromSettingsFile[6] == "True" ? true : false, audioColorChannelsFromSettingsFile[7] == "True" ? true : false, audioColorChannelsFromSettingsFile[8] == "True" ? true : false, audioColorChannelsFromSettingsFile[9] == "True" ? true : false};
                }

            }
            redBass.Checked = audioColorChannels[0];
            redMid.Checked = audioColorChannels[1];
            redTreble.Checked = audioColorChannels[2];
            greenBass.Checked = audioColorChannels[3];
            greenMid.Checked = audioColorChannels[4];
            greenTreble.Checked = audioColorChannels[5];
            blueBass.Checked = audioColorChannels[6];
            blueMid.Checked = audioColorChannels[7];
            blueTreble.Checked = audioColorChannels[8];
            Console.WriteLine("Settings imported");
            #endregion

            #region audio
            deviceEnumerator = new MMDeviceEnumerator();

            Console.WriteLine("Populating audio device list");
            var outputDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var device in outputDevices)
            {
                audioOutput.Items.Add(device.FriendlyName);
                renderDevices.Add(device);
            }
            var captureDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in captureDevices)
            {
                audioInput.Items.Add(device.FriendlyName);
                inputDevices.Add(device);
            }

            if (useSpeakers)
            {
                audioInput.Hide();
                audioOutput.Show();
            }
            else
            {
                audioOutput.Hide();
                audioInput.Show();
            }

            audioInput.SelectedIndex = defaultAudioInput;
            audioOutput.SelectedIndex = defaultAudioOutput;
            Console.WriteLine("Audio setup complete");
            #endregion

            #region combo boxes
            Console.WriteLine("Populating COM port list");
            string[] ports = SerialPort.GetPortNames();
            comPort.Items.AddRange(ports);
            try {
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
            if (serialPort.IsOpen)
            {
                Console.WriteLine("Closing COM port");
                serialPort.Close();
                Console.WriteLine("COM port closed");
            }
            Console.WriteLine("Stopping screen capture");
            ScreenCapturer.StopCapture();
            Console.WriteLine("Screen capture stopped");
            Console.WriteLine("Stopping music sync");
            StopMusicSync();
            Console.WriteLine("Music sync stopped");
        }

        private void connect_Click(object sender, EventArgs e)
        {
            serialPort.Close();
            serialPort.PortName = comPort.SelectedItem.ToString();
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;

            try{
                // Open Serial Port
                serialPort.Open();
            }
            catch
            {
                MessageBox.Show("There was an error when establishing a connection with " + comPort.SelectedItem.ToString() + ". Check if the COM port is being used by another program");
            }
        }

        private void saveAndClose_Click(object sender, EventArgs e)
        {
            audioColorChannels = new bool[] {redBass.Checked, redMid.Checked, redTreble.Checked, greenBass.Checked, greenMid.Checked, greenTreble.Checked, blueBass.Checked, blueMid.Checked, blueTreble.Checked};
            Console.WriteLine("Writing settings to file");
            using (StreamWriter writer = new StreamWriter(Application.StartupPath + "\\Settings.txt"))
            {
                writer.Flush();
                writer.WriteLine("Default COM port:" + comPort.SelectedIndex.ToString());
                writer.WriteLine("Default monitor:" + screenSelection.SelectedIndex.ToString());
                writer.WriteLine("Default audio input:" + audioInput.SelectedIndex.ToString());
                writer.WriteLine("Default audio output:" + audioOutput.SelectedIndex.ToString());
                writer.WriteLine("Use speakers for audio sync:" + useSpeakers.ToString());
                writer.WriteLine($"Audio color channels:{audioColorChannels[0]}:{audioColorChannels[1]}:{audioColorChannels[2]}:{audioColorChannels[3]}:{audioColorChannels[4]}:{audioColorChannels[5]}:{audioColorChannels[6]}:{audioColorChannels[7]}:{audioColorChannels[8]}:");
            }
            Console.WriteLine("Settings written to file");
        }

        private void useSpeakersChk_CheckedChanged(object sender, EventArgs e)
        {
            useSpeakers = useSpeakersChk.Checked;
            if (useSpeakers)
            {
                audioInput.Hide();
                audioOutput.Show();
            }
            else
            {
                audioOutput.Hide();
                audioInput.Show();
            }
        }

        public void screenSync_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            ScreenCapturer.StopCapture();
            StopMusicSync();
            currentMode = "screenSync";
            Console.WriteLine("Starting screen sync");
            try {
                ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
            } catch { }
        }

        public void musicSync_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            StopMusicSync();
            ScreenCapturer.StopCapture();
            Console.WriteLine("Starting audio sync");
            StartMusicSync(useSpeakers);
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
                StopMusicSync();

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
            else if (e.Mode == PowerModes.Resume)
            {
                Console.WriteLine("System resumed...");

                if (!serialPort.IsOpen && comPort.SelectedItem != null)
                {
                    serialPort.PortName = comPort.SelectedItem.ToString();
                    try { serialPort.Open(); } catch { /* handle or ignore */ }
                }
                if (currentMode == "audioSync")
                    StartMusicSync(useSpeakers);
                else if (currentMode == "screenSync")
                    ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
                else if (currentMode == "color")
                {
                    sendColorToArduino(averageColor);
                }
            }
        }

        private void redBass_CheckedChanged(object sender, EventArgs e)
        {
            if (redBass.Checked == true)
            {
                greenBass.Checked = false;
                blueBass.Checked = false;
                redMid.Checked = false;
                redTreble.Checked = false;
            }
        }

        private void greenBass_CheckedChanged(object sender, EventArgs e)
        {
            if (greenBass.Checked == true)
            {
                redBass.Checked = false;
                blueBass.Checked = false;
                greenMid.Checked = false;
                greenTreble.Checked = false;
            }
        }

        private void blueBass_CheckedChanged(object sender, EventArgs e)
        {
            if (blueBass.Checked == true)
            {
                redBass.Checked = false;
                greenBass.Checked = false;
                blueMid.Checked = false;
                blueTreble.Checked = false;
            }
        }

        private void redMid_CheckedChanged(object sender, EventArgs e)
        {
            if (redMid.Checked == true)
            {
                greenMid.Checked = false;
                blueMid.Checked = false;
                redBass.Checked = false;
                redTreble.Checked = false;
            }
        }

        private void greenMid_CheckedChanged(object sender, EventArgs e)
        {
            if (greenMid.Checked == true)
            {
                redMid.Checked = false;
                blueMid.Checked = false;
                greenBass.Checked = false;
                greenTreble.Checked = false;
            }
        }

        private void blueMid_CheckedChanged(object sender, EventArgs e)
        {
            if (blueMid.Checked == true)
            {
                redMid.Checked = false;
                greenMid.Checked = false;
                blueBass.Checked = false;
                blueTreble.Checked = false;
            }
        }

        private void redTreble_CheckedChanged(object sender, EventArgs e)
        {
            if (redTreble.Checked == true)
            {
                greenTreble.Checked = false;
                blueTreble.Checked = false;
                redBass.Checked = false;
                redMid.Checked = false;
            }
        }

        private void greenTreble_CheckedChanged(object sender, EventArgs e)
        {
            if (greenTreble.Checked == true)
            {
                redTreble.Checked = false;
                blueTreble.Checked = false;
                greenBass.Checked = false;
                greenMid.Checked = false;
            }
        }

        private void blueTreble_CheckedChanged(object sender, EventArgs e)
        {
            if (blueTreble.Checked == true)
            {
                redTreble.Checked = false;
                greenTreble.Checked = false;
                blueBass.Checked = false;
                blueMid.Checked = false;
            }
        }
        #endregion

        #region utility
        public float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public void sendColorToArduino(Color inputColor)
        {
            Color color = Color.FromArgb(Convert.ToInt32(Clamp(inputColor.R, 0, 205)), Convert.ToInt32(Clamp(inputColor.G, 0, 205)), Convert.ToInt32(Clamp(inputColor.B, 0, 205)));
            if (serialPort.IsOpen)
            {
                Task.Run(() => serialPort.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}"));
            }
            Debug.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}");
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
        #region bitmap processing
        private Bitmap DownscaleBitmap(Bitmap bitmap, int factor)
        {
            int newWidth = bitmap.Width / factor;
            int newHeight = bitmap.Height / factor;

            var downscaledBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(downscaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }
            return downscaledBitmap;
        }

        private Color GetAverageColor(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            double red = 0, green = 0, blue = 0;
            double totalWeight = 0;

            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = scan0 + (y * bitmapData.Stride);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int b = row[x * 4];
                        int g = row[x * 4 + 1];
                        int r = row[x * 4 + 2];

                        int brightness = (r + g + b) / 3;  // Get pixel brightness

                        double weight = brightness < 20 ? 0.001 : 1.0; // Reduce influence of black pixels

                        red += r * weight;
                        green += g * weight;
                        blue += b * weight;
                        totalWeight += weight;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            if (totalWeight == 0) return Color.Black;  // Avoid division by zero

            return Color.FromArgb(
                (int)(red / totalWeight),
                (int)(green / totalWeight),
                (int)(blue / totalWeight)
            );
        }
        #endregion

        void OnScreenUpdated(object sender, OnScreenUpdatedEventArgs e)
        {
            Bitmap screenshot = DownscaleBitmap(e.Bitmap, downscaleFactor);

            averageColor = GetAverageColor(screenshot);

            sendColorToArduino(averageColor);
        }
        #endregion

        #region audio
        private MMDevice GetSelectedInputDevice()
        {
            int index = audioInput.SelectedIndex;
            return (index >= 0 && index < inputDevices.Count) ? inputDevices[index] : null;
        }

        private MMDevice GetSelectedOutputDevice()
        {
            int index = audioOutput.SelectedIndex;
            return (index >= 0 && index < renderDevices.Count) ? renderDevices[index] : null;
        }

        private void StartMusicSync(bool useOutput)
        {
            StopMusicSync();
            currentMode = "audioSync";
            if (useOutput)
            {
                var device = GetSelectedOutputDevice();
                loopbackCapture = new WasapiLoopbackCapture(device);
                loopbackCapture.DataAvailable += (s, e) => ProcessFFT(e.Buffer);
                loopbackCapture.StartRecording();
            }
            else
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = audioInput.SelectedIndex,
                    WaveFormat = new WaveFormat(44100, 1)
                };
                waveIn.DataAvailable += (s, e) => ProcessFFT(e.Buffer);
                waveIn.StartRecording();
            }
        }

        public void StopMusicSync()
        {
            currentMode = "audioSync";
            loopbackCapture?.StopRecording();
            waveIn?.StopRecording();
            loopbackCapture?.Dispose();
            waveIn?.Dispose();
            loopbackCapture = null;
            waveIn = null;

            // Reset smoothing state
            smoothedBass = smoothedMid = smoothedTreble = 0;
        }

        private void ProcessFFT(byte[] audioBuffer)
        {
            int sampleCount = Math.Min(audioBuffer.Length / 2, FFT_SIZE);
            for (int i = 0; i < FFT_SIZE; i++)
            {
                float sample = 0;
                if (i < sampleCount)
                    sample = BitConverter.ToInt16(audioBuffer, i * 2) / 32768f;

                // Apply Hamming window
                float windowed = sample * HammingWindow(i, FFT_SIZE);
                fftBuffer[i].X = windowed;
                fftBuffer[i].Y = 0;
            }

            FastFourierTransform.FFT(true, (int)Math.Log(FFT_SIZE, 2), fftBuffer);

            double bassEnergy = 0, midEnergy = 0, trebleEnergy = 0;
            int bassCount = 0, midCount = 0, trebleCount = 0;
            double resolution = (double)SampleRate / FFT_SIZE;

            for (int i = 1; i < FFT_SIZE / 2; i++)
            {
                double freq = i * resolution;
                double magnitude = Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);

                if (freq > 40 && freq <= bassFrequency)
                {
                    bassEnergy += magnitude;
                    bassCount++;
                }
                else if (freq > bassFrequency && freq <= midFrequency)
                {
                    midEnergy += magnitude;
                    midCount++;
                }
                else if (freq > midFrequency)
                {
                    trebleEnergy += magnitude;
                    trebleCount++;
                }
            }

            if (bassCount > 0) bassEnergy /= bassCount;
            if (midCount > 0) midEnergy /= midCount;
            if (trebleCount > 0) trebleEnergy /= trebleCount;

            // Exponential smoothing
            smoothedBass = SMOOTHING_ALPHA * smoothedBass + (1 - SMOOTHING_ALPHA) * bassEnergy;
            smoothedMid = SMOOTHING_ALPHA * smoothedMid + (1 - SMOOTHING_ALPHA) * midEnergy;
            smoothedTreble = SMOOTHING_ALPHA * smoothedTreble + (1 - SMOOTHING_ALPHA) * trebleEnergy;

            // Map to color channels
            int b = (int)Clamp((float)(Math.Pow((smoothedBass * 8000) / 255, 3) * 255), 0, 255);
            int m = (int)Clamp((float)(Math.Pow((smoothedMid * 70000) / 255, 2) * 255), 0, 255);
            int t = (int)Clamp((float)(Math.Pow((smoothedTreble * 500000) / 255, 4) * 255), 0, 255);

            sendColorToArduino(Color.FromArgb(redBass.Checked?b:redMid.Checked?m:redTreble.Checked?t:0, greenBass.Checked?b:greenMid.Checked?m:greenTreble.Checked?t:0, blueBass.Checked?b:blueMid.Checked?m:blueTreble.Checked?t:0));
        }

        private float HammingWindow(int i, int n)
        {
            return 0.54f - 0.46f * (float)Math.Cos((2 * Math.PI * i) / (n - 1));
        }
        #endregion
    }
    public class PipeServer
    {
        private const string PipeName = "ArduinoLEDStripStreamDeckPluginComms";
        private Form1 form;

        public PipeServer(Form1 formInstance)
        {
            this.form = formInstance;
        }

        public void StartListening()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine("Creating .NET pipe");
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        try
                        {
                            server.WaitForConnection();

                            byte[] buffer = new byte[256];
                            int bytesRead = server.Read(buffer, 0, buffer.Length);
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine(".NET pipe created");

                            HandleMessage(message);
                        }
                        catch
                        {
                            Console.WriteLine(".NET pipe connection failed");
                        }
                    }
                }
            });
        }

        private void HandleMessage(string message)
        {
            form.BeginInvoke(new Action(() =>
            {
                Debug.WriteLine(message);
                if (message == "audiosync")
                {
                    form.musicSync_Click(null, EventArgs.Empty);
                }
                else if (message == "screensync")
                {
                    form.screenSync_Click(null, EventArgs.Empty);
                }
                else if (message == "fade")
                {
                    
                }
                else if (message.Split("|"[0])[0]=="color")
                {
                    Console.WriteLine("Stopping other processes");
                    form.StopMusicSync();
                    ScreenCapturer.StopCapture();
                    form.currentMode = "color";
                    Console.WriteLine("Sending color");
                    form.averageColor = form.HexToColor(message.Split("|"[0])[1]);
                    form.sendColorToArduino(form.averageColor);
                }
            }));
        }
    }
}