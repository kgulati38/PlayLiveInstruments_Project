using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;

namespace PlayLiveInstruments
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private Button[] fileButtons = new Button[3];
        private string[] filePaths = new string[3];
        private ProgressBar[] progressBars = new ProgressBar[3];
        private BeatBar[] beatBars = new BeatBar[3];
        private TrackBar[] volumeBars = new TrackBar[3];
        private Button playButton;
        private Button stopButton;
        private Button toggleButton;

        private WaveOutEvent[] outputDevices = new WaveOutEvent[3];
        private AudioFileReader[] audioFiles = new AudioFileReader[3];
        private Timer progressTimer;

        private readonly string settingsFile = "selected_files.txt";

        public MainForm()
        {
            this.Text = "Play 3 Files Simultaneously (Loop)";
            this.Width = 650;
            this.Height = 300; // Increased height to ensure the button is visible

            LoadSelectedFiles();

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                fileButtons[i] = new Button() { Text = $"Select File {i + 1}", Left = 30, Top = 20 + i * 60, Width = 200 };
                fileButtons[i].Click += (s, e) =>
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Audio Files|*.wav;*.mp3";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            filePaths[idx] = ofd.FileName;
                            fileButtons[idx].Text = $"File {idx + 1}: {System.IO.Path.GetFileName(ofd.FileName)}";
                            SaveSelectedFiles();
                        }
                    }
                };
                this.Controls.Add(fileButtons[i]);

                progressBars[i] = new ProgressBar() { Left = 250, Top = 25 + i * 60, Width = 200, Height = 20 };
                progressBars[i].Minimum = 0;
                progressBars[i].Maximum = 1000;
                progressBars[i].Value = 0;
                this.Controls.Add(progressBars[i]);

                beatBars[i] = new BeatBar() { Left = 250, Top = 25 + i * 60, Width = 200, Height = 24 };
                this.Controls.Add(beatBars[i]);

                volumeBars[i] = new TrackBar() { Left = 470, Top = 25 + i * 60, Width = 120, Minimum = 0, Maximum = 100, Value = 80, TickFrequency = 10 };
                volumeBars[i].Scroll += (s, e) =>
                {
                    if (audioFiles[idx] != null)
                        audioFiles[idx].Volume = volumeBars[idx].Value / 100f;
                };
                this.Controls.Add(volumeBars[i]);
                var volLabel = new Label() { Text = "Volume", Left = 470, Top = 10 + i * 60, Width = 60 };
                this.Controls.Add(volLabel);
            }

            playButton = new Button() { Text = "Play All (Loop)", Left = 30, Top = 200, Width = 200 };
            stopButton = new Button() { Text = "Stop All", Left = 250, Top = 200, Width = 200, Enabled = false };

            playButton.Click += PlayButton_Click;
            stopButton.Click += StopButton_Click;

            this.Controls.Add(playButton);
            this.Controls.Add(stopButton);

            // Toggle Button
            toggleButton = new Button()
            {
                Text = "Switch to Media Players",
                Left = 30,
                Top = 220, // Adjusted position to ensure it's visible
                Width = 200
            };
            toggleButton.Click += (s, e) =>
            {
                this.Hide();
                var mediaPlayerForm = new MediaPlayerForm();
                mediaPlayerForm.FormClosed += (sender, args) => this.Show();
                mediaPlayerForm.Show();
            };
            this.Controls.Add(toggleButton);

            progressTimer = new Timer();
            progressTimer.Interval = 100; // ms
            progressTimer.Tick += ProgressTimer_Tick;
        }

        private void LoadSelectedFiles()
        {
            if (File.Exists(settingsFile))
            {
                var lines = File.ReadAllLines(settingsFile);
                for (int i = 0; i < Math.Min(3, lines.Length); i++)
                {
                    if (File.Exists(lines[i]))
                    {
                        filePaths[i] = lines[i];
                        fileButtons[i] = new Button() { Text = $"File {i + 1}: {Path.GetFileName(lines[i])}", Left = 30, Top = 20 + i * 60, Width = 200 };
                    }
                }
            }
        }

        private void SaveSelectedFiles()
        {
            File.WriteAllLines(settingsFile, filePaths.Select(p => p ?? ""));
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            StopAll();

            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(filePaths[i]))
                {
                    audioFiles[i] = new AudioFileReader(filePaths[i]);
                    audioFiles[i].Volume = volumeBars[i].Value / 100f;
                    outputDevices[i] = new WaveOutEvent();
                    int idx = i;
                    outputDevices[i].Init(audioFiles[i]);
                    outputDevices[i].PlaybackStopped += (s, args) =>
                    {
                        if (audioFiles[idx] != null)
                        {
                            audioFiles[idx].Position = 0;
                            outputDevices[idx].Play();
                        }
                    };
                    outputDevices[i].Play();
                }
            }
            stopButton.Enabled = true;
            progressTimer.Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopAll();
        }

        private void StopAll()
        {
            progressTimer.Stop();
            for (int i = 0; i < 3; i++)
            {
                outputDevices[i]?.Stop();
                outputDevices[i]?.Dispose();
                outputDevices[i] = null;

                audioFiles[i]?.Dispose();
                audioFiles[i] = null;

                progressBars[i].Value = 0;

                beatBars[i].LitBeats = 0;
                beatBars[i].Invalidate();
            }
            stopButton.Enabled = false;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (audioFiles[i] != null && outputDevices[i] != null)
                {
                    if (audioFiles[i].Length > 0)
                    {
                        double progress = (double)audioFiles[i].Position / audioFiles[i].Length;
                        int barValue = (int)(progress * progressBars[i].Maximum);
                        progressBars[i].Value = Math.Min(progressBars[i].Maximum, Math.Max(progressBars[i].Minimum, barValue));

                        int beats = beatBars[i].Beats;
                        beatBars[i].LitBeats = Math.Min(beats, Math.Max(0, (int)(progress * beats)));
                    }
                    else
                    {
                        progressBars[i].Value = 0;
                        beatBars[i].LitBeats = 0;
                    }
                }
                else
                {
                    progressBars[i].Value = 0;
                    beatBars[i].LitBeats = 0;
                }
                beatBars[i].Invalidate();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSelectedFiles();
            StopAll();
            base.OnFormClosing(e);
        }
    }
}