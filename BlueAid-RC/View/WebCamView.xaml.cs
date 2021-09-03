using BlueAid_RC.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace BlueAid_RC.View
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class WebCamView : Page
    {
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private MediaCapture _mediaCapture;
        private CameraRotationHelper _rotationHelper;
        //카메라가 실행되는 동안 절전모드로 전환되지 않도록 방지
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        private StorageFolder _captureFolder = null;

        private bool _isInitialized;
        private bool _isRecording;
        private bool _isPreviewing;

        // Information about the camera device
        private bool _mirroringPreview;
        private bool _externalCamera;

        private bool _isActivePage;
        private bool _isUIActive;

        private Task _setupTask = Task.CompletedTask;

        public WebCamView()
        {
            this.InitializeComponent();
            _isInitialized = false;

            _isActivePage = true;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Useful to know when to initialize/clean up the camera
            //Application.Current.Suspending += Application_Suspending;
            //Application.Current.Resuming += Application_Resuming;

            _isActivePage = true;
            //await SetUpBasedOnStateAsync();
        }

        
    }
}
