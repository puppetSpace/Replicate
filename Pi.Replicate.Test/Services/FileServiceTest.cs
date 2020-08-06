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
using Pi.Replicate.Worker.Host.Common;

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

		[TestCleanup]
		public void Cleanup()
		{
			var testDir = System.IO.Path.Combine(PathBuilder.BasePath, "TestFileService");
			if(System.IO.Directory.Exists(testDir))
				System.IO.Directory.Delete(testDir);
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
				.Callback<File, byte[]>((x, y) => fileAddedToDb = true);

			var folderRepositoryMock = new Mock<IFolderRepository>();
			var recipientRepositoryMock = new Mock<IRecipientRepository>();

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var createdFile = await fileService.CreateNewFile(Guid.Empty, file);

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

			var folderRepositoryMock = new Mock<IFolderRepository>();
			var recipientRepositoryMock = new Mock<IRecipientRepository>();

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
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

			var folderRepositoryMock = new Mock<IFolderRepository>();
			var recipientRepositoryMock = new Mock<IRecipientRepository>();

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var createdFile = await fileService.CreateNewFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is null);
		}

		[TestMethod]
		public async Task UpdateFile_FileExists_UpdatesFile()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = new File(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddNewFile(It.IsAny<File>(), It.IsAny<byte[]>()))
				.ReturnsAsync(() => Result.Success());

			fileRepositoryMock.Setup(x => x.GetLastVersionOfFile(It.IsAny<Guid>(), It.IsAny<string>()))
				.ReturnsAsync(() => Result<File>.Success(domainFile));

			var folderRepositoryMock = new Mock<IFolderRepository>();
			var recipientRepositoryMock = new Mock<IRecipientRepository>();

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
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

			var folderRepositoryMock = new Mock<IFolderRepository>();
			var recipientRepositoryMock = new Mock<IRecipientRepository>();

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var createdFile = await fileService.CreateUpdateFile(Guid.Empty, file);

			Assert.IsTrue(createdFile is null);
		}

		[TestMethod]
		public async Task AddReceivedFile_FolderShouldBeAddedToDatabase()
		{
			var folderName = "TestFileService";
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddReceivedFile(It.IsAny<File>()))
				.ReturnsAsync(() => Result.Success());

			var recipientRepositoryMock = new Mock<IRecipientRepository>();
			recipientRepositoryMock.Setup(x => x.AddRecipientToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Success());

			var isCorrectPassedFolder = false;
			var folderRepositoryMock = new Mock<IFolderRepository>();
			folderRepositoryMock.Setup(x => x.AddFolder(It.IsAny<string>()))
				.Callback<string>(x =>
				{
					isCorrectPassedFolder = x == folderName;
				})
				.ReturnsAsync(() => Result<Guid>.Success(Guid.NewGuid()));

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var result = await fileService.AddReceivedFile(Guid.NewGuid(), folderName, "testFile", 1, 1, DateTime.Now, "TestPath", "Localhost");

			Assert.IsTrue(result.WasSuccessful);
			Assert.IsTrue(isCorrectPassedFolder);
		}

		[TestMethod]
		public async Task AddReceivedFile_FolderShouldBeCreatedOnFileSystem()
		{
			var folderName = "TestFileService";
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddReceivedFile(It.IsAny<File>()))
				.ReturnsAsync(() => Result.Success());

			var recipientRepositoryMock = new Mock<IRecipientRepository>();
			recipientRepositoryMock.Setup(x => x.AddRecipientToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Success());

			var isCorrectPassedFolder = false;
			var folderRepositoryMock = new Mock<IFolderRepository>();
			folderRepositoryMock.Setup(x => x.AddFolder(It.IsAny<string>()))
				.Callback<string>(x =>
				{
					isCorrectPassedFolder = x == folderName;
				})
				.ReturnsAsync(() => Result<Guid>.Success(Guid.NewGuid()));

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var result = await fileService.AddReceivedFile(Guid.NewGuid(), folderName, "testFile", 1, 1, DateTime.Now, "TestPath", "Localhost");

			Assert.IsTrue(System.IO.Directory.Exists(System.IO.Path.Combine(PathBuilder.BasePath, folderName)));
		}

		[TestMethod]
		public async Task AddReceivedFile_FileShoudBeAdded()
		{
			File addedFile = null;
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddReceivedFile(It.IsAny<File>()))
				.Callback<File>(x => addedFile = x)
				.ReturnsAsync(() => Result.Success());

			var recipientRepositoryMock = new Mock<IRecipientRepository>();
			recipientRepositoryMock.Setup(x => x.AddRecipientToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Success());

			var newFolderGuid = Guid.NewGuid();
			var folderRepositoryMock = new Mock<IFolderRepository>();
			folderRepositoryMock.Setup(x => x.AddFolder(It.IsAny<string>()))
				.ReturnsAsync(() => Result<Guid>.Success(newFolderGuid));
			var currentTimeStamp = DateTime.Now;

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var result = await fileService.AddReceivedFile(Guid.NewGuid(), "TestFolder", "testFile", 1, 2, currentTimeStamp, "TestPath", "Localhost");

			Assert.IsNotNull(addedFile);
			Assert.AreEqual(newFolderGuid, addedFile.FolderId);
			Assert.AreEqual("testFile", addedFile.Name);
			Assert.AreEqual(1, addedFile.Size);
			Assert.AreEqual(2, addedFile.Version);
			Assert.AreEqual(currentTimeStamp, addedFile.LastModifiedDate);
			Assert.AreEqual("TestPath", addedFile.Path);
		}

		[TestMethod]
		public async Task AddReceivedFile_RecipientShouldBeLinkedToFolder()
		{
			var fileRepositoryMock = new Mock<IFileRepository>();
			fileRepositoryMock.Setup(x => x.AddReceivedFile(It.IsAny<File>()))
				.ReturnsAsync(() => Result.Success());

			string addedHost = null;
			string addedAddress = null;
			Guid addedFolderId = Guid.Empty;
			var recipientRepositoryMock = new Mock<IRecipientRepository>();
			recipientRepositoryMock.Setup(x => x.AddRecipientToFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.Callback<string,string,Guid>((x,y,z)=>
				{
					addedHost = x;
					addedAddress = y;
					addedFolderId = z;
				})
				.ReturnsAsync(() => Result.Success());

			var newFolderGuid = Guid.NewGuid();
			var folderRepositoryMock = new Mock<IFolderRepository>();
			folderRepositoryMock.Setup(x => x.AddFolder(It.IsAny<string>()))
				.ReturnsAsync(() => Result<Guid>.Success(newFolderGuid));
			var currentTimeStamp = DateTime.Now;

			var fileService = new FileService(fileRepositoryMock.Object, folderRepositoryMock.Object, recipientRepositoryMock.Object);
			var result = await fileService.AddReceivedFile(Guid.NewGuid(), "TestFolder", "testFile", 1, 2, currentTimeStamp, "TestPath", "Localhost");


			Assert.AreEqual(newFolderGuid, addedFolderId);
			Assert.AreEqual("Localhost", addedHost);
			Assert.AreEqual(DummyAdress.Create("Localhost"), addedAddress);

		}
	}
}
