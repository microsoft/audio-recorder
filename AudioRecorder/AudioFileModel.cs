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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioRecorder
{
    /// <summary>
    /// Data model of an audio file.
    /// </summary>
    public class AudioFileModel : INotifyPropertyChanged
    {
        private string _fileName;
        /// <summary>
        /// Audio file name property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        private string _fileSize;
        /// <summary>
        /// Audio file size property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                if (value != _fileSize)
                {
                    _fileSize = value;
                    NotifyPropertyChanged("FileSize");
                }
            }
        }

        private int _channelCount;
        /// <summary>
        /// Audio file channel count property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
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
                    ChannelCountString = value == 1 ? "Mono" : "Stereo";
                    NotifyPropertyChanged("ChannelCount");
                }
            }
        }

        private string _channelCountString;
        /// <summary>
        /// Audio file channel count string property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string ChannelCountString
        {
            get
            {
                return _channelCountString;
            }
            set
            {
                if (value != _channelCountString)
                {
                    _channelCountString = value;
                    NotifyPropertyChanged("ChannelCountString");
                }
            }
        }

        private int _sampleRate;
        /// <summary>
        /// Audio file sample rate property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
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
                    SampleRateString = value + " Hz";
                    NotifyPropertyChanged("SampleRate");
                }
            }
        }

        private string _sampleRateString;
        /// <summary>
        /// Audio file sample rate string property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string SampleRateString
        {
            get
            {
                return _sampleRateString;
            }
            set
            {
                if (value != _sampleRateString)
                {
                    _sampleRateString = value;
                    NotifyPropertyChanged("SampleRateString");
                }
            }
        }

        private string _fileLength;
        /// <summary>
        /// Audio file length property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string FileLength
        {
            get
            {
                return _fileLength;
            }
            set
            {
                if (value != _fileLength)
                {
                    _fileLength = value;
                    NotifyPropertyChanged("FileLength");
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
    }
}
