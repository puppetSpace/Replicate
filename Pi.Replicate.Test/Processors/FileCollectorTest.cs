using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Files;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileCollectorTest
    {


        [TestMethod]
        public async Task ProcessFiles_NewFiles_5New()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var fileCount = 0;

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File>());

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(()=>fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(() => { fileCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockOutQueue.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
            Assert.AreEqual(5, fileCount);
        }

        [TestMethod]
        public async Task ProcessFiles_NewFiles_3New2Old()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var oldFile1 = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };

            var oldFile2 = new File
            {
                Folder = folder,
                Name = "test2.txt",
                Status = FileStatus.New
            };

            var fileCount = 0;

            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc;
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File> { oldFile1, oldFile2 });

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);


            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(() => { fileCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockOutQueue.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);

            await collector.WorkAsync();

            //asert
            Assert.AreEqual(3, fileCount);
        }

        [TestMethod]
        public async Task ProcessFiles_NewFiles_3New1Changed()
        {

            var currentDir = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            //assign
            var folder = new Folder(currentDir)
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var oldFile1 = new File
            {
                Folder = folder,
                Name = "test1.txt",
                Status = FileStatus.Sent
            };

            var oldFile2 = new File
            {
                Folder = folder,
                Name = "test2.txt",
                Status = FileStatus.New
            };

            var fileCount = 0;
            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc.AddHours(1);
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File> { oldFile1, oldFile2 });

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(() => { fileCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockOutQueue.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
            Assert.AreEqual(4, fileCount);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ProcessFiles_InvalidFolder_ThrowInvalidOperationException()
        {

            var currentDir = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            //assign
            var folder = new Folder(currentDir)
            {
                Id = Guid.NewGuid(),
                Name = "DoesNotExist"
            };

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File>());

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(true); 

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(Task.CompletedTask);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>()).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>()).Returns(mockOutQueue.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
        }

        //todo verify save
    }
}
