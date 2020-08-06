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
		[TestInitialize]
		public void Initialize()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public async Task ProcessFile_CorrectAmountOfChunksShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));
			var compressedFile = await Helper.CompressFile(fileInfo.FullName);
			var calculatedAmountOfChunks = Math.Ceiling((double)compressedFile.Length / int.Parse(configuration.Object[Constants.FileSplitSizeOfChunksInBytes]));
			var amountOfCalls = 0;

			var fileRepositoryMock = new Mock<IFileRepository>();
			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object,fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await fileDisassemblerServiceFactory.Get(Helper.GetFileModel(fileInfo), mockChunkWriter.Object).ProcessFile();

			Assert.AreEqual(calculatedAmountOfChunks, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_EofMessageShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));
			var amountOfCalls = 0;

			var fileRepositoryMock = new Mock<IFileRepository>();
			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var eofMessage = await fileDisassemblerServiceFactory.Get(Helper.GetFileModel(fileInfo), mockChunkWriter.Object).ProcessFile();

			Assert.IsNotNull(eofMessage);
			Assert.AreEqual(eofMessage.AmountOfChunks, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_Changed_CallToGetPreviousSignatureShouldBeMade()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));
			var domainFile = Helper.GetFileModel(fileInfo);
			domainFile.Update(fileInfo);
			var amountOfCalls = 0;
			var getPreviousSignatureOfFileQueryCalled = false;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(Helper.GetByteArray()))
				.Callback<Guid>(x=>getPreviousSignatureOfFileQueryCalled = true);

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var eofFile = await fileDisassemblerServiceFactory.Get(domainFile, mockChunkWriter.Object).ProcessFile();

			Assert.IsTrue(getPreviousSignatureOfFileQueryCalled);
		}
		[TestMethod]
		public async Task ProcessFile_Changed_EofMessageShouldbeCreated()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));
			var domainFile = Helper.GetFileModel(fileInfo);
			var amountOfCalls = 0;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(domainFile.CreateSignature()));

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);

			domainFile.Update(fileInfo);
			var eofFile = await fileDisassemblerServiceFactory.Get(Helper.GetFileModel(fileInfo), mockChunkWriter.Object).ProcessFile();

			Assert.IsNotNull(eofFile);
			Assert.AreEqual(eofFile.AmountOfChunks,amountOfCalls);
		}

		[TestMethod]
		public async Task ProcessFile_Changed_1ChunkShouldBeMade()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));
			var domainFile = Helper.GetFileModel(fileInfo);
			domainFile.Update(fileInfo);
			var amountOfCalls = 0;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetSignatureOfPreviousFile(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<byte[]>.Success(domainFile.CreateSignature()));

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var eofFile = await fileDisassemblerServiceFactory.Get(domainFile, mockChunkWriter.Object).ProcessFile();

			Assert.AreEqual(1, amountOfCalls);

		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_ShouldReturnEmptyResultSet()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var amountOfCalls = 0;

			var fileRepositoryMock = new Mock<IFileRepository>();

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();
			mockChunkWriter.Setup(x => x.Push(It.IsAny<FileChunk>())).Callback(() => amountOfCalls++);

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			var eofMessage = await fileDisassemblerServiceFactory.Get(Helper.GetFileModel(fileInfo), mockChunkWriter.Object).ProcessFile();

			Assert.AreEqual(0, amountOfCalls);
			Assert.IsNull(eofMessage);

		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_FailedFileShouldBeCreated()
		{
			var configuration = CreateConfigurationMock();
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var isFailedFileRequestCalled = false;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.UpdateFileAsFailed(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Success())
				.Callback<Guid>(x => isFailedFileRequestCalled = true);

			var eofMessageRepositoryMock = new Mock<IEofMessageRepository>();
			eofMessageRepositoryMock.Setup(x => x.AddEofMessage(It.IsAny<EofMessage>()))
				.ReturnsAsync(() => Result.Success());

			var mockChunkWriter = new Mock<IChunkWriter>();

			var fileDisassemblerServiceFactory = new FileDisassemblerServiceFactory(configuration.Object, fileRepositoryMock.Object, eofMessageRepositoryMock.Object);
			await fileDisassemblerServiceFactory.Get(Helper.GetFileModel(fileInfo), mockChunkWriter.Object).ProcessFile();

			Assert.IsTrue(isFailedFileRequestCalled);

		}

		private Mock<IConfiguration> CreateConfigurationMock()
		{
			var minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
				x switch
				{
					Constants.FileSplitSizeOfChunksInBytes => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});

			return configurationMock;
		}
	}
}
