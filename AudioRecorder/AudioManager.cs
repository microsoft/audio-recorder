/**
 * Copyright (c) 2013 Nokia Corporation. All rights reserved.
 *
 * Nokia, Nokia Connecting People, Nokia Developer, and HERE are trademarks
 * and/or registered trademarks of Nokia Corporation. Other product and company
 * names mentioned herein may be trademarks or trade names of their respective
 * owners.
 *
 * See the license text file delivered with this project for more information.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WasapiAudioComp;

namespace AudioRecorder
{
    /// <summary>
    /// Class handling the audio recording and playback.
    /// </summary>
    public class AudioManager
    {
        XnaAudio xnaAudio = null;
        WasapiAudio wasapiAudio = null;
        Boolean wasapiInUse = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioManager()
        {
            // Timer to simulate the XNA Framework game loop (Microphone in 
            // XnaAudio.cs is from the XNA Framework). Timer is also used to
            // monitor the state of audio playback.
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(33);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();

            xnaAudio = new XnaAudio();
            wasapiAudio = new WasapiAudio();
        }

        /// <summary>
        /// Updates the XNA FrameworkDispatcher and updates the playing state
        /// if sound has stopped playing.
        /// </summary>
        /// <param name="sender">DispatcherTimer.</param>
        /// <param name="e">Event arguments.</param>
        void dt_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (true == App.AudioModel.IsPlaying)
            {
                if (!wasapiInUse)
                {
                    if (!xnaAudio.SoundIsPlaying())
                    {
                        xnaAudio.StopPlayback();
                        App.AudioModel.IsPlaying = false;
                    }
                }
                else
                {
                    App.AudioModel.IsPlaying = wasapiAudio.Update();
                }
            }
            else if (true == App.AudioModel.IsRecording && wasapiInUse)
            {
                // XnaAudio stores the buffer in callback method.
                // Buffer is retrieved manually when recording using WASAPI.
                byte[] bytes = null;
                int size = wasapiAudio.ReadBytes(out bytes);

                if (size > 0)
                {
                    App.AudioModel.AudioBuffer = bytes;
                    App.AudioModel.stream.Write(bytes, 0, size);
                }
            }
        }

        /// <summary>
        /// Starts recording.
        /// </summary>
        public void StartRecording()
        {
            App.AudioModel.IsRecording = true;

            // XNA Microphone records audio in 16-bit, Mono, 16.000 Hz.
            if (App.AudioModel.ChannelCount == 1 && App.AudioModel.SampleRate == 16000)
            {
                wasapiInUse = false;
                xnaAudio.StartRecording();
            }
            // 16-bit, Stereo 44100 Hz is used in WASAPI recording
            else
            {
                wasapiInUse = true;
                App.AudioModel.stream.SetLength(0);
                wasapiAudio.StartAudioCapture();
            }
        }

        /// <summary>
        /// Stops recording.
        /// </summary>
        public void StopRecording()
        {
            // XNA Microphone records audio in 16-bit, Mono, 16.000 Hz.
            if (App.AudioModel.ChannelCount == 1 && App.AudioModel.SampleRate == 16000)
            {
                xnaAudio.StopRecording();
            }
            // 16-bit, Stereo 44100 Hz is used in WASAPI recording
            else
            {
                wasapiAudio.StopAudioCapture();
            }
            saveAudioBuffer();
            App.AudioModel.IsRecording = false;
        }

        /// <summary>
        /// Starts playback.
        /// </summary>
        /// <param name="fileName">Name of the file to play.</param>
        /// <param name="useWasapi">True to use WASAPI, False to use XNA.</param>
        public void StartPlayback(String fileName, Boolean useWasapi)
        {
            if (!App.AudioModel.IsPlaying)
            {
                wasapiInUse = useWasapi;
                if (!wasapiInUse)
                {
                    xnaAudio.StartPlayback(fileName);
                }
                else
                {
                    App.AudioModel.LoadAudioBuffer(fileName);
                    byte[] byteArray = App.AudioModel.stream.ToArray();
                    if (App.AudioModel.stream.Length > 0)
                    {
                        App.AudioModel.stream.Position = 0;
                    }

                    wasapiAudio.SetAudioBytes(byteArray);
                    wasapiAudio.StartAudioRender();
                }
                App.AudioModel.IsPlaying = true;
            }
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void StopPlayback()
        {
            if (App.AudioModel.IsPlaying)
            {
                if (!wasapiInUse)
                {
                    xnaAudio.StopPlayback();
                }
                else
                {
                    wasapiAudio.StopAudioRender();
                }
                App.AudioModel.IsPlaying = false;
            }
        }

        /// <summary>
        /// Skips the next five seconds of audio playback.
        /// </summary>
        public void SkipFiveSecs()
        {
            if (App.AudioModel.IsPlaying)
            {
                if (!wasapiInUse)
                {
                    xnaAudio.SkipFiveSecs();
                }
                else
                {
                    wasapiAudio.SkipFiveSecs();
                }
            }
//            LOLOLO - What abuot wasapi
        }

        /// <summary>
        /// Saves the audio buffer into isolated storage as a wav-file.
        /// </summary>
        private void saveAudioBuffer()
        {
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                DateTime dateTime = DateTime.Now;
                string fileName = dateTime.ToString("yyyy_MM_dd_HH_mm_ss.wav");
                using (var isoFileStream = new IsolatedStorageFileStream(
                    fileName,
                    FileMode.OpenOrCreate,
                    myStore))
                {
                    // Write a header before the actual pcm data
                    int sampleBits = 16;
                    int sampleBytes = sampleBits / 8;
                    int byteRate = App.AudioModel.SampleRate * sampleBytes * App.AudioModel.ChannelCount;
                    int blockAlign = sampleBytes * App.AudioModel.ChannelCount;
                    Encoding encoding = Encoding.UTF8;

                    isoFileStream.Write(encoding.GetBytes("RIFF"), 0, 4);                       // "RIFF"
                    isoFileStream.Write(BitConverter.GetBytes(0), 0, 4);                        // Chunk Size
                    isoFileStream.Write(encoding.GetBytes("WAVE"), 0, 4);                       // Format - "Wave"
                    isoFileStream.Write(encoding.GetBytes("fmt "), 0, 4);                       // sub chunk - "fmt"
                    isoFileStream.Write(BitConverter.GetBytes(16), 0, 4);                       // sub chunk size
                    isoFileStream.Write(BitConverter.GetBytes((short)1), 0, 2);                 // audio format
                    isoFileStream.Write(BitConverter.GetBytes((short)App.AudioModel.ChannelCount), 0, 2); // num of channels
                    isoFileStream.Write(BitConverter.GetBytes(App.AudioModel.SampleRate), 0, 4);    // sample rate
                    isoFileStream.Write(BitConverter.GetBytes(byteRate), 0, 4);                 // byte rate
                    isoFileStream.Write(BitConverter.GetBytes((short)(blockAlign)), 0, 2);      // block align
                    isoFileStream.Write(BitConverter.GetBytes((short)(sampleBits)), 0, 2);      // bits per sample
                    isoFileStream.Write(encoding.GetBytes("data"), 0, 4);                       // sub chunk - "data"
                    isoFileStream.Write(BitConverter.GetBytes(0), 0, 4);                        // sub chunk size

                    // write the actual pcm data
                    App.AudioModel.stream.Position = 0;
                    App.AudioModel.stream.CopyTo(isoFileStream);

                    // and fill in the blanks
                    long previousPos = isoFileStream.Position;
                    isoFileStream.Seek(4, SeekOrigin.Begin);
                    isoFileStream.Write(BitConverter.GetBytes((int)isoFileStream.Length - 8), 0, 4);
                    isoFileStream.Seek(40, SeekOrigin.Begin);
                    isoFileStream.Write(BitConverter.GetBytes((int)isoFileStream.Length - 44), 0, 4);
                    isoFileStream.Seek(previousPos, SeekOrigin.Begin);

                    isoFileStream.Flush();
                }
            }
            catch
            {
                MessageBox.Show("Error while trying to store audio stream.");
            }
        }
    }
}
