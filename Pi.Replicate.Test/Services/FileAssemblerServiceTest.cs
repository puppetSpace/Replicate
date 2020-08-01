using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Shared.Models;

namespace Pi.Replicate.Test.Processors
{
	//todo test if delta is being applied correctly
	[TestClass]
	public class FileAssemblerServiceTest
	{
		[TestInitialize]
		public void Initialize()
		{
			var stream = System.IO.File.Create(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt"));
			stream.Close();
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_QueryToGetChunkShouldGetCalledTwice()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var amountofTimesQueryCalled = 0;
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns((Guid x, int y, int z, IDatabase d) =>
				{
					amountofTimesQueryCalled++;
					return Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>()));
				});

			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(2, amountofTimesQueryCalled);
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_QueryToGetChunkShouldGetCorrectParameters()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel(); 
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();
			var toSkipSum = 0;
			var toTakeSum = 0;

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns((Guid x, int y, int z, IDatabase d) =>
				{
					toSkipSum += y;
					toTakeSum += z;
					return Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>()));
				});

			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(11, toSkipSum);
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_FinalizationShouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>())));

			var fileChunkDeleteCalled = false;
			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Callback(() => fileChunkDeleteCalled = true)
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			var updateFileCalled = false;
			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Callback(() => updateFileCalled = true)
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(updateFileCalled);
			Assert.IsTrue(fileChunkDeleteCalled);
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_QueryToGetChunkShouldGetCalledTwice()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var amountofTimesQueryCalled = 0;
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns((Guid x, int y, int z, IDatabase d) =>
				{
					amountofTimesQueryCalled++;
					return Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>()));
				});

			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(2, amountofTimesQueryCalled);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_QueryToGetChunkShouldGetCorrectParameters()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var toSkipSum = 0;
			var toTakeSum = 0;
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns((Guid x, int y, int z, IDatabase d) =>
				{
					toSkipSum += y;
					toTakeSum += z;
					return Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>()));
				});

			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(11, toSkipSum);
			Assert.AreEqual(30, toTakeSum);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_ApplyDeltaChouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>())));

			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService( databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_FinalizationShouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = new EofMessage(Guid.Empty, 15);
			var executedStatements = new List<string>();
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>())));

			var fileChunkDeleteCalled = false;
			fileChunkRepositoryMock.Setup(x => x.DeleteChunksForFile(It.IsAny<Guid>(), It.IsAny<IDatabase>()))
				.Callback(() => fileChunkDeleteCalled = true)
				.Returns(() => Task.FromResult(Result.Success()));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			var updateFileCalled = false;
			fileRepositoryMock.Setup(x => x.UpdateFileAsAssembled(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<byte[]>(), It.IsAny<IDatabase>()))
				.Callback(() => updateFileCalled = true)
				.Returns(() => Task.FromResult(Result.Success()));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(updateFileCalled);
			Assert.IsTrue(fileChunkDeleteCalled);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_CallToWebhookShouldBeMade()
		{
			var configMock = CreateConfigurationMock();
			var domainFile = Helper.GetFileModel();
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage =new EofMessage(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();

			var fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
			fileChunkRepositoryMock.Setup(x => x.GetFileChunkData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<byte[]>>.Success(new List<byte[]>())));

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetAllVersionsOfFile(It.IsAny<File>(), It.IsAny<IDatabase>()))
				.Returns(() => Task.FromResult(Result<ICollection<File>>.Success(new List<File>())));

			var fileConflictServiceMock = new Mock<IFileConflictService>();
			fileConflictServiceMock.Setup(x => x.Check(It.IsAny<File>(), It.IsAny<ICollection<File>>())).ReturnsAsync(true);

			var webhookIsCalled = false;
			var webhookMock = Helper.GetWebhookServiceMock(x => { webhookIsCalled = true; }, x => { }, x => { });


			var fileAssemblerService = new FileAssemblerService(databaseMock.Object, webhookMock.Object, fileRepositoryMock.Object, fileChunkRepositoryMock.Object, fileConflictServiceMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(webhookIsCalled);
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
