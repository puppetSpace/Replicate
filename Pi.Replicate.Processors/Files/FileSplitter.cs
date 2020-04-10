using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Helpers;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace Pi.Replicate.Processing.Files
{
	public sealed class FileSplitter
	{
		private readonly int _sizeofChunkInBytes;
		private readonly PathBuilder _pathBuilder;
		private readonly Action<byte[]> _chunkCreatedDelegate;
		private static byte[] _emptyResultResult = new byte[0];

		public FileSplitter(IConfiguration configuration, PathBuilder pathBuilder, Action<byte[]> chunkCreatedDelegate)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_pathBuilder = pathBuilder;
			_chunkCreatedDelegate = chunkCreatedDelegate;
		}

		public async Task<byte[]> ProcessFile(File file)
		{
			var path = _pathBuilder.BuildPath(file);

			if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				Log.Verbose($"File '{path}' is being processed and turned into chunks of {_sizeofChunkInBytes} bytes");

				var pathOfCompressed = await CompressFile(path);

				using (var stream = System.IO.File.OpenRead(pathOfCompressed))
				{
					return await SplitStream(stream);
				}
			}
			else
			{
				Log.Warning($"File '{path}' does not exist or is locked. File will not be processedd");
				return _emptyResultResult;
			}
		}

		private async Task<string> CompressFile(string path)
		{
			var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			using var stream = System.IO.File.OpenRead(path);
			using var output = System.IO.File.OpenWrite(tempPath);
			using var gzip = new GZipStream(output, CompressionMode.Compress);

			await stream.CopyToAsync(gzip);

			return tempPath;
		}

		private async Task<byte[]> SplitStream(System.IO.Stream stream)
		{
			MD5 hashCreator = MD5.Create();
			var buffer = ArrayPool<byte>.Shared.Rent(_sizeofChunkInBytes);
			int bytesRead;
			//todo compression
			while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);
				_chunkCreatedDelegate?.Invoke(buffer);
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

		public FileSplitter Get(Action<byte[]> chunkCreatedDelegate)
		{
			return new FileSplitter(_configuration, _pathBuilder,chunkCreatedDelegate);
		}
	}
}
