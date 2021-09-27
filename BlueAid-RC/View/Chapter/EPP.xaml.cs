using BlueAid_RC.Util;
using System;
using System.Collections.Generic;
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
    public sealed partial class EPP : UserControl, IChaperControl
    {
        private AudioPlayHandler audioPlayHandler;
        private MediaPlayer videoPlayer;

        public EPP()
        {
            this.InitializeComponent();
            Init();
        }

        public void Init()
        {
            audioPlayHandler = new AudioPlayHandler();
            audioPlayHandler.audioPlayEndedEvent += AudioPlayHandler_audioPlayEndedEvent;
            VideoPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/test.mp4"));
            videoPlayer = VideoPlayerElement.MediaPlayer;
        }

        private void AudioPlayHandler_audioPlayEndedEvent(bool obj)
        {
            videoPlayer.Play();
            videoPlayer.IsLoopingEnabled = false;
        }

        public void Start()
        {
            audioPlayHandler.Start("ms-appx:///Assets/epp.mp3");
        }

        public void Dispose()
        {
            videoPlayer?.Pause();
            if (audioPlayHandler != null)
            {
                audioPlayHandler.audioPlayEndedEvent -= AudioPlayHandler_audioPlayEndedEvent;
                audioPlayHandler.Dispose();
            }
        }
    }
}
