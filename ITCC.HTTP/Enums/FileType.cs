using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
