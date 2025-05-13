using System;
using System.Windows.Forms;
using NAudio.Wave;

namespace PlayLiveInstruments
{
    public class MediaPlayerForm : Form
    {
        private WaveOutEvent[] outputDevices = new WaveOutEvent[4];
        private AudioFileReader[] audioFiles = new AudioFileReader[4];
        private TrackBar[] volumeBars = new TrackBar[4];
        private Button[] playButtons = new Button[4];
        private Button[] stopButtons = new Button[4];
        private ProgressBar[] progressBars = new ProgressBar[4]; // Add progress bars
        private Timer progressTimer; // Timer to update progress bars
        private string[] filePaths = new string[]
        {
            @"C:\Users\BMG\Music\Dhol Loop Pack\DHOLAK-090-intro-01.wav", // Default file for Player 1
            @"C:\Users\BMG\Music\Dhol Loop Pack\DHOLAK-090-01.wav", // Default file for Player 2
            @"C:\Users\BMG\Music\Dhol Loop Pack\DHOLAK-6_8-095-fill-01.wav", // Default file for Player 3
            @"C:\Users\BMG\Music\Dhol Loop Pack\DHOLAK-090-end-04.wav"  // Default file for Player 4
        };

        public MediaPlayerForm()
        {
            this.Text = "Media Players";
            this.Width = 800;
            this.Height = 500;

            progressTimer = new Timer();
            progressTimer.Interval = 100; // Update every 100ms
            progressTimer.Tick += ProgressTimer_Tick;

            for (int i = 0; i < 4; i++)
            {
                int idx = i;

                // Play Button
                playButtons[i] = new Button() { Text = $"Play {i + 1}", Left = 30, Top = 20 + i * 100, Width = 100 };
                playButtons[i].Click += (s, e) => PlayMedia(idx);
                this.Controls.Add(playButtons[i]);

                // Stop Button
                stopButtons[i] = new Button() { Text = $"Stop {i + 1}", Left = 140, Top = 20 + i * 100, Width = 100 };
                stopButtons[i].Click += (s, e) => StopMedia(idx);
                this.Controls.Add(stopButtons[i]);

                // Volume Control
                volumeBars[i] = new TrackBar() { Left = 250, Top = 20 + i * 100, Width = 200, Minimum = 0, Maximum = 100, Value = 80, TickFrequency = 10 };
                volumeBars[i].Scroll += (s, e) =>
                {
                    if (audioFiles[idx] != null)
                        audioFiles[idx].Volume = volumeBars[idx].Value / 100f;
                };
                this.Controls.Add(volumeBars[i]);

                // Progress Bar
                progressBars[i] = new ProgressBar() { Left = 470, Top = 20 + i * 100, Width = 200, Height = 20 };
                progressBars[i].Minimum = 0;
                progressBars[i].Maximum = 1000; // Progress is scaled to 1000
                progressBars[i].Value = 0;
                this.Controls.Add(progressBars[i]);
            }

            // Add keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += MediaPlayerForm_KeyDown;
        }

        private void PlayMedia(int idx)
        {
            StopMedia(idx); // Stop the current media if it's already playing
            if(idx >1) 
            {
                StopMedia(1);
            }

            if (outputDevices[idx] == null)
            {
                audioFiles[idx] = new AudioFileReader(filePaths[idx]);
                audioFiles[idx].Volume = volumeBars[idx].Value / 100f;

                outputDevices[idx] = new WaveOutEvent();
                outputDevices[idx].Init(audioFiles[idx]);
                outputDevices[idx].PlaybackStopped += (s, e) =>
                {
                    if (idx == 0) // Player 1 finishes, restart it in a loop
                    {
                        PlayMedia(1);
                    }
                    else if (idx == 1) // Player 2 finishes, start Player 3 in loop
                    {
                        PlayMedia(1);
                    }
        
                    else if (idx == 2) // Player 3 finishes, start Player 2 in loop
                    {
                        PlayMedia(1);
                    }
                };
            }

    
            outputDevices[idx].Play();
            progressTimer.Start(); // Start updating progress bars
        }

        private void StopMedia(int idx)
        {
            if (outputDevices[idx] != null)
            {
                outputDevices[idx].Stop();
                outputDevices[idx].Dispose();
                outputDevices[idx] = null;

                audioFiles[idx]?.Dispose();
                audioFiles[idx] = null;

                progressBars[idx].Value = 0; // Reset progress bar
            }
        }

        private void StopAllMedia()
        {
            for (int i = 0; i < 4; i++)
            {
                StopMedia(i);
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (audioFiles[i] != null && outputDevices[i] != null)
                {
                    if (audioFiles[i].Length > 0)
                    {
                        double progress = (double)audioFiles[i].Position / audioFiles[i].Length;
                        int barValue = (int)(progress * progressBars[i].Maximum);
                        progressBars[i].Value = Math.Min(progressBars[i].Maximum, Math.Max(progressBars[i].Minimum, barValue));
                    }
                    else
                    {
                        progressBars[i].Value = 0;
                    }
                }
                else
                {
                    progressBars[i].Value = 0;
                }
            }
        }

        private void MediaPlayerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S) // Left arrow: Play Player 1
            {
                StopAllMedia();
                PlayMedia(0);
            }
            else if (e.KeyCode == Keys.P) // Down arrow: Play Player 2 in loop
            {
                StopAllMedia();
                PlayMedia(1); 
            }
            else if (e.KeyCode == Keys.T) // Up arrow: Stop all and play Player 3 once
            {
                StopAllMedia();
                PlayMedia(2);
            }
            else if (e.KeyCode == Keys.E) // Right arrow: Stop all and play Player 4 once
            {
                StopAllMedia();
                PlayMedia(3);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAllMedia();
            progressTimer.Stop(); // Stop the timer when the form closes
            base.OnFormClosing(e);
        }
    }
}