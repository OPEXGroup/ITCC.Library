using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.Server.Files
{
    internal static class Constants
    {
        /// <summary>
        ///     Resolution multipliers for video and image compression
        /// </summary>
        public static readonly double[] ResolutionMultipliers = { 0.1, 0.25, 0.5, 0.75, 1.0 };

        public const string ChangedString = "_CHANGED_";
    }
}
