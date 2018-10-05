using Newtonsoft.Json;
using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Http
{
    public class HttpUploadLink : IUploadLink
    {
        private static ILogger _logger = LoggerFactory.Get<HttpUploadLink>();

        public Task<UploadResponse> FileReceived(Uri baseAddress, Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<UploadResponse> RequestResendOfFile(Uri baseAddress, Guid fileId)
        {
            throw new NotImplementedException();
        }

        public async Task<UploadResponse> UploadData(Uri baseAddress,FileChunk input)
        {
            _logger.Info($"Uploading data to {baseAddress}");
            _logger.Debug($"Data: {input}");

            var stringContent = new StringContent(JsonConvert.SerializeObject(input), System.Text.Encoding.UTF8, "application/json");
            var client = HttpClientCache.GetHttpClient(baseAddress);
            var response = await client.PostAsync("/api/FileChunk", stringContent);
            if (!response.IsSuccessStatusCode)
                _logger.Warn($"Failed to upload data to {baseAddress}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

            return new UploadResponse
            {
                IsSuccessful = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? String.Empty : $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}"
            };
        }
    }
}
