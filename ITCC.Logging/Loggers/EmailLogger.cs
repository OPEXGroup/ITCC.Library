﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ITCC.Logging.Utils;

namespace ITCC.Logging.Loggers
{
    public class EmailLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }
        public async void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;

            _messageQueue.Enqueue(args);
            if (args.Level <= FlushLevel)
            {
                _updateTimer.Stop();
                await Flush(true);
                _updateTimer.Start();
            }
        }
        #endregion

        #region public

        public EmailLogger(EmailLoggerConfiguration configuration)
        {
            Level = Logger.Level;
            ReadConfiguration(configuration);
        }

        public EmailLogger(LogLevel level, EmailLoggerConfiguration configuration)
        {
            Level = level;
            ReadConfiguration(configuration);
        }

        public void Start()
        {
            _updateTimer = new Timer(ReportPeriod * 1000);
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        public string Login { get; private set; }

        public string Password { get; private set; }

        public string Subject { get; private set; }

        public string Sender { get; private set; }

        public List<string> Receivers { get; private set; } = new List<string>();
        
        public string SmtpHost { get; private set; }

        public int SmptPort { get; private set; }

        public double ReportPeriod { get; private set; }

        public LogLevel FlushLevel { get; private set; }

        public bool SendEmptyReports { get; private set; }
        #endregion

        #region private

        private void ReadConfiguration(EmailLoggerConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (! configuration.IsEnough())
                throw new ArgumentException("Configuration is incorrect", nameof(configuration));

            Login = configuration.Login;
            Password = configuration.Password;
            Subject = configuration.Subject;
            Sender = configuration.Sender;
            Receivers = configuration.Receivers;
            SmtpHost = configuration.SmtpHost;
            SmptPort = configuration.SmptPort;
            ReportPeriod = configuration.ReportPeriod;
            FlushLevel = configuration.FlushLevel;
            SendEmptyReports = configuration.SendEmptyReports;
        }

        private async void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await Flush(false);
        }

        private async Task Flush(bool isForced)
        {
            if (_flushInProgress)
                return;
            _flushInProgress = true;

            string logMessage;
            string subject;
            var body = string.Empty;
            MailPriority priority;
            if (_messageQueue.IsEmpty)
            {
                if (!SendEmptyReports)
                {
                    logMessage = "Skipping mail send";
                    Logger.LogEntry("MAIL LOG", LogLevel.Debug, logMessage);
                    return;
                }
                logMessage = "Sending empty email";
                subject = $"{Subject} (No new entries)";
                body = $"No new entries of level {Level} and higher";
                priority = MailPriority.Low;
            }
            else
            {
                var counter = 0;
                LogEntryEventArgs temp;
                while (_messageQueue.TryDequeue(out temp))
                {
                    counter++;
                    body += $"\n\n{temp.Representation}";
                }
                logMessage = $"Sending email with {counter} entries";
                var alarmStr = isForced ? ", ALARM" : string.Empty;
                priority = isForced ? MailPriority.High : MailPriority.Normal;
                subject = $"{Subject} ({counter} new entries{alarmStr})";
            }
            Logger.LogEntry("MAIL LOG", LogLevel.Debug, logMessage);
            await SendEmail(subject, body, priority);

            _flushInProgress = false;
        }

        private async Task SendEmail(string subject, string body, MailPriority priority)
        {
            try
            {
                using (var client = new SmtpClient
                {
                    Port = SmptPort,
                    Host = SmtpHost,
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(Login, Password),
                })
                {
                    var mailMessage = new MailMessage(Sender, Receivers.First(), subject, body)
                    {
                        BodyEncoding = Encoding.UTF8,
                        DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                        Priority = priority
                    };
                    Receivers.ForEach(r =>
                    {
                        if (mailMessage.To.All(ma => ma.Address != r))
                            mailMessage.To.Add(r);
                    });
                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MAIL LOG", LogLevel.Warning, ex);
            }
        }

        private Timer _updateTimer;

        private readonly ConcurrentQueue<LogEntryEventArgs> _messageQueue = new ConcurrentQueue<LogEntryEventArgs>();

        private bool _flushInProgress;

        #endregion
    }
}