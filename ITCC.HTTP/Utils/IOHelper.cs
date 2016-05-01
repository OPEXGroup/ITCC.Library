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
    }
}
