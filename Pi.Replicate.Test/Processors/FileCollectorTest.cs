using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Files;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileCollectorTest
    {


        [TestMethod]
        public async Task Work_NewFiles_5New()
        {
            //assign
            var folder = EntityBuilder.BuildFolder();

            var fileCount = 0;

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.GetSent(folder.Id)).Returns(Task.FromResult<IEnumerable<File>>(new List<File>()));

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(()=>fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(() => { fileCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
            Assert.AreEqual(5, fileCount);
        }

        [TestMethod]
        public async Task Work_NewFiles_3New2Old()
        {
            //assign
            var folder = EntityBuilder.BuildFolder();
            var files = EntityBuilder.BuildFiles(folder).ToList();

            var oldFile1 = files[0];
            oldFile1.Status = FileStatus.Sent;


            var oldFile2 = files[1];
            oldFile2.Status = FileStatus.New;

            var fileCount = 0;

            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc;
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.GetSent(folder.Id)).Returns(Task.FromResult<IEnumerable<File>>(new List<File> { oldFile1, oldFile2 }));


            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(() => { fileCount++; return Task.CompletedTask; });

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);

            await collector.WorkAsync();

            //asert
            Assert.AreEqual(3, fileCount);
        }

        [TestMethod]
        public async Task Work_NewFiles_3New1Changed()
        {

            var folder = EntityBuilder.BuildFolder();
            var files = EntityBuilder.BuildFiles(folder).ToList();

            var oldFile1 = files[2];
            oldFile1.Status = FileStatus.Sent;


            var oldFile2 = files[3];
            oldFile2.Status = FileStatus.New;

            var fileCount = 0;
            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc.AddHours(1);
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.GetSent(folder.Id)).Returns(Task.FromResult<IEnumerable<File>>(new List<File> { oldFile1, oldFile2 }));

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(() => fileCount == 0); //only one item . second call should return false

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(Task.CompletedTask).Callback(()=> fileCount++);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
            Assert.AreEqual(4, fileCount);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Work_InvalidFolder_ThrowInvalidOperationException()
        {

            //assign
            var folder = EntityBuilder.BuildFolder();
            folder.Name = "DoesNotExist";

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.GetSent(folder.Id)).Returns(Task.FromResult<IEnumerable<File>>(new List<File>()));

            var mockInQueue = new Mock<IWorkItemQueue<Folder>>();
            mockInQueue.Setup(x => x.Dequeue()).Returns(Task.FromResult(folder));
            mockInQueue.Setup(x => x.HasItems()).Returns(true); 

            var mockOutQueue = new Mock<IWorkItemQueue<File>>();
            mockOutQueue.Setup(x => x.Enqueue(It.IsAny<File>())).Returns(Task.CompletedTask);

            var mockFactoryQueue = new Mock<IWorkItemQueueFactory>();
            mockFactoryQueue.Setup(x => x.GetQueue<Folder>(It.IsAny<QueueKind>())).Returns(mockInQueue.Object);
            mockFactoryQueue.Setup(x => x.GetQueue<File>(It.IsAny<QueueKind>())).Returns(mockOutQueue.Object);

            var mockRepository = new Mock<IRepository>();
            mockRepository.SetupGet(x => x.FileRepository).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(mockRepository.Object, mockFactoryQueue.Object);
            await collector.WorkAsync();

            //asert
        }

        //todo verify save
    }
}
