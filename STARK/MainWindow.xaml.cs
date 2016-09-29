using MahApps.Metro;
using MahApps.Metro.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STARK {
    public partial class MainWindow : MetroWindow {

        //VARIABLES
        #region "variables"
        QueuedSpeechSynthesizer qss;

		//Formats
		WaveFormat audioFormat = WaveFormat.CreateIeeeFloatWaveFormat(22050, 1);

		//Mixers
		MixingSampleProvider mspStandard;
		MixingSampleProvider mspLoopback;

		//Volume Controllers
		VolumeSampleProvider vspMicrophone;

		//Buffers
		BufferedWaveProvider bwpMicrophone;

		//Inputs & Outputs
		WaveIn wiMicrophone;
		bool wiMicrophoneRecording = false;
		WaveOut woStandard; //Synth + Microphone
		WaveOut woLoopback; //Synth

        AudioPlaybackEngine ape; //Rip Harambe

        int combinedOutputSelectedIndex;
        int loopbackOutputSelectedIndex;
        int inputSelectedIndex;

        bool loaded = false;
        #endregion

        public MainWindow() {
			InitializeComponent();
			populateAccentComboBox();

			InitializeMixer(ref mspStandard);
			InitializeMixer(ref mspLoopback);

			qss = new QueuedSpeechSynthesizer(ref mspStandard, ref mspLoopback, 50, -1);

			InitializeSynthQueue();

			InitializeMicrophone(ref wiMicrophone, 0, 25, 100);

            InitializeOutput(ref woStandard, 1, 100, ref mspStandard);
            InitializeOutput(ref woLoopback, 0, 100, ref mspLoopback);


            InitializeVoiceComboBox();
			populateSetupComboBoxes();

            //Startup fam
			//qss.AddToQueue(new QSSQueueItem("Project STARK", ""));
            ape = new AudioPlaybackEngine(ref audioFormat, ref mspStandard, ref mspLoopback);
            ape.PlayAudioFile("C:\\Users\\axynos\\Desktop\\sick.mp3", 1);

			loaded = true;
		}

        
        //UTILS
        #region "utils"
        private float IntToFloat(int input) {
			return 0.01f * input;
		}
        #endregion

        //EVENTS
        #region "events"
        private void WiMicrophone_DataAvailable(object sender, WaveInEventArgs e) {
            //Gives a buffer full exception if not outputting to anything.
            //The reason I don't enable buffer discarding is that the exception never gets thrown and usually something else is wrong.
            if (bwpMicrophone != null) {
				bwpMicrophone.AddSamples(e.Buffer, 0, e.BytesRecorded);
			}
		}
        #endregion

        //CHANGE I/O
        #region "changeI/O"
        private bool changeMicrophone(int deviceID) {
            if (wiMicrophone == null) return false;
            wiMicrophone.DeviceNumber = deviceID;
            return true;
        }

        private bool changeCombinedOutput(int deviceID) {
            if (woStandard == null) return false;
            woStandard.DeviceNumber = deviceID;
            return true;
        }

        private bool changeLoopbackOutput(int deviceID) {
            if (woLoopback == null) return false;
            woLoopback.DeviceNumber = deviceID;
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

		//private void changeAudioFromFileVolume(int value) {
		//	if (loaded && vspAudioFromFile != null) {
		//		vspAudioFromFile.Volume = IntToFloat(value);
		//	}
		//}
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
			wiMicrophoneRecording = true;
		}

		private void InitializeMixer(ref MixingSampleProvider msp) {
			msp = new MixingSampleProvider(audioFormat);
			msp.ReadFully = true; //makes it an infinite provider
		}

        private void InitializeOutput(ref WaveOut wo, int id, int iLat, ref MixingSampleProvider source) {

            //Gets rid of previous instance, if there is one
            if (wo != null) wo.Dispose();
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

        private void TTS_VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (loaded) qss.ChangeVoice(e.AddedItems[0] as String);
        }

        private void TTS_AddTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            if (TTS_AddTextBox.Text.Length > 0) {
                qss.AddToQueue(new QSSQueueItem(TTS_AddTextBox.Text, "STARK"));
                TTS_AddTextBox.Text = "";
            }
            e.Handled = true;
        }

		private void TTS_RateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (loaded) {
				(sender as Slider).Value = Math.Round(e.NewValue, 0);
				if (qss != null) qss.ChangeRate((int)(sender as Slider).Value);
			}
		}

		private void TTS_VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (loaded) {
				(sender as Slider).Value = Math.Round(e.NewValue, 0);
				if (qss != null) qss.ChangeVolume((int)(sender as Slider).Value);
			}
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
			//if (loaded) {
			//	(sender as Slider).Value = Math.Round(e.NewValue, 0);
			//	changeAudioFromFileVolume((int)(sender as Slider).Value);
			//}
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
        #endregion
    }
}
