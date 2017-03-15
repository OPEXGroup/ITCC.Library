// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Enums
{
    internal enum FileType
    {
        /// <summary>
        ///     No preprocessing and no request params are enabled
        /// </summary>
        Default,
        /// <summary>
        ///     Compression enabled (aspect ration is always preserved)
        /// </summary>
        Image,
        /// <summary>
        ///     Transcoding enabled (aspect ratio is always preserved)
        /// </summary>
        Video,
    }
}
