using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            
            //act
            var splitter = new FileSplitter(file, chunkSize);
            var adjustedFile = await splitter.Split();


            //assert
            var amountOfChunksThatShouldBeCreated =
                Math.Ceiling((double)new System.IO.FileInfo(file.GetPath()).Length / (double)chunkSize);

            Assert.AreEqual(adjustedFile.AmountOfChunks, amountOfChunksThatShouldBeCreated);

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

            //act
            var splitter = new FileSplitter(file, chunkSize);
            var adjustedFile = await splitter.Split();


            //assert
            var md5HashOfFile = MD5.Create().ComputeHash(System.IO.File.ReadAllBytes(file.GetPath()));

            Assert.AreEqual(adjustedFile.Hash, Convert.ToBase64String(md5HashOfFile));

        }

        [TestMethod]
        public async Task Split_NotifyIsCalledForeachChunk()
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
            int notifyCalledCount = 0;

            //act
            var splitter = new FileSplitter(file, chunkSize);
            splitter.Subscribe(x => { notifyCalledCount++; });
            await splitter.Split();


            //assert
            var amountOfChunksThatShouldBeCreated =
                Math.Ceiling((double)new System.IO.FileInfo(file.GetPath()).Length / (double)chunkSize);

            Assert.AreEqual(notifyCalledCount, amountOfChunksThatShouldBeCreated);

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

            //act
            var splitter = new FileSplitter(file, chunkSize);
            splitter.Subscribe(x => 
            {
                var bytes = Convert.FromBase64String(x.Value);
                memoryStream.Write(bytes,0,bytes.Length);
            });
            await splitter.Split();


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
            int notifyCalledCount = 0;

            //act
            var splitter = new FileSplitter(null, chunkSize);
            splitter.Subscribe(x => { notifyCalledCount++; });
            await splitter.Split();


            //assert
            Assert.AreEqual(notifyCalledCount, 0);

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
            uint chunkSize = 1 * 1024;
            int notifyCalledCount = 0;

            var writeStream = System.IO.File.OpenWrite(file.GetPath());

            //act
            var splitter = new FileSplitter(file, chunkSize);
            splitter.Subscribe(x => { notifyCalledCount++; });
            await splitter.Split();


            //assert
            Assert.AreEqual(0, notifyCalledCount);

        }
    }
}
