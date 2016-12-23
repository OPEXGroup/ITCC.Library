using System;
using ITCC.HTTP.Server.Enums;
using System.Collections.Generic;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class VideoPreprocessTask : BaseFilePreprocessTask
    {
        #region BaseFilePreprocessTask
        public override FileType FileType => FileType.Video;
        public override string FileName { get; set; }

        public override bool Perform() => true;

        public override List<string> GetAllFiles()
            => new List<string> { FileName };

        #endregion
    }
}
