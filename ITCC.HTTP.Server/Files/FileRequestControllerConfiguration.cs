using System.Collections.Generic;
using ITCC.HTTP.Server.Common;

namespace ITCC.HTTP.Server.Files
{
    internal class FileRequestControllerConfiguration<TAccount>
        where TAccount : class 
    {
        /// <summary>
        ///     File location on disk
        /// </summary>
        public string FilesLocation { get; set; }
        /// <summary>
        ///     If true, every file request will be checked with Authorizer
        /// </summary>
        public bool FilesNeedAuthorization { get; set; }
        /// <summary>
        ///     If true, then files will be preprocessed (image sizes variants, video transcoding)
        /// </summary>
        public bool FilesPreprocessingEnabled { get; set; } = true;
        /// <summary>
        ///     Number of threads used for files preprocessing. All CPU cores will be used for negative values
        /// </summary>
        public int FilesPreprocessorThreads { get; set; } = -1;
        /// <summary>
        ///     How often do we preprocess existing files
        /// </summary>
        public double ExistingFilesPreprocessingFrequency { get; set; } = 60;
        /// <summary>
        ///     File sections for separate access grants
        /// </summary>
        public List<FileSection> FileSections { get; set; } = new List<FileSection>();
        /// <summary>
        ///     Method used to check authorization tokens for files requests
        /// </summary>
        public Delegates.FilesAuthorizer<TAccount> FilesAuthorizer { get; set; }
        /// <summary>
        ///     Favicon requests (Just for fun)
        /// </summary>
        public string FaviconPath { get; set; }
    }
}
