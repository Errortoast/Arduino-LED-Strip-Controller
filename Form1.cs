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

namespace Arduino_LED_Strip_Controller
{
    public partial class Form1 : Form
    {
        #region audio
        private List<MMDevice> renderDevices = new List<MMDevice>();
        private List<MMDevice> inputDevices = new List<MMDevice>();

        private MMDeviceEnumerator deviceEnumerator;

        float bassSmoothing = 0.5f;
        float midSmoothing = 0.5f;
        float trebleSmoothing = 0.5f;

        float bassBoost = 1.2f;

        int bassFrequency = 250; //in Hz
        int midFrequency = 1500;
        int trebleFrequency = 16000;

        private WasapiLoopbackCapture loopbackCapture;
        private WaveInEvent waveIn;
        private float[] fftBuffer = new float[256];
        private Complex[] fftComplex = new Complex[256];
        private int fftSize = 256;
        private bool musicSyncEnabled = false;
        private object fftLock = new object();

        private float bassEMA = 0, midEMA = 0, trebleEMA = 0;
        private float bassGain = 0.1f, midGain = 0.2f, trebleGain = 0.4f;
        #endregion

        private SerialPort serialPort;
        Color averageColor;
        int downscaleFactor = 6;

        int defaultComPort = 0;
        int defaultMonitor = 0;
        int defaultAudioInput = 0;
        int defaultAudioOutput = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort = new SerialPort();

            #region file
            //Read and apply settings
            if (!File.Exists(Application.StartupPath + "\\Settings.txt"))
            {
                var settingsFile = File.Create(Application.StartupPath + "\\Settings.txt");
                settingsFile.Close();
            }
            else
            {
                string[] lines = File.ReadAllLines(Application.StartupPath + "\\Settings.txt");
                if (new FileInfo(Application.StartupPath + "\\Settings.txt").Length !=0 && lines.Length >= 4)
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
                }

            }
            #endregion

            #region audio
            deviceEnumerator = new MMDeviceEnumerator();

