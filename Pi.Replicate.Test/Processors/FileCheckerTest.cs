using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Files;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileCheckerTest
    {
		[TestInitialize]
		public void InitializeTest()
		{
			EntityBuilder.InitializePathBuilder();
		}

        [TestMethod]
        public async Task Work_1CompleteFile_FileShouldBeAddedToQueue()
        {
            var folder = EntityBuilder.BuildFolder();
            var files = EntityBuilder.BuildFiles(folder).ToList();

            var file = files[0];
            file.Hash = EntityBuilder.CreateHashForFile(file);
            var fileCount = 0;

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(Task.CompletedTask).Callback(() => fileCount++);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetCompletedReceivedFiles()).Returns(Task.FromResult<IEnumerable<File>>(new[] { file }));

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.GetForFile(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file));

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);

            var fileChecker = new FileChecker(mockFactoryQueue.Object, mockRepository.Object, null);
            await fileChecker.WorkAsync();

            Assert.AreEqual(1, fileCount);
        }

        [TestMethod]
        public async Task Work_1IncompleteFile_FileShouldNotBeAddedToQueue_ResendRequested()
        {
            var folder = EntityBuilder.BuildFolder();
            var files = EntityBuilder.BuildFiles(folder).ToList();

            var file = files[0];
            file.Hash = EntityBuilder.CreateHashForFile(file);
            var fileCount = 0;
            var isFileDelete = false;
            var areChunksDeleted = false;
            var isResendRequested = false;


            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(Task.CompletedTask).Callback(() => fileCount++);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(x => x.GetCompletedReceivedFiles()).Returns(Task.FromResult<IEnumerable<File>>(new[] { file }));
            mockFileRepository.Setup(x => x.Delete(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => isFileDelete = true);

            var mockFileChunkRepository = new Mock<IFileChunkRepository>();
            mockFileChunkRepository.Setup(x => x.GetForFile(It.IsAny<Guid>()))
                .Returns(async () =>
                {
                    var chunks = (await EntityBuilder.BuildChunks(file)).ToList();
                    chunks.RemoveAt(0);
                    return chunks;
                });
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.RequestResendOfFile(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isResendRequested = true);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);
            mockRepository.SetupGet(x => x.FileChunkRepository).Returns(mockFileChunkRepository.Object);


            var fileChecker = new FileChecker(mockFactoryQueue.Object, mockRepository.Object, mockUploadLink.Object);
            await fileChecker.WorkAsync();

            Assert.AreEqual(0, fileCount);
            Assert.IsTrue(isFileDelete);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isResendRequested);
        }
    }
}
