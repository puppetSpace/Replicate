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
		private static readonly byte[] _emptyResultResult = Array.Empty<byte>();

		public FileSplitter(IConfiguration configuration, PathBuilder pathBuilder)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_pathBuilder = pathBuilder;
		}

		public async Task<byte[]> ProcessFile(File file, Func<byte[],Task> chunkCreatedDelegate)
		{
			var path = _pathBuilder.BuildPath(file.Path);

			if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				Log.Information($"'{path}' is being compressed");
				var pathOfCompressed = await CompressFile(path);

				Log.Information($"'{path}' is being split");
				byte[] hash;
				using (var stream = System.IO.File.OpenRead(pathOfCompressed))
				{
					hash =  await SplitStream(stream, chunkCreatedDelegate);
				}
				Log.Information($"'compressed file of {path}' is being deleted");
				System.IO.File.Delete(pathOfCompressed);
				return hash;
			}
			else
			{
				Log.Warning($"File '{path}' does not exist or is locked. File will not be processedd");
				return _emptyResultResult;
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

		private async Task<byte[]> SplitStream(System.IO.Stream stream, Func<byte[],Task> chunkCreatedDelegate)
		{
			MD5 hashCreator = MD5.Create();
			var buffer = ArrayPool<byte>.Shared.Rent(_sizeofChunkInBytes);
			int bytesRead;
			while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);
				await chunkCreatedDelegate(buffer);
			}
			hashCreator.TransformFinalBlock(buffer, 0, bytesRead);

			ArrayPool<byte>.Shared.Return(buffer);
			return hashCreator.Hash;
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
