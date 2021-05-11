using System;
using System.IO;
using System.Security.Cryptography;
using ImageMagick;

namespace image_manipulation
{
    class Program
    {
        static void Main(string[] args)
        {
            MagickNET.SetTempDirectory(Path.Combine("temp"));

            string[] fileEntries = Directory.GetFiles(Path.Combine("images"));

            foreach (string file in fileEntries)
            {
                try
                {
                    ProcessImage(file);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed:" + file);
                }
            }
        }

        private static void ProcessImage(string path)
        {
            var autoRotate = false;
            var rotateValue = 0;
            var compress = true;
            var addWatermark = false;
            var compressionValue = 75;

            var fileInfo = new FileInfo(path);
            var ext = fileInfo.Extension.ToLowerInvariant();

            using (var image = new MagickImage(path))
            {
                if (autoRotate)
                {
                    ExifProfile profile = (ExifProfile)image.GetExifProfile();
                    image.AutoOrient();
                    profile.SetValue(ExifTag.Orientation, (UInt16)0);
                }
                else if (rotateValue != 0)
                {
                    image.Rotate(rotateValue);
                }

                if (compress)
                {
                    image.Quality = compressionValue;
                    image.Depth = 8;
                    image.ColorFuzz = (Percentage)2;
                }

                // if (image.Width > 1400)
                // {
                // }
                {
                    var size = new MagickGeometry(300, 0)
                    {
                        IgnoreAspectRatio = false
                    };
                    image.Resize(size);
                }

                if (addWatermark)
                {
                    using (var watermark = new MagickImage(Path.Combine("watermark", "image.jpg")))
                    {
                        var size = new MagickGeometry(500, 0)
                        {
                            IgnoreAspectRatio = false
                        };
                        watermark.Resize(size);
                        watermark.Format = MagickFormat.Png;
                        watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 2);
                        image.Composite(watermark, Gravity.Center, CompositeOperator.Over);
                    }
                }

                //var fullPath = Path.Combine("new-images", $"{DateTime.Now:yyyy-MM-dd-HH-mm-ssFFFF}{RandomNumberGenerator.GetInt32(100, 999)}{ext}");
                var fullPath = Path.Combine("new-images", fileInfo.Name);

                image.Write(fullPath);

                Console.WriteLine(fullPath);
            }
        }
    }
}
