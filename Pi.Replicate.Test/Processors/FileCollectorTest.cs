using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
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
            var folder = new Folder { Name = "FileFolder", FolderOptions = FolderOption.Empty };
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(System.IO.Directory.GetCurrentDirectory());
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest<ICollection<File>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ICollection<File>)new List<File>()));

            var fileCollector = new FileCollector(new PathBuilder(configurationMock.Object), mockMediator.Object, folder);
            var files = await fileCollector.GetNewFiles();

            Assert.AreEqual(5, files.Count);
            Assert.IsTrue(files.Any(x => x.Name == "test1.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test2.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test3.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test4.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test5.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetNewFiles_FolderDoesNotExists_ShouldThrowException()
        {
            var folder = new Folder { Name = "DoesNotExists", FolderOptions =FolderOption.Empty };
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(System.IO.Directory.GetCurrentDirectory());
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest<ICollection<File>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ICollection<File>)new List<File>()));

            var fileCollector = new FileCollector(new PathBuilder(configurationMock.Object), mockMediator.Object, folder);
            var files = await fileCollector.GetNewFiles();

        }

        [TestMethod]
        public async Task GetNewFiles_2Changed_ShouldFind3NewFiles()
        {
            var folder = new Folder { Name = "FileFolder", FolderOptions = FolderOption.Empty };
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(System.IO.Directory.GetCurrentDirectory());
            var pathBuilder = new PathBuilder(configurationMock.Object);
            ICollection<File> existingFiles = new List<File>
            {
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath),
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath)
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest<ICollection<File>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingFiles));

            var fileCollector = new FileCollector(pathBuilder, mockMediator.Object, folder);
            var files = await fileCollector.GetNewFiles();

            Assert.AreEqual(3, files.Count);
            Assert.IsTrue(files.Any(x => x.Name == "test3.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test4.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test5.txt"));
        }

        [TestMethod]
        public async Task GetChanged_2Existing_ShouldFind2ChangedFiles()
        {
            var folder = new Folder { Name = "FileFolder", FolderOptions = FolderOption.Empty };
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(System.IO.Directory.GetCurrentDirectory());
            var pathBuilder = new PathBuilder(configurationMock.Object);
            ICollection<File> existingFiles = new List<File>
            {
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now),
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now)
            };

            existingFiles.ToList().ForEach(x=>x.MarkAsHandled());

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest<ICollection<File>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingFiles));

            var fileCollector = new FileCollector(pathBuilder, mockMediator.Object, folder);
            var files = await fileCollector.GetChangedFiles();

            Assert.AreEqual(2, files.Count);
            Assert.IsTrue(files.Any(x => x.Name == "test1.txt"));
            Assert.IsTrue(files.Any(x => x.Name == "test2.txt"));
        }

        [TestMethod]
        public async Task GetChanged_2Existing_ShouldFind1ChangedFiles()
        {
            var folder = new Folder { Name = "FileFolder", FolderOptions = FolderOption.Empty };
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(System.IO.Directory.GetCurrentDirectory());
            var pathBuilder = new PathBuilder(configurationMock.Object);
            ICollection<File> existingFiles = new List<File>
            {
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test1.txt")),System.Guid.Empty,pathBuilder.BasePath),
                File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath,"FileFolder","test2.txt")),System.Guid.Empty,pathBuilder.BasePath,DateTime.Now)
            };

            existingFiles.Last().MarkAsHandled();

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest<ICollection<File>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingFiles));

            var fileCollector = new FileCollector(pathBuilder, mockMediator.Object, folder);
            var files = await fileCollector.GetChangedFiles();

            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files.Any(x => x.Name == "test2.txt"));
        }
    }
}
