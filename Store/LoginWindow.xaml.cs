using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Store
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static readonly RoutedUICommand CloseWindowCommand = new RoutedUICommand(
            "CloseWindow", "CloseWindowCommand", typeof(LoginWindow),
            new InputGestureCollection(new InputGesture[]
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt),
                new KeyGesture(Key.Escape)
            }));

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SingIn_Click(object sender, RoutedEventArgs e)
        {
            SignIn(loginBox.Text, passBox.Password);
        }

        private void SingUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (newPassBox.Password == repeatPassBox.Password)
                {
                    if (!Security.SetUserCredentials(newLoginBox.Text, newPassBox.Password))
                        throw new Exception("Пользователь с таким логином уже зарегистрирован");
                else
                        SignIn(newLoginBox.Text, newPassBox.Password);
                }
                else throw new Exception("Пароли не совпадают");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignIn(string login, string pass)
        {
            try
            {
                if (Security.VerifyCredentials(login, pass))
                {
                    int? id = Security.GetUserID(login);
                    if (id.HasValue)
                    {
                        new StoreWindow(id.Value).Show();
                        Close();
                    }
                    else throw new Exception("Пользователь не существует");
                }
                else throw new Exception("Пользователь не существует");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
