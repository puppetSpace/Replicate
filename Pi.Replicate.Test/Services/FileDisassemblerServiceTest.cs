using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.EofMessages.Commands.AddToSendEofMessage;
using Pi.Replicate.Application.Files.Processing;
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
		//[TestInitialize]
		//public void InitializeTest()
		//{
		//    EntityBuilder.InitializePathBuilder();
		//}

		[TestMethod]
		public async Task ProcessFile_CorrectAmountOfChunksShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var calculatedAmountOfChunks = Math.Ceiling((double)compressedFile.Length / int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]));
			int amountOfCalls = 0;
			var chunkCreated = new Action<FileChunk>(x => amountOfCalls++);

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())));

			var processService = new FileDisassemblerService(configuration, new CompressionService(), pathBuilder, new DeltaService(), mockmockMediator.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.AreEqual(calculatedAmountOfChunks, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_EofMessageShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			int amountOfCalls = 0;
			int eofMessageAmountOfChunks = 0;
			var chunkCreated = new Action<FileChunk>(x => amountOfCalls++);

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())))
				.Callback<IRequest<Result<EofMessage>>,CancellationToken>((x,c) =>
			{
				if (x is AddToSendEofMessageCommand eof)
					eofMessageAmountOfChunks = eof.AmountOfChunks;
			});

			var processService = new FileDisassemblerService(configuration, new CompressionService(), pathBuilder, new DeltaService(), mockmockMediator.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

			Assert.AreEqual(eofMessageAmountOfChunks, amountOfCalls);

		}


		[TestMethod]
		public async Task ProcessFile_FileIsLocked_ShouldReturnEmptyResultSet()
		{
			var configuration = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configuration);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			int amountOfCalls = 0;
			var chunkCreated = new Action<FileChunk>(x => amountOfCalls++);

			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest<Result<EofMessage>>>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Result<EofMessage>.Success(new EofMessage())));

			var processService = new FileDisassemblerService(configuration, new CompressionService(), pathBuilder, new DeltaService(), mockmockMediator.Object);
			await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);


			Assert.AreEqual(0, amountOfCalls);

		}


		private IConfiguration CreateConfigurationMock()
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

			return configurationMock.Object;
		}

		private IMediator CreateBareMediatorMock()
		{
			var mockmockMediator = new Mock<IMediator>();
			mockmockMediator.Setup(x => x.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
				.Returns(Unit.Task);

			return mockmockMediator.Object;
		}
	}
}
