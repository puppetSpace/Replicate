using Octodiff.Core;
using Octodiff.Diagnostics;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
    //https://github.com/OctopusDeploy/Octodiff
    public class DeltaService
    {

        public ReadOnlyMemory<byte> CreateSignature(Stream input)
        {
            var signatureBuilder = new SignatureBuilder();
            var memoryStream = new MemoryStream();
            var signatureWriter = new SignatureWriter(memoryStream);
            signatureBuilder.Build(input, signatureWriter);
            return memoryStream.ToArray();
        }

        public ReadOnlyMemory<byte> CreateSignature(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
                return CreateSignature(fs);
        }

        public ReadOnlyMemory<byte> CreateDelta(Stream input, ReadOnlyMemory<byte> signature)
        {
            var deltaStream = new MemoryStream();
            var signatureReader = new SignatureReader(new MemoryStream(signature.ToArray()), LogProgressReporter.Get());
            var deltaWriter = new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream));
            var deltaBuilder = new DeltaBuilder();
            deltaBuilder.BuildDelta(input, signatureReader, deltaWriter);

            return deltaStream.ToArray();
        }

        public ReadOnlyMemory<byte> CreateDelta(string path, ReadOnlyMemory<byte> signature)
        {
            using (var fs = new FileStream(path, FileMode.Open))
                return CreateDelta(fs, signature);
        }

        public void ApplyDelta(Stream input, ReadOnlyMemory<byte> delta, Stream output)
        {
            var deltaReader = new BinaryDeltaReader(new MemoryStream(delta.ToArray()), LogProgressReporter.Get());
            var deltaApplier = new DeltaApplier { SkipHashCheck = true };
            deltaApplier.Apply(input, deltaReader, output);
        }

        public void ApplyDelta(string path, ReadOnlyMemory<byte> delta)
        {
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var fsout = new FileStream(tempPath, FileMode.Create))
                {
                    ApplyDelta(fs,delta,fsout);
                }
            }

            //move temp to original
            System.IO.File.Copy(tempPath,path,overwrite:true);
        }

        private class LogProgressReporter : IProgressReporter
        {
            public void ReportProgress(string operation, long currentPosition, long total)
            {
                Log.Verbose($"opertaion: {operation}, currentPosition: {currentPosition}, total: {total}");
            }

            public static IProgressReporter Get() => new LogProgressReporter();
        }
    }

    
}
