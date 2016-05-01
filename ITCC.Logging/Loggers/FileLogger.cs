using System;
using System.IO;
using System.Text;

namespace ITCC.Logging.Loggers
{
    public class FileLogger : ILogReceiver
    {
        public FileLogger(string filename, bool clearFile = false)
        {
            Filename = filename;
            Level = Logger.Level;
            if (clearFile)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        public FileLogger(string filename, LogLevel level, bool clearFile = false)
        {
            Filename = filename;
            Level = level;
            if (clearFile)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        public LogLevel Level { get; set; }

        public string Filename { get; }

        public virtual void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;

            lock (LockObject)
            {
                using (var fileStream = new FileStream(Filename, FileMode.Append, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append(DateTime.Now);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(args);
                        streamWriter.WriteLine(stringBuilder.ToString());
                    }
                }
            }
            
        }

        protected readonly object LockObject = new object();
    }
}