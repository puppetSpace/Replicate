using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Infrastructure.Services
{
    public class CompressionService : ICompressionService
    {
        public async Task Compress(string source,string destination)
        {
            using var stream = System.IO.File.OpenRead(source);
            using var output = System.IO.File.OpenWrite(destination);
            using var gzip = new GZipStream(output, CompressionMode.Compress);

            await stream.CopyToAsync(gzip);
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
