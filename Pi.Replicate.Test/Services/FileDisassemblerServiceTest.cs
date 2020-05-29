using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
	public class FileDisassemblerServiceTest
	{

		[TestMethod]
		public async Task ProcessFile_CorrectAmountOfChunksShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var calculatedAmountOfChunks = Math.Ceiling((double)compressedFile.Length / int.Parse(configuration.Object[Constants.FileSplitSizeOfChunksInBytes]));
			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(),webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.AreEqual(calculatedAmountOfChunks, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_EofMessageShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var amountOfCalls = 0;
			var eofMessageAmountOfChunks = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success())
				.Callback<EofMessage>(x => eofMessageAmountOfChunks = x.AmountOfChunks);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.AreEqual(eofMessageAmountOfChunks, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_Changed_CallToGetPreviousSignatureShouldBeMade()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var amountOfCalls = 0;
			var getPreviousSignatureOfFileQueryCalled = false;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(Helper.GetByteArray()))
				.Callback<Guid>(x=>getPreviousSignatureOfFileQueryCalled = true);

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var deltaServiceMock = CreateDeltaServiceMock();
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var domainFile = File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(fileInfo);
			var eofFile = await processService.ProcessFile(domainFile, chunkCreated);

			Assert.IsTrue(getPreviousSignatureOfFileQueryCalled);
		}
		[TestMethod]
		public async Task ProcessFile_Changed_EofMessageShouldbeMade()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(Helper.GetByteArray()));

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var deltaServiceMock = CreateDeltaServiceMock();
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var domainFile = File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(fileInfo);
			var eofFile = await processService.ProcessFile(domainFile, chunkCreated);

			Assert.IsNotNull(eofFile);
		}

		[TestMethod]
		public async Task ProcessFile_Changed_1ChunkShouldBeMade()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(Helper.GetByteArray()));

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var deltaServiceMock = CreateDeltaServiceMock();
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var domainFile = File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(fileInfo);
			var eofFile = await processService.ProcessFile(domainFile, chunkCreated);

			Assert.AreEqual(1, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_ShouldReturnEmptyResultSet()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);


			Assert.AreEqual(0, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_WebhookFileDissambledShouldBeCalled()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var webhookCalled = false;
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { webhookCalled = true; }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.IsTrue(webhookCalled);
		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_WebhookFailedFileShouldBeCalled()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var fileRepositoryMock = new Mock<IFileRepository>();
			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var webhookCalled = false;
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { webhookCalled = true; });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);


			Assert.IsTrue(webhookCalled);

		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_FailedFileShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; });
			var isFailedFileRequestCalled = false;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.UpdateFileAsFailed(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Success())
				.Callback<Guid>(x => isFailedFileRequestCalled = true);

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), webhookMock.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.IsTrue(isFailedFileRequestCalled);

		}

		private Mock<IConfiguration> CreateConfigurationMock()
		{
			var minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
				x switch
				{
					Constants.ReplicateBasePath => System.IO.Directory.GetCurrentDirectory(),
					Constants.FileSplitSizeOfChunksInBytes => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});

			return configurationMock;
		}

		private Mock<IDeltaService> CreateDeltaServiceMock()
		{
			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>())).Returns(() => Helper.GetReadOnlyMemory());
			return deltaServiceMock;
		}
	}
}
