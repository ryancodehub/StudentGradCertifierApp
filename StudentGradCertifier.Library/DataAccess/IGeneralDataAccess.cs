using SmallApp.Library.GlobalFeatures.EmailTemplate;

namespace StudentGradCertifier.Library.DataAccess;

public interface IGeneralDataAccess
{
    string ProcessUploadFileList(string filePath, string academicYear, string academicTerm, bool testing, Guid emailId);
    string ProcessAcademicQuery(string academicYear, string academicTerm, bool testing, bool sendEmail);
    List<EmailTemplateModel> GetEmailTemplates();
    string SendEmailNotification(string studentId, Guid emailId);
}