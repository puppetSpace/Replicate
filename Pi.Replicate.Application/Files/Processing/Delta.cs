using Octodiff.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Processing
{
    //https://github.com/OctopusDeploy/Octodiff
    public class Delta
    {

        public byte[] CreateSignature(Stream input)
        {
            var signatureBuilder = new SignatureBuilder();
            var memoryStream = new MemoryStream();
            var signatureWriter = new SignatureWriter(memoryStream);
            signatureBuilder.Build(input, signatureWriter);
            return memoryStream.ToArray();
        }

        public byte[] CreateSignature(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
                return CreateSignature(fs);
        }

        public byte[] CreateDelta(Stream input, byte[] signature)
        {
            var deltaStream = new MemoryStream();
            var signatureReader = new SignatureReader(new MemoryStream(signature), null);
            var deltaWriter = new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream));
            var deltaBuilder = new DeltaBuilder();
            deltaBuilder.BuildDelta(input, signatureReader, deltaWriter);

            return deltaStream.ToArray();
        }

        public byte[] CreateDelta(string path, byte[] signature)
        {
            using (var fs = new FileStream(path, FileMode.Open))
                return CreateDelta(fs, signature);
        }

        public void ApplyDelta(Stream input, byte[] delta, Stream output)
        {
            var deltaReader = new BinaryDeltaReader(new MemoryStream(delta), null);
            var deltaApplier = new DeltaApplier { SkipHashCheck = true };
            deltaApplier.Apply(input, deltaReader, output);
        }

        public void ApplyDelta(string path, byte[] delta)
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
    }
}
