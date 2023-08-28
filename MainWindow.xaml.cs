using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace WpfTutorialSamples.Audio_and_Video
{
	public partial class AudioVideoPlayerCompleteSample : Window
	{
		private bool mediaPlayerIsPlaying = false;
		private bool userIsDraggingSlider = false;
		private bool loopMedia = false;
		private int speedRatio = 1;
		List<string> CutList = new List<string>();
		List<string> RestartList = new List<string>();
		List<string> Playlist = new List<string>();
		private int currentVideo = 0;

		public AudioVideoPlayerCompleteSample()
		{
			InitializeComponent();
			mePlayer.MediaEnded += new RoutedEventHandler(Media_Ended);

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(.1);
			timer.Tick += timer_Tick;
			timer.Start();
		}

        private void SkipBlockedTimes()
        {
            //If the mediaElement is between blocked times, skip past it. Uses mePlayer????
            for (int i = 0; i < CutList.Count; i++)
            {
                double cut = Convert.ToDouble(CutList[i]);
                double restart = Convert.ToDouble(RestartList[i]);
                if (mePlayer.Position.TotalSeconds >= cut && mePlayer.Position.TotalSeconds < restart)
                {
					mePlayer.Position = TimeSpan.FromSeconds(restart);
                }

            }
        }

        private void Block_Button_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{

				using (var reader = new StreamReader(openFileDialog.FileName))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						char[] delimiters = { ' ', '\t' };
						string[] words = line.Split(delimiters);
						CutList.Add(words[0]);
						RestartList.Add(words[1]);
						timesCombo.Items.Add(words[0] + " - " + words[1]);
					}
				}

			}
		}

		private void Mute_Button_Click(object sender, RoutedEventArgs e)
		{

			BitmapImage muteIcon = new BitmapImage();

			muteIcon.BeginInit();

			if (mePlayer.IsMuted == true)
            {
				mePlayer.IsMuted = false;
				muteIcon.UriSource = new Uri("Resources/Images/mute_button.png", UriKind.Relative);
				muteButton.Source = muteIcon;
				muteButtonContainer.ToolTip = "Mute";

			}
			else
            {
				mePlayer.IsMuted = true;
				muteIcon.UriSource = new Uri("Resources/Images/unmute_button.png", UriKind.Relative);
				muteButton.Source = muteIcon;
				muteButtonContainer.ToolTip = "Unmute";

			}
			muteIcon.EndInit();
		}

		private void Loop_Button_Click(object sender, RoutedEventArgs e)
		{
			BitmapImage loopIcon = new BitmapImage();

			loopIcon.BeginInit();

			if (loopMedia)
            {
				loopMedia = false;
				loopIcon.UriSource = new Uri("Resources/Images/loop_button.png", UriKind.Relative);
				loopButtonImage.Source = loopIcon;
				loopButton.ToolTip = "Loop";
			} else
            {
				loopMedia = true;
				loopIcon.UriSource = new Uri("Resources/Images/dont_loop_button.png", UriKind.Relative);
				loopButtonImage.Source = loopIcon;
				loopButton.ToolTip = "Dont Loop";
			}
			loopIcon.EndInit();
		}

		private void Fast_Forward_Button_Click(object sender, RoutedEventArgs e)
		{
			BitmapImage ffIcon = new BitmapImage();

			ffIcon.BeginInit();

			if (speedRatio == 1)
			{
				speedRatio = 2;
				mePlayer.SpeedRatio = speedRatio;
				ffIcon.UriSource = new Uri("Resources/Images/fast_forward_2.png", UriKind.Relative);
				ffButtonImage.Source = ffIcon;
			}
			else if(speedRatio == 2)
			{
				speedRatio = 4;
				mePlayer.SpeedRatio = speedRatio;
				ffIcon.UriSource = new Uri("Resources/Images/fast_forward_4.png", UriKind.Relative);
				ffButtonImage.Source = ffIcon;
			}
			else if (speedRatio == 4)
			{
				speedRatio = 1;
				mePlayer.SpeedRatio = speedRatio;
				ffIcon.UriSource = new Uri("Resources/Images/fast_forward.png", UriKind.Relative);
				ffButtonImage.Source = ffIcon;
			}
			ffIcon.EndInit();
		}

		private void Playlist_Button_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.wmv)|*.mp3;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
            {
				Playlist.Add(openFileDialog.FileName);
				playlistCombo.Items.Add(openFileDialog.SafeFileName);
			}
		}

		private void playlistCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			int selection = playlistCombo.SelectedIndex;
			currentVideo = selection;
			mePlayer.Source = new Uri(Playlist[currentVideo]);
			mePlayer.Play();
			mediaPlayerIsPlaying = true;
		}

		private void Media_Ended(object sender, EventArgs e)
		{
			mePlayer.Stop();
			mediaPlayerIsPlaying = false;
			if (loopMedia)
            {
				mePlayer.Position = TimeSpan.FromMilliseconds(1);
				mePlayer.Play();
			}
            if (Playlist.Count > 0 && !loopMedia)
            {
                if ((currentVideo + 1) < Playlist.Count)
                {
					//Empty cut and restart list since they should be more video specific
					//and we are now changing videos.
					CutList = new List<string>();
					RestartList = new List<string>();
					timesCombo.Items.Clear();


					currentVideo += 1;
					mePlayer.Source = new Uri(Playlist[currentVideo]);
					playlistCombo.SelectedItem = playlistCombo.Items[currentVideo];

					mePlayer.Play();
					mediaPlayerIsPlaying = true;

				}

			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			SkipBlockedTimes();
			if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
			{
				sliProgress.Minimum = 0;
				sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
				sliProgress.Value = mePlayer.Position.TotalSeconds;
			}
		}

		private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.wmv)|*.mp3;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
				mePlayer.Source = new Uri(openFileDialog.FileName);

			//Added these so you can tell what you tried to open actually opened.
			mePlayer.Play();
			mediaPlayerIsPlaying = true;
			mePlayer.Pause();

		}

		private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
		}

		private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mePlayer.Play();
			mediaPlayerIsPlaying = true;
		}

		private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = mediaPlayerIsPlaying;
		}

		private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mePlayer.Pause();
		}

		private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = mediaPlayerIsPlaying;
		}

		private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mePlayer.Stop();
			mediaPlayerIsPlaying = false;
		}

		private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
		{
			userIsDraggingSlider = true;
		}

		private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			userIsDraggingSlider = false;
			mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
		}

		private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
		}

		private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
		}

        
    }
}