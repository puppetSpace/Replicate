using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Files;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileSplitterTest
    {

        [TestMethod]
        public async Task Split_CorrectAmountOfChunksCreated()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var file = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };
            uint chunkSize = 1 * 1024;
            int fileChunkCount = 0;
            int fileCount = 0;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(()=>fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>()).Returns(mockOutQueue.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

            //act
            var splitter = new FileSplitter(mockConfiguration.Object,mockFactoryQueue.Object);
            await splitter.WorkAsync();


            //assert
            var amountOfChunksThatShouldBeCreated =
                Math.Ceiling((double)new System.IO.FileInfo(file.GetPath()).Length / (double)chunkSize);

            Assert.AreEqual(fileChunkCount, amountOfChunksThatShouldBeCreated);
            Assert.AreEqual(file.AmountOfChunks, amountOfChunksThatShouldBeCreated);

        }

        [TestMethod]
        public async Task Split_CorrectHashCreated()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var file = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };
            uint chunkSize = 1 * 1024;
            int fileCount = 0;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(()=>fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(Task.CompletedTask);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>()).Returns(mockOutQueue.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

            //act
            var splitter = new FileSplitter(mockConfiguration.Object, mockFactoryQueue.Object);
            await splitter.WorkAsync();

            //assert
            var md5HashOfFile = MD5.Create().ComputeHash(System.IO.File.ReadAllBytes(file.GetPath()));

            Assert.AreEqual(file.Hash, Convert.ToBase64String(md5HashOfFile));

        }


        [TestMethod]
        public async Task Split_ChunkFile_IsSameAsOrginal()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var file = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };
            uint chunkSize = 1 * 1024;
            var memoryStream = new System.IO.MemoryStream();

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file));
            mockInQueue.Setup(x => x.HasItems()).Returns(() => memoryStream.Length == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>()))
                .Callback(new Action<FileChunk>(x =>
                {
                    var bytes = Convert.FromBase64String(x.Value);
                    memoryStream.Write(bytes, 0, bytes.Length);
                })).Returns(Task.CompletedTask);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>()).Returns(mockOutQueue.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());


            //act

            var splitter = new FileSplitter(mockConfiguration.Object, mockFactoryQueue.Object);
            await splitter.WorkAsync();


            //assert
            memoryStream.Position = 0;
            var originalContent = System.IO.File.ReadAllText(file.GetPath());
            var reassembledContent = new System.IO.StreamReader(memoryStream).ReadToEnd();
            memoryStream.Close();

            Assert.AreEqual(originalContent, reassembledContent);

        }

        [TestMethod]
        public async Task Split_NullFile_ShouldNotCallNotify()
        {
            //assign
            uint chunkSize = 1 * 1024;
            int fileChunkCount = 0;
            int fileCount = 0;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult<File>(null)).Callback(()=>fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>()).Returns(mockOutQueue.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

            //act
            var splitter = new FileSplitter(mockConfiguration.Object, mockFactoryQueue.Object);
            await splitter.WorkAsync();


            //assert
            Assert.AreEqual(fileChunkCount, 0);

        }

        [TestMethod]
        public async Task Split_FileInUse_NotBeingProcessed()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var file = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };
            var writeStream = System.IO.File.OpenWrite(file.GetPath());
            uint chunkSize = 1 * 1024;
            int fileCount = 0;
            int fileChunkCount = 0;

            var mockInQueue = new Mock<IWorkItemQueue<File>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult<File>(null)).Callback(() => fileCount++);
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>()).Returns(mockOutQueue.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

            //act
            var splitter = new FileSplitter(mockConfiguration.Object, mockFactoryQueue.Object);
            await splitter.WorkAsync();


            //assert
            Assert.AreEqual(0, fileChunkCount);

        }
    }
}
