using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Pi.Replicate.Shared;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
	public class FileServiceTest
	{
		[TestInitialize]
		public void Initialize()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public async Task CreateNewFile_FileExists_CreatesNewFile()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());
			var fileAddedToDb = false;

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddNewFile(It.IsAny<File>(), It.IsAny<byte[]>()))
				.ReturnsAsync(() => Result.Success())
				.Callback<File,byte[]>((x,y)=> fileAddedToDb = true);

			var fileService = new FileService(fileRepositoryMock.Object);
			var createdFile = await fileService.CreateNewFile(Guid.Empty , file);

			Assert.IsTrue(createdFile is object);
			Assert.IsTrue(fileAddedToDb);
			Assert.AreEqual(file.Name, createdFile.Name);
			Assert.AreEqual(file.Length, createdFile.Size);
			Assert.AreEqual(file.LastWriteTimeUtc, createdFile.LastModifiedDate);
			Assert.AreEqual(1, createdFile.Version);
			Assert.AreEqual(FileSource.Local, createdFile.Source);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task CreateNewFile_FileDoesNotExists_ThrowInvalidOperationException()
		{
			var file = new System.IO.FileInfo("FileFolder/text1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddNewFile(It.IsAny<File>(), It.IsAny<byte[]>()))
				.ReturnsAsync(() => Result.Success());

			var fileService = new FileService(fileRepositoryMock.Object);
			var createdFile = await fileService.CreateNewFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is object);
			Assert.AreEqual(file.Name, createdFile.Name);
			Assert.AreEqual(file.Length, createdFile.Size);
			Assert.AreEqual(file.LastWriteTimeUtc, createdFile.LastModifiedDate);
			Assert.AreEqual(1, createdFile.Version);
			Assert.AreEqual(FileSource.Local, createdFile.Source);
		}

		[TestMethod]
		public async Task CreateNewFile_DatabaseError_ReturnNull()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddNewFile(It.IsAny<File>(), It.IsAny<byte[]>()))
				.ReturnsAsync(() => Result.Failure());

			var fileService = new FileService(fileRepositoryMock.Object);
			var createdFile = await fileService.CreateNewFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is null);
		}

		[TestMethod]
		public async Task UpdateFile_FileExists_UpdatesFile()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddNewFile(It.IsAny<File>(), It.IsAny<byte[]>()))
				.ReturnsAsync(() => Result.Success());

			fileRepositoryMock.Setup(x => x.GetLastVersionOfFile(It.IsAny<Guid>(),It.IsAny<string>()))
				.ReturnsAsync(() => Result<File>.Success(domainFile));

			var fileService = new FileService(fileRepositoryMock.Object);
			var createdFile = await fileService.CreateUpdateFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is object);
			Assert.AreEqual(file.Name, createdFile.Name);
			Assert.AreEqual(file.Length, createdFile.Size);
			Assert.AreEqual(file.LastWriteTimeUtc, createdFile.LastModifiedDate);
			Assert.AreEqual(2, createdFile.Version);
			Assert.AreEqual(FileSource.Local, createdFile.Source);
		}

		[TestMethod]
		public async Task UpdateFile_DatabaseError_ReturnsNull()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.GetLastVersionOfFile(It.IsAny<Guid>(), It.IsAny<string>()))
				.ReturnsAsync(() => Result<File>.Failure());

			var fileService = new FileService(fileRepositoryMock.Object);
			var createdFile = await fileService.CreateUpdateFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is null);
		}
	}
}
