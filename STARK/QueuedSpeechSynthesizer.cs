using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace STARK {
    public class QueuedSpeechSynthesizer : IDisposable {

        //VARIABLES
        #region "variables"
        SpeechAudioFormatInfo synthesizerAudioFormat;
		SpeechSynthesizer synthesizer;

		MixingSampleProvider mspStandard;
		MixingSampleProvider mspLoopback;

        //There needs to be a separate one for each device unfortunately
        VolumeSampleProvider vsp;
        VolumeSampleProvider vsp2;

		Timer speakLoop;

        //Use a observable collection whenever you want to bind a changing list to a ui element, saves you a lot of trouble. #devtips lol
		ObservableCollection<QSSQueueItem> queue;

		//This needs to be reassigned whenever the current token gets used up
		System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();

		int volume;
		int rate;

        bool enabled = true;
		bool currentlySpeaking = false;
		bool paused = false;
		bool playingFromPausedState = false;
		bool overridePausedState = false;
        bool stopLoop = false;
		TimeSpan savedTime = TimeSpan.Zero;
        #endregion

        public QueuedSpeechSynthesizer(ref MixingSampleProvider mspStandard, ref MixingSampleProvider mspLoopback, int volume, int rate) {
			synthesizer = new SpeechSynthesizer();

            this.volume = volume;
			this.rate = rate;

			queue = new ObservableCollection<QSSQueueItem>();

			this.mspStandard = mspStandard;
			this.mspLoopback = mspLoopback;

			synthesizerAudioFormat = new SpeechAudioFormatInfo(22050, AudioBitsPerSample.Sixteen, AudioChannel.Mono);

			startSpeakLoop();
		}

        private async void Speak(System.Threading.CancellationToken token) {
			if (currentlySpeaking == false) {
				currentlySpeaking = true;

                String prompt = queue.First().QIText;

                Stream rawStream = new MemoryStream();
                Stream tempStream1 = new MemoryStream();
                Stream tempStream2 = new MemoryStream();


                synthesizer.SetOutputToWaveStream(rawStream);
				synthesizer.Rate = rate;

                synthesizer.Speak(prompt);

				rawStream.Seek(0, SeekOrigin.Begin);
                WriteToStream(tempStream1, new WaveFileReader(rawStream));
                rawStream.Seek(0, SeekOrigin.Begin);
                WriteToStream(tempStream2, new WaveFileReader(rawStream));

                tempStream1.Seek(0, SeekOrigin.Begin);
                tempStream2.Seek(0, SeekOrigin.Begin);
                

                WaveFileReader reader = new WaveFileReader(tempStream1);
				reader.CurrentTime = savedTime;

                WaveFileReader reader2 = new WaveFileReader(tempStream2);
                reader2.CurrentTime = savedTime;
                
                if (volume < 0) volume = 0;
                vsp = new VolumeSampleProvider(reader.ToSampleProvider());
                vsp2 = new VolumeSampleProvider(reader2.ToSampleProvider());
                vsp.Volume = this.IntToFloat(volume);
                vsp2.Volume = this.IntToFloat(volume);

                mspStandard.AddMixerInput(vsp);
				mspLoopback.AddMixerInput(vsp2);

                    try {
					await Task.Delay(reader.TotalTime, token); //Wait for current playback to finish
				}
#pragma warning disable CS0168 // Variable is declared but never used
				catch (TaskCanceledException e) {
#pragma warning restore CS0168 // Variable is declared but never used
					//We cancelled the task, so remove inputs to stop playback
					mspStandard.RemoveMixerInput(vsp);
					mspLoopback.RemoveMixerInput(vsp2);
					
					if (paused) savedTime = reader.CurrentTime;
				}

				//Allow next item to be played
				if (!paused && queue.Count > 0) removeFirst();
				playingFromPausedState = false;
				currentlySpeaking = false;
			}
		}

        //SPEAKING LOOP
        #region "loop"
        private void startSpeakLoop() {
			speakLoop = new System.Timers.Timer(10);
			speakLoop.Elapsed += Loop_Elapsed;
			speakLoop.AutoReset = false;
			speakLoop.Start();
		}

		private void Loop_Elapsed(object sender, ElapsedEventArgs e) {
			if (!paused) {
				if(!currentlySpeaking) {
					if (queue.Count > 0) {
						//Don't remove this line, it's needed to fix a playback time bug when spamming pause/resume button
						if (!playingFromPausedState) savedTime = TimeSpan.Zero;
						if (overridePausedState) {
							savedTime = TimeSpan.Zero;
							overridePausedState = false;
						}
						Speak(tokenSource.Token);
					}
				}
			}

            if (stopLoop) speakLoop.Dispose(); else speakLoop.Start();
		}
        #endregion

        //ACTIONS
        #region "actions"
        public void AddToQueue(QSSQueueItem item) {
            if (enabled) {
                Application.Current.Dispatcher.Invoke(delegate {
                    queue.Add(item);
                });
            }
		}

		private void removeFirst() {
            if (enabled) {
                Application.Current.Dispatcher.Invoke(delegate {
                    queue.RemoveAt(0);
                });
            }
		}

		public void PauseSpeaking() {
            if (enabled) {
			    paused = true;
			    tokenSource.Cancel();
			    tokenSource.Dispose();
			    tokenSource = new System.Threading.CancellationTokenSource();
            }
		}

		public void ResumeSpeaking() {
            if (enabled) {
			    playingFromPausedState = true;
			    paused = false;
			    tokenSource.Dispose();
			    tokenSource = new System.Threading.CancellationTokenSource();
            }
		}

		public void SkipCurrent() {
            if (enabled) {
			    if (paused) {
				    removeFirst();
				    overridePausedState = true;
			    } else {
				    tokenSource.Cancel();
			    }
			    tokenSource.Dispose();
			    tokenSource = new System.Threading.CancellationTokenSource();
            }
		}

        public void Remove(QSSQueueItem item) {
            Application.Current.Dispatcher.Invoke(delegate {
                queue.Remove(item);
            });
        }

        internal void Clear() {
            Application.Current.Dispatcher.Invoke(delegate {
                queue.Clear();
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = new System.Threading.CancellationTokenSource();
            });
        }

        public void Disable() {
            PauseSpeaking();
            Clear();
            enabled = false;
        }

        public void Enable() {
            enabled = true;
            savedTime = TimeSpan.Zero;
            ResumeSpeaking();
        }

        public void ToggleState() {
            if (enabled) Disable();
            else Enable();
        }
        #endregion

        //UTILS
        #region "utils"
        private float IntToFloat(int input) {
            return 0.01f * input;
        }

        /// <summary>
        /// Writes to a stream by reading all the data from a WaveProvider
        /// BEWARE: the WaveProvider MUST return 0 from its Read method when it is finished,
        /// or the Wave File will grow indefinitely.
        /// </summary>
        /// <param name="outStream">The stream the method will output to</param>
        /// <param name="sourceProvider">The source WaveProvider</param>
        private static void WriteToStream(Stream outStream, IWaveProvider sourceProvider) {
            using (var writer = new WaveFileWriter(new IgnoreDisposeStream(outStream), sourceProvider.WaveFormat)) {
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                while (true) {
                    int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) {
                        // end of source provider
                        outStream.Flush();
                        break;
                    }

                    writer.Write(buffer, 0, bytesRead);
                }
            }
        }
        #endregion

        //GET METHODS
        #region "getmethods"
        public IReadOnlyCollection<InstalledVoice> GetVoices() {
			return synthesizer.GetInstalledVoices();
		}

		public VoiceInfo getVoice() {
			return synthesizer.Voice;
		}

		public ObservableCollection<QSSQueueItem> getQueue() {
			return queue;
		}

		public bool IsCurrentlySpeaking() {
			return currentlySpeaking;
		}
        #endregion

        //CHANGE METOHDS
        #region "changeMethods"
        public void ChangeVolume(int volume) {
            if (vsp != null) vsp.Volume = IntToFloat(volume);
            this.volume = volume;
		}

		public void ChangeRate(int newRate) {
            rate = newRate;
		}

		public void ChangeVoice(string voiceName) {
			synthesizer.SelectVoice(voiceName);
		}
        #endregion

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (synthesizer != null) synthesizer.Dispose();
                if (speakLoop != null) speakLoop.Dispose();
                if (tokenSource != null) tokenSource.Dispose();

                synthesizer = null;
                speakLoop = null;
                tokenSource = null;
            }
		}
    }
}
