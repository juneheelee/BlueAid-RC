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

namespace BlueAid_RC.View.Questions
{
    public sealed partial class Question1 : UserControl, IChaperControl
    {
        private AudioPlayHandler audioPlayHandler;
        public Question1()
        {
            this.InitializeComponent();

            audioPlayHandler = new AudioPlayHandler();
        }

        public void Start()
        {
            //audioPlayHandler.Start("ms-appx:///Assets/Q4.mp3");
        }

        public void Dispose()
        {
        }
    }
}
