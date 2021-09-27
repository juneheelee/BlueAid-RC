using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace BlueAid_RC.Util
{
    public class AudioPlayHandler
    {

        private MediaPlayer mediaPlayer;
        public event Action<bool> audioPlayEndedEvent;
        public AudioPlayHandler()
        {
            mediaPlayer = new MediaPlayer();
        }

        public void Start(string audioPath)
        {
            System.Threading.Thread.Sleep(500);
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(audioPath));
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.Play();
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (audioPlayEndedEvent != null)
            {
                audioPlayEndedEvent(true);
            }
        }

        public void Dispose()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                mediaPlayer.Dispose();
            }
        }
    }
}
