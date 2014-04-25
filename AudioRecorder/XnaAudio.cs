/**
 * Copyright (c) 2013-2014 Microsoft Mobile. All rights reserved.
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

namespace AudioRecorder
{
    /// <summary>
    /// Class handling the XNA API to record and play audio.
    /// </summary>
    public class XnaAudio
    {
        private Microphone microphone = Microphone.Default;  // Object representing the physical microphone on the device
        private byte[] buffer;                               // Dynamic buffer to retrieve audio data from the microphone
        private DynamicSoundEffectInstance playback;         // Used to play back audio
        private int sampleSize;
        private int position;
        private byte[] byteArray;

        /// <summary>
        /// Constructor.
        /// </summary>
        public XnaAudio()
        {
            // Event handler for getting audio data when the buffer is full
            microphone.BufferDuration = TimeSpan.FromMilliseconds(100);
            microphone.BufferReady += new EventHandler<EventArgs>(microphone_BufferReady);

            // initialize dynamic sound effect instance
            playback = new DynamicSoundEffectInstance(microphone.SampleRate, AudioChannels.Mono);
            playback.BufferNeeded += GetSamples;
            sampleSize = playback.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(100));
        }

        /// <summary>
        /// Gives samples to the DynamicSoundEffectInstance to play.
        /// Updates the position of the playback in the buffer.
        /// </summary>
        /// <param name="sender">DynamicSoundEffectInstance.</param>
        /// <param name="e">Event arguments.</param>
        private void GetSamples(object sender, EventArgs e)
        {
            // -44 for taking wav header into account
            while (playback.PendingBufferCount < 2 && position < byteArray.Length - 44) 
            {
                playback.SubmitBuffer(byteArray, position, sampleSize);
                position += sampleSize;
            }
        }

        /// <summary>
        /// Checks if there is an audio still playing.
        /// </summary>
        /// <returns>True if an audio is being played.</returns>
        public Boolean SoundIsPlaying()
        {
            Boolean ret = true;
            if (playback.PendingBufferCount == 0)
            {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// The Microphone.BufferReady event handler.
        /// Gets the audio data from the microphone, stores it in a buffer,
        /// then writes that buffer to a stream for later playback.
        /// Any action in this event handler should be quick!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void microphone_BufferReady(object sender, EventArgs e)
        {
            // Retrieve audio data
            microphone.GetData(buffer);

            // send data to model, for visualization
            App.AudioModel.AudioBuffer = buffer;

            // Store the audio data in a stream
            App.AudioModel.stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Plays the audio using DynamicSoundEffectInstance 
        /// so we can monitor the playback status.
        /// </summary>
        private void playSoundEffect()
        {
            playback.Play();
        }

        /// <summary>
        /// Start playback from stream.
        /// </summary>
        /// <param name="fileName">Name of the file to play.</param>
        public void StartPlayback(String fileName)
        {
            App.AudioModel.LoadAudioBuffer(fileName);

            if (App.AudioModel.stream.Length > 0)
            {
                App.AudioModel.stream.Position = 0;
                position = 44; // take wav header into account
                byteArray = App.AudioModel.stream.ToArray();

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSoundEffect));
                soundThread.Start();
            }
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void StopPlayback()
        {
            if (playback.State == SoundState.Playing)
            {
                playback.Stop();
            }
        }

        /// <summary>
        /// Start recording.
        /// </summary>
        public void StartRecording()
        {
            // Get audio data in 100 ms chunks
            microphone.BufferDuration = TimeSpan.FromMilliseconds(100);

            // Allocate memory to hold the audio data
            buffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];

            // Set the stream back to zero in case there is already something in it
            App.AudioModel.stream.SetLength(0);

            // Start recording
            microphone.Start();
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public void StopRecording()
        {
            if (microphone.State == MicrophoneState.Started)
            {
                microphone.Stop();
            }
        }

        /// <summary>
        /// Skips next five seconds of playback if possible.
        /// </summary>
        public void SkipFiveSecs()
        {
            if (playback.State == SoundState.Playing)
            {
                if (position + sampleSize * 50 < byteArray.Length)
                {
                    position += sampleSize * 50;
                }
            }
        }
    }
}
