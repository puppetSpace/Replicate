using Octodiff.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Files
{
    //https://github.com/OctopusDeploy/Octodiff
    public class Delta
    {
        
        public ReadOnlyMemory<byte> CreateSignature(Stream input)
        {
            var signatureBuilder = new SignatureBuilder();
            var memoryStream = new MemoryStream();
            var signatureWriter = new SignatureWriter(memoryStream);
            signatureBuilder.Build(input, signatureWriter);
            return memoryStream.ToArray().AsMemory();
        }

    }
}
