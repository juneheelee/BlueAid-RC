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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            ErrorMessage.Visibility = Visibility.Collapsed;
            FileStorageUtils.GetInstance.Init();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            
            User user = new User(txtUsername.Text, txtUserNumber.Text);

            bool isValidate = ValidationInputCheck(user);

            bool isDuplicated = await DuplicatedUserInfoCheck(user);

            if (!isDuplicated && !isValidate)
            {
                LoginCheckDialog dialog = new LoginCheckDialog();
                dialog.GetLoginInfo(user);

                ContentDialogResult contentDialogResult = await dialog.ShowAsync();

                if (contentDialogResult == ContentDialogResult.Primary)
                {
                    Debug.WriteLine("취소");
                }
                else if (contentDialogResult == ContentDialogResult.Secondary)
                {
                    Frame frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(StartPage), user);
                    Debug.WriteLine("확인");
                }
            }
        }

        private bool ValidationInputCheck(User user)
        {
            if (String.IsNullOrEmpty(user.userName) || String.IsNullOrWhiteSpace(user.userName))
            {
                ErrorMessage.Text = "환자이름을 입력해주세요.";
                ErrorMessage.Visibility = Visibility.Visible;
                return true;
            }
            else if (String.IsNullOrEmpty(user.userNumber) || String.IsNullOrWhiteSpace(user.userNumber))
            {
                ErrorMessage.Text = "환자 번호를 입력해주세요.";
                ErrorMessage.Visibility = Visibility.Visible;
                return true;
            }

            return false;
        }

        private async Task<bool> DuplicatedUserInfoCheck(User user)
        {
            bool result = await FileStorageUtils.GetInstance.ExistUserFile(user);
            if (result)
            {
                ErrorMessage.Text = "중복된 사용자 정보입니다.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            return result;
        }
    }
}
