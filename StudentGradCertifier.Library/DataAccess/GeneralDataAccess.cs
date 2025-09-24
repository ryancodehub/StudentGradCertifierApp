using Microsoft.Extensions.Configuration;
using SmallApp.Library;
using SmallApp.Library.Authentication;
using SmallApp.Library.GlobalFeatures.Audit;
using SmallApp.Library.GlobalFeatures.EmailTemplate;
using SmallApp.Library.Helper;

namespace StudentGradCertifier.Library.DataAccess
{
    public class GeneralDataAccess : IGeneralDataAccess
    {
        private readonly ISqlDataAccess _db;
        private readonly IUserSession _userSession;
        private readonly IAuditLogDataAccess _audit;
        private readonly IConfiguration _config;
        private readonly string InHouseConnect = "InHouse";

        public GeneralDataAccess(ISqlDataAccess db, IUserSession userSession, IAuditLogDataAccess audit, IConfiguration config)
        {
            _db = db;
            _userSession = userSession;
            _audit = audit;
            _config = config;
        }

        public List<EmailTemplateModel> GetEmailTemplates()
        {
            var parameters = new
            {
                CategoryId = _config["AppSettings:EmailCategoryId"] ??
                             throw new InvalidOperationException("Missing EmailCategoryId in config")
            };
            return _db.LoadData<EmailTemplateModel, dynamic>(
                "Select * from smallApp.EmailTemplate where CategoryId = @CategoryId",
                parameters, InHouseConnect);
        }

        public string ProcessUploadFileList(string filePath, string academicYear, string academicTerm, bool testing, Guid emailId)
        {
            var studentIds = new List<string>();
            var csvRows = CsvActions.ReadCSV(filePath);
            foreach (var stu in csvRows)
            {
                var studentId = stu[0].Trim();
                if (!string.IsNullOrEmpty(studentId))
                {
                    try
                    {
                        CertifyGraduation(studentId, academicYear, academicTerm, testing);
                        if (emailId != Guid.Empty && !testing)
                        {
                            SendEmailNotification(studentId, emailId);
                        }
                    }
                    catch (Exception e)
                    {
                        return "Error: " + e.Message;
                    }
                    studentIds.Add(studentId);
                }
            }

            if (!testing)
                CreateAudit(studentIds, academicYear, academicTerm, emailId);

            return "Success";
        }

        public string ProcessAcademicQuery(string academicYear, string academicTerm, bool testing, bool sendEmail)
        {
            return "Not yet implemented";
        }

        private void CertifyGraduation(string studentId, string year, string term, bool testing)
        {
            var parameters = new
            {
                Testing = testing,
                StudentId = studentId,
                AcademicYear = year,
                AcademicTerm = term,
                OpId = _userSession.GetCreateOPID()
            };

            _db.SaveData("smallApp.spCertifyGraduation", parameters, InHouseConnect, true);
        }

        private void CreateAudit(List<string> studentIds, string year, string term, Guid emailId)
        {
            var sendEmailText = emailId != Guid.Empty ? $"with email notification {emailId} " : "without email notification ";
            var log = "Applied graduation certification "+sendEmailText+$"to students for {term} {year}";
            var logHtml = $"<p>{log}</p><ul>";
            logHtml = studentIds.Aggregate(logHtml, (current, id) => current + $"<li>{id}</li>");
            logHtml += "</ul>";

            _audit.LogAction(log, logHtml);
        }

        public string SendEmailNotification(string studentId, Guid emailId)
        {
            var username = _db.LoadData<string, dynamic>("smallApp.spGetUsername", new { StudentId = studentId },
                InHouseConnect, true).FirstOrDefault();
            var parameters = new
            {
                Username = username,
                EmailId = emailId
            };

            try
            {
                _db.SaveData("smallApp.spSendEmailTemplate", parameters, InHouseConnect, true);
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            return "Successfully sent Email";
        }
    }
}
