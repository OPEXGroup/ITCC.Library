using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Utils;
using ITCC.Logging;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class ImagePreprocessTask : BaseFilePreprocessTask
    {
        #region static
        
        #endregion

        #region BaseFilePreprocessTask
        public override FileType FileType => FileType.Image;
        public override string FileName { get; set; }

        public override bool Perform()
        {
            try
            {
                float originalWidth;
                float originalHeight;
                using (var img = Image.FromFile(FileName))
                {
                    originalWidth = img.Width;
                    originalHeight = img.Height;
                }

                LogMessage(LogLevel.Debug,
                    $"Processing image {FileName}. Original resolution {(int) originalWidth}x{(int) originalHeight}");

                ImageFormat format;
                var extension = IoHelper.GetExtension(FileName);
                if (extension == null)
                    return true;
                if (!ImageFormatDictionary.TryGetValue(extension, out format))
                {
                    LogMessage(LogLevel.Warning, $"Unknown image format: {extension}");
                    return false;
                }
                foreach (var multiplier in Constants.ResolutionMultipliers)
                {
                    var newWidth = (int) (multiplier*originalWidth);
                    var newHeight = (int) (multiplier*originalHeight);
                    using (var bitmap = (Bitmap) Image.FromFile(FileName))
                    {
                        using (var newBitmap = new Bitmap(bitmap, newWidth, newHeight))
                        {
                            var newFileName = IoHelper.AddBeforeExtension(FileName,
                                $"{Constants.ChangedString}{newWidth}x{newHeight}");
                            LogMessage(LogLevel.Trace, $"Creating image {newFileName}");
                            newBitmap.Save(newFileName, format);
                        }
                    }
                }

                return true;
            }
            catch (OutOfMemoryException)
            {
                using (File.Create(IoHelper.AddBeforeExtension(FileName, $"{Constants.ChangedString}_FAILED")))
                {
                }
                return false;
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return false;
            }
        }
        #endregion

        #region private
        private static readonly Dictionary<string, ImageFormat> ImageFormatDictionary = new Dictionary<string, ImageFormat>
        {
            {"jpg", ImageFormat.Jpeg },
            {"jpeg", ImageFormat.Jpeg },
            {"png", ImageFormat.Png },
            {"gif", ImageFormat.Gif },
            {"ico", ImageFormat.Icon },
            {"bmp", ImageFormat.Bmp }
        };
        #endregion
    }
}
