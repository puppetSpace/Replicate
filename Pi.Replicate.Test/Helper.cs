using Moq;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test
{
    internal static class Helper
    {

        public static string CreateBase64HashForFile(string path)
        {
            MD5 hashCreator = MD5.Create();
            var hash = hashCreator.ComputeHash(File.ReadAllBytes(path));
            return Convert.ToBase64String(hash);
        }

        public static async Task<FileInfo> CompressFile(string path)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            using var stream = System.IO.File.OpenRead(path);
            using var output = System.IO.File.OpenWrite(tempPath);
            using var gzip = new GZipStream(output, CompressionMode.Compress);

            await stream.CopyToAsync(gzip);

            return new FileInfo(tempPath);
        }

		public static byte[] GetByteArray()
		{
			return Encoding.UTF8.GetBytes("thisISTest");
		}

		public static ReadOnlyMemory<byte> GetReadOnlyMemory()
		{
			return Encoding.UTF8.GetBytes("thisISTest").AsMemory();
		}

		public static Worker.Host.Models.File GetFileModel(string directory = "DropLocation",string filename="dummy.txt", DateTime? lastModifiedDate = null)
		{
			return new Worker.Host.Models.File(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), directory, filename)), Guid.Empty, PathBuilder.BasePath,lastModifiedDate);
		}

		public static Worker.Host.Models.File GetFileModel(FileInfo fileInfo)
		{
			return new Worker.Host.Models.File(fileInfo, Guid.Empty, PathBuilder.BasePath);
		}

		public static Mock<IWebhookService> GetWebhookServiceMock(Action<Worker.Host.Models.File> notifyFileAssembled
			, Action<Worker.Host.Models.File> notifyFileDisassembled
			, Action<Worker.Host.Models.File> notifyFileFailed
			)
		{
			var mock = new Mock<IWebhookService>();
			mock.Setup(x => x.NotifyFileAssembled(It.IsAny<Worker.Host.Models.File>())).Callback(notifyFileAssembled);
			mock.Setup(x => x.NotifyFileDisassembled(It.IsAny<Worker.Host.Models.File>())).Callback(notifyFileDisassembled);
			mock.Setup(x => x.NotifyFileFailed(It.IsAny<Worker.Host.Models.File>())).Callback(notifyFileFailed);

			return mock;
		}
    }
}
