﻿using Octodiff.Core;
using Octodiff.Diagnostics;
using Pi.Replicate.Application.Common.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Infrastructure.Services
{
    //https://github.com/OctopusDeploy/Octodiff
    public class DeltaService : IDeltaService
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
			using (var fs = System.IO.File.OpenRead(path))
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
            using (var fs = System.IO.File.OpenRead(path))
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
			using (var fs = System.IO.File.OpenRead(path))

			{
				using (var fsout = new FileStream(tempPath, FileMode.Create))
				{
					ApplyDelta(fs, delta, fsout);
				}
			}

            //move temp to original
            System.IO.File.Copy(tempPath,path,overwrite:true);
        }

        private class LogProgressReporter : IProgressReporter
        {
            public void ReportProgress(string operation, long currentPosition, long total)
            {
                Log.Verbose($"operation: {operation}, currentPosition: {currentPosition}, total: {total}");
            }

            public static IProgressReporter Get() => new LogProgressReporter();
        }
    }

    
}