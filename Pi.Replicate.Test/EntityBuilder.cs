using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Test
{
    internal static class EntityBuilder
    {
        public static Folder BuildFolder()
        {
            var currentDir = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            return new Folder(currentDir)
            {
                Id = Guid.NewGuid(),
                Name = "FileFolder"
            };
        }

        public static IEnumerable<File> BuildFiles(Folder folder)
        {
            return new List<File>
            {
                new File
                {
                    Folder = folder,
                    Name = "test1.txt",
                    Status = FileStatus.Sent
                },
                new File
                {
                    Folder = folder,
                    Name = "test2.txt",
                    Status = FileStatus.Sent
                },
                new File
                {
                    Folder = folder,
                    Name = "test3.txt",
                    Status = FileStatus.Sent
                },
                new File
                {
                    Folder = folder,
                    Name = "test4.txt",
                    Status = FileStatus.Sent
                },
                new File
                {
                    Folder = folder,
                    Name = "test5.txt",
                    Status = FileStatus.Sent
                },
            };
        }

        public static async Task<IEnumerable<FileChunk>> BuildChunks(File file, string alternateFolderPath = "")
        {
            var buffer = new byte[1024];
            int bytesRead = 0;
            int chunksCreated = 0;
            MD5 hashCreator = MD5.Create();
            //todo compression
            var fileChunks = new List<FileChunk>();
            var stream = System.IO.File.OpenRead(alternateFolderPath == "" ? file.GetPath() : System.IO.Path.Combine(alternateFolderPath,file.Name));
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);

                var toWriteBytes = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, toWriteBytes, 0, bytesRead);

                fileChunks.Add(new FileChunk
                {
                    Id = Guid.NewGuid(),
                    File = file,
                    SequenceNo = ++chunksCreated,
                    Value = Convert.ToBase64String(toWriteBytes),
                    Status = ChunkStatus.New
                });
            }

            hashCreator.TransformFinalBlock(buffer, 0, bytesRead);
            file.Hash = Convert.ToBase64String(hashCreator.Hash);
            file.AmountOfChunks = chunksCreated;

            return fileChunks;
        }

        public static string CreateHashForFile(File file, string alternateFolderPath = "")
        {
            MD5 hashCreator = MD5.Create();
            var hash = hashCreator.ComputeHash(System.IO.File.ReadAllBytes(alternateFolderPath == "" ? file.GetPath() : System.IO.Path.Combine(alternateFolderPath, file.Name)));
            return Convert.ToBase64String(hash);
        }
    }
}
