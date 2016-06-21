using System.Collections.Generic;
using System.Linq;

namespace ITCC.Logging.Utils
{
    public class EmailLoggerConfiguration
    {
        #region properties
        public string Login { get; set; }

        public string Password { get; set; }

        public string Subject { get; set; }

        public string Sender { get; set; }

        public List<string> Receivers { get; set; } = new List<string>();

        public string SmtpHost { get; set; }

        public int SmptPort { get; set; }

        public double ReportPeriod { get; set; }

        public LogLevel FlushLevel { get; set; }

        public bool SendEmptyReports { get; set; }
        #endregion

        #region methods
        public bool IsEnough()
        {
            if (Login == null)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No login in email config");
                return false;
            }

            if (Password == null)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No password in email config");
                return false;
            }

            if (Subject == null)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No subject in email config");
                return false;
            }

            if (Sender == null)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No sender in email config");
                return false;
            }

            if (Receivers == null || !Receivers.Any())
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No receivers in email config");
                return false;
            }

            if (SmtpHost == null)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "No smtp host in email config");
                return false;
            }

            if (SmptPort < 1)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "Incorrect smtp port in email config");
                return false;
            }

            if (ReportPeriod < 1)
            {
                Logger.LogEntry("MAIL CONFOG", LogLevel.Warning, "Incorrect report period in email config");
                return false;
            }

            return true;
        }
        #endregion
    }
}
