using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
    public class FileServiceTest
    {
		[TestMethod]
        public async Task CreateNewFile_FileExists_CreatesNewFile()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateSignature(It.IsAny<string>())).Returns(ReadOnlyMemory<byte>.Empty);

			var mediatorMock = new Mock<IMediator>();
			mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<Result<File>>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Result<File>.Success(domainFile)));

			var fileService = new FileService(deltaServiceMock.Object, mediatorMock.Object);
			var createdFile = await fileService.CreateNewFile(new Folder { Id = Guid.Empty }, file);

			Assert.IsTrue(createdFile is object);
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

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateSignature(It.IsAny<string>())).Returns(ReadOnlyMemory<byte>.Empty);

			var mediatorMock = new Mock<IMediator>();
			mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<Result<File>>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Result<File>.Success(domainFile)));

			var fileService = new FileService(deltaServiceMock.Object, mediatorMock.Object);
			var createdFile = await fileService.CreateNewFile(new Folder { Id = Guid.Empty }, file);

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

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateSignature(It.IsAny<string>())).Returns(ReadOnlyMemory<byte>.Empty);

			var mediatorMock = new Mock<IMediator>();
			mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<Result<File>>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Result<File>.Failure()));

			var fileService = new FileService(deltaServiceMock.Object, mediatorMock.Object);
			var createdFile = await fileService.CreateNewFile(new Folder { Id = Guid.Empty }, file);

			Assert.IsTrue(createdFile is null);
		}

		[TestMethod]
		public async Task UpdateFile_FileExists_UpdatesFile()
		{
			var file = new System.IO.FileInfo("FileFolder/test1.txt");
			var domainFile = Domain.File.Build(file, Guid.Empty, System.IO.Directory.GetCurrentDirectory());

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateSignature(It.IsAny<string>())).Returns(ReadOnlyMemory<byte>.Empty);

			var mediatorMock = new Mock<IMediator>();
			mediatorMock
				.Setup(x => x.Send(It.IsAny<IRequest<Result<File>>>(), It.IsAny<CancellationToken>()))
				.Returns(() => {
					domainFile.Update(file);
					return Task.FromResult(Result<File>.Success(domainFile));
				});

			var fileService = new FileService(deltaServiceMock.Object, mediatorMock.Object);
			var createdFile = await fileService.CreateUpdateFile(new Folder { Id = Guid.Empty }, file);

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

			var deltaServiceMock = new Mock<IDeltaService>();
			deltaServiceMock.Setup(x => x.CreateSignature(It.IsAny<string>())).Returns(ReadOnlyMemory<byte>.Empty);

			var mediatorMock = new Mock<IMediator>();
			mediatorMock
				.Setup(x => x.Send(It.IsAny<IRequest<Result<File>>>(), It.IsAny<CancellationToken>()))
				.Returns(() => {
					domainFile.Update(file);
					return Task.FromResult(Result<File>.Failure());
				});

			var fileService = new FileService(deltaServiceMock.Object, mediatorMock.Object);
			var createdFile = await fileService.CreateUpdateFile(new Folder { Id = Guid.Empty }, file);

			Assert.IsTrue(createdFile is null);
		}
	}
}
