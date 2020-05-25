using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.EofMessages.Commands.AddToSendEofMessage;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Files.Queries.GetPreviousSignatureOfFile;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Infrastructure.Services;
using Pi.Replicate.Shared;
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
			int amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk, Task>(x => { amountOfCalls++; return Task.CompletedTask; }) ;

			var mediator = CreateMediatorEofMessageMock();

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), mediator.Object);
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
			int amountOfCalls = 0;
			int eofMessageAmountOfChunks = 0;
			var chunkCreated = new Func<FileChunk,Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())))
				.Callback<IRequest<Result<EofMessage>>, CancellationToken>((x, c) =>
			 {
				 if (x is AddToSendEofMessageCommand eof)
					 eofMessageAmountOfChunks = eof.AmountOfChunks;
			 });

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), mockmockMediator.Object);
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
			int amountOfCalls = 0;
			bool getPreviousSignatureOfFileQueryCalled = false;
			var chunkCreated = new Func<FileChunk,Task>(x => { amountOfCalls++;return Task.CompletedTask; });

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<ReadOnlyMemory<byte>>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((IBaseRequest x, CancellationToken y) =>
				 {
					 getPreviousSignatureOfFileQueryCalled = true;
					 return Result<ReadOnlyMemory<byte>>.Success(Helper.GetReadOnlyMemory());
				 });
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IBaseRequest x, CancellationToken y) => Result<EofMessage>.Success(new EofMessage()));

			var deltaServiceMock = CreateDeltaServiceMock();

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, mockmockMediator.Object);
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
			int amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk,Task>(x => { amountOfCalls++;return Task.CompletedTask; });

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<ReadOnlyMemory<byte>>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((IBaseRequest x, CancellationToken y) => Result<ReadOnlyMemory<byte>>.Success(Helper.GetReadOnlyMemory()));
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IBaseRequest x, CancellationToken y) => Result<EofMessage>.Success(new EofMessage()));

			var deltaServiceMock = CreateDeltaServiceMock();

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, mockmockMediator.Object);
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
			int amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk,Task>(x => { amountOfCalls++;return Task.CompletedTask; });

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<ReadOnlyMemory<byte>>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((IBaseRequest x, CancellationToken y) => Result<ReadOnlyMemory<byte>>.Success(Helper.GetReadOnlyMemory()));

			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IBaseRequest x, CancellationToken y) => Result<EofMessage>.Success(new EofMessage()));

			var deltaServiceMock = CreateDeltaServiceMock();

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, deltaServiceMock.Object, mockmockMediator.Object);
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

			int amountOfCalls = 0;
			var chunkCreated = new Func<FileChunk,Task>(x => { amountOfCalls++; return Task.CompletedTask; });

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())));

			var processService = new FileDisassemblerService(configuration.Object, new CompressionService(), pathBuilder, new DeltaService(), mockmockMediator.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);


			Assert.AreEqual(0, amountOfCalls);

		}

		private Mock<IConfiguration> CreateConfigurationMock()
		{
			int minimumAmountOfBytesRentedByArrayPool = 128;
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

		private Mock<IMediator> CreateMediatorEofMessageMock()
		{
			var mockMediator = new Mock<IMediator>();
			mockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())));

			return mockMediator;
		}

	}
}
