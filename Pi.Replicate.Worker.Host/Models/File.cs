using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Models
{
	public class File
	{

		public File()
		{

		}

		public File(Guid id, Guid folderId, string name, string relativePath, DateTime lastModifiedDate, long size, FileSource source, int version)
		{
			Id = id;
			FolderId = folderId;
			Name = name;
			RelativePath = relativePath;
			LastModifiedDate = lastModifiedDate;
			Size = size;
			Source = source;
			Version = version;
		}

		public File(System.IO.FileInfo file, Guid folderId, string basePath, DateTime? customLastModified = null):this()
		{
			if (file is null || !file.Exists)
				throw new InvalidOperationException($"Cannot created a File object for a file that does not exists: '{file?.FullName}'");

			Id = Guid.NewGuid();
			FolderId = folderId;
			LastModifiedDate = customLastModified ?? file.LastWriteTimeUtc;
			Name = file.Name;
			RelativePath = file.FullName.Replace(basePath + "\\", ""); //must be relative to base
			Size = file.Length;
			Source = FileSource.Local;
			Version = 1;
		}

		public Guid Id { get; protected set; }

		public Guid FolderId { get; protected set; }

		public string Name { get; protected set; }

		public int Version { get; protected set; }

		public long Size { get; protected set; }

		public FileSource Source { get; protected set; }

		public DateTime LastModifiedDate { get; protected set; }

		public string RelativePath { get; protected set; }

		public bool IsNew() => Version == 1;

		public void Update(System.IO.FileInfo file)
		{
			if (!file.Exists)
				throw new FileNotFoundException("File does not exist. Can only use existing files to update");
			if (!string.Equals(file.Name, Name, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("Cannot use file to update current file because the names differ");
			Id = Guid.NewGuid();
			LastModifiedDate = file.LastWriteTimeUtc;
			Size = file.Length;
			Version++;

		}

		public string GetFullPath()
		{
			return System.IO.Path.Combine(PathBuilder.BasePath, RelativePath??"");
		}

		public async Task CompressTo(string path)
		{
			using var stream = System.IO.File.OpenRead(GetFullPath());
			using var output = System.IO.File.OpenWrite(path);
			using var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress);

			await stream.CopyToAsync(gzip);
		}

		public async Task DecompressFrom(string compressedFilePath)
		{
			var path = GetFullPath();
			var fileFolder = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(fileFolder))
				System.IO.Directory.CreateDirectory(fileFolder);

			using var stream = System.IO.File.OpenRead(compressedFilePath);
			using var output = System.IO.File.Create(path);
			using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);

			await gzip.CopyToAsync(output);
		}

		//todo check to see if you can change Octodiff to use byte[] instead of stream
		public byte[] CreateSignature()
		{
			using (var fs = System.IO.File.OpenRead(GetFullPath()))
			{
				var signatureBuilder = new Octodiff.Core.SignatureBuilder();
				var memoryStream = new System.IO.MemoryStream();
				var signatureWriter = new Octodiff.Core.SignatureWriter(memoryStream);
				signatureBuilder.Build(fs, signatureWriter);
				return memoryStream.ToArray();
			}
		}

		public byte[] CreateDelta(byte[] signature)
		{
			using (var fs = System.IO.File.OpenRead(GetFullPath()))
			{
				var deltaStream = new System.IO.MemoryStream();
				var signatureReader = new Octodiff.Core.SignatureReader(new System.IO.MemoryStream(signature), LogProgressReporter.Get());
				var deltaWriter = new Octodiff.Core.AggregateCopyOperationsDecorator(new Octodiff.Core.BinaryDeltaWriter(deltaStream));
				var deltaBuilder = new Octodiff.Core.DeltaBuilder();
				deltaBuilder.BuildDelta(fs, signatureReader, deltaWriter);

				return deltaStream.ToArray();
			}

		}

		public void ApplyDelta(byte[] delta)
		{
			if (delta.Length == 0)
				return;

			var path = GetFullPath();
			var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			using (var fs = System.IO.File.OpenRead(path))
			{
				using (var fsout = new FileStream(tempPath, FileMode.Create))
				{
					var deltaReader = new Octodiff.Core.BinaryDeltaReader(new MemoryStream(delta), LogProgressReporter.Get());
					var deltaApplier = new Octodiff.Core.DeltaApplier { SkipHashCheck = true };
					deltaApplier.Apply(fs, deltaReader, fsout);
				}
			}

			//move temp to original
			System.IO.File.Copy(tempPath, path, overwrite: true);
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
		public RequestFile() : base()
		{

		}

		public RequestFile(Guid id, Guid folderId, string name, string relativePath, DateTime lastModifiedDate, long size, FileSource source, int version, List<Recipient> neededRecipients) 
			: base(id,folderId,name,relativePath,lastModifiedDate,size,source,version)
		{
			Recipients = neededRecipients;
		}

		public ICollection<Recipient> Recipients { get; set; }
	}

	public enum FileSource
	{
		Local = 0,
		Remote = 1
	}
}