            // Get output (render) devices
            var outputDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var device in outputDevices)
            {
                audioOutput.Items.Add(device.FriendlyName);
                renderDevices.Add(device);
            }

            // Get input (capture) devices
            var captureDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in captureDevices)
            {
                audioInput.Items.Add(device.FriendlyName);
                inputDevices.Add(device);
            }
            #endregion

            #region combo boxes
            string[] ports = SerialPort.GetPortNames();
            comPort.Items.AddRange(ports);
            try {
                comPort.SelectedIndex = defaultComPort;
            }
            catch
            {
                MessageBox.Show("Error finding COM port. Check if your device is plugged in");
            }

            // Populate screens
            foreach (var screen in Screen.AllScreens)
            {
                screenSelection.Items.Add(screen);
            }
            screenSelection.SelectedIndex = defaultMonitor;

            audioInput.SelectedIndex = defaultAudioInput;
            audioOutput.SelectedIndex = defaultAudioOutput;
            #endregion
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            ScreenCapturer.StopCapture();
            StopMusicSync();
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

        private void modeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mode = modeSelect.SelectedItem.ToString();

            if (mode == "Screen Sync")
            {
                StopMusicSync();
                ScreenCapturer.OnScreenUpdated += OnScreenUpdated;
                ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
            }
            else
            {
                ScreenCapturer.StopCapture();
                ScreenCapturer.OnScreenUpdated -= OnScreenUpdated;
                this.BackColor = DefaultBackColor;
            }

            if (mode == "Music Sync")
            {
                bool useOutput = false; // toggle with checkbox later
                StartMusicSync(useOutput);
            }
            else
            {
                StopMusicSync();
            }
        }

        private void saveAndClose_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(Application.StartupPath + "\\Settings.txt"))
            {
                writer.Flush();
                writer.WriteLine("Default COM port:" + comPort.SelectedIndex.ToString());
                writer.WriteLine("Default Monitor:" + screenSelection.SelectedIndex.ToString());
                writer.WriteLine("Default Audio Input:" + audioInput.SelectedIndex.ToString());
                writer.WriteLine("Default Audio Output:" + audioOutput.SelectedIndex.ToString());
            }
        }

        private void sendColorToArduino(Color color)
        {
            if (serialPort.IsOpen)
            {
                Task.Run(() => serialPort.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}"));
            }
            Debug.WriteLine($"color,{color.R:D3},{color.G:D3},{color.B:D3}");
        }

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
            this.BeginInvoke((Action)(() => this.BackColor = averageColor));

            sendColorToArduino(averageColor);
        }
        #endregion

        #region audio
        // Returns the selected input device from the renderDevices list.
        private MMDevice GetSelectedInputDevice()
        {
            int index = audioInput.SelectedIndex;
            return (index >= 0 && index < inputDevices.Count) ? inputDevices[index] : null;
        }

        // Returns the selected output device from the renderDevices list.
        private MMDevice GetSelectedOutputDevice()
        {
            int index = audioOutput.SelectedIndex;
            return (index >= 0 && index < renderDevices.Count) ? renderDevices[index] : null;
        }

        private void StartMusicSync(bool useOutput)
        {
            musicSyncEnabled = true;
            StopMusicSync(); // Stop any previous capture

            if (useOutput)
            {
                var device = GetSelectedOutputDevice();
                loopbackCapture = new WasapiLoopbackCapture(device);
                loopbackCapture.DataAvailable += (s, e) =>
                {
                    ProcessFFT(e.Buffer, e.BytesRecorded);
                };
                loopbackCapture.StartRecording();
            }
            else
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = audioInput.SelectedIndex,
                    WaveFormat = new WaveFormat(44100, 1)
                };
                waveIn.DataAvailable += (s, e) =>
                {
                    ProcessFFT(e.Buffer, e.BytesRecorded);
                };
                waveIn.StartRecording();
            }
        }

        private void StopMusicSync()
        {
            loopbackCapture?.StopRecording();
            waveIn?.StopRecording();
            loopbackCapture?.Dispose();
            waveIn?.Dispose();
            loopbackCapture = null;
            waveIn = null;
        }

        private void ProcessFFT(byte[] buffer, int bytesRecorded)
        {
            int samples = bytesRecorded / 2;
            for (int i = 0; i < fftSize && i < samples; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                fftComplex[i] = new Complex { X = (float)(sample / 32768.0), Y = 0 };
            }

            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftComplex);

            float bass = 0, mid = 0, treble = 0;
            int nyquist = 44100 / 2;

            for (int i = 0; i < fftSize / 2; i++)
            {
                float freq = i * nyquist / (fftSize / 2);
                float magnitude = (float)Math.Sqrt(fftComplex[i].X * fftComplex[i].X + fftComplex[i].Y * fftComplex[i].Y);

                if (freq >= 20 && freq < bassFrequency)
                    bass += magnitude;
                else if (freq < midFrequency)
                    mid += magnitude;
                else if (freq < trebleFrequency)
                    treble += magnitude;
            }

            // === Smoothing (EMA) ===
            bassEMA = bassEMA * (1 - bassSmoothing) + bass * bassSmoothing;
            midEMA = midEMA * (1 - midSmoothing) + mid * midSmoothing;
            trebleEMA = trebleEMA * (1 - trebleSmoothing) + treble * trebleSmoothing;

            // === Auto-gain ===
            bassGain = Math.Max(bassGain * 0.9f, bassEMA);   //lower = faster decay
            midGain = Math.Max(midGain * 0.9f, midEMA);
            trebleGain = Math.Max(trebleGain * 0.9f, trebleEMA);

            // Spike multiplier for bass transient boost
            float boostedBass = bassEMA * bassBoost;

            bool isSilent = (bass + mid + treble) < 0.001f;

            if (isSilent)
            {
                bassEMA = midEMA = trebleEMA = 0;
                bassGain = Math.Max(bassGain * 0.95f, 0.1f);
                midGain = Math.Max(midGain * 0.95f, 0.1f);
                trebleGain = Math.Max(trebleGain * 0.95f, 0.1f);
            }

            // Clamp gain
            bassGain = Math.Max(bassGain, 0.05f);
            midGain = Math.Max(midGain, 0.05f);
            trebleGain = Math.Max(trebleGain, 0.05f);

            // Normalize
            float normBass = Clamp(boostedBass / bassGain, 0, 1);
            float normMid = Clamp(midEMA / midGain, 0, 1);
            float normTreble = Clamp(trebleEMA / trebleGain, 0, 1);

            // Normalize and clamp
            int r = Math.Min(255, (int)(Math.Pow(boostedBass / bassGain, 2.5) * 255*0.6));
            int g = Math.Min(255, (int)(midEMA / midGain * 255*0.6));
            int b = Math.Min(255, (int)(trebleEMA / trebleGain * 255*0.6));

            Color musicColor = Color.FromArgb(0, g, 0);
            this.BeginInvoke((Action)(() => this.BackColor = musicColor));
            sendColorToArduino(musicColor);
        }
        #endregion
        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}