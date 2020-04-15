﻿using Octodiff.Core;
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

    }
}
