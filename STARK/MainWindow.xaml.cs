using MahApps.Metro;
using MahApps.Metro.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STARK {
    public partial class MainWindow : MetroWindow, IDisposable {

        //VARIABLES
        #region "variables"
		//Mixers
		MixingSampleProvider mspStandard;
		MixingSampleProvider mspLoopback;

		//Inputs & Outputs
		WaveIn wiMicrophone;
		WaveOut woStandard; //Synth + Microphone
		WaveOut woLoopback; //Synth

        //Formats
        WaveFormat audioFormat = WaveFormat.CreateIeeeFloatWaveFormat(22050, 1);

		//Volume Controllers & Buffers
		VolumeSampleProvider vspMicrophone;
		BufferedWaveProvider bwpMicrophone;

        //Engines
        public QueuedSpeechSynthesizer qss;
        public AudioFileManager afm;
        AudioPlaybackEngine ape; //Rip Harambe
        CommandReader cmdReader;
        ConsoleUI conUI;

        int combinedOutputSelectedIndex;
        int loopbackOutputSelectedIndex;
        int inputSelectedIndex;

        public static bool whitelistedOnlyTTS;
        public static bool whitelistedOnlyPlayCmd = true;
        public static bool whitelistedOnlyPauseCmd = true;
        public static bool whitelistedOnlyResumeCmd = true;
        public static bool whitelistedOnlyStopCmd = true;
        public static bool whitelistedOnlySkipCurrentCmd = true;
        public static bool whitelistedOnlyClearQueueCmd = true;
        public static bool whitelistedOnlyBlockUserCmd = true;
        public static bool whitelistedOnlyBlockWordCmd = true;

        MainWindow mw;

        string currentSettingsVersion = "1.0.0";

        bool loaded = false;

        //Timer steamAppsLoop; BURN IN HELL YOU FUCKING CUNT
        #endregion

        public MainWindow() {
			InitializeComponent();

            //DEBUG CODE
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;


            //
			populateAccentComboBox();
            CommandManager.synthCmd.changeCommand(TTS_CommandTextBox.Text);
            mw = this;

			InitializeMixer(ref mspStandard);
			InitializeMixer(ref mspLoopback);

            //Startup fam
			qss = new QueuedSpeechSynthesizer(ref mspStandard, ref mspLoopback, 50, -1);
            afm = new AudioFileManager();
            ape = new AudioPlaybackEngine(ref audioFormat, ref mspStandard, ref mspLoopback, ref afm, 10);


			InitializeSynthQueue();
            InitializeVoiceComboBox();
			populateSetupComboBoxes();
            InitializeAudioItemList();

            loadSettings();

            //latency has to be >= 25
			InitializeMicrophone(ref wiMicrophone, Setup_Microphone.SelectedIndex, 25, 100);

            //latency has to be >= 100
            InitializeOutput(ref woStandard, Setup_OutputCombined.SelectedIndex, 25, ref mspStandard);
            InitializeOutput(ref woLoopback, Setup_SynthesizerOnly.SelectedIndex, 25, ref mspLoopback);

            changeMicrophone(Setup_Microphone.SelectedIndex);
            changeCombinedOutput(Setup_OutputCombined.SelectedIndex);
            changeLoopbackOutput(Setup_SynthesizerOnly.SelectedIndex);

            FindSteamApps();

            if (!File.Exists("blocked_users.txt"))
            {
                File.Create("blocked_users.txt");
            }

            if (!File.Exists("blocked_words.txt"))
            {
                File.Create("blocked_words.txt");
            }

            if (!File.Exists("whitelisted_users.txt"))
            {
                File.Create("whitelisted_users.txt");
            }

            if (!File.Exists("replace.txt"))
            {
                File.Create("replace.txt");
            }

            loaded = true;
        }

        //DEBUG
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args) {
            Exception e = (Exception)args.ExceptionObject;

            var message = new System.Text.StringBuilder();
            message.AppendLine("Handler Caught: " + e.Message);
            message.AppendLine("Stacktrace: " + e.StackTrace);
            if (e.InnerException != null) {
                message.AppendLine("InnerException" + e.InnerException.Message);
                message.AppendLine("IE ST: " + e.InnerException.StackTrace);
            }
            message.AppendLine("Terminating: " + args.IsTerminating);
            message.AppendLine("Target Site:" + e.TargetSite);
            MessageBox.Show(message.ToString());
        }

        private async void FindSteamApps() {
            if (string.IsNullOrWhiteSpace(PathManager.steamApps)) {
                //Finds all proccesses that have "Steam" in them
                Process[] procs = Process.GetProcessesByName("Steam");
                Process steam = null;

                string steamPath;
                if (procs.Length > 0) {
                    //Gets actual Steam process
                    steam = procs[0];

                    //Gets steam directory
                    steamPath = Path.GetDirectoryName(steam.MainModule.FileName);
                    PathManager.steamApps = steamPath + @"\SteamApps";
                    steamAppsFolderPath.Text = PathManager.steamApps;

                    //Gets the current game
                    SourceGame game = SourceGameManager.getByName(Setup_Game.Text);
                    Setup_steamAppsLabel.Content = "SteamApps Folder (Found)";

                    //Init things that need the steamapps folder
                    conUI = new ConsoleUI(game, ref afm, ref mw);
                    cmdReader = new CommandReader(ref qss, ref ape, ref afm, game);
                }
                else {
                    //keep looking for the steam process each 500ms
                    await Task.Delay(new TimeSpan(0, 0, 0, 0, 500));
                    FindSteamApps();
                }
            } else {
                //Gets the current game
                SourceGame game = SourceGameManager.getByName(Setup_Game.Text);
                Setup_steamAppsLabel.Content = "SteamApps Folder (Found)";

                //Init things that need the steamapps folder
                if (!Directory.Exists(PathManager.steamApps + PathManager.steamApps + @"\common\Team Fortress 2")) {
                    conUI = new ConsoleUI(game, ref afm, ref mw);
                    cmdReader = new CommandReader(ref qss, ref ape, ref afm, game);
                }
            }
        }

        public void saveSettings() {
            var settings = Properties.Settings.Default;

            settings.Synthesizer_Command = CommandManager.synthCmd.getCommand();
            settings.Synthesizer_Volume = (int) TTS_VolumeSlider.Value;
            settings.Synthesizer_Rate = (int) TTS_RateSlider.Value;

            settings.Microphone_Volume = (int) Audio_MicVolumeSlider.Value;
            settings.Microphone_Buffer_Time = (int) Audio_MicBufferSlider.Value;

            settings.Media_Volume = (int) Audio_AFFVolumeSlider.Value;
            settings.Standard_Volume = (int) Audio_OutputVolumeSlider.Value;
            settings.Loopback_Volume = (int) Audio_LoopbackVolumeSlider.Value;

            settings.Setup_Microphone_SelectedIndex = Setup_Microphone.SelectedIndex;
            settings.Setup_Loopback_Output_SelectedIndex = Setup_SynthesizerOnly.SelectedIndex;
            settings.Setup_Standard_Output_SelectedIndex = Setup_OutputCombined.SelectedIndex;

            settings.Setup_SteamApps_Folder = PathManager.steamApps;

            settings.Setup_Audio_Watch_Folder = PathManager.watchFolder;

            settings.Interface_Theme_Accent = AccentComboBox.SelectedIndex;

            settings.Save();
            settings.Reload();
        }

        public void loadSettings() {
            var settings = Properties.Settings.Default;
            settings.Upgrade();

            if (settings.SettingsVersion == currentSettingsVersion) {
                TTS_CommandTextBox.Text = settings.Synthesizer_Command;
                TTS_VolumeSliderBox.Text = "" + settings.Synthesizer_Volume;
                TTS_RateSliderBox.Text = "" + settings.Synthesizer_Rate;

                Audio_MicVolumeTextBox.Text = "" + settings.Microphone_Volume;
                Audio_MicBufferTextBox.Text = "" + settings.Microphone_Buffer_Time;

                Audio_AFFVolumeTextBox.Text = "" + settings.Media_Volume;
                Audio_OutputVolumeTextBox.Text = "" + settings.Standard_Volume;
                Audio_LoopbackVolumeTextBox.Text = "" + settings.Loopback_Volume;

                Setup_Microphone.SelectedIndex = settings.Setup_Microphone_SelectedIndex;
                Setup_SynthesizerOnly.SelectedIndex = settings.Setup_Loopback_Output_SelectedIndex;
                Setup_OutputCombined.SelectedIndex = settings.Setup_Standard_Output_SelectedIndex;

                PathManager.steamApps = settings.Setup_SteamApps_Folder;
                steamAppsFolderPath.Text = settings.Setup_SteamApps_Folder;

                PathManager.watchFolder = settings.Setup_Audio_Watch_Folder;
                watchFolderPath.Text = settings.Setup_Audio_Watch_Folder;
                afm.ChangeWatchFolder(PathManager.watchFolder);

                AccentComboBox.SelectedIndex = settings.Interface_Theme_Accent;
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(AccentComboBox.SelectedItem as String), ThemeManager.DetectAppStyle().Item1);

            }
        }

        public void RenderConUI() {
            if (conUI != null) {
                conUI.Render();
            }
        }

        //UTILS
        #region "utils"
        private float IntToFloat(int input) {
			return 0.01f * input;
		}

        public QueuedSpeechSynthesizer getQSS() {
            return qss;
        }

        public AudioFileManager getAFM() {
            return afm;
        }
        #endregion

        //EVENTS
        #region "events"
        private void WiMicrophone_DataAvailable(object sender, WaveInEventArgs e) {
            //Gives a buffer full exception if not outputting to anything.
            //The reason I don't enable buffer discarding is that the exception never gets thrown and usually something else is wrong. axynos(05.10.2016): nvm lol
            if (bwpMicrophone != null) {
				bwpMicrophone.AddSamples(e.Buffer, 0, e.BytesRecorded);
			}
		}

        private void STARK_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            saveSettings();
            if (conUI != null) {
                conUI.DeleteFiles();
            }
        }

        private void donateButton_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LB5YVGD9F8U5L"));
        }
        #endregion

        //CHANGE I/O
        #region "changeI/O"
        private bool changeMicrophone(int deviceID) {
            if (wiMicrophone == null) return false;
            mspStandard.RemoveMixerInput(vspMicrophone);
            wiMicrophone.StopRecording();
            wiMicrophone.Dispose();
            wiMicrophone = new WaveIn();
            wiMicrophone.DeviceNumber = deviceID;
            InitializeMicrophone(ref wiMicrophone, wiMicrophone.DeviceNumber, wiMicrophone.BufferMilliseconds, (int)Audio_MicVolumeSlider.Value);
            return true;
        }

        private bool changeCombinedOutput(int deviceID) {
            if (woStandard == null) return false;
            woStandard.Stop();
            woStandard.Dispose();
            woStandard = new WaveOut();
            woStandard.DeviceNumber = deviceID;
            woStandard.Init(mspStandard);
            woStandard.Play();
            return true;
        }

        private bool changeLoopbackOutput(int deviceID) {
            if (woLoopback == null) return false;
            woLoopback.Stop();
            woLoopback.Dispose();
            woLoopback = new WaveOut();
            woLoopback.DeviceNumber = deviceID;
            woLoopback.Init(mspLoopback);
            woLoopback.Play();
            return true;
        }

        private void changeMicrophoneVolume(int volume) {
			vspMicrophone.Volume = IntToFloat(volume);
		}

		private void changeMicrophoneBuffer(int value) {
			wiMicrophone.BufferMilliseconds = value;
		}

		private void changeLoopbackVolume(int value) {
			if (woLoopback != null) {
				woLoopback.Volume = IntToFloat(value);
			}
		}

        private void changeStandardVolume(int value) {
            if (woStandard != null) {
                woStandard.Volume = IntToFloat(value);
            }
        }
        #endregion

        //INITIALIZATION
        #region "initialization"
        private void InitializeMicrophone(ref WaveIn wi, int deviceID, int iBufMS, int volume) {
			
            //Gets rid of previous instance if there is one
			if (wi != null) wi.Dispose();
			wi = new WaveIn();

			wi.DeviceNumber = deviceID; //0 is default input device always
			wi.NumberOfBuffers = 4;
			wi.BufferMilliseconds = iBufMS;

			wi.WaveFormat = audioFormat;
			wi.DataAvailable += WiMicrophone_DataAvailable;

			if (bwpMicrophone != null && bwpMicrophone.BufferLength > 0) bwpMicrophone.ClearBuffer(); //Just in case i fuck up later. edit 03-09-2016 i have no idea why i wrote that lol
			bwpMicrophone = new BufferedWaveProvider(audioFormat);
            bwpMicrophone.DiscardOnBufferOverflow = true;
            vspMicrophone = new VolumeSampleProvider(bwpMicrophone.ToSampleProvider()); //Allows us to control the volume

			vspMicrophone.Volume = IntToFloat(volume);
			
			mspStandard.AddMixerInput(vspMicrophone);
			wi.StartRecording();
		}

		private void InitializeMixer(ref MixingSampleProvider msp) {
			msp = new MixingSampleProvider(audioFormat);
			msp.ReadFully = true; //makes it an infinite provider
		}

        private void InitializeOutput(ref WaveOut wo, int id, int iLat, ref MixingSampleProvider source) {

            //Gets rid of previous instance, if there is one
            if (wo != null) {
                wo.Stop();
                wo.Dispose();
            }
            wo = new WaveOut();

            wo.DeviceNumber = id; //0 is default audio device
            wo.DesiredLatency = iLat;
            wo.Init(source);
            wo.Play();
        }

        private void InitializeSynthQueue() {
			SynthQueue.ItemsSource = qss.getQueue();
			SynthQueue.SelectedIndex = -1; //Prevents the first text from getting selected automatically
		}

        private void InitializeAudioItemList() {
            afm.LoadCurrentFiles();
            AudioItemList.ItemsSource = afm.getCollection();
            AudioItemList.SelectedIndex = -1;
        }

		private void InitializeVoiceComboBox() {
			if (TTS_VoiceComboBox.Items.Count > 0) TTS_VoiceComboBox.Items.Clear();
			foreach (InstalledVoice voice in qss.GetVoices()) {
				TTS_VoiceComboBox.Items.Add(voice.VoiceInfo.Name);
				if (voice.VoiceInfo == qss.getVoice()) {
					TTS_VoiceComboBox.SelectedValue = voice.VoiceInfo.Name;
				}
			}
		}

		private void populateAccentComboBox() {
			if (AccentComboBox.HasItems) AccentComboBox.Items.Clear();
			foreach (Accent accent in ThemeManager.Accents) {
				AccentComboBox.Items.Add(accent.Name);
				if (accent.Name == ThemeManager.DetectAppStyle().Item2.Name) {
					AccentComboBox.SelectedItem = accent.Name;
				}
			}
		}

        private void populateSetupComboBoxes() {
            //Clear it if it is not empty like on a refresh
            if (Setup_Microphone.Items.Count > 0) Setup_Microphone.Items.Clear();
            if (Setup_OutputCombined.Items.Count > 0) Setup_OutputCombined.Items.Clear();
            if (Setup_SynthesizerOnly.Items.Count > 0) Setup_SynthesizerOnly.Items.Clear();
            
            //Microphone
            for (int dID = 0; dID < WaveIn.DeviceCount; dID++) {
				WaveInCapabilities wiCap = WaveIn.GetCapabilities(dID);
				Setup_Microphone.Items.Add(wiCap.ProductName);
			}

            //Combined Output
            for (int dID = 0; dID < WaveOut.DeviceCount; dID++) {
                WaveOutCapabilities woCap = WaveOut.GetCapabilities(dID);
                Setup_OutputCombined.Items.Add(woCap.ProductName);
            }

            //Synth Only Output
            for (int dID = 0; dID < WaveOut.DeviceCount; dID++) {
                WaveOutCapabilities woCap = WaveOut.GetCapabilities(dID);
                Setup_SynthesizerOnly.Items.Add(woCap.ProductName);
            }

            //Game Selection
            Setup_Game.ItemsSource = SourceGameManager.gamesList;
            Setup_Game.SelectedIndex = 0;
            SourceGameManager.selectedGame = Setup_Game.SelectedItem as SourceGame;

            //Set selected
            Setup_Microphone.SelectedItem = WaveIn.GetCapabilities(0).ProductName;
            Setup_OutputCombined.SelectedItem = WaveOut.GetCapabilities(0).ProductName;
            if (WaveOut.DeviceCount > 1) Setup_SynthesizerOnly.SelectedItem = WaveOut.GetCapabilities(1).ProductName;

            combinedOutputSelectedIndex = Setup_OutputCombined.SelectedIndex;
            loopbackOutputSelectedIndex = Setup_SynthesizerOnly.SelectedIndex;
            inputSelectedIndex = Setup_Microphone.SelectedIndex;

        }
        #endregion

        //SYNTHESIZER PANEL EVENTS
        #region "synthpanelevents"
            #region "mediaButtons"
                private void TTS_Pause_Click(object sender, RoutedEventArgs e) {
			        switch(TTS_Pause.Content.ToString()) {
				        case "Pause":
					        qss.PauseSpeaking();
					        TTS_Pause.Content = "Resume";
					        break;
				        case "Resume":
					        qss.ResumeSpeaking();
					        TTS_Pause.Content = "Pause";
					        break;
			        }
		        }

		        private void TTS_Skip_Click(object sender, RoutedEventArgs e) {
			        qss.SkipCurrent();
		        }

		        private void TTS_Clear_Click(object sender, RoutedEventArgs e) {
                    qss.Clear();
		        }

                private void TTS_Delete_Click(object sender, RoutedEventArgs e) {
                    for (int i = 0; i < SynthQueue.SelectedItems.Count; i++) {
                        qss.Remove((QSSQueueItem)SynthQueue.SelectedItems[i]);
                    }
                }

                private void TTS_Add_Click(object sender, RoutedEventArgs e) {
                    if (TTS_AddTextBox.Text.Length > 0) {
                        qss.AddToQueue(new QSSQueueItem(TTS_AddTextBox.Text, "STARK"));
                        TTS_AddTextBox.Text = "";
                    }
                }

                //Use enter to add to QSS
                private void TTS_AddTextBox_KeyUp(object sender, KeyEventArgs e) {
                    if (e.Key != System.Windows.Input.Key.Enter) return;

                    if (TTS_AddTextBox.Text.Length > 0) {
                        qss.AddToQueue(new QSSQueueItem(TTS_AddTextBox.Text, "STARK"));
                        TTS_AddTextBox.Text = "";
                    }
                    e.Handled = true;
                }
            #endregion

            private void synthesizerToggle_Checked(object sender, RoutedEventArgs e) {
            if (loaded) {
                if ((sender as CheckBox).IsChecked ?? true) {
                    qss.Enable();
                } else if ((sender as CheckBox).IsChecked == false) {
                    qss.Disable();
                }
            }
        }

        private void whitelistToggleTTS_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyTTS = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyTTS = false;
                }
            }
        }

        private void whitelistTogglePlayCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyPlayCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyPlayCmd = false;
                }
            }
        }

        private void whitelistTogglePauseCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyPauseCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyPauseCmd = false;
                }
            }
        }

        private void whitelistToggleResumeCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyResumeCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyResumeCmd = false;
                }
            }
        }

        private void whitelistToggleStopCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyStopCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyStopCmd = false;
                }
            }
        }

        private void whitelistToggleSkipCurrentCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlySkipCurrentCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlySkipCurrentCmd = false;
                }
            }
        }

        private void whitelistToggleClearQueueCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyClearQueueCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyClearQueueCmd = false;
                }
            }
        }

        private void whitelistToggleBlockUserCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyBlockUserCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyBlockUserCmd = false;
                }
            }
        }

        private void whitelistToggleBlockWordCmd_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                if ((sender as CheckBox).IsChecked ?? true)
                {
                    whitelistedOnlyBlockWordCmd = true;
                }
                else if ((sender as CheckBox).IsChecked == false)
                {
                    whitelistedOnlyBlockWordCmd = false;
                }
            }
        }

        private void TTS_CommandTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (loaded) {
                CommandManager.synthCmd.changeCommand((sender as TextBox).Text);
                //saveSettings();
                if (conUI != null) conUI.Render();
            }
        }

            private void TTS_VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			    if (loaded) {
				    (sender as Slider).Value = Math.Round(e.NewValue, 0);
				    if (qss != null) qss.ChangeVolume((int)(sender as Slider).Value);
			    }
		    }

		    private void TTS_RateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			    if (loaded) {
				    (sender as Slider).Value = Math.Round(e.NewValue, 0);
				    if (qss != null) qss.ChangeRate((int)(sender as Slider).Value);
			    }
		    }

            private void TTS_VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
                if (loaded) qss.ChangeVoice(e.AddedItems[0] as String);
            }
        #endregion

        //AUDIO PANEL EVENTS
        #region "audiopanelevents"
        private void Audio_MicVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (loaded) {
				(sender as Slider).Value = Math.Round(e.NewValue, 0);
				changeMicrophoneVolume((int)(sender as Slider).Value);
			}
		}

		private void Audio_MicBufferSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (loaded) {
				(sender as Slider).Value = Math.Round(e.NewValue, 0);
				changeMicrophoneBuffer((int)(sender as Slider).Value);
			}
		}

		private void Audio_LoopbackVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (loaded) {
				(sender as Slider).Value = Math.Round(e.NewValue, 0);
				changeLoopbackVolume((int)(sender as Slider).Value);
			}
		}

		private void Audio_AFFVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                (sender as Slider).Value = Math.Round(e.NewValue, 0);
                ape.ChangeVolume((int)(sender as Slider).Value);
            }
		}

        private void Audio_OutputVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                (sender as Slider).Value = Math.Round(e.NewValue, 0);
                changeStandardVolume((int)(sender as Slider).Value);
            }
        }

        private void Audio_MicCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (loaded) {
                if ((sender as CheckBox).IsChecked == false) {
                    if (wiMicrophone != null) {
                        mspStandard.RemoveMixerInput(vspMicrophone);
                        wiMicrophone.StopRecording();
                    }
                } else if ((sender as CheckBox).IsChecked ?? true) {
                    if (wiMicrophone != null) {
                        mspStandard.AddMixerInput(vspMicrophone);
                        wiMicrophone.StartRecording();
                    }
                }
            }
        }
        #endregion

        //INTERFACE PANEL EVENTS
        #region "interfacepanelevents"
        private void AccentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded) {
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent((sender as ComboBox).SelectedItem as String), ThemeManager.DetectAppStyle().Item1);
			}
		}
        #endregion

        //SETUP PANEL EVENTS
        #region "setuppanelevents"
        private void Setup_Microphone_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (loaded) {
                for (int i = 0; i < WaveIn.DeviceCount; i++) {
                    if (WaveIn.GetCapabilities(i).ProductName == (e.AddedItems[0] as String)) {
                        if (changeMicrophone(i)) {
                            inputSelectedIndex = (sender as ComboBox).SelectedIndex;
                            e.Handled = true;
                        } else {
                            (sender as ComboBox).SelectedIndex = inputSelectedIndex;
                        }
                        break;
                    }
                }
            }
        }

        private void Setup_OutputCombined_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (loaded) {
                for (int i = 0; i < WaveOut.DeviceCount; i++) {
                    if (WaveOut.GetCapabilities(i).ProductName == e.AddedItems[0] as String) {
                        if (changeCombinedOutput(i)) {
                            combinedOutputSelectedIndex = (sender as ComboBox).SelectedIndex;
                            e.Handled = true;
                        }
                        else {
                            (sender as ComboBox).SelectedIndex = combinedOutputSelectedIndex;
                        }
                        break;
                    }
                }
            }
        }

        private void Setup_SynthesizerOnly_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (loaded) {
                for (int i = 0; i < WaveOut.DeviceCount; i++) {
                    if (WaveOut.GetCapabilities(i).ProductName == e.AddedItems[0] as String) {
                        if (changeLoopbackOutput(i)) {
                            loopbackOutputSelectedIndex = (sender as ComboBox).SelectedIndex;
                            e.Handled = true;
                        } else {
                            (sender as ComboBox).SelectedIndex = loopbackOutputSelectedIndex;
                        }
                        break;
                    }
                }
            }
        }

        private void steamAppsFolderSelectButton_Click(object sender, EventArgs e) {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog()) {
                fbd.ShowDialog();
                //true if clicks OK, false otherwise
                if (DialogResult ?? true) {
                    if (!string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                        PathManager.steamApps = fbd.SelectedPath;
                        steamAppsFolderPath.Text = fbd.SelectedPath;

                        conUI = null;
                        conUI = new ConsoleUI(SourceGameManager.selectedGame, ref afm, ref mw);

                        cmdReader = null;
                        cmdReader = new CommandReader(ref qss, ref ape, ref afm, SourceGameManager.selectedGame);
                    }
                }
            }
        }

        private void watchFolderSelectButton_Click(object sender, RoutedEventArgs e) {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog()) {
                fbd.ShowDialog();

                if (DialogResult ?? true) {
                    if (!string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                        PathManager.watchFolder = fbd.SelectedPath;
                        watchFolderPath.Text = fbd.SelectedPath;
                        afm.ChangeWatchFolder(PathManager.watchFolder);
                    }
                }
            }
        }

        private void Setup_Game_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (loaded) {
                var game = (e.AddedItems[0] as SourceGame);
                SourceGameManager.ChangeSelectedGame(game);
                cmdReader.ChangeSelectedGame(game);
                if (conUI != null) {
                    conUI.Render();
                }
            }
        }

        private void Setup_GameLaunch_Click(object sender, RoutedEventArgs e) {
            if (loaded) {
                SourceGameManager.selectedGame.launchGame();
            }
        }
        #endregion

        //Dispose
        #region "dispose"
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (qss != null) {
                qss.Dispose();
                qss = null;
            }
            if (afm != null) {
                afm.Dispose();
                afm = null;
            }
            if (ape != null) {
                ape.Dispose(); //rip Harambe
                ape = null;
            }
            if (wiMicrophone != null) {
                wiMicrophone.StopRecording();
                wiMicrophone.Dispose();
                wiMicrophone = null;
            }
            if (woLoopback != null) {
                woLoopback.Stop();
                woLoopback.Dispose();
                woLoopback = null;
            }
            if (woStandard != null) {
                woStandard.Stop();
                woStandard.Dispose();
                woStandard = null;
            }

        }
        #endregion

        public void setConUItoNull() {
            conUI = null;
        }

        public void setCmdReadertoNull() {
            cmdReader = null;
        }
    }
}
