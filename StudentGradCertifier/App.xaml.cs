using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmallApp.Library.Authentication;
using SmallApp.Library.DataAccess;
using SmallApp.Library.GlobalFeatures.EmailTemplate;
using SmallApp.Library;
using System.Windows;
using SmallApp.Library.GlobalFeatures.Audit;
using StudentGradCertifier.FunctionWindows;
using StudentGradCertifier.Library.DataAccess;
using StudentGradCertifier.UserWindows;

namespace StudentGradCertifier
{
    public partial class App : Application
    {
        public static ServiceProvider serviceProvider;
        public static bool isDevMode;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            // Register WPF pages
            services.AddTransient<Login>();
            services.AddTransient<MainMenu>();
            services.AddTransient<EmailTemplateEditor>();
            services.AddTransient<CertifyList>();
            services.AddTransient<CertifyPowerCampus>();
            services.AddTransient<UserAppControl>();
            services.AddTransient<AddUserWindow>();
            // Register Interfaces and their implementations
            services.AddTransient<ISqlDataAccess, SqlDataAccessConfigured>();
            // Register DataAccess cs
            services.AddTransient<IApplicationUserDataAccess, ApplicationUserDataAccess>();
            services.AddTransient<IEmailTemplateDataAccess, EmailTemplateDataAccess>();
            services.AddTransient<IGeneralDataAccess, GeneralDataAccess>();
            services.AddTransient<IAuditLogDataAccess, AuditLogDataAccess>();
            services.AddTransient<IGeneralDataPulls, GeneralDataPulls>();
            // Register Authentication cs
            services.AddTransient<IAuthentication, LdapAuthentication>();
            services.AddTransient<IApplicationSecurityDataAccess, ApplicationSecurityDataAccess>();
            services.AddTransient<IPowerCampusUserInfo, PowerCampusUserInfoConfigured>();
            services.AddTransient<IUserSession, LdapUserSessionConfigured>();


            isDevMode = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["DeveloperMode"] ?? throw new InvalidOperationException());
            var connectionStringName = isDevMode ? "Campus6_test" : "Campus6";
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString ?? throw new InvalidOperationException();

            var appUniqueId = System.Configuration.ConfigurationManager.AppSettings["AppUniqueId"]
                              ?? throw new InvalidOperationException("Missing AppUniqueId in App.config");

            var emailCategory = System.Configuration.ConfigurationManager.AppSettings["EmailCategoryId"]
                                ?? throw new InvalidOperationException("Missing EmailCategoryId in App.config");

            var inMemorySettings = new Dictionary<string, string>
            {

                { "ConnectionStrings:Campus6", connectionString },
                { "ConnectionStrings:InHouse", System.Configuration.ConfigurationManager.ConnectionStrings["InHouse"].ConnectionString },
                { "ConnectionStrings:Campus6_test", System.Configuration.ConfigurationManager.ConnectionStrings["Campus6_test"].ConnectionString },
                { "AppSettings:AppUniqueId", appUniqueId },
                { "AppSettings:EmailCategoryId", emailCategory }
            };

            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings);

            IConfiguration config = builder.Build();

            var domain = "LSUA";
            var container = "DC=LSUA,DC=EDU";

            services.AddSingleton<IAuthentication>(provider => new LdapAuthentication(domain, container));

            services.AddSingleton(config);

            serviceProvider = services.BuildServiceProvider();

            var login = serviceProvider.GetRequiredService<Login>();
            login.ShowDialog();
        }
    }
}
