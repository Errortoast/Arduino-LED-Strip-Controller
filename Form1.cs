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
        #region Global Variables
        #region audio
        private List<MMDevice> renderDevices = new List<MMDevice>();
        private List<MMDevice> inputDevices = new List<MMDevice>();

        private MMDeviceEnumerator deviceEnumerator;

        private const int FFT_SIZE = 1024; // Must be a power of 2
        private const int SampleRate = 44100; // Should match your audio format

        // Assuming you already have an audio capture that provides raw sample bytes.
        private float[] audioSamples = new float[FFT_SIZE];
        private NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[FFT_SIZE];


        int bassFrequency = 50; //in Hz
        int midFrequency = 2000;

        private WasapiLoopbackCapture loopbackCapture;
        private WaveInEvent waveIn;
        
        private bool musicSyncEnabled = false;
        #endregion

        private SerialPort serialPort;
        Color averageColor;
        int downscaleFactor = 6;

        int defaultComPort = 0;
        int defaultMonitor = 0;
        int defaultAudioInput = 0;
        int defaultAudioOutput = 0;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        #region Event Handlers
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
                StartMusicSync(false);
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
        #endregion

        #region utility
        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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
                    ProcessFFT(e.Buffer);
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
                    ProcessFFT(e.Buffer);
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

        private void ProcessFFT(byte[] audioBuffer)
        {
            // 1. Convert raw audio bytes to float samples.
            //    Here we assume each sample is 16-bit PCM: 2 bytes per sample.
            int sampleCount = Math.Min(audioBuffer.Length / 2, FFT_SIZE);
            for (int i = 0; i < FFT_SIZE; i++)
            {
                float sample = 0;
                if (i < sampleCount)
                {
                    // Convert 2 bytes to a 16-bit integer and normalize to [-1.0f, 1.0f].
                    sample = BitConverter.ToInt16(audioBuffer, i * 2) / 32768f;
                }
                // Apply a Hamming window to reduce spectral leakage.
                float windowedSample = sample * HammingWindow(i, FFT_SIZE);

                // Load sample into FFT buffer (imaginary part is 0).
                fftBuffer[i].X = windowedSample;
                fftBuffer[i].Y = 0;
            }

            // 2. Perform FFT. The second parameter is log2(FFT_SIZE).
            FastFourierTransform.FFT(true, (int)Math.Log(FFT_SIZE, 2), fftBuffer);

            // 3. Compute magnitudes for each frequency bin (only half are unique).
            double[] magnitudes = new double[FFT_SIZE / 2];
            for (int i = 0; i < FFT_SIZE / 2; i++)
            {
                double real = fftBuffer[i].X;
                double imag = fftBuffer[i].Y;
                magnitudes[i] = Math.Sqrt(real * real + imag * imag);
            }

            // 4. Map frequency bands to color components:
            // Define three bands: bass (< bassFrequency), mid (between bassFrequency and midFrequency),
            // treble (> midFrequency)
            double bassEnergy = 0, midEnergy = 0, trebleEnergy = 0;
            int bassCount = 0, midCount = 0, trebleCount = 0;

            // Frequency resolution: each bin corresponds to SampleRate/FFT_SIZE Hz.
            double freqResolution = (double)SampleRate / FFT_SIZE;
            for (int i = 1; i < magnitudes.Length; i++)  // start at 1 to skip the DC component
            {
                double freq = i * freqResolution;
                if (freq < bassFrequency)
                {
                    bassEnergy += magnitudes[i];
                    bassCount++;
                }
                else if (freq < midFrequency)
                {
                    midEnergy += magnitudes[i];
                    midCount++;
                }
                else
                {
                    trebleEnergy += magnitudes[i];
                    trebleCount++;
                }
            }
            if (bassCount > 0) bassEnergy /= bassCount;
            if (midCount > 0) midEnergy /= midCount;
            if (trebleCount > 0) trebleEnergy /= trebleCount;

            // 5. Clamp the energy values to the 0–255 range and add exponential brightness.
            int r = (int)Clamp((float)(Math.Pow((bassEnergy * 4000) / 255, 2) * 255), 0, 255);
            int g = (int)Clamp((float)(Math.Pow((midEnergy * 30000) / 255, 2) * 255), 0, 255);
            int b = (int)Clamp((float)(Math.Pow((trebleEnergy * 400000) / 255, 2) * 255), 0, 255);

            // 6. Create a color from the energy values and send it to the Arduino.
            Color outputColor = Color.FromArgb(r, g, b);
            sendColorToArduino(outputColor);
        }

        private float HammingWindow(int i, int windowSize)
        {
            return (float)(0.54 - 0.46 * Math.Cos((2 * Math.PI * i) / (windowSize - 1)));
        }
        #endregion
    }
}