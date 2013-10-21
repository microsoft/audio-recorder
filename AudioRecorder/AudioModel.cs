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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioRecorder
{
    // Delegate types for hooking up change notifications.
    public delegate void AudioBufferChangedEventHandler(object sender, EventArgs e);
    public delegate void PlayingEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Data model to hold audio state information.
    /// </summary>
    public class AudioModel : INotifyPropertyChanged
    {
        // Members
        public ObservableCollection<AudioFileModel> AudioFiles { get; private set; }
        public MemoryStream stream = new MemoryStream(); // Stream to hold audio data

        /// <summary>
        /// AudioBuffer property.
        /// Use this property to access current audio buffer.
        /// </summary>
        private byte[] _audioBuffer;
        public byte[] AudioBuffer
        {
            get
            {
                return _audioBuffer;
            }
            set
            {
                _audioBuffer = value;
                OnAudioBufferChanged(EventArgs.Empty);
            }
        }

        // An event used to notify clients whenever the audio buffer changes.
        public event AudioBufferChangedEventHandler AudioBufferChanged;

        protected virtual void OnAudioBufferChanged(EventArgs e)
        {
            if (AudioBufferChanged != null)
            {
                AudioBufferChanged(this, e);
            }
        }

        /// <summary>
        /// IsPlaying property.
        /// Use this property to determine if audio is being played.
        /// </summary>
        private Boolean _isPlaying;
        public Boolean IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPlayingChanged(EventArgs.Empty);
                }
            }
        }

        // An event used to notify clients whenever the playing state changes.
        public event PlayingEventHandler PlayingChanged;

        protected virtual void OnPlayingChanged(EventArgs e)
        {
            if (PlayingChanged != null)
            {
                PlayingChanged(this, e);
            }
        }

        /// <summary>
        /// IsRecording property.
        /// Use this property to determine if audio is being recorded.
        /// </summary>
        private Boolean _isRecording = false;
        public Boolean IsRecording
        {
            get
            {
                return _isRecording;
            }
            set
            {
                if (value != _isRecording)
                {
                    _isRecording = value;
                    NotifyPropertyChanged("IsRecording");
                }
            }
        }

        /// <summary>
        /// ChannelCount property.
        /// Use this property to determine whether mono or stereo is recorded.
        /// </summary>
        private int _channelCount = 1;
        public int ChannelCount
        {
            get
            {
                return _channelCount;
            }
            set
            {
                if (value != _channelCount)
                {
                    _channelCount = value;
                    NotifyPropertyChanged("ChannelCount");
                }
            }
        }

        /// <summary>
        /// SampleRate property.
        /// Use this property to determine the sample rate used in recording.
        /// </summary>
        private int _sampleRate = 16000;
        public int SampleRate
        {
            get
            {
                return _sampleRate;
            }
            set
            {
                if (value != _sampleRate)
                {
                    _sampleRate = value;
                    NotifyPropertyChanged("SampleRate");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioModel()
        {
            AudioFiles = new ObservableCollection<AudioFileModel>();
        }

        /// <summary>
        /// Loads local audio information.
        /// </summary>
        public void ReadAudioFileInfo()
        {
            AudioFiles.Clear();

            // Load the image which was filtered from isolated app storage.
            System.IO.IsolatedStorage.IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                string[] fileNames = myStore.GetFileNames();
                foreach (string s in fileNames)
                {
                    AudioFileModel audioFile = new AudioFileModel();
                    audioFile.FileName = s;
                    IsolatedStorageFileStream fileStream = myStore.OpenFile(s, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    audioFile.FileSize = "" + fileStream.Length + " bytes";

                    // Read sample rate and channel count
                    Encoding encoding = Encoding.UTF8;
                    byte[] bytes = new byte[4];

                    // channel count
                    fileStream.Seek(22, SeekOrigin.Begin);
                    fileStream.Read(bytes, 0, 2);
                    audioFile.ChannelCount = BitConverter.ToInt16(bytes, 0);

                    // sample rate
                    fileStream.Read(bytes, 0, 4);
                    audioFile.SampleRate = BitConverter.ToInt32(bytes, 0);

                    audioFile.FileLength = "" + fileStream.Length / (2 * audioFile.SampleRate * audioFile.ChannelCount) + " seconds";
                    AudioFiles.Add(audioFile);

                    fileStream.Dispose();
                }
            }
            catch
            {
                MessageBox.Show("Error while trying to read audio files.");
            }
        }

        /// <summary>
        /// Loads an audio buffer from isolated storage.
        /// </summary>
        public void LoadAudioBuffer(String fileName)
        {
            // Set the stream back to zero in case there is already something in it
            stream.SetLength(0);

            // Retrieve the named audio file from the storage.
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                using (var isoFileStream = new IsolatedStorageFileStream(
                    fileName,
                    FileMode.Open,
                    myStore))
                {
                    isoFileStream.CopyTo(stream);
                    stream.Flush();
                }
            }
            catch
            {
                MessageBox.Show("Error while trying to load audio buffer.");
            }
        }
    }
}
