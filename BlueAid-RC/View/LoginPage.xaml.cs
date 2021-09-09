﻿using BlueAid_RC.Model;
using BlueAid_RC.View.StartAndView;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            User user = new User(txtUsername.Text, txtUserNumber.Text);
            
            LoginCheckDialog dialog = new LoginCheckDialog();
            dialog.GetLoginInfo(user);

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine("취소");
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(StartPage), user);
                Debug.WriteLine("확인");
            }
        }
    }
}
