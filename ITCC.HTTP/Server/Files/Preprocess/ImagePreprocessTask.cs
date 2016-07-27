using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class ImagePreprocessTask : BaseFilePreprocessTask
    {
        #region BaseFilePreprocessTask
        public override FileType FileType => FileType.Image;
        public override string FileName { get; set; }

        public override bool Perform()
        {
            return true;
        }
        #endregion
    }
}
