using BlueAid_RC.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace BlueAid_RC.View.Chapter
{
    public sealed partial class Chapter1 : UserControl, IChaperControl
    {
        private AudioPlayHandler audioPlayHandler;
        private MediaPlayer videoPlayer;
        public event Action<bool> MediaEndEvent;

        public Chapter1()
        {
            this.InitializeComponent();
            //Init();
            VideoPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/chapter1.mp4"));
            videoPlayer = VideoPlayerElement.MediaPlayer;
            videoPlayer.MediaEnded += VideoPlayer_MediaEnded;
        }

        public void Init()
        {
            if (audioPlayHandler == null)
            {
                audioPlayHandler = new AudioPlayHandler();
                audioPlayHandler.audioPlayEndedEvent += AudioPlayHandler_audioPlayEndedEvent;
            }
        }

        private void VideoPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if(MediaEndEvent != null)
            {
                MediaEndEvent(true);
            }
        }

        private void AudioPlayHandler_audioPlayEndedEvent(bool obj)
        {
            videoPlayer.Play();
            videoPlayer.IsLoopingEnabled = false;
        }

        public void Start()
        {
            Init();
            audioPlayHandler.Start("ms-appx:///Assets/Q1.mp3");
        }

        public void Dispose()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                //videoPlayer.MediaEnded -= VideoPlayer_MediaEnded;
                //videoPlayer.Dispose();
                //videoPlayer = null;
                //MediaEndEvent = null;
            }
            
            if (audioPlayHandler != null)
            {
                audioPlayHandler.audioPlayEndedEvent -= AudioPlayHandler_audioPlayEndedEvent;
                audioPlayHandler.Dispose();
                audioPlayHandler = null;
            }
        }
    }
}
