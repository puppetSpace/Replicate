using Octodiff.Core;
using Octodiff.Diagnostics;
using System;
using System.IO;

namespace Pi.Replicate.Worker.Host.Services
{
	//https://github.com/OctopusDeploy/Octodiff

	public interface IDeltaService
	{
		ReadOnlyMemory<byte> CreateSignature(string path);
		ReadOnlyMemory<byte> CreateDelta(string path, ReadOnlyMemory<byte> signature);
		void ApplyDelta(string path, ReadOnlyMemory<byte> delta);
	}

	public class DeltaService : IDeltaService
	{

		public ReadOnlyMemory<byte> CreateSignature(string path)
		{
			using (var fs = System.IO.File.OpenRead(path))
			{
				var signatureBuilder = new SignatureBuilder();
				var memoryStream = new MemoryStream();
				var signatureWriter = new SignatureWriter(memoryStream);
				signatureBuilder.Build(fs, signatureWriter);
				return memoryStream.ToArray();
			}
		}

		public ReadOnlyMemory<byte> CreateDelta(string path, ReadOnlyMemory<byte> signature)
		{
			using (var fs = System.IO.File.OpenRead(path))
			{
				var deltaStream = new MemoryStream();
				var signatureReader = new SignatureReader(new MemoryStream(signature.ToArray()), LogProgressReporter.Get());
				var deltaWriter = new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream));
				var deltaBuilder = new DeltaBuilder();
				deltaBuilder.BuildDelta(fs, signatureReader, deltaWriter);

				return deltaStream.ToArray();
			}

		}

		public void ApplyDelta(string path, ReadOnlyMemory<byte> delta)
		{
			var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			using (var fs = System.IO.File.OpenRead(path))
			{
				using (var fsout = new FileStream(tempPath, FileMode.Create))
				{
					var deltaReader = new BinaryDeltaReader(new MemoryStream(delta.ToArray()), LogProgressReporter.Get());
					var deltaApplier = new DeltaApplier { SkipHashCheck = true };
					deltaApplier.Apply(fs, deltaReader, fsout);
				}
			}

			//move temp to original
			System.IO.File.Copy(tempPath, path, overwrite: true);
		}

		private class LogProgressReporter : IProgressReporter
		{
			public void ReportProgress(string operation, long currentPosition, long total)
			{
				WorkerLog.Instance.Verbose($"operation: {operation}, currentPosition: {currentPosition}, total: {total}");
			}

			public static IProgressReporter Get() => new LogProgressReporter();
		}
	}


}
