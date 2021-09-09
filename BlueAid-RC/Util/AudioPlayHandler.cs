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
        public AudioPlayHandler()
        {
            mediaPlayer = new MediaPlayer();
        }

        public void Start(string audioPath)
        {
            System.Threading.Thread.Sleep(500);
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(audioPath));
            mediaPlayer.Play();
        }
        
    }
}
