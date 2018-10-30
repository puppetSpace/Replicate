using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Files;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileAssemblerTest
    {
        [TestInitialize]
        public void InitializeTest()
        {
            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DropLocation";

            System.IO.Directory.Delete(folder.GetPath(), true);
            System.IO.Directory.CreateDirectory(folder.GetPath());
        }

        [TestMethod]
        public async Task Work_FileGetAssembled()
        {
            var inputFolder = EntityBuilder.BuildFolder();

            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DropLocation";

            var files = EntityBuilder.BuildFiles(folder).ToList();
            var file = files[0];
            var fileCount = 0;
            var isFileReceived = false;
            var areChunksDeleted = false;
            var isFileUpdated = false;
            var isTempFileDeleted = false;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(()=>fileCount == 0);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetTempFileIfExists(It.IsAny<File>())).Returns(Task.FromResult<TempFile>(null));
            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask).Callback<File>(x => isFileUpdated = (x.Status == FileStatus.ReceivedComplete));
            mockFileRepository.Setup(x => x.DeleteTempFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => isTempFileDeleted = true);

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file,inputFolder.GetPath()));
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.FileReceived(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isFileReceived = true);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);

            var fileAssembler = new FileAssembler(mockFactoryQueue.Object,mockRepository.Object, mockUploadLink.Object);
            await fileAssembler.WorkAsync();

            Assert.IsTrue(System.IO.File.Exists(file.GetPath()));
            Assert.IsTrue(isFileReceived);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isFileUpdated);
            Assert.IsTrue(isTempFileDeleted);

        }


        [TestMethod]
        public async Task Work_ExistingFileLocked_ShouldSaveTempFile()
        {
            var inputFolder = EntityBuilder.BuildFolder();

            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DropLocation";

            var files = EntityBuilder.BuildFiles(folder).ToList();
            var file = files[0];
            var fileCount = 0;
            var isFileReceived = false;
            var areChunksDeleted = false;
            var isFileUpdated = false;
            var isTempFileDeleted = false;
            var isTempFileSaved = false;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetTempFileIfExists(It.IsAny<File>())).Returns(Task.FromResult<TempFile>(null));
            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask).Callback<File>(x => isFileUpdated = (x.Status == FileStatus.ReceivedComplete));
            mockFileRepository.Setup(x => x.DeleteTempFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => isTempFileDeleted = true);
            mockFileRepository.Setup(x => x.SaveTemp(It.IsAny<TempFile>())).Returns(Task.CompletedTask).Callback(() => isTempFileSaved = true);

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file, inputFolder.GetPath()));
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.FileReceived(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isFileReceived = true);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);

            //act
            System.IO.File.Copy(System.IO.Path.Combine(inputFolder.GetPath(), file.Name), file.GetPath());
            var tempStream = System.IO.File.OpenWrite(file.GetPath());

            var fileAssembler = new FileAssembler(mockFactoryQueue.Object, mockRepository.Object, mockUploadLink.Object);
            await fileAssembler.WorkAsync();

            tempStream.Close();

            Assert.IsTrue(isTempFileSaved);
            Assert.IsFalse(isFileReceived);
            Assert.IsFalse(areChunksDeleted);
            Assert.IsFalse(isFileUpdated);
            Assert.IsFalse(isTempFileDeleted);

        }

        [TestMethod]
        public async Task Work_TempFileExists_TempFileShouldBeUsed()
        {
            var inputFolder = EntityBuilder.BuildFolder();

            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DropLocation";

            var files = EntityBuilder.BuildFiles(folder).ToList();
            var file = files[0];
            var fileCount = 0;
            var isFileReceived = false;
            var areChunksDeleted = false;
            var isFileUpdated = false;
            var isTempFileDeleted = false;
            var areChunksRetrieved = false;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);

            var tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(),System.IO.Path.GetRandomFileName());
            System.IO.File.Copy(System.IO.Path.Combine(inputFolder.GetPath(), file.Name), tempFilePath);
            var tempFile = new TempFile { FileId = file.Id, Hash = EntityBuilder.CreateHashForFile(file, inputFolder.GetPath()), Path = tempFilePath };

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetTempFileIfExists(It.IsAny<File>())).Returns(Task.FromResult(tempFile));
            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask).Callback<File>(x => isFileUpdated = (x.Status == FileStatus.ReceivedComplete));
            mockFileRepository.Setup(x => x.DeleteTempFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => isTempFileDeleted = true);

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file, inputFolder.GetPath())).Callback(()=>areChunksRetrieved = true);
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.FileReceived(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isFileReceived = true);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);

            var fileAssembler = new FileAssembler(mockFactoryQueue.Object, mockRepository.Object, mockUploadLink.Object);
            await fileAssembler.WorkAsync();

            Assert.IsTrue(System.IO.File.Exists(file.GetPath()));
            Assert.IsTrue(isFileReceived);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isFileUpdated);
            Assert.IsTrue(isTempFileDeleted);
            Assert.IsFalse(areChunksRetrieved);

        }

        [TestMethod]
        public async Task Work_TempFileExists_HashDiffers_TempFileShouldNotBeUsed()
        {
            var inputFolder = EntityBuilder.BuildFolder();

            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DropLocation";

            var files = EntityBuilder.BuildFiles(folder).ToList();
            var file = files[0];
            var fileCount = 0;
            var isFileReceived = false;
            var areChunksDeleted = false;
            var isFileUpdated = false;
            var isTempFileDeleted = false;
            var areChunksRetrieved = false;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);

            var tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            System.IO.File.Copy(System.IO.Path.Combine(inputFolder.GetPath(), file.Name), tempFilePath);
            var tempFile = new TempFile { FileId = file.Id, Hash = EntityBuilder.CreateHashForFile(file, inputFolder.GetPath()) + "s", Path = tempFilePath };

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetTempFileIfExists(It.IsAny<File>())).Returns(Task.FromResult(tempFile));
            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask).Callback<File>(x => isFileUpdated = (x.Status == FileStatus.ReceivedComplete));
            mockFileRepository.Setup(x => x.DeleteTempFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => isTempFileDeleted = true);

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file, inputFolder.GetPath())).Callback(() => areChunksRetrieved = true);
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.FileReceived(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isFileReceived = true);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);

            var fileAssembler = new FileAssembler(mockFactoryQueue.Object, mockRepository.Object, mockUploadLink.Object);
            await fileAssembler.WorkAsync();

            Assert.IsTrue(System.IO.File.Exists(file.GetPath()));
            Assert.IsTrue(isFileReceived);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isFileUpdated);
            Assert.IsTrue(isTempFileDeleted);
            Assert.IsTrue(areChunksRetrieved);

        }
    }
}
