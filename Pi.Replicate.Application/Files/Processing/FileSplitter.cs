using Microsoft.Extensions.Configuration;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Buffers;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace Pi.Replicate.Application.Files.Processing
{
	public sealed class FileSplitter
	{
		private readonly int _sizeofChunkInBytes;
		private readonly PathBuilder _pathBuilder;

		public FileSplitter(IConfiguration configuration, PathBuilder pathBuilder)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_pathBuilder = pathBuilder;
		}

		public async Task ProcessFile(File file, Func<byte[],Task> chunkCreatedDelegate)
		{
			var path = _pathBuilder.BuildPath(file.Path);

			if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				Log.Information($"'{path}' is being compressed");
				var pathOfCompressed = await CompressFile(path);

				Log.Information($"'{path}' is being split");
				using (var stream = System.IO.File.OpenRead(pathOfCompressed))
					await SplitStream(stream, chunkCreatedDelegate);

				Log.Information($"'compressed file of {path}' is being deleted");
				System.IO.File.Delete(pathOfCompressed);
			}
			else
			{
				Log.Warning($"File '{path}' does not exist or is locked. File will not be processedd");
			}
		}

		//todo move to seperate class
		private async Task<string> CompressFile(string path)
		{
			var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			using var stream = System.IO.File.OpenRead(path);
			using var output = System.IO.File.OpenWrite(tempPath);
			using var gzip = new GZipStream(output, CompressionMode.Compress);

			await stream.CopyToAsync(gzip);

			return tempPath;
		}

		private async Task SplitStream(System.IO.Stream stream, Func<byte[],Task> chunkCreatedDelegate)
		{
			var buffer = ArrayPool<byte>.Shared.Rent(_sizeofChunkInBytes);
			while ((await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				await chunkCreatedDelegate(buffer);
			}
			ArrayPool<byte>.Shared.Return(buffer);
		}

	}


	public class FileSplitterFactory
	{
		private readonly IConfiguration _configuration;
		private readonly PathBuilder _pathBuilder;

		public FileSplitterFactory(IConfiguration configuration, PathBuilder pathBuilder)
		{
			_configuration = configuration;
			_pathBuilder = pathBuilder;
		}

		public FileSplitter Get()
		{
			return new FileSplitter(_configuration, _pathBuilder);
		}
	}
}
