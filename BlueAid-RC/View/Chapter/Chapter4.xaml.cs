﻿using BlueAid_RC.Util;
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
    public sealed partial class Chapter4 : UserControl, IMediaControl
    {
        private MediaPlayer mediaPlayer;
        public Chapter4()
        {
            this.InitializeComponent();

            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Q4.mp3"));
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Debug.WriteLine("chapter4 end");
        }

        public void Start()
        {
            System.Threading.Thread.Sleep(1000);
            mediaPlayer.Play();
        }

        public void Dispose()
        {
        }
    }
}
