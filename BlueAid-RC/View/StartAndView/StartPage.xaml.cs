using BlueAid_RC.Model;
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

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace BlueAid_RC.View.StartAndView
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        private User userInfo;

        private MediaPlayer videoPlayer;
        public StartPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(RecordView), userInfo);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            userInfo = e.Parameter as User;

            VideoPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/intro.mp4"));
            videoPlayer = VideoPlayerElement.MediaPlayer;
            videoPlayer.Play();
            videoPlayer.IsLoopingEnabled = false;
        }

        public void Dispose()
        {
            videoPlayer?.Pause();
        }
    }
}
