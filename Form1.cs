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
using System.Linq;

namespace Arduino_LED_Strip_Controller
{
    public partial class Form1 : Form
    {
        #region audio
        private List<MMDevice> renderDevices = new List<MMDevice>();
        private List<MMDevice> inputDevices = new List<MMDevice>();

        const int bassThreshold = 50;

        float smoothedBrightness = 0;
        float smoothingFactor = 0.6f; // Higher = smoother

        // AUDIO VARIABLES FOR MUSIC SYNC
        private MMDeviceEnumerator deviceEnumerator;
        private WasapiLoopbackCapture audioCapture; // Using loopback capture on the selected audio device
        private BufferedWaveProvider bufferedWaveProvider;

        // FFT and audio processing parameters
        private const int fftSize = 2048;   // Must be a power of 2
        private Complex[] fftBuffer;
        private float[] precomputedHamming;
        private float[] sampleBuffer;
        private int bufferOffset = 0;
        #endregion

        private SerialPort serialPort;
        Color averageColor;
        int downscaleFactor = 6;

        int defaultComPort = 0;
        int defaultMonitor = 0;
        int defaultAudioInput = 0;
        int defaultAudioOutput = 0;
        bool useAudioPassthrough = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            // Clear previous entries
            audioInput.Items.Clear();
            audioOutput.Items.Clear();
            renderDevices.Clear();
            inputDevices.Clear();

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

            // FFT Initialization
            precomputedHamming = new float[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                // Precompute the Hamming window
                precomputedHamming[i] = (float)FastFourierTransform.HammingWindow(i, fftSize);
            }
            fftBuffer = new Complex[fftSize];
            sampleBuffer = new float[fftSize];
            #endregion

            // Initialize serial port
            serialPort = new SerialPort();

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
            if (modeSelect.SelectedItem.ToString() == "Screen Sync")
            {
                ScreenCapturer.OnScreenUpdated += OnScreenUpdated;
                ScreenCapturer.StartCapture(screenSelection.SelectedIndex);
            }
            else
            {
                //stop screen capturing
                ScreenCapturer.StopCapture();
                //reset the background to white
                this.BackColor = DefaultBackColor;
            }
            if (modeSelect.SelectedItem.ToString() == "Music Sync")
            {
                StartMusicSync();
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

        /// <summary>
        /// Starts the music sync by initializing the audio capture and optionally setting up passthrough.
        /// </summary>
        private void StartMusicSync()
        {
            try
            {
                // Use the selected input device for loopback capture.
                var selectedDevice = GetSelectedOutputDevice();
                if (selectedDevice == null)
                {
                    MessageBox.Show("No audio input device selected.");
                    return;
                }

                audioCapture = new WasapiLoopbackCapture(selectedDevice);
                audioCapture.DataAvailable += AudioCapture_DataAvailable;
                audioCapture.RecordingStopped += AudioCapture_RecordingStopped;

                // Initialize FFT buffers
                bufferOffset = 0;
                Array.Clear(sampleBuffer, 0, sampleBuffer.Length);
                audioCapture.StartRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting music sync: " + ex.Message);
            }
        }

        /// <summary>
        /// Stops the music sync by stopping and disposing the audio capture and playback devices.
        /// </summary>
        private void StopMusicSync()
        {
            if (audioCapture != null)
            {
                audioCapture.StopRecording();
                audioCapture.Dispose();
                audioCapture = null;
            }
        }

        /// <summary>
        /// Audio data callback for processing captured audio for bass detection and passthrough.
        /// </summary>
        // This method is called whenever audio data is available.
        private void AudioCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            int bytesPerSample = audioCapture.WaveFormat.BitsPerSample / 8;
            int sampleCount = e.BytesRecorded / bytesPerSample;
            int offset = 0;

            // Process all samples in the provided buffer.
            while (offset < sampleCount)
            {
                // Fill the FFT sample buffer until full.
                while (bufferOffset < fftSize && offset < sampleCount)
                {
                    short sample = BitConverter.ToInt16(e.Buffer, offset * bytesPerSample);
                    float normalizedSample = sample / 32768f; // Normalize to [-1, 1]
                    sampleBuffer[bufferOffset++] = normalizedSample;
                    offset++;
                }

                // When the buffer is full, process the FFT block.
                if (bufferOffset == fftSize)
                {
                    ProcessFFTBlock();
                }
            }
        }

        // This helper method processes the audio block, performing FFT and computing bass energy.
        private void ProcessFFTBlock()
        {
            // Apply the Hann window and copy to the FFT buffer.
            for (int j = 0; j < fftSize; j++)
            {
                // Hann window formula: 0.5 * [1 - cos(2πj/(N-1))]
                float windowValue = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * j / (fftSize - 1)));
                fftBuffer[j].X = sampleBuffer[j] * windowValue;
                fftBuffer[j].Y = 0; // Imaginary part is zero
            }

            // Perform FFT. The 2nd parameter is log2(fftSize).
            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftBuffer);

            // Determine how many bins correspond to bass (approximately 20Hz to 250Hz).
            float sampleRate = audioCapture.WaveFormat.SampleRate;
            float freqResolution = sampleRate / fftSize;
            // bassThreshold here is set to 250Hz; adjust if needed.
            int bassBinCount = (int)(250 / freqResolution);
            bassBinCount = Math.Min(bassBinCount, fftBuffer.Length / 2);

            // Calculate the maximum magnitude among bass bins.
            float maxMagnitude = 0;
            for (int k = 0; k < bassBinCount; k++)
            {
                float magnitude = (float)Math.Sqrt(fftBuffer[k].X * fftBuffer[k].X +
                                                     fftBuffer[k].Y * fftBuffer[k].Y);
                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                }
            }

            // Map the maximum bass magnitude to a brightness value.
            // The multiplier (5000) is an empirical value; adjust it according to your input level.
            float rawBrightness = Math.Min(255, maxMagnitude * 5000);

            // Apply exponential moving average for smoother transitions.
            smoothedBrightness = smoothingFactor * smoothedBrightness + (1 - smoothingFactor) * rawBrightness;

            // Apply a deadzone to ignore negligible energy.
            if (smoothedBrightness < 5)
                smoothedBrightness = 0;

            int brightnessInt = (int)Math.Min(255, smoothedBrightness);

            // Output a red tone based solely on bass intensity.
            Color bassColor = Color.FromArgb(brightnessInt, 0, 0);
            sendColorToArduino(bassColor);

            // Reset the buffer offset for the next FFT block.
            bufferOffset = 0;
        }


        /// <summary>
        /// Called when audio capture stops, cleans up resources.
        /// </summary>
        private void AudioCapture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            // Cleanup is handled in StopMusicSync.
            if (e.Exception != null)
            {
                MessageBox.Show("Audio capture error: " + e.Exception.Message);
            }
        }
        #endregion
    }
}