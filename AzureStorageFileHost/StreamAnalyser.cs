using System.IO;

namespace AzureStorageFileHost
{
    public static class StreamAnalyser
    {
        public static bool ProbablyResizableImage(Stream stream, string contentType)
        {
            stream.Position = 0;
            using (var reader = new BinaryReader(stream))
            {
                switch (contentType)
                {
                    case "image/png":
                        return
                            reader.ReadUInt64() == 0xa1a0a0d474e5089;
                    case "image/jpeg":
                        var firstBytes = reader.ReadUInt32();
                        return
                            firstBytes == 0xdbffd8ff ||
                            firstBytes == 0xe1ffd8ff;
                    default:
                        return false;
                }
            }
        }
    }
}