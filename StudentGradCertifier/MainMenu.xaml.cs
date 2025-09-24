using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmallApp.Library.Authentication;
using StudentGradCertifier.FunctionWindows;
using StudentGradCertifier.UserWindows;

namespace StudentGradCertifier
{
    public partial class MainMenu : Window
    {
        private readonly IUserSession _userSession;

        public MainMenu(IUserSession userSession)
        {
            _userSession = userSession;
            InitializeComponent();

            if (_userSession.GetSqlSysAdmin())
            {
                UserControlBtn.Visibility = Visibility.Visible;
                UserControlBtn.IsEnabled = true;
            }

            // Disabling for now, Registrar has something set up for how they create a list of students. Just let them keeping using that for now.
            RunFromPcBtn.Visibility = Visibility.Collapsed;
            RunFromPcBtn.IsEnabled = false;
        }

        // Note: Had an idea to implement some stuff from Degree Navigator: Realized I didn't need it. Can pull it all from PC if needed.

        private void EditEmailBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var emailTemplateWindow = App.serviceProvider.GetRequiredService<EmailTemplateEditor>();
            this.Hide();
            emailTemplateWindow.ShowDialog();
            this.Show();
        }

        private void UserControlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var userControlWindow = App.serviceProvider.GetRequiredService<UserAppControl>();
            this.Hide();
            userControlWindow.ShowDialog();
            this.Show();
        }

        private void RunFromPcBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var certifyPcWindow = App.serviceProvider.GetRequiredService<CertifyPowerCampus>();
            this.Hide();
            certifyPcWindow.ShowDialog();
            this.Show();
        }

        private void UploadPcidListBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var certifyListWindow = App.serviceProvider.GetRequiredService<CertifyList>();
            this.Hide();
            certifyListWindow.ShowDialog();
            this.Show();
        }
    }
}
