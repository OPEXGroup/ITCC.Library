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
