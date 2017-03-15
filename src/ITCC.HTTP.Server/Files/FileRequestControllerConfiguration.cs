// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using ITCC.HTTP.Server.Common;

namespace ITCC.HTTP.Server.Files
{
    internal class FileRequestControllerConfiguration<TAccount>
        where TAccount : class 
    {
        /// <summary>
        ///     If false, controller will do nothing
        /// </summary>
        public bool FilesEnabled { get; set; }
        /// <summary>
        ///     Files base uri
        /// </summary>
        public string FilesBaseUri { get; set; }
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
        ///     If true, zipped copies of all files will be created. Works only with FilesPreprocessingEnabled
        /// </summary>
        public bool FilesCompressionEnabled { get; set; } = false;
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
