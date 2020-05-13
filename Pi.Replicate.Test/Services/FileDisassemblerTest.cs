using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Infrastructure.Services;
using Pi.Replicate.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Services
{
    [TestClass]
    public class FileDisassemblerTest
    {
        //[TestInitialize]
        //public void InitializeTest()
        //{
        //    EntityBuilder.InitializePathBuilder();
        //}

        [TestMethod]
        public async Task ProcessFile_CorrectAmountOfChunksShouldBeCreated()
        {
            int minimumAmountOfBytesRentedByArrayPool = 128;
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
                x switch
                {
                    "ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
                    "FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
                    _ => ""
                });
            var pathBuilder = new PathBuilder(configurationMock.Object);
            var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));
            var compressedFile = await Helper.CompressFile(fileInfo.FullName);
            var calculatedAmountOfChunks = Math.Ceiling((double)compressedFile.Length / minimumAmountOfBytesRentedByArrayPool);
            int amountOfCalls = 0;
            var chunkCreated = new Action<FileChunk>(x =>
            {
                amountOfCalls++;
            });

			var mockMediator = new Mock<IMediator>();
			mockMediator.Setup(x => x.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
				.Returns(Unit.Task);

			var processService = new FileDisassemblerService(configurationMock.Object, new CompressionService(), pathBuilder, new DeltaService(), mockMediator.Object);
            await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);

            Assert.AreEqual(calculatedAmountOfChunks, amountOfCalls);

        }


        [TestMethod]
        public async Task ProcessFile_FileIsLocked_ShouldReturnEmptyResultSet()
        {
            int minimumAmountOfBytesRentedByArrayPool = 128;
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(x =>
                x switch
                {
                    "ReplicateBasePath" => System.IO.Directory.GetCurrentDirectory(),
                    "FileSplitSizeOfChunksInBytes" => minimumAmountOfBytesRentedByArrayPool.ToString(),
                    _ => ""
                });
            var pathBuilder = new PathBuilder(configurationMock.Object);
            var fileInfo = new System.IO.FileInfo(System.IO.Path.Combine(pathBuilder.BasePath, "FileFolder", "test1.txt"));

            using var fs = fileInfo.OpenWrite();

            int amountOfCalls = 0;
            var chunkCreated = new Action<FileChunk>(x =>
            {
                amountOfCalls++;
            });

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Unit.Task);

            var processService = new FileDisassemblerService(configurationMock.Object, new CompressionService(), pathBuilder, new DeltaService(), mockMediator.Object);
            await processService.ProcessFile(File.Build(fileInfo, System.Guid.Empty, pathBuilder.BasePath), chunkCreated);


            Assert.AreEqual(0, amountOfCalls);

        }
    }
}
