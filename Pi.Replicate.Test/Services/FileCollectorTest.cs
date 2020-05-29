using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
	public class FileCollectorTest
	{

		[TestInitialize]
		public void InitializeTest()
		{
		}

		[TestMethod]
		public async Task GetNewFiles_ShouldFind5NewFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[Constants.ReplicateBasePath]).Returns(System.IO.Directory.GetCurrentDirectory());
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<ICollection<File>>.Success(new List<File>()));

			var fileCollector = new FileCollector(new PathBuilder(configurationMock.Object), fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();


			Assert.AreEqual(5, fileCollector.NewFiles.Count);
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test2.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test3.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test4.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test5.txt"));
		}


		[TestMethod]
		public async Task GetNewFiles_2Changed_ShouldFind3NewFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[Constants.ReplicateBasePath]).Returns(System.IO.Directory.GetCurrentDirectory());
			var pathBuilder = new PathBuilder(configurationMock.Object);
			ICollection<File> existingFiles = new List<File>
			{
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath),
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath)
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(pathBuilder, fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(3, fileCollector.NewFiles.Count);
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test3.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test4.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test5.txt"));
		}

		[TestMethod]
		public async Task GetChanged_2Existing_ShouldFind2ChangedFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[Constants.ReplicateBasePath]).Returns(System.IO.Directory.GetCurrentDirectory());
			var pathBuilder = new PathBuilder(configurationMock.Object);
			ICollection<File> existingFiles = new List<File>
			{
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now),
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now)
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(pathBuilder, fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(2, fileCollector.ChangedFiles.Count);
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test1.txt"));
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test2.txt"));
		}

		[TestMethod]
		public async Task GetChanged_2Existing_ShouldFind1ChangedFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x[Constants.ReplicateBasePath]).Returns(System.IO.Directory.GetCurrentDirectory());
			var pathBuilder = new PathBuilder(configurationMock.Object);
			ICollection<File> existingFiles = new List<File>
			{
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath),
				File.Build(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now)
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(pathBuilder, fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(1, fileCollector.ChangedFiles.Count);
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test2.txt"));
		}
	}
}
