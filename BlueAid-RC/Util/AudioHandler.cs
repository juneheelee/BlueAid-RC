using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BlueAid_RC.Util
{
    public class AudioHandler : IDisposable
    {
        private readonly string AUDIO_FILE_PATH = "audio";

        private MediaCapture _audioCapture;
        private InMemoryRandomAccessStream _memoryBuffer;
        private StorageFile _storageFile;
        public bool IsRecording { get; set; }

        public AudioHandler()
        {
        }

        public async Task InitializeAudioAsync()
        {
            if (IsRecording)
                Dispose();

            _memoryBuffer = new InMemoryRandomAccessStream();
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };
            _audioCapture = new MediaCapture();
            await _audioCapture.InitializeAsync(settings);
        }

        public async Task SetAudioSavePath(string fileName)
        {
            var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            StorageFolder storageFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
            _storageFile = await storageFolder.CreateFileAsync(
                   Path.Combine(AUDIO_FILE_PATH, fileName),
                   CreationCollisionOption.ReplaceExisting);
        }

        public async Task StartAudioRecording()
        {
            await _audioCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto), _memoryBuffer);
            IsRecording = true;
        }

        public async Task StopAudioRecording()
        {
            await _audioCapture.StopRecordAsync();
            IsRecording = false;
            SaveAudioToFile();
        }

        private async void SaveAudioToFile()
        {
            IRandomAccessStream audioStream = _memoryBuffer.CloneStream();

            try
            {
                using (IRandomAccessStream fileStream = await _storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(audioStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                    await audioStream.FlushAsync();
                    audioStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            _memoryBuffer?.Dispose();
            _audioCapture?.Dispose();
            IsRecording = false;
        }
    }
}
