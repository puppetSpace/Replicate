//using Microsoft.Extensions.Configuration;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Pi.Replicate.Test.Processors
//{
//    [TestClass]
//    public class FileSplitterTest
//    {
//        [TestInitialize]
//        public void InitializeTest()
//        {
//            EntityBuilder.InitializePathBuilder();
//        }

//        [TestMethod]
//        public async Task Work_CorrectAmountOfChunksCreated()
//        {
//            //assign
//            var folder = EntityBuilder.BuildFolder();
//            var files = EntityBuilder.BuildFiles(folder).ToList();

//            var file = files[4];
//            file.Status = FileStatus.Sent;

//            uint chunkSize = 1 * 1024;
//            int fileChunkCount = 0;
//            int fileCount = 0;

//            var mockInQueue = new Mock<IWorkItemQueue<File>>();
//            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
//            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

//            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
//            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

//            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
//            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
//            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

//            var mockFileRepository = new Mock<IFileRepository>();
//            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask);

//            var mockRepository = new Mock<IRepository>();
//            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

//            //act
//            var splitter = new FileSplitter(mockFactoryQueue.Object, mockRepository.Object, mockConfiguration.Object);
//            await splitter.WorkAsync();


//            //assert
//            var amountOfChunksThatShouldBeCreated =
//                Math.Ceiling((double)new System.IO.FileInfo(file.GetPath()).Length / (double)chunkSize);

//            Assert.AreEqual(fileChunkCount, amountOfChunksThatShouldBeCreated);
//            Assert.AreEqual(file.AmountOfChunks, amountOfChunksThatShouldBeCreated);

//        }

//        [TestMethod]
//        public async Task Work_CorrectHashCreated()
//        {
//            //assign
//            var folder = EntityBuilder.BuildFolder();
//            var files = EntityBuilder.BuildFiles(folder).ToList();

//            var file = files[3];
//            file.Status = FileStatus.Sent;

//            uint chunkSize = 1 * 1024;
//            int fileCount = 0;

//            var mockInQueue = new Mock<IWorkItemQueue<File>>();
//            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file)).Callback(() => fileCount++);
//            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

//            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
//            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(Task.CompletedTask);

//            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
//            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
//            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

//            var mockFileRepository = new Mock<IFileRepository>();
//            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask);

//            var mockRepository = new Mock<IRepository>();
//            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

//            //act
//            var splitter = new FileSplitter(mockFactoryQueue.Object, mockRepository.Object, mockConfiguration.Object);
//            await splitter.WorkAsync();

//            //assert
//            var md5HashOfFile = EntityBuilder.CreateHashForFile(file);

//            Assert.AreEqual(file.Hash, md5HashOfFile);

//        }


//        [TestMethod]
//        public async Task Work_ChunkFile_IsSameAsOrginal()
//        {
//            //assign
//            var folder = EntityBuilder.BuildFolder();
//            var files = EntityBuilder.BuildFiles(folder).ToList();

//            var file = files[0];
//            file.Status = FileStatus.Sent;

//            uint chunkSize = 1 * 1024;
//            var memoryStream = new System.IO.MemoryStream();

//            var mockInQueue = new Mock<IWorkItemQueue<File>>();
//            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(file));
//            mockInQueue.Setup(x => x.HasItems()).Returns(() => memoryStream.Length == 0); //only one item . second call should return false

//            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
//            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>()))
//                .Callback(new Action<FileChunk>(x =>
//                {
//                    var bytes = Convert.FromBase64String(x.Value);
//                    memoryStream.Write(bytes, 0, bytes.Length);
//                })).Returns(Task.CompletedTask);

//            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
//            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
//            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());


//            var mockFileRepository = new Mock<IFileRepository>();
//            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask);

//            var mockRepository = new Mock<IRepository>();
//            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

//            //act
//            var splitter = new FileSplitter(mockFactoryQueue.Object, mockRepository.Object, mockConfiguration.Object);
//            await splitter.WorkAsync();


//            //assert
//            memoryStream.Position = 0;
//            var originalContent = System.IO.File.ReadAllText(file.GetPath());
//            var reassembledContent = new System.IO.StreamReader(memoryStream).ReadToEnd();
//            memoryStream.Close();

//            Assert.AreEqual(originalContent, reassembledContent);

//        }

//        [TestMethod]
//        public async Task Work_NullFile_ShouldNotCallNotify()
//        {
//            //assign
//            uint chunkSize = 1 * 1024;
//            int fileChunkCount = 0;
//            int fileCount = 0;

//            var mockInQueue = new Mock<IWorkItemQueue<File>>();
//            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult<File>(null)).Callback(() => fileCount++);
//            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

//            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
//            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

//            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
//            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
//            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

//            var mockFileRepository = new Mock<IFileRepository>();
//            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask);

//            var mockRepository = new Mock<IRepository>();
//            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

//            //act
//            var splitter = new FileSplitter(mockFactoryQueue.Object, mockRepository.Object, mockConfiguration.Object);
//            await splitter.WorkAsync();


//            //assert
//            Assert.AreEqual(fileChunkCount, 0);

//        }

//        [TestMethod]
//        public async Task Work_FileInUse_NotBeingProcessed()
//        {
//            //assign
//            var folder = EntityBuilder.BuildFolder();
//            var files = EntityBuilder.BuildFiles(folder).ToList();

//            var file = files[0];
//            file.Status = FileStatus.Sent;

//            var writeStream = System.IO.File.OpenWrite(file.GetPath());
//            uint chunkSize = 1 * 1024;
//            int fileCount = 0;
//            int fileChunkCount = 0;

//            var mockInQueue = new Mock<IWorkItemQueue<File>>();
//            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult<File>(null)).Callback(() => fileCount++);
//            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

//            var mockOutQueue = new Mock<IWorkItemQueue<FileChunk>>();
//            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<FileChunk>())).Returns(() => { fileChunkCount++; return Task.CompletedTask; });

//            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
//            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
//            mockFactoryQueue.Setup(x => x.GetQueue<FileChunk>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

//            var mockConfiguration = new Mock<IConfiguration>();
//            mockConfiguration.SetupGet(x => x[Constants.FileSplitSizeOfChunksInBytes]).Returns(chunkSize.ToString());

//            var mockFileRepository = new Mock<IFileRepository>();
//            mockFileRepository.Setup(x => x.Update(It.IsAny<File>())).Returns(Task.CompletedTask);

//            var mockRepository = new Mock<IRepository>();
//            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

//            //act
//            var splitter = new FileSplitter(mockFactoryQueue.Object, mockRepository.Object, mockConfiguration.Object);
//            await splitter.WorkAsync();


//            //assert
//            Assert.AreEqual(0, fileChunkCount);

//        }
//    }
//}
