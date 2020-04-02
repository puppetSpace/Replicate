//using Serilog;
//using System;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Threading.Tasks;

//namespace Pi.Replicate.Processing.Files
//{
//    internal sealed class FileChecker 
//    {


//        public async Task DoWork()
//        {
//            var files = await _repository.FileRepository.GetCompletedReceivedFiles();
//            Log.Information($"Checking hash of {files.Count()} received files");
//            foreach (var file in files)
//            {
//                var hash = await CreateHashFromChunks(file);
//                if (file.Hash == hash)
//                {
//                    Log.Information($"Hash of file {file.Name} is good. Adding item to queue");
//                    await AddItem(file);
//                }
//                else
//                {
//                    Log.Information($"Hash of file {file.Name} is bad. Requesting a resend");
//                    Log.Debug($"Requesting resend of host {file.HostSource}");
//                    await _repository.FileRepository.Delete(file.Id);
//                    await _repository.FileChunkRepository.DeleteForFile(file.Id);
//                    await _uploadLink.RequestResendOfFile(new Uri(file.HostSource), file.Id);
//                }
//            }
//        }

//        private async Task<string> CreateHashFromChunks(File file)
//        {
//            var chunks = await _repository.FileChunkRepository.GetForFile(file.Id);
//            var rawBytes = chunks.OrderBy(x => x.SequenceNo)
//                .SelectMany(x => System.Convert.FromBase64String(x.Value))
//                .ToArray();

//            var hashCreator = MD5.Create();
//            var hash = System.Convert.ToBase64String(hashCreator.ComputeHash(rawBytes));
//            return hash;
//        }
//    }
//}
