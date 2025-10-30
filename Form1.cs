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
using System.Linq;

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
        private const double SMOOTHING_ALPHA = 0.6; // 0 < alpha < 1

        private WasapiLoopbackCapture loopbackCapture;
        private WaveInEvent waveIn;

        // smoothing queues (Bassinator style)
        private readonly Queue<double> bassQueue = new Queue<double>();
        private readonly Queue<double> midQueue = new Queue<double>();
        private readonly Queue<double> trebleQueue = new Queue<double>();
        private readonly Queue<double> volumeQueue = new Queue<double>();

        private int pitchSmoothnessRate = 6;   // number of frames to average for pitch (tweak 3..12)
        private int volumeSmoothnessRate = 6;  // number of frames to average for volume

        private double[] dataFFT = null;       // temp fft magnitudes per-frame

        private WaveFormat currentWaveFormat;
        #endregion

        private SerialPort serialPort;
        public Color averageColor;
        int downscaleFactor = 24;
        const int FrameIntervalMs = 16;     // ~60 FPS
        DateTime lastScreenSyncTime = DateTime.MinValue;

        int defaultComPort = 0;
        int defaultMonitor = 0;
        int defaultAudioInput = 0;
        int defaultAudioOutput = 0;
        bool useSpeakers = true;
        bool[] audioColorChannels = { true, false, false, false, true, false, false, false, true };

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
            useSpeakersChk.Checked = useSpeakers;
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
            ScreenCapturer.StopCapture();
            StopMusicSync();
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
            stopFade();
            currentMode = "screenSync";
            Console.WriteLine("Starting screen sync");
            ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
        }

        public void musicSync_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            StopMusicSync();
            stopFade();
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

        public void fade_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            stopFade();
            StopMusicSync();
            ScreenCapturer.StopCapture();
            currentMode = "fade";
            Console.WriteLine("Starting color fade");
            startFade();
        }

        private void colorPicker_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping other processes");
            stopFade();
            StopMusicSync();
            ScreenCapturer.StopCapture();
            currentMode = "color";
            Console.WriteLine("Opening color dialog");
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if(colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("Sending color to arduino");
                    sendColorToArduino(colorDialog.Color);
                }
            }
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
                currentWaveFormat = loopbackCapture.WaveFormat;
                loopbackCapture.DataAvailable += (s, e) => ProcessFFT(e.Buffer, e.BytesRecorded, useOutput);
                loopbackCapture.StartRecording();
            }
            else
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = audioInput.SelectedIndex,
                    WaveFormat = new WaveFormat(44100, 1)
                };
                currentWaveFormat = waveIn.WaveFormat;
                waveIn.DataAvailable += (s, e) => ProcessFFT(e.Buffer, e.BytesRecorded, useOutput);
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
        }

        private void ProcessFFT(byte[] audioBuffer, int bytesRecorded, bool useOutput)
        {
            if (currentWaveFormat == null || bytesRecorded <= 0) return;

            // --- Safe format detection ---
            WaveFormatEncoding encoding = currentWaveFormat.Encoding;
            int bits = currentWaveFormat.BitsPerSample;
            int channels = currentWaveFormat.Channels;
            int bytesPerSample = bits / 8;

            // Defensive: handle loopback (float) and input (PCM)
            bool isFloat = encoding == WaveFormatEncoding.IeeeFloat || bits == 32;
            bool isPcm = encoding == WaveFormatEncoding.Pcm || bits == 16;

            // Fallback if unknown
            if (!isFloat && !isPcm)
            {
                Console.WriteLine($"Unsupported format: {encoding}, {bits} bits");
                return;
            }

            // --- Prepare FFT size ---
            int samples = bytesRecorded / (bytesPerSample * channels);
            int fftPoints = 1;
            while (fftPoints * 2 <= samples && fftPoints * 2 <= FFT_SIZE)
                fftPoints *= 2;
            if (fftPoints < 256) fftPoints = 256;

            var fftFull = new NAudio.Dsp.Complex[fftPoints];

            // --- Convert to mono + apply Hamming window ---
            for (int i = 0; i < fftPoints; i++)
            {
                float sample = 0f;
                if (i < samples)
                {
                    int baseIdx = i * bytesPerSample * channels;
                    float sum1 = 0f;

                    for (int ch = 0; ch < channels; ch++)
                    {
                        int offset = baseIdx + ch * bytesPerSample;

                        if (isFloat)
                        {
                            // Guard against out-of-range reads
                            if (offset + 4 <= audioBuffer.Length)
                                sum1 += BitConverter.ToSingle(audioBuffer, offset);
                        }
                        else
                        {
                            if (offset + 2 <= audioBuffer.Length)
                                sum1 += BitConverter.ToInt16(audioBuffer, offset) / 32768f;
                        }
                    }

                    sample = sum1 / channels;
                }

                float window = (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (fftPoints - 1)));
                fftFull[i].X = sample * window;
                fftFull[i].Y = 0f;
            }

            // --- FFT ---
            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftPoints, 2), fftFull);

            // --- Magnitudes: copy to dataFFT (half) ---
            int half = fftPoints / 2;
            if (dataFFT == null || dataFFT.Length != half) dataFFT = new double[half];

            for (int i = 0; i < half; i++)
            {
                // mirror-sum to get symmetric energy
                double left = Math.Abs(fftFull[i].X) + Math.Abs(fftFull[i].Y);
                double right = Math.Abs(fftFull[fftPoints - i - 1].X) + Math.Abs(fftFull[fftPoints - i - 1].Y);
                dataFFT[i] = left + right;
            }

            // --- Frequency-to-index resolution ---
            double resolution = (double)currentWaveFormat.SampleRate / fftPoints; // Hz per bin

            // define ranges similar to earlier suggestion but using index ranges
            int bassStart = Math.Max(0, (int)Math.Floor(20.0 / resolution));
            int bassLen = Math.Max(1, (int)Math.Floor(250.0 / resolution));  // number of bins to sum
            int midStart = Math.Max(0, (int)Math.Floor(250.0 / resolution));
            int midLen = Math.Max(1, (int)Math.Floor(4000.0 / resolution) - (int)Math.Floor(250.0 / resolution));
            int highStart = Math.Max(0, (int)Math.Floor(4000.0 / resolution));
            int highLen = Math.Max(1, Math.Min(half - 1, (int)Math.Floor(16000.0 / resolution)) - (int)Math.Floor(4000.0 / resolution) + 1);

            // --- Compute simple average energies for each band ---
            double bassEnergy = 0, midEnergy = 0, highEnergy = 0;
            for (int i = bassStart; i < Math.Min(half, bassStart + bassLen); i++) bassEnergy += dataFFT[i];
            for (int i = midStart; i < Math.Min(half, midStart + midLen); i++) midEnergy += dataFFT[i];
            for (int i = highStart; i < Math.Min(half, highStart + highLen); i++) highEnergy += dataFFT[i];

            bassEnergy = bassEnergy / Math.Max(1, Math.Min(bassLen, half - bassStart));
            midEnergy = midEnergy / Math.Max(1, Math.Min(midLen, half - midStart));
            highEnergy = highEnergy / Math.Max(1, Math.Min(highLen, half - highStart));

            // --- Update smoothing queues ---
            void EnqueueAndTrim(Queue<double> q, double v, int limit)
            {
                q.Enqueue(v);
                while (q.Count > limit) q.Dequeue();
            }
            EnqueueAndTrim(bassQueue, bassEnergy, Math.Max(1, pitchSmoothnessRate));
            EnqueueAndTrim(midQueue, midEnergy, Math.Max(1, pitchSmoothnessRate));
            EnqueueAndTrim(trebleQueue, highEnergy, Math.Max(1, pitchSmoothnessRate));

            // --- Volume smoothing using device peak if available ---
            double devicePeak = 0.0;
            try { devicePeak = GetSelectedOutputDevice()?.AudioMeterInformation?.MasterPeakValue ?? 0.0; } catch { devicePeak = 0.0; }
            EnqueueAndTrim(volumeQueue, devicePeak, Math.Max(1, volumeSmoothnessRate));

            // compute averages
            double Avg(Queue<double> q)
            {
                if (q.Count == 0) return 0.0;
                double s = 0;
                foreach (var x in q) s += x;
                return s / q.Count;
            }

            double smBass = Avg(bassQueue);
            double smMid = Avg(midQueue);
            double smHigh = Avg(trebleQueue);
            double smVol = Avg(volumeQueue);

            // --- Adaptive normalization: scale by recent max of each band to avoid tiny divisors ---
            // Use max of queue as local peak estimate
            double peakBass = bassQueue.Count > 0 ? bassQueue.Max() : 1e-6;
            double peakMid = midQueue.Count > 0 ? midQueue.Max() : 1e-6;
            double peakHigh = trebleQueue.Count > 0 ? trebleQueue.Max() : 1e-6;

            // avoid zero
            peakBass = Math.Max(peakBass, 1e-8);
            peakMid = Math.Max(peakMid, 1e-8);
            peakHigh = Math.Max(peakHigh, 1e-8);

            double bassNorm = smBass / peakBass;
            double midNorm = smMid / peakMid;
            double highNorm = smHigh / peakHigh;

            // optional global normalization by summed energy to favor hue over brightness
            double sum = bassNorm + midNorm + highNorm + 1e-9;
            bassNorm = bassNorm / sum;
            midNorm = midNorm / sum;
            highNorm = highNorm / sum;

            // small floor so LEDs never go totally off unless input is silent
            const double FLOOR = 0.03;
            bassNorm = Math.Max(bassNorm, FLOOR * (smBass > 0 ? 1.0 : 0.0));
            midNorm = Math.Max(midNorm, FLOOR * (smMid > 0 ? 1.0 : 0.0));
            highNorm = Math.Max(highNorm, FLOOR * (smHigh > 0 ? 1.0 : 0.0));

            // --- Map to 0..255 with gamma and gain tuned for visibility ---
            double gainB = 1.6;
            double gainM = 1.2;
            double gainH = 1.0;
            double gamma = 1.8;

            int outB = (int)Clamp(Math.Pow(bassNorm * gainB * (useOutput ? 1.4 : 1), gamma) * 255.0, 0, 255);
            int outM = (int)Clamp(Math.Pow(midNorm * gainM * (useOutput ? 1.7 : 1), gamma) * 255.0, 0, 255);
            int outH = (int)Clamp(Math.Pow(highNorm * gainH * (useOutput?2.3:1), gamma) * 255.0, 0, 255);

            // --- Map bands to RGB respecting user checkboxes ---
            int R = (redBass.Checked ? outB : 0) | (redMid.Checked ? outM : 0) | (redTreble.Checked ? outH : 0);
            int G = (greenBass.Checked ? outB : 0) | (greenMid.Checked ? outM : 0) | (greenTreble.Checked ? outH : 0);
            int B = (blueBass.Checked ? outB : 0) | (blueMid.Checked ? outM : 0) | (blueTreble.Checked ? outH : 0);

            // final send
            sendColorToArduino(Color.FromArgb(R, G, B));
        }


        private float HammingWindow(int i, int n)
        {
            return 0.54f - 0.46f * (float)Math.Cos((2 * Math.PI * i) / (n - 1));
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
                    form.fade_Click(null, EventArgs.Empty);
                }
                else if (message.Split("|"[0])[0]=="color")
                {
                    Console.WriteLine("Stopping other processes");
                    form.StopMusicSync();
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