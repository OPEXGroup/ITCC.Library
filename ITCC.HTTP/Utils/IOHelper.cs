using System;
using System.IO;

namespace ITCC.HTTP.Utils
{
    internal static class IOHelper
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
    }
}
