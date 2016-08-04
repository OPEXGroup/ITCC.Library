namespace ITCC.HTTP.Enums
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
