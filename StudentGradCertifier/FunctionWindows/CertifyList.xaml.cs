using System.Windows;
using SmallApp.Library.Authentication;
using SmallApp.Library.DataAccess;
using SmallApp.Library.GlobalFeatures.EmailTemplate;
using StudentGradCertifier.Library.DataAccess;

namespace StudentGradCertifier.FunctionWindows
{
    public partial class CertifyList : Window
    {
        private readonly IGeneralDataAccess _db;
        private readonly IUserSession _userSession;
        private readonly IGeneralDataPulls _codes;

        public CertifyList(IGeneralDataAccess db, IUserSession userSession, IGeneralDataPulls codes)
        {
            _db = db;
            _userSession = userSession;
            _codes = codes;
            InitializeComponent();
            LoadData();

            RunInTestCb.IsChecked = true;
        }

        private void LoadData()
        {
            EmailTemplateSelectCmb.ItemsSource = _db.GetEmailTemplates();
            EmailTemplateSelectCmb.DisplayMemberPath = "TemplateName";
            EmailTemplateSelectCmb.SelectedValuePath = "Id";

            YearCmb.ItemsSource = _codes.GetAcademicYears();

            var terms = _codes.GetAcademicTerms();
            TermCmb.ItemsSource = terms;
            TermCmb.DisplayMemberPath = "LONG_DESC";
            TermCmb.SelectedValuePath = "CODE_VALUE_KEY";
        }

        private void BrowseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePathTb.Text = openFileDialog.FileName;
            }
        }

        private void UploadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!AllRequiredFieldsFilled()) return;

            var emailId = EmailTemplateSelectCmb.SelectedValue != null && SendEmailCb.IsChecked == true
                ? (Guid)EmailTemplateSelectCmb.SelectedValue
                : Guid.Empty;

            var parameters = new {
                FilePath = FilePathTb.Text,
                AcademicYear = YearCmb.SelectedValue.ToString(),
                AcademicTerm = TermCmb.SelectedValue.ToString(),
                Testing = RunInTestCb.IsChecked == true,
                EmailId = emailId
            };

            var message = _db.ProcessUploadFileList(parameters.FilePath, parameters.AcademicYear, parameters.AcademicTerm,
                parameters.Testing, parameters.EmailId);

            if (SendEmailToSelfCb.IsChecked == true)
                SendSelfEmail();

            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool AllRequiredFieldsFilled()
        {
            if (string.IsNullOrEmpty(FilePathTb.Text))
            {
                MessageBox.Show("Please select a file.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (YearCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an academic year.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (TermCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an academic term.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (EmailTemplateSelectCmb.SelectedValue == null && SendEmailCb.IsChecked == true)
            {
                MessageBox.Show("Please select an email template.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void SendTestEmailNowBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (EmailTemplateSelectCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an email template.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            SendSelfEmail();
        }

        private void SendSelfEmail()
        {
            var userPcid = _userSession.GetPowerCampusId();
            var emailId = (Guid)EmailTemplateSelectCmb.SelectedValue;

            var message = _db.SendEmailNotification(userPcid, emailId);
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RunInTestCb_Checked(object sender, RoutedEventArgs e)
        {
            EmailTemplateSelectCmb.Visibility = Visibility.Collapsed;
            EmailTemplateSelectLbl.Visibility = Visibility.Collapsed;
            SendTestEmailNowBtn.Visibility = Visibility.Collapsed;
            SendEmailSelfLbl.Visibility = Visibility.Collapsed;
            SendEmailToSelfCb.Visibility = Visibility.Collapsed;
            SendEmailLbl.Visibility = Visibility.Collapsed;
            SendEmailCb.Visibility = Visibility.Collapsed;

            SendEmailToSelfCb.IsChecked = false;
            SendEmailCb.IsChecked = false;
            SendEmailToSelfCb.IsChecked = false;
            EmailTemplateSelectCmb.SelectedValue = null;
        }

        private void RunInTestCb_OnUnchecked(object sender, RoutedEventArgs e)
        {
            EmailTemplateSelectCmb.Visibility = Visibility.Visible;
            EmailTemplateSelectLbl.Visibility = Visibility.Visible;
            SendTestEmailNowBtn.Visibility = Visibility.Visible;
            SendEmailSelfLbl.Visibility = Visibility.Visible;
            SendEmailToSelfCb.Visibility = Visibility.Visible;
            SendEmailLbl.Visibility = Visibility.Visible;
            SendEmailCb.Visibility = Visibility.Visible;
        }
    }
}
