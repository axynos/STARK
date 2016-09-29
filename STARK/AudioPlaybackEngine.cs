using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STARK {
    class AudioPlaybackEngine : IDisposable {

        //VARIABLES
        #region "variables
        WaveFormat audioFormat;

        VolumeSampleProvider vsp;
        VolumeSampleProvider vsp2;

        MixingSampleProvider mspStandard;
        MixingSampleProvider mspLoopback;

        ObservableCollection<AudioPlaybackItem> items;

        bool currentlyPlaying = false;
        bool paused;

        TimeSpan currentTime;

        #endregion


        public AudioPlaybackEngine(ref WaveFormat format, ref MixingSampleProvider mspStandard, ref MixingSampleProvider mspLoopback) {
            this.mspStandard = mspStandard;
            this.mspLoopback = mspLoopback;
            audioFormat = format;

            items = new ObservableCollection<AudioPlaybackItem>();
        }

        public void PlayAudioFile(string path, int volume) {
            WaveStream stream = new MediaFoundationReader(path);
            Stream tempStream = new MemoryStream();
            Stream tempStream2 = new MemoryStream();

            MediaFoundationResampler resampler = new MediaFoundationResampler(stream, new WaveFormat(22050, 16, 1));
            resampler.ResamplerQuality = 60;

            WriteToStream(tempStream, resampler);

            tempStream.Seek(0, SeekOrigin.Begin);
            tempStream2.Seek(0, SeekOrigin.Begin);

            //currentTrack
            var reader = new MediaFoundationReader(path);
            vsp = new VolumeSampleProvider(reader.ToSampleProvider());
            vsp.Volume = IntToFloat(volume);

            var reader2 = new MediaFoundationReader(path);
            vsp2 = new VolumeSampleProvider(reader2.ToSampleProvider());
            vsp2.Volume = IntToFloat(volume);


            mspStandard.AddMixerInput(vsp);
            mspLoopback.AddMixerInput(vsp2);
        }

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
        #endregion

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
