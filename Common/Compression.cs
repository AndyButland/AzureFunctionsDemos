namespace Common
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Helper methods to compress/decompress a string.
    /// Hat-tip: https://stackoverflow.com/a/17993002/489433
    /// </summary>
    public static class StringCompressor
    {
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var stream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var zipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, zipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, zipBuffer, 0, 4);
            return Convert.ToBase64String(zipBuffer);
        }

        public static string DecompressString(string compressedText)
        {
            byte[] zipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(zipBuffer, 0);
                memoryStream.Write(zipBuffer, 4, zipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var stream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    stream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}