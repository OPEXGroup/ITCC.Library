using System.IO;
using System.Text;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;

namespace ITCC.Logging.Windows.Loggers
{
    public class FileLogger : ILogReceiver
    {
        #region ILogReceiver
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
                        streamWriter.WriteLine(args.ToString());
                    }
                }
            }
        }
        #endregion

        #region public
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

        #endregion

        #region protected
        protected readonly object LockObject = new object();
        #endregion
    }
}