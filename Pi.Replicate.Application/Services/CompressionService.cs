using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
    public class CompressionService
    {
        public async Task<string> CompressFileToTempFile(string path)
        {
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
            using var stream = System.IO.File.OpenRead(path);
            using var output = System.IO.File.OpenWrite(tempPath);
            using var gzip = new GZipStream(output, CompressionMode.Compress);

            await stream.CopyToAsync(gzip);

            return tempPath;
        }

        public async Task Decompress(string compressedFile,string destination)
        {
            using var stream = System.IO.File.OpenRead(compressedFile);
            using var output = System.IO.File.Create(destination);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);

            await gzip.CopyToAsync(output);

        }
    }
}
