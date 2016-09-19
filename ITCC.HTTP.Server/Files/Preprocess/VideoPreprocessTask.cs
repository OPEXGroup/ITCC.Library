using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class VideoPreprocessTask : BaseFilePreprocessTask
    {
        #region BaseFilePreprocessTask
        public override FileType FileType => FileType.Video;
        public override string FileName { get; set; }

        public override bool Perform() => true;

        #endregion
    }
}
