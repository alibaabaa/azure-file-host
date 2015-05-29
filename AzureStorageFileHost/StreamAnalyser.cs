using System;
using System.Drawing;
using System.IO;

namespace AzureStorageFileHost
{
    public static class StreamAnalyser
    {
        public static bool ProbablyResizableImage(Stream stream, string contentType)
        {
            stream.Position = 0;
            var reader = new BinaryReader(stream);
            switch (contentType)
            {
                case "image/png":
                    return
                        reader.ReadUInt64() == 0xa1a0a0d474e5089;
                case "image/jpeg":
                    //Adapted from http://stackoverflow.com/questions/772388/c-sharp-how-can-i-test-a-file-is-a-jpeg
                    var startOfImage = reader.ReadUInt16();
                    var marker = reader.ReadUInt16();
                    return startOfImage == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
                default:
                    return false;
            }
        }

        public static Tuple<int, int> DimensionsFromImageStream(Stream stream)
        {
            stream.Position = 0;
            var image = Image.FromStream(stream);
            return new Tuple<int, int>(image.Width, image.Height);
        }
    }
}