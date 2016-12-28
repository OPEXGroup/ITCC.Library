// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal static class FilePreprocessTaskFactory
    {
        #region public

        public static BaseFilePreprocessTask BuildTask(string fileName)
        {
            var type = FileTypeSelector.GetFileTypeByName(fileName);
            switch (type)
            {
                case FileType.Default:
                    return null;
                case FileType.Image:
                    return new ImagePreprocessTask { FileName = fileName };
                case FileType.Video:
                    return new VideoPreprocessTask { FileName = fileName };
                default:
                    return null;
            }
        }
        #endregion
    }
}
