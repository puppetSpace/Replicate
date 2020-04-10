using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
	[TestClass]
	public class FileSplitterTest
	{
		//[TestInitialize]
		//public void InitializeTest()
		//{
		//    EntityBuilder.InitializePathBuilder();
		//}

		[TestMethod]
		public async Task ProcessFile_ShouldCreateRightAmountOfChunks()
		{
			int minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x => 
				x switch
				{
					"ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
					"FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
			});
			var pathBuilder = new Application.Common.PathBuilder(configurationMock.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
			var calculatedAmountOfChunks = Math.Ceiling((double)fileInfo.Length / minimumAmountOfBytesRentedByArrayPool);


			var fileSplitter = new FileSplitter(configurationMock.Object, pathBuilder);
			var result = await fileSplitter.ProcessFile(File.BuildPartial(fileInfo,null,pathBuilder.BasePath));


			Assert.AreEqual(calculatedAmountOfChunks, result.Chunks.Count);

		}

		[TestMethod]
		public async Task ProcessFile_ShouldProduceCorrectHash()
		{
			int minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
				x switch
				{
					"ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
					"FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});
			var pathBuilder = new Application.Common.PathBuilder(configurationMock.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			var fileSplitter = new FileSplitter(configurationMock.Object, pathBuilder);
			var result = await fileSplitter.ProcessFile(File.BuildPartial(fileInfo, null, pathBuilder.BasePath));
			var createdHashOfFile = Helper.CreateBase64HashForFile(fileInfo.FullName);


			Assert.AreEqual(createdHashOfFile, Convert.ToBase64String(result.Hash));

		}

		[TestMethod]
		public async Task ProcessFile_FileDoesNotExists_ShouldReturnEmptyResultSet()
		{
			int minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
				x switch
				{
					"ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
					"FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});
			var pathBuilder = new Application.Common.PathBuilder(configurationMock.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "doesnotexists.txt"));

			var fileSplitter = new FileSplitter(configurationMock.Object, pathBuilder);
			var result = await fileSplitter.ProcessFile(File.BuildPartial(fileInfo, null, pathBuilder.BasePath));


			Assert.AreEqual(0, result.Chunks.Count);
			Assert.AreEqual(0, result.Hash.Length);

		}

		[TestMethod]
		public async Task ProcessFile_FileIsLocked_ShouldReturnEmptyResultSet()
		{
			int minimumAmountOfBytesRentedByArrayPool = 128;
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
				x switch
				{
					"ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
					"FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
					_ => ""
				});
			var pathBuilder = new Application.Common.PathBuilder(configurationMock.Object);
			var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

			using var fs = fileInfo.OpenWrite();

			var fileSplitter = new FileSplitter(configurationMock.Object, pathBuilder);
			var result = await fileSplitter.ProcessFile(File.BuildPartial(fileInfo, null, pathBuilder.BasePath));


			Assert.AreEqual(0, result.Chunks.Count);
			Assert.AreEqual(0, result.Hash.Length);

		}
	}
}
