using System;
using System.Windows;
using System.Windows.Controls;
using UILibrary.ViewModel;
using System.Diagnostics;

namespace UILibrary
{
    /// <summary>
    /// Interaction logic for ValidateApplicationCaptcha.xaml
    /// </summary>
    public partial class ValidateApplicationCaptchaWindow : Window
    {
        public void CloseApplicationAndReturnCaptach(string captcha)
        {
            int exitReturn = 0;
            if (Int32.TryParse(captcha, out exitReturn))
            {
                Application.Current.Shutdown(Int32.Parse(captcha));
            }

            Application.Current.Shutdown(0);
        }

        private ValidateApplicationCaptchaViewModel _vm;

        public ValidateApplicationCaptchaWindow()
        {
            InitializeComponent();

            _vm = new ValidateApplicationCaptchaViewModel();
            _vm.ExitRequested += CloseApplicationAndReturnCaptach;

            this.DataContext = _vm;
        }

        public ValidateApplicationCaptchaWindow(string url)
            : this()
        {
            _vm.Url = url;
        }

        private void focus_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox txtBox = sender as TextBox;

                if (txtBox.Text == "ENTER YOUR CAPTCHA HERE!")
                {
                    _vm.Captcha = "";
                }
            }
        }

        private void focus_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox txtBox = sender as TextBox;

                if (txtBox.Text == "")
                {
                     _vm.Captcha = "ENTER YOUR CAPTCHA HERE!";
                }
            }
        }

        private void TextBlock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(_vm.Url);
        }
    }
}
