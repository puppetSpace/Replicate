using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pi.Replicate.Worker.Host.Models;

namespace Pi.Replicate.Test.Processors
{
	[TestClass]
	public class FileAssemblerServiceTest
	{
		[TestInitialize]
		public void Initialize()
		{
			var stream = System.IO.File.Create(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt"));
			stream.Close();
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_QueryToGetChunkShouldGetCalledTwice()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var amountofTimesQueryCalled = 0;
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, object y) =>
				{
					amountofTimesQueryCalled++;
					return new List<byte[]>();
				});
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });

			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, new DeltaService(), databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(2, amountofTimesQueryCalled);
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_QueryToGetChunkShouldGetCorrectParameters()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();
			var toSkipSum = 0;
			var toTakeSum = 0;
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) =>
				{
					toSkipSum += (int)y.ToSkip;
					toTakeSum += (int)y.ToTake;
					return new List<byte[]>();
				});

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, new DeltaService(), databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(11, toSkipSum);
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_DecompressShoudBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var pathToDecompressTo = "";
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) => new List<byte[]>());

			var compressionServiceMock = new Mock<ICompressionService>();
			compressionServiceMock.Setup(x => x.Decompress(It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string s, string d) =>
				{
					pathToDecompressTo = d;
				});
			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(compressionServiceMock.Object, pathBuilder, new DeltaService(), databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt"), pathToDecompressTo);
		}

		[TestMethod]
		public async Task ProcessFile_NewFile_FinializationShouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var executedStatements = new List<string>();
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) => new List<byte[]>());

			databaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string x, dynamic y) => executedStatements.Add(x));

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, new DeltaService(), databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(executedStatements.Any(x => x.Contains("UPDATE DBO.[File]", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(executedStatements.Any(x => x.Contains("DELETE FROM dbo.FileChunk", StringComparison.OrdinalIgnoreCase)));
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_QueryToGetChunkShouldGetCalledTwice()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var amountofTimesQueryCalled = 0;
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, object y) =>
				{
					amountofTimesQueryCalled++;
					return new List<byte[]>();
				});

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.ApplyDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()));

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, deltaServiceMock.Object, databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(2, amountofTimesQueryCalled);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_QueryToGetChunkShouldGetCorrectParameters()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var toSkipSum = 0;
			var toTakeSum = 0;
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) =>
				{
					toSkipSum += (int)y.ToSkip;
					toTakeSum += (int)y.ToTake;
					return new List<byte[]>();
				});

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.ApplyDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()));

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, deltaServiceMock.Object, databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.AreEqual(11, toSkipSum);
			Assert.AreEqual(30, toTakeSum);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_ApplyDeltaChouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, object y) =>
				{
					return new List<byte[]>();
				});
			var isApplyDeltaCalled = false;
			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.ApplyDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()))
				.Callback(() => isApplyDeltaCalled = true);

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, deltaServiceMock.Object, databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(isApplyDeltaCalled);
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_FinializationShouldBeCalled()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var executedStatements = new List<string>();
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) => new List<byte[]>());
			databaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string x, dynamic y) => executedStatements.Add(x));

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.ApplyDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()));

			var webhookMock = Helper.GetWebhookServiceMock(x => { }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, deltaServiceMock.Object, databaseMock.Object, webhookMock.Object);
			await fileAssemblerService.ProcessFile(domainFile, eofMessage);

			Assert.IsTrue(executedStatements.Any(x => x.Contains("UPDATE DBO.[File]", StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(executedStatements.Any(x => x.Contains("DELETE FROM dbo.FIleChunk", StringComparison.OrdinalIgnoreCase)));
			Assert.IsFalse(domainFile.IsNew());
		}

		[TestMethod]
		public async Task ProcessFile_ChangedFile_CallToWebhookShouldBeMade()
		{
			var configMock = CreateConfigurationMock();
			var pathBuilder = new PathBuilder(configMock.Object);
			var domainFile = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")), Guid.Empty, pathBuilder.BasePath);
			domainFile.Update(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DropLocation", "dummy.txt")));
			var eofMessage = EofMessage.Build(Guid.Empty, 15);
			var databaseMock = new Mock<IDatabase>();
			databaseMock.Setup(x => x.Query<byte[]>(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync((string x, dynamic y) => new List<byte[]>());
			databaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<object>()));

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.ApplyDelta(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>()));

			var webhookIsCalled = false;
			var webhookMock = Helper.GetWebhookServiceMock(x => { webhookIsCalled = true; }, x => { }, x => { });
			var fileAssemblerService = new FileAssemblerService(new CompressionService(), pathBuilder, deltaServiceMock.Object, databaseMock.Object, webhookMock.Object);
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
					Constants.ReplicateBasePath => System.IO.Directory.GetCurrentDirectory(),
					Constants.FileSplitSizeOfChunksInBytes => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});

			return configurationMock;
		}

	}
}
