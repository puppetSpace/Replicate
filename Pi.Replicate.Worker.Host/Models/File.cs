using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Models
{
	public class File
	{
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Name { get; set; }

		public int Version { get; set; }

		public long Size { get; set; }

		public FileSource Source { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string Path { get; set; }

		public bool IsNew() => Version == 1;

		public void Update(System.IO.FileInfo file)
		{
			Id = Guid.NewGuid();
			LastModifiedDate = file.LastWriteTimeUtc;
			Size = file.Length;
			Version++;

		}

		public async Task CompressTo(string path)
		{
			using var stream = System.IO.File.OpenRead(PathBuilder.BuildPath(Path));
			using var output = System.IO.File.OpenWrite(path);
			using var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress);

			await stream.CopyToAsync(gzip);
		}

		public async Task DecompressFrom(string compressedFilePath)
		{
			using var stream = System.IO.File.OpenRead(compressedFilePath);
			using var output = System.IO.File.Create(PathBuilder.BuildPath(Path));
			using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);

			await gzip.CopyToAsync(output);
		}

		public ReadOnlyMemory<byte> CreateSignature()
		{
			using (var fs = System.IO.File.OpenRead(PathBuilder.BuildPath(Path)))
			{
				var signatureBuilder = new Octodiff.Core.SignatureBuilder();
				var memoryStream = new System.IO.MemoryStream();
				var signatureWriter = new Octodiff.Core.SignatureWriter(memoryStream);
				signatureBuilder.Build(fs, signatureWriter);
				return memoryStream.ToArray();
			}
		}

		public ReadOnlyMemory<byte> CreateDelta(ReadOnlyMemory<byte> signature)
		{
			using (var fs = System.IO.File.OpenRead(PathBuilder.BuildPath(Path)))
			{
				var deltaStream = new System.IO.MemoryStream();
				var signatureReader = new Octodiff.Core.SignatureReader(new System.IO.MemoryStream(signature.ToArray()), LogProgressReporter.Get());
				var deltaWriter = new Octodiff.Core.AggregateCopyOperationsDecorator(new Octodiff.Core.BinaryDeltaWriter(deltaStream));
				var deltaBuilder = new Octodiff.Core.DeltaBuilder();
				deltaBuilder.BuildDelta(fs, signatureReader, deltaWriter);

				return deltaStream.ToArray();
			}

		}

		public void ApplyDelta(ReadOnlyMemory<byte> delta)
		{
			var path = PathBuilder.BuildPath(Path);
			var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			using (var fs = System.IO.File.OpenRead(path))
			{
				using (var fsout = new FileStream(tempPath, FileMode.Create))
				{
					var deltaReader = new Octodiff.Core.BinaryDeltaReader(new MemoryStream(delta.ToArray()), LogProgressReporter.Get());
					var deltaApplier = new Octodiff.Core.DeltaApplier { SkipHashCheck = true };
					deltaApplier.Apply(fs, deltaReader, fsout);
				}
			}

			//move temp to original
			System.IO.File.Copy(tempPath, path, overwrite: true);
		}

		public static File Build(System.IO.FileInfo file, Guid folderId, string basePath, DateTime? customLastModified = null)
		{
			if (file is null || !file.Exists)
				throw new InvalidOperationException($"Cannot created a File object for a file that does not exists: '{file?.FullName}'");

			return new File
			{
				Id = Guid.NewGuid(),
				FolderId = folderId,
				LastModifiedDate = customLastModified ?? file.LastWriteTimeUtc,
				Name = file.Name,
				Path = file.FullName.Replace(basePath + "\\", ""), //must be relative to base
				Size = file.Length,
				Source = FileSource.Local,
				Version = 1

			};
		}

		private class LogProgressReporter : Octodiff.Diagnostics.IProgressReporter
		{
			public void ReportProgress(string operation, long currentPosition, long total)
			{
				WorkerLog.Instance.Verbose($"operation: {operation}, currentPosition: {currentPosition}, total: {total}");
			}

			public static Octodiff.Diagnostics.IProgressReporter Get() => new LogProgressReporter();
		}

	}

	public class RequestFile : File
	{
		public ICollection<Recipient> Recipients { get; set; }
	}

	public enum FileSource
	{
		Local = 0,
		Remote = 1
	}
}