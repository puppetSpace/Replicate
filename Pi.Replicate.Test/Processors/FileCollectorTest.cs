using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processors;
using Pi.Replicate.Processors.Files;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class FileCollectorTest
    {
        [TestMethod]
        public void ProcessFiles_NewFiles_5New()
        {
            //assign
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };

            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File>());

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(folder, mockRepository.Object);
            var fileCount = 0;
            collector.Subscribe(x => { fileCount++; });

            collector.ProcessFiles();

            //asert
            Assert.AreEqual(5, fileCount);
        }

        [TestMethod]
        public void ProcessFiles_NewFiles_3New2Old()
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

            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc;
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File> { oldFile1, oldFile2 });

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(folder, mockRepository.Object);
            var fileCount = 0;
            collector.Subscribe(x => { fileCount++; });

            collector.ProcessFiles();

            //asert
            Assert.AreEqual(3, fileCount);
        }

        [TestMethod]
        public void ProcessFiles_NewFiles_3New1Changed()
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

            oldFile1.LastModifiedDate = new System.IO.FileInfo(oldFile1.GetPath()).LastWriteTimeUtc.AddHours(1);
            oldFile2.LastModifiedDate = new System.IO.FileInfo(oldFile2.GetPath()).LastWriteTimeUtc;


            var mockFileRepository = new Mock<IFileRepository>();
            mockFileRepository.Setup(fr => fr.Get(folder.Id)).Returns(new List<File> { oldFile1, oldFile2 });

            var mockRepository = new Mock<IRepositoryFactory>();
            mockRepository.Setup(mr => mr.CreateFileRepository()).Returns(mockFileRepository.Object);

            //act
            var collector = new FileCollector(folder, mockRepository.Object);
            var fileCount = 0;
            collector.Subscribe(x => { fileCount++; });

            collector.ProcessFiles();

            //asert
            Assert.AreEqual(4, fileCount);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProcessFiles_InvalidFolder_ThrowInvalidOperationException()
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

            //act
            var collector = new FileCollector(folder, mockRepository.Object);
            var fileCount = 0;
            collector.Subscribe(x => { fileCount++; });

            collector.ProcessFiles();

            //asert
        }

        //todo verify save
    }
}
