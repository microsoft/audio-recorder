/**
 * Copyright (c) 2013-2014 Microsoft Mobile. All rights reserved.
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
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace AudioRecorder
{
    /// <summary>
    /// Page for displaying audio files in isolated storage.
    /// </summary>
    public partial class AudioFilePage : PhoneApplicationPage
    {
        // Members
        String selectedFileName = "";

        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioFilePage()
        {
            InitializeComponent();
            App.AudioModel.ReadAudioFileInfo();
            DataContext = App.AudioModel;

            CreateAppBar();
            App.AudioModel.PlayingChanged += new PlayingEventHandler(PlayingChanged);
        }

        /// <summary>
        /// Stop the playback when navigating away from the page.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (App.AudioModel.IsPlaying)
            {
                App.AudioManager.StopPlayback();
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Called whenever the audio play mode changes.
        /// </summary>
        /// <param name="sender">AudioModel.</param>
        /// <param name="e">Event arguments.</param>
        private void PlayingChanged(object sender, EventArgs e)
        {
            if (App.AudioModel.IsPlaying)
            {
                StopPlayButton.IconUri = new Uri("Assets/Stop.png", UriKind.Relative);
                StopPlayButton.Text = "stop";
            }
            else
            {
                StopPlayButton.IconUri = new Uri("Assets/Play.png", UriKind.Relative);
                StopPlayButton.Text = "play";
            }
            ForwardButton.IsEnabled = App.AudioModel.IsPlaying;
        }

        /// <summary>
        /// Called when an audio file is selected.
        /// </summary>
        /// <param name="sender">AudioFilesList (LongListSelector).</param>
        /// <param name="e">Event arguments.</param>
        void OnAudioFileSelected(Object sender, SelectionChangedEventArgs e)
        {
            AudioFileModel selected = (AudioFileModel)AudioFilesList.SelectedItem;

            if (App.AudioModel.IsPlaying)
            {
                App.AudioManager.StopPlayback();
            }

            if (selected != null)
            {
                selectedFileName = selected.FileName;

                StopPlayButton.IsEnabled = true;
                SaveButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Start/Stop the audio playback.
        /// </summary>
        /// <param name="sender">StopPlay menu item</param>
        /// <param name="e">Event arguments</param>
        private void StopPlay_Click(object sender, EventArgs e)
        {
            if (App.AudioModel.IsPlaying)
            {
                App.AudioManager.StopPlayback();
            }
            else
            {
                if (selectedFileName.Length == 0)
                {
                    MessageBox.Show("Select an audio file first.");
                }
                else
                {
                    AudioFileModel selected = (AudioFileModel)AudioFilesList.SelectedItem;
                    Boolean useWasapi = selected.SampleRate != 16000 || selected.ChannelCount != 1;
                    App.AudioManager.StartPlayback(selectedFileName, useWasapi);
                }
            }
        }

        /// <summary>
        /// Save the audio file to media library.
        /// </summary>
        /// <param name="sender">Save menu item</param>
        /// <param name="e">Event arguments</param>
        private void Save_Click(object sender, EventArgs e)
        {
            AudioFileModel selected = (AudioFileModel)AudioFilesList.SelectedItem;
            if (selected != null)
            {
                var library = new MediaLibrary();
                Song s = library.SaveSong(
                    new Uri(selected.FileName, UriKind.RelativeOrAbsolute),
                    null,
                    /*
                    new SongMetadata()
                    {
                        ArtistName = "ArtistName",
                        AlbumArtistName = "AlbumArtistName",
                        Name = "SongName",
                        AlbumName = "AlbumName",
                        Duration = TimeSpan.FromSeconds(10),
                        TrackNumber = 1,
                        AlbumReleaseDate = DateTime.Now,
                        GenreName = "Genre"
                    }, 
                    */
                    SaveSongOperation.CopyToLibrary);
            }
        }

        /// <summary>
        /// Delete the audio file.
        /// </summary>
        /// <param name="sender">Delete menu item</param>
        /// <param name="e">Event arguments</param>
        private void Delete_Click(object sender, EventArgs e)
        {
            AudioFileModel selected = (AudioFileModel)AudioFilesList.SelectedItem;
            if (selected != null)
            {
                IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();
                try
                {
                    myStore.DeleteFile(selected.FileName);
                    App.AudioModel.AudioFiles.Remove(selected);
                    selectedFileName = "";
                }
                catch
                {
                    MessageBox.Show("Error while deleting audio file " + selected.FileName + ".");
                }

                StopPlayButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                ForwardButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Skip next 5 secs of the audio playback.
        /// </summary>
        /// <param name="sender">Stop menu item</param>
        /// <param name="e">Event arguments</param>
        private void Forward_Click(object sender, EventArgs e)
        {
            App.AudioManager.SkipFiveSecs();
        }

        private ApplicationBarIconButton StopPlayButton;
        private ApplicationBarIconButton SaveButton;
        private ApplicationBarIconButton DeleteButton;
        private ApplicationBarIconButton ForwardButton;

        /// <summary>
        /// Create the application bar with audio controls.
        /// </summary>
        private void CreateAppBar()
        {
            ApplicationBar appBar = new ApplicationBar();
            
            StopPlayButton = new ApplicationBarIconButton(new Uri("Assets/Play.png", UriKind.Relative));
            StopPlayButton.Click += new EventHandler(StopPlay_Click);
            StopPlayButton.Text = "play";
            StopPlayButton.IsEnabled = false;
            appBar.Buttons.Add(StopPlayButton);

            SaveButton = new ApplicationBarIconButton(new Uri("Assets/Save.png", UriKind.Relative));
            SaveButton.Click += new EventHandler(Save_Click);
            SaveButton.Text = "stop";
            SaveButton.IsEnabled = false;
            appBar.Buttons.Add(SaveButton);

            DeleteButton = new ApplicationBarIconButton(new Uri("Assets/Delete.png", UriKind.Relative));
            DeleteButton.Click += new EventHandler(Delete_Click);
            DeleteButton.Text = "delete";
            DeleteButton.IsEnabled = false;
            appBar.Buttons.Add(DeleteButton);

            ForwardButton = new ApplicationBarIconButton(new Uri("Assets/Forward.png", UriKind.Relative));
            ForwardButton.Click += new EventHandler(Forward_Click);
            ForwardButton.Text = "forward";
            ForwardButton.IsEnabled = false;
            appBar.Buttons.Add(ForwardButton);

            ApplicationBar = appBar;
        }
    }
}