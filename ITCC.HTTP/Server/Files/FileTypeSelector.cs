using System.Collections.Generic;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Utils;

namespace ITCC.HTTP.Server.Files
{
    internal static class FileTypeSelector
    {
        public static bool RegisterFileExtension(string extension, FileType type)
        {
            lock (DictLock)
            {
                if (FileTypeDictionary.ContainsKey(extension))
                    return false;
                FileTypeDictionary.Add(extension, type);
                return true;
            }
        }

        public static FileType GetFileTypeByExtension(string extension)
        {
            if (extension == null)
                return FileType.Default;
            lock (DictLock)
            {
                FileType result;
                if (FileTypeDictionary.TryGetValue(extension, out result)) 
                    return result;
            }
            return FileType.Default;
        }

        public static FileType GetFileTypeByName(string fileName)
        {
            var extension = IOHelper.GetExtension(fileName);
            return GetFileTypeByExtension(extension);
        }

        private static readonly Dictionary<string, FileType> FileTypeDictionary = new Dictionary<string, FileType>
        {
            {"jpg", FileType.Image },
            {"jpeg", FileType.Image },
            {"png", FileType.Image },
            {"mp4", FileType.Video }
        };

        private static readonly object DictLock = new object();
    }
}
