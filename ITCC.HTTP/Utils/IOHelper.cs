using System;
using System.IO;

namespace ITCC.HTTP.Utils
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

            var lastDotIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
            return filename.Remove(0, lastDotIndex + 1);
        }

        internal static string GetNameWithoutExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;
            if (!filename.Contains("."))
                return filename;

            var lastDotIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
            return filename.Remove(lastDotIndex);
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
    }
}
