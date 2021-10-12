using BlueAid_RC.Model;
using BlueAid_RC.Util;
using BlueAid_RC.View.StartAndView;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class RecordView : Page
    {
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        //카메라가 실행되는 동안 절전모드로 전환되지 않도록 방지
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        // Folder in which the captures will be stored (initialized in SetupUiAsync)
        private StorageFolder _captureFolder = null;
        // For listening to media property changes
        private readonly SystemMediaTransportControls _systemMediaControls = SystemMediaTransportControls.GetForCurrentView();

        // MediaCapture and its state variables
        private MediaCapture _mediaCapture;
        private bool _isInitialized;
        private bool _isPreviewing;
        private bool _isRecording;

        // UI state
        private bool _isSuspending;
        private bool _isActivePage;
        private bool _isUIActive;
        private Task _setupTask = Task.CompletedTask;

        // Information about the camera device
        private bool _mirroringPreview;
        private bool _externalCamera;

        private readonly string videoPath = "video";

        private string userName = string.Empty;
        private string userNumber = string.Empty;

        private AudioRecordingHandler _audioHandler;
        private AudioPlayHandler _audioPlayHandler;

        public RecordView()
        {
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Disabled;

            _audioHandler = new AudioRecordingHandler();
            _audioPlayHandler = new AudioPlayHandler();
            _audioPlayHandler.audioPlayEndedEvent += _audioPlayHandler_audioPlayEndedEvent;
        }

        private async Task Refresh()
        {
            //call your database here...
            //and update the UI afterwards:
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                // Your UI update code goes here!
                FlipViewControl.Focus(FocusState.Pointer);
            });
        }
        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            _isSuspending = false;

            var deferral = e.SuspendingOperation.GetDeferral();
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
                deferral.Complete();
            });
        }

        private void Application_Resuming(object sender, object o)
        {
            _isSuspending = false;

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Useful to know when to initialize/clean up the camera
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
            Window.Current.VisibilityChanged += Window_VisibilityChanged;

            _isActivePage = true;
            await SetUpBasedOnStateAsync();

            User user = e.Parameter as User;
            userName = user.userName;
            userNumber = user.userNumber;

            await Refresh();

            (FlipViewControl.Items[FlipViewControl.SelectedIndex] as IChaperControl).Start();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Handling of this event is included for completenes, as it will only fire when navigating between pages and this sample only includes one page
            Application.Current.Suspending -= Application_Suspending;
            Application.Current.Resuming -= Application_Resuming;
            Window.Current.VisibilityChanged -= Window_VisibilityChanged;

            _isActivePage = false;
            await SetUpBasedOnStateAsync();

            (FlipViewControl.Items[FlipViewControl.SelectedIndex] as IChaperControl)?.Dispose();
        }


        #region Event handlers
        private async void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            await SetUpBasedOnStateAsync();
        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                await StartRecordingAsync();
                await _audioHandler?.StartAudioRecording();
            }

            // After starting or stopping video recording, update the UI to reflect the MediaCapture state
            UpdateCaptureControls();
        }
        private async void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                await StopRecordingAsync();
                await _audioHandler?.StopAudioRecording();
            }
            //UpdateCaptureControls();
            StopBtn.IsEnabled = false;
            _audioPlayHandler.Start("ms-appx:///Assets/good.mp3");
        }

        private async void _audioPlayHandler_audioPlayEndedEvent(bool obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                UpdateCaptureControls();
                NextBtn.IsEnabled = true;
            }); 
        }

        private async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
        {
            // This is a notification that recording has to stop, and the app is expected to finalize the recording

            await StopRecordingAsync();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateCaptureControls());
        }

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            await CleanupCameraAsync();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateCaptureControls());
        }

        #endregion Event handlers


        #region MediaCapture methods

        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, starts preview and unlocks the UI
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                // Attempt to get the back camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

                if (cameraDevice == null)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }

                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();

                // Register for a notification when video recording has reached the maximum time and when something goes wrong
                _mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
                _mediaCapture.Failed += MediaCapture_Failed;

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                // Initialize MediaCapture
                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    // Figure out where the camera is located
                    if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                    {
                        // No information on the location of the camera, assume it's an external camera, not integrated on the device
                        _externalCamera = true;
                    }
                    else
                    {
                        // Camera is fixed on the device
                        _externalCamera = false;

                        // Only mirror the preview if the camera is on the front panel
                        _mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                    }

                    await StartPreviewAsync();

                    UpdateCaptureControls();
                }
            }
        }

        /// <summary>
        /// Starts the preview and adjusts it for for rotation and mirroring after making a request to keep the screen on
        /// </summary>
        /// <returns></returns>
        private async Task StartPreviewAsync()
        {
            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _mediaCapture;
            PreviewControl.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start the preview
            await _mediaCapture.StartPreviewAsync();
            _isPreviewing = true;
        }

        /// <summary>
        /// Stops the preview and deactivates a display request, to allow the screen to go into power saving modes
        /// </summary>
        /// <returns></returns>
        private async Task StopPreviewAsync()
        {
            // Stop the preview
            _isPreviewing = false;
            await _mediaCapture.StopPreviewAsync();

            // Use the dispatcher because this method is sometimes called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Cleanup the UI
                PreviewControl.Source = null;

                // Allow the device screen to sleep now that the preview is stopped
                _displayRequest.RequestRelease();
            });
        }

        /// <summary>
        /// Records an MP4 video to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        private async Task StartRecordingAsync()
        {
            try
            {
                // Create storage file for the capture
                var userPath = userName + "_" + userNumber;
                var captureFileName = $"chapter{FlipViewControl.SelectedIndex + 1}.mp4";
                var captureFullPath = Path.Combine(videoPath, userPath, captureFileName);
                var videoFile = await _captureFolder.CreateFileAsync(captureFullPath, CreationCollisionOption.ReplaceExisting);

                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                var audioFileName = $"chapter{FlipViewControl.SelectedIndex + 1}.wav";
                await _audioHandler?.SetAudioSavePath(Path.Combine(userPath, audioFileName));

                Debug.WriteLine("Starting recording to " + videoFile.Path);

                await _mediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);

                _isRecording = true;

                Debug.WriteLine("Started recording!");
            }
            catch (Exception ex)
            {
                // File I/O errors are reported as exceptions
                Debug.WriteLine("Exception when starting video recording: " + ex.ToString());
            }
        }

        /// <summary>
        /// Stops recording a video
        /// </summary>
        /// <returns></returns>
        private async Task StopRecordingAsync()
        {
            Debug.WriteLine("Stopping recording...");

            _isRecording = false;
            await _mediaCapture.StopRecordAsync();
            Debug.WriteLine("Stopped recording!");
        }

        /// <summary>
        /// Cleans up the camera resources (after stopping any video recording and/or preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");

            if (_isInitialized)
            {
                // If a recording is in progress during cleanup, stop it to save the recording
                if (_isRecording)
                {
                    await StopRecordingAsync();
                }

                if (_isPreviewing)
                {
                    // The call to stop the preview is included here for completeness, but can be
                    // safely removed if a call to MediaCapture.Dispose() is being made later,
                    // as the preview will be automatically stopped at that point
                    await StopPreviewAsync();
                }

                _isInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.RecordLimitationExceeded -= MediaCapture_RecordLimitationExceeded;
                _mediaCapture.Failed -= MediaCapture_Failed;
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        #endregion MediaCapture methods


        /// <summary>
        /// Initialize or clean up the camera and our UI,
        /// depending on the page state.
        /// </summary>
        /// <returns></returns>
        private async Task SetUpBasedOnStateAsync()
        {
            // Avoid reentrancy: Wait until nobody else is in this function.
            while (!_setupTask.IsCompleted)
            {
                await _setupTask;
            }

            // We want our UI to be active if
            // * We are the current active page.
            // * The window is visible.
            // * The app is not suspending.
            bool wantUIActive = _isActivePage && Window.Current.Visible && !_isSuspending;

            if (_isUIActive != wantUIActive)
            {
                _isUIActive = wantUIActive;

                Func<Task> setupAsync = async () =>
                {
                    if (wantUIActive)
                    {
                        await SetupUiAsync();
                        await InitializeCameraAsync();
                        await _audioHandler?.InitializeAudioAsync();
                    }
                    else
                    {
                        await CleanupCameraAsync();
                        await CleanupUiAsync();
                    }
                };
                _setupTask = setupAsync();
            }

            await _setupTask;
        }

        /// <summary>
        /// Attempts to lock the page orientation, hide the StatusBar (on Phone) and registers event handlers for hardware buttons and orientation sensors
        /// </summary>
        /// <returns></returns>
        private async Task SetupUiAsync()
        {
            // Attempt to lock page to landscape orientation to prevent the CaptureElement from rotating, as this gives a better experience
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            StorageLibrary picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            // Fall back to the local app storage if the Pictures Library is not available
            _captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
            
        }

        /// <summary>
        /// Unregisters event handlers for hardware buttons and orientation sensors, allows the StatusBar (on Phone) to show, and removes the page orientation lock
        /// </summary>
        /// <returns></returns>
        private async Task CleanupUiAsync()
        {
            // Show the status bar
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                //await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
            }

            // Revert orientation preferences
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
        }

        /// <summary>
        /// This method will update the icons, enable/disable and show/hide the photo/video buttons depending on the current state of the app and the capabilities of the device
        /// </summary>
        private void UpdateCaptureControls()
        {
            // The buttons should only be enabled if the preview started sucessfully
            RecordBtn.IsEnabled = !_isRecording;
            RecordEnableIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            RecordDisEnableIcon.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;

            StopBtn.IsEnabled = _isRecording;
            RecordStopEnableIcon.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;
            RecordStopDisEnableIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Attempts to find and return a device mounted on the panel specified, and on failure to find one it will return the first device listed
        /// </summary>
        /// <param name="desiredPanel">The desired panel on which the returned device should be mounted, if available</param>
        /// <returns></returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            (FlipViewControl.Items[FlipViewControl.SelectedIndex] as IChaperControl)?.Dispose();
            if (FlipViewControl.Items.Count -1 > FlipViewControl.SelectedIndex)
            {
                FlipViewControl.SelectedIndex++;
                (FlipViewControl.Items[FlipViewControl.SelectedIndex] as IChaperControl).Start();
            }
            else
            {
                Debug.WriteLine("더이상 페이지가 없습니다.");
                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(EndPage));
            }
            NextBtn.IsEnabled = false;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage));
        }
    }
}
