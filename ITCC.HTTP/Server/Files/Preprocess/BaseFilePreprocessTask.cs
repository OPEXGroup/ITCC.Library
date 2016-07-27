using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal abstract class BaseFilePreprocessTask
    {
        #region public
        public abstract FileType FileType { get; }

        public abstract string FileName { get; set; }

        public abstract bool Perform();

        #endregion

    }
}
