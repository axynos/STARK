using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STARK {
    class AudioPlaybackEngine : IDisposable {

        //VARIABLES
        #region "variables"
        WaveFormat audioFormat;
        AudioFileManager afm;
        TimeSpan savedTime;

        MixingSampleProvider mspStandard;
        MixingSampleProvider mspLoopback;

        VolumeSampleProvider vsp;
        VolumeSampleProvider vsp2;

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ObservableCollection<AudioPlaybackItem> items;

        int masterVolume;
        int fileVolume;

        bool currentlyPlaying = false;
        bool paused = false;
        #endregion


        public AudioPlaybackEngine(ref WaveFormat format, ref MixingSampleProvider mspStandard, ref MixingSampleProvider mspLoopback, ref AudioFileManager afm, int masterVolume) {
            this.mspStandard = mspStandard;
            this.mspLoopback = mspLoopback;
            this.masterVolume = masterVolume;
            this.afm = afm;

            audioFormat = format;
            items = new ObservableCollection<AudioPlaybackItem>();
        }

        //this method is the fucking worst I've ever dealt with in my life. It has broken so many times for so many reasons. May it burn in hell.
        private async void PlayAudioFile(string path, int volume, CancellationToken token) {
            if (!currentlyPlaying) {

                currentlyPlaying = true;
                if (volume < 0) volume = 0;
                fileVolume = volume;

                bool fromPausedState = false;
                if (paused) fromPausedState = true;

                if (fromPausedState == false) {
                    using (var stream = new AudioFileReader(path))
                    using(var resampler = new MediaFoundationResampler(stream, audioFormat)) {
                        resampler.ResamplerQuality = 30;
                        WaveFileWriter.CreateWaveFile16("currentTrack.wav", resampler.ToSampleProvider());
                    }
                }

                var inputStream = File.OpenRead("currentTrack.wav");
                var reader = new WaveFileReader(inputStream);
                vsp = new VolumeSampleProvider(reader.ToSampleProvider());
                vsp.Volume = IntToFloat(masterVolume * (fileVolume / 100));

                var inputStream2 = File.OpenRead("currentTrack.wav");
                var reader2 = new WaveFileReader(inputStream2);
                vsp2 = new VolumeSampleProvider(reader2.ToSampleProvider());
                vsp2.Volume = IntToFloat(masterVolume * (fileVolume / 100));

                if (fromPausedState) {
                    reader.CurrentTime = savedTime;
                    reader2.CurrentTime = savedTime;
                }

                mspStandard.AddMixerInput(vsp);
                mspLoopback.AddMixerInput(vsp2);

                try {
                    await Task.Delay(reader.TotalTime, token);
                }
                catch (TaskCanceledException e) {
                    mspStandard.RemoveMixerInput(vsp);
                    mspLoopback.RemoveMixerInput(vsp2);

                    if (paused) savedTime = reader.CurrentTime;
                    if (fromPausedState) fromPausedState = false;
                }

                if (fromPausedState) {
                    savedTime = TimeSpan.Zero;
                    fromPausedState = false;
                    paused = false;
                }

                mspStandard.RemoveMixerInput(vsp);
                mspLoopback.RemoveMixerInput(vsp2);
                reader.Close();
                reader2.Close();
                inputStream.Close();
                inputStream2.Close();

                reader.Dispose();
                reader2.Dispose();
                inputStream.Dispose();
                inputStream2.Dispose();
                currentlyPlaying = false;
            }
        }

        //MEDIA CONTROL
        #region "Media Control"
        public void Play(int id) {
            AudioPlaybackItem item = afm.getCollection().ElementAt(id);
            PlayAudioFile(item.path, item.volume, tokenSource.Token);
        }

        public void Play(string path, int volume) {
            PlayAudioFile(path, volume, tokenSource.Token);
        }

        public void Pause() {
            if (!paused) {
                paused = true;
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
            }
        }

        public void Resume() {
            if (paused) {
                Play("", fileVolume);
            }
        }

        public void Stop() {
            paused = false;
            savedTime = TimeSpan.Zero;
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
        }
        #endregion

        //CHANGE METHODS
        #region "Change Methods"
        public void ChangeVolume(int volume) {
            masterVolume = volume;
            if (vsp != null) vsp.Volume = IntToFloat(masterVolume * (fileVolume/100));
            if (vsp2 != null) vsp2.Volume = IntToFloat(masterVolume * (fileVolume/100));
        }

        public void ChangeMasterVolume(int volume) {
            masterVolume = volume;
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
            using (var writer = new WaveFileWriter(new IgnoreDisposeStream(outStream), sourceProvider.WaveFormat)) 
            {
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                while(true) 
                {
                    int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) 
                    {
                        // end of source provider
                        outStream.Flush();
                        break;
                    }

                    writer.Write(buffer, 0, bytesRead);
                }
            }
        }

        private static void WriteToStream16(Stream outStream, ISampleProvider sourceProvider) {
            WriteToStream(outStream, new SampleToWaveProvider16(sourceProvider));
        }
        #endregion

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
