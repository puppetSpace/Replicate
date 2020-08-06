using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
	public class FileCollectorTest
	{

		[TestInitialize]
		public void InitializeTest()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public async Task GetNewFiles_ShouldFind8NewFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.ReturnsAsync(() => Result<ICollection<File>>.Success(new List<File>()));

			var fileCollector = new FileCollector(fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();


			Assert.AreEqual(8, fileCollector.NewFiles.Count);
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_changed.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_compressed.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_copy.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test2.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test3.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test4.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test5.txt"));
		}


		[TestMethod]
		public async Task GetNewFiles_2Changed_ShouldFind6NewFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			ICollection<File> existingFiles = new List<File>
			{
				Helper.GetFileModel("FileFolder","test1.txt"),
				Helper.GetFileModel("FileFolder","test2.txt")
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(6, fileCollector.NewFiles.Count);
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_changed.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_compressed.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test1_copy.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test3.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test4.txt"));
			Assert.IsTrue(fileCollector.NewFiles.Any(x => x.Name == "test5.txt"));
		}

		[TestMethod]
		public async Task GetChanged_2Existing_ShouldFind2ChangedFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			ICollection<File> existingFiles = new List<File>
			{
				Helper.GetFileModel("FileFolder","test1.txt",DateTime.Now),
				Helper.GetFileModel("FileFolder","test2.txt",DateTime.Now)
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(2, fileCollector.ChangedFiles.Count);
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test1.txt"));
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test2.txt"));
		}

		[TestMethod]
		public async Task GetChanged_2Existing_ShouldFind1ChangedFiles()
		{
			var folder = new CrawledFolder { Name = "FileFolder" };
			ICollection<File> existingFiles = new List<File>
			{
				Helper.GetFileModel("FileFolder","test1.txt"),
				Helper.GetFileModel("FileFolder","test2.txt",DateTime.Now)
			};

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetFilesForFolder(It.IsAny<Guid>()))
				.Returns(Task.FromResult(Result<ICollection<File>>.Success(existingFiles)));

			var fileCollector = new FileCollector(fileRepositoryMock.Object, folder);
			await fileCollector.CollectFiles();

			Assert.AreEqual(1, fileCollector.ChangedFiles.Count);
			Assert.IsTrue(fileCollector.ChangedFiles.Any(x => x.Name == "test2.txt"));
		}
	}
}
