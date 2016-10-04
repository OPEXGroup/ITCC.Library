using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ITCC.HTTP.Common;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files
{
    internal static class IoHelper
    {
        internal static bool HasWriteAccessToDirectory(string folderPath)
        {
            try
            {
                var ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        internal static string GetExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;
            if (!filename.Contains("."))
                return null;

            return Path.GetExtension(filename).TrimStart('.');
        }

        internal static string GetDirectory(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            return Path.GetDirectoryName(filename);
        }

        internal static string GetRelativeFileName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            return Path.GetFileName(filename);
        }

        internal static string GetNameWithoutExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;
            if (!filename.Contains("."))
                return filename;

            return Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, Path.GetFileNameWithoutExtension(filename));
        }

        internal static string AddBeforeExtension(string fileName, string addition)
        {
            var pureName = GetNameWithoutExtension(fileName);
            if (fileName == null)
                return null;
            var extension = GetExtension(fileName);
            if (extension == null)
                return null;
            return $"{pureName}{addition}.{extension}";
        }

        internal static List<string> LoadAllChanged(string fileName)
        {
            var result = new List<string>();

            var directory = Path.GetDirectoryName(fileName);
            if (directory == null)
                return result;

            var relativeName = Path.GetFileNameWithoutExtension(fileName);
            try
            {
                var files = Directory.GetFiles(directory);
                result.AddRange(files.Where(n => n.Contains(Constants.ChangedString) && Path.GetFileName(n).StartsWith(relativeName)));
            }
            catch (Exception ex)
            {
                Logger.LogException("IO", LogLevel.Warning, ex);
                return result;
            }

            return result;
        }
    }
}
