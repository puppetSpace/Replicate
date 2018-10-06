﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Processors.Files;
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
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(EntityBuilder.BuildChunks(file));

            var fileChecker = new FileChecker(mockFactoryQueue.Object, mockFileChunkRepository.Object, mockFileRepository.Object, null);
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
            mockFileChunkRepository.Setup(x => x.Get(It.IsAny<Guid>()))
                .Returns(async () =>
                {
                    var chunks = (await EntityBuilder.BuildChunks(file)).ToList();
                    chunks.RemoveAt(0);
                    return chunks;
                });
            mockFileChunkRepository.Setup(x => x.DeleteForFile(It.IsAny<Guid>())).Returns(Task.CompletedTask).Callback(() => areChunksDeleted = true);

            var mockUploadLink = new Mock<IUploadLink>();
            mockUploadLink.Setup(x => x.RequestResendOfFile(It.IsAny<Uri>(), It.IsAny<Guid>())).Returns(Task.FromResult(new UploadResponse())).Callback(() => isResendRequested = true);


            var fileChecker = new FileChecker(mockFactoryQueue.Object, mockFileChunkRepository.Object, mockFileRepository.Object, mockUploadLink.Object);
            await fileChecker.WorkAsync();

            Assert.AreEqual(0, fileCount);
            Assert.IsTrue(isFileDelete);
            Assert.IsTrue(areChunksDeleted);
            Assert.IsTrue(isResendRequested);
        }
    }
}
