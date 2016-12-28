// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using ITCC.HTTP.Server.Enums;

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
            var extension = IoHelper.GetExtension(fileName);
            return GetFileTypeByExtension(extension);
        }

        private static readonly Dictionary<string, FileType> FileTypeDictionary = new Dictionary<string, FileType>
        {
            {"jpg", FileType.Image },
            {"jpeg", FileType.Image },
            {"png", FileType.Image },
            {"bmp", FileType.Image },
            {"gif", FileType.Image },
            {"ico", FileType.Image },
            {"mp4", FileType.Video },
            {"mkv", FileType.Video },
            {"avi", FileType.Video }
        };

        private static readonly object DictLock = new object();
    }
}
