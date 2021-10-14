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


// 시나리오
namespace BlueAid_RC.View.Chapter
{
    public sealed partial class Chapter7 : UserControl, IChaperControl
    {
        private AudioPlayHandler audioPlayHandler;
        public Chapter7()
        {
            this.InitializeComponent();

            audioPlayHandler = new AudioPlayHandler();
        }

        public void Start()
        {
            audioPlayHandler.Start("ms-appx:///Assets/Q5.mp3");
        }

        public void Dispose()
        {
        }
    }
}
