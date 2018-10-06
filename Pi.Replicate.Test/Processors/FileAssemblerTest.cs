using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Processors.Files;
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

            var fileAssembler = new FileAssembler(mockFactoryQueue.Object, mockFileChunkRepository.Object, mockFileRepository.Object, mockUploadLink.Object);
            await fileAssembler.WorkAsync();

            Assert.IsTrue(System.IO.File.Exists(file.GetPath()));
            Assert.IsTrue(isFileReceived);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isFileUpdated);
            Assert.IsTrue(isTempFileDeleted);

        }
    }
}
