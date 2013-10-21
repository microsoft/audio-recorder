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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AudioRecorder.Resources;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace AudioRecorder
{
    /// <summary>
    /// Main page of the application - The Recorder.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Members
        private Boolean updateVisualization = false;
        private enum VisualizationStyle { Value, Deteriorating, Time };
        private VisualizationStyle visualizationStyle = VisualizationStyle.Time;

        private int barCount = 11;
        private Image[] barImages;
        private int barBottomMargin;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            if (App.Current.Host.Content.ScaleFactor == 150)
            {
                BgBrush.ImageSource = new BitmapImage(
                    new Uri(@"Assets/Background-720p.png", UriKind.Relative)
                );
                barBottomMargin = 444;
            }
            else
            {
                BgBrush.ImageSource = new BitmapImage(
                    new Uri(@"Assets/Background.png", UriKind.Relative)
                );
                barBottomMargin = 394;
            }

            SystemTray.SetOpacity(this, 0.01);

            App.AudioModel.AudioBufferChanged += new AudioBufferChangedEventHandler(AudioBufferChanged);

            InitializeVisualizationBars();
        }

        /// <summary>
        /// Initialize visualization bars. Position changes with aspect ratio.
        /// </summary>
        protected void InitializeVisualizationBars()
        {
            // Initialize visualization bars
            barImages = new Image[barCount];
            for (int i = 0; i < barCount; i++)
            {
                Image bar = new Image();
                BitmapImage barImage = new BitmapImage(new Uri("Assets/FullBar.png", UriKind.Relative));
                bar.Source = barImage;
                bar.Stretch = Stretch.None;
                bar.HorizontalAlignment = HorizontalAlignment.Left;
                bar.VerticalAlignment = VerticalAlignment.Bottom;
                bar.Margin = new Thickness(36 + ((35 + 36) * i) / 2, 0, 0, barBottomMargin);
                bar.Height = 0;
                barImages[i] = bar;
                ContentPanel.Children.Add(bar);
            }
        }

        /// <summary>
        /// Start/Stop recording. Updates the button graphics.
        /// </summary>
        /// <param name="sender">RecButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRecButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.AudioModel.IsRecording)
            {
                App.AudioManager.StopRecording();
                BitmapImage recImage = new BitmapImage(new Uri("Assets/RecButtonUp.png", UriKind.Relative));
                RecButton.Source = recImage;
                BitmapImage lightImage = new BitmapImage(new Uri("Assets/RecLightOff.png", UriKind.Relative));
                RecLight.Source = lightImage;
                updateVisualization = false;
                for (int i = 0; i < barImages.Length; i++)
                {
                    barImages[i].Height = 0;
                }
            }
            else
            {
                updateVisualization = true;
                App.AudioManager.StartRecording();
                BitmapImage recImage = new BitmapImage(new Uri("Assets/RecButtonDown.png", UriKind.Relative));
                RecButton.Source = recImage;
                BitmapImage lightImage = new BitmapImage(new Uri("Assets/RecLightOn.png", UriKind.Relative));
                RecLight.Source = lightImage;
            }
        }

        /// <summary>
        /// Change recording quality.
        /// </summary>
        /// <param name="sender">Quality stack panel.</param>
        /// <param name="e">Event arguments.</param>
        private void OnQualityTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.AudioModel.IsRecording)
            {
                if (MonoStereoText.Text == "Mono")
                {
                    MonoStereoText.Text = "Stereo";
                    SampleRateText.Text = "44100 Hz";
                    App.AudioModel.ChannelCount = 2;
                    App.AudioModel.SampleRate = 44100;
                }
                else
                {
                    MonoStereoText.Text = "Mono";
                    SampleRateText.Text = "16000 Hz";
                    App.AudioModel.ChannelCount = 1;
                    App.AudioModel.SampleRate = 16000;
                }
            }
        }

        /// <summary>
        /// Change visualization.
        /// </summary>
        /// <param name="sender">VisualizationRect.</param>
        /// <param name="e">Event arguments.</param>
        private void OnVisualizationTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (visualizationStyle == VisualizationStyle.Value)
            {
                visualizationStyle = VisualizationStyle.Deteriorating;
            }
            else if (visualizationStyle == VisualizationStyle.Deteriorating)
            {
                visualizationStyle = VisualizationStyle.Time;
            }
            else if (visualizationStyle == VisualizationStyle.Time)
            {
                visualizationStyle = VisualizationStyle.Value;
            }
        }

        /// <summary>
        /// Navigate to AudioFilePage.
        /// </summary>
        /// <param name="sender">FilesButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFilesButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.AudioModel.IsRecording)
            {
                NavigationService.Navigate(new Uri("/AudioFilePage.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// Change files button graphics when button is pressed.
        /// </summary>
        /// <param name="sender">FilesButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFilesButtonEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BitmapImage image = new BitmapImage(new Uri("Assets/FilesButtonDown.png", UriKind.Relative));
            FilesButton.Source = image;
        }

        /// <summary>
        /// Change files button graphics when button is unpressed.
        /// </summary>
        /// <param name="sender">FilesButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFilesButtonLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BitmapImage image = new BitmapImage(new Uri("Assets/FilesButtonUp.png", UriKind.Relative));
            FilesButton.Source = image;
        }

        /// <summary>
        /// Update visualization whenever the audio buffer changes.
        /// </summary>
        /// <param name="sender">AudioModel.</param>
        /// <param name="e">Event arguments.</param>
        private void AudioBufferChanged(object sender, EventArgs e)
        {
            UpdateVisualization(App.AudioModel.AudioBuffer, 0, App.AudioModel.AudioBuffer.Length);
        }

        /// <summary>
        /// Call correct visualization function.
        /// </summary>
        /// <param name="array">Audio data buffer.</param>
        /// <param name="position">Position of buffer from where to visualize.</param>
        /// <param name="sampleSize">Sample size to visualize.</param>
        private void UpdateVisualization(byte[] array, int position, int sampleSize)
        {
            if (updateVisualization == false) return;

            if (visualizationStyle == VisualizationStyle.Value)
            {
                VisualizeValue(array, position, sampleSize);
            }
            else if (visualizationStyle == VisualizationStyle.Deteriorating)
            {
                VisualizeDeterioratingValue(array, position, sampleSize);
            }
            else
            {
                VisualizeValueByTime(array, position, sampleSize);
            }
        }

        /// <summary>
        /// Helper function to get the highest value in the audio buffer.
        /// </summary>
        /// <param name="array">Audio data buffer.</param>
        /// <param name="position">Position of buffer from where to visualize.</param>
        /// <param name="sampleSize">Sample size to visualize.</param>
        private long GetHighestValue(byte[] array, int position, int sampleSize)
        {
            long highest = 0;
            int end = position + sampleSize;
            for (int i = position; i < end; i++)
            {
                long value = BitConverter.ToInt16(array, i);

                if (value < 0)
                {
                    value *= -1;
                }

                if (highest < value)
                {
                    highest = value;
                }
                i++;
            }
            return highest;
        }

        /// <summary>
        /// Update the visualization bars to show value.
        /// </summary>
        /// <param name="array">Audio data buffer.</param>
        /// <param name="position">Position of buffer from where to visualize.</param>
        /// <param name="sampleSize">Sample size to visualize.</param>
        private void VisualizeValue(byte[] array, int position, int sampleSize)
        {
            long highest = GetHighestValue(array, position, sampleSize);

            // Maximum value of a 16-bit signed integer.
            const double MaxValue = 32768f;

            int barsVisible = (int)(highest / MaxValue * 16 + 0.5);
            double barHeight = 15 * barsVisible; // full bar image height = 248 pixels = 16 bars

            if (barHeight < barImages[barCount / 2].Height / 2)
            {
                barsVisible = (int)((barImages[barCount / 2].Height) / 15);
                barImages[barCount / 2].Height = barsVisible / 2 * 15;
            }
            else
            {
                barImages[barCount / 2].Height = barHeight;
            }

            for (int i = barCount / 2 - 1; i >= 0; i--)
            {
                barsVisible /= 2;
                double tempHeight = barsVisible * 15;
                if (tempHeight < 0) tempHeight = 0;
                barImages[i].Height = barImages[barCount - 1 - i].Height = tempHeight;
            }
        }

        /// <summary>
        /// Update the visualization bars to show deteriorating value.
        /// </summary>
        /// <param name="array">Audio data buffer.</param>
        /// <param name="position">Position of buffer from where to visualize.</param>
        /// <param name="sampleSize">Sample size to visualize.</param>
        private void VisualizeDeterioratingValue(byte[] array, int position, int sampleSize)
        {
            long highest = GetHighestValue(array, position, sampleSize);

            // Maximum value of a 16-bit signed integer.
            const double MaxValue = 32768f;

            int barsVisible = (int)(highest / MaxValue * 16 + 0.5);
            double barHeight = 15 * barsVisible; // full bar image height = 248 pixels = 16 bars

            for (int i = 0; i < barCount / 2; i++)
            {
                int tempBarsVisible = (int)(barImages[i + 1].Height / 15 / 2);
                barImages[i].Height = barImages[barCount - 1 - i].Height = tempBarsVisible * 15; //barImages[i + 1].Height / 2;
            }
            barImages[barCount / 2].Height = barHeight;
        }

        /// <summary>
        /// Update the visualization bars to show value by time.
        /// </summary>
        /// <param name="array">Audio data buffer.</param>
        /// <param name="position">Position of buffer from where to visualize.</param>
        /// <param name="sampleSize">Sample size to visualize.</param>
        private void VisualizeValueByTime(byte[] array, int position, int sampleSize)
        {
            long highest = GetHighestValue(array, position, sampleSize);

            // Maximum value of a 16-bit signed integer.
            const double MaxValue = 32768f;

            int barsVisible = (int)(highest / MaxValue * 16 + 0.5);
            double barHeight = 15 * barsVisible; // full bar image height = 248 pixels = 16 bars

            for (int i = 0; i < barCount - 1; i++)
            {
                barImages[i].Height = barImages[i + 1].Height;
            }
            barImages[barCount - 1].Height = barHeight;
        }
    }
}