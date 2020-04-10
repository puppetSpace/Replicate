using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Test
{
    internal static class Helper
    {

        public static string CreateBase64HashForFile(string path)
        {
            MD5 hashCreator = MD5.Create();
            var hash = hashCreator.ComputeHash(File.ReadAllBytes(path));
            return Convert.ToBase64String(hash);
        }

        public static async Task<FileInfo> CompressFile(string path)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            using var stream = System.IO.File.OpenRead(path);
            using var output = System.IO.File.OpenWrite(tempPath);
            using var gzip = new GZipStream(output, CompressionMode.Compress);

            await stream.CopyToAsync(gzip);

            return new FileInfo(tempPath);
        }
    }
}
