using Newtonsoft.Json;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Http
{
    public sealed class HttpUploadLink : IUploadLink
    {
        private static ILogger _logger = LoggerFactory.Get<HttpUploadLink>();

        public async Task<UploadResponse> FileReceived(Uri baseAddress, Guid fileId)
        {
            _logger.Info($"Sending confirmation that file is received");
            _logger.Debug($"Host: {baseAddress}, File: {fileId}");

            var client = HttpClientFactory.Get();
            var response = await client.GetAsync($"{baseAddress}/api/File/Received?fileid={fileId}");
            if (!response.IsSuccessStatusCode)
                _logger.Warn($"Failed to confirm file has been received from {baseAddress}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

            return new UploadResponse
            {
                IsSuccessful = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? String.Empty : $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}"
            };
        }

        public async Task<UploadResponse> RequestResendOfFile(Uri baseAddress, Guid fileId)
        {
            _logger.Info($"Request resend of file");
            _logger.Debug($"Host: {baseAddress}, File: {fileId}");

            var client = HttpClientFactory.Get();
            var response = await client.GetAsync($"{baseAddress}/api/File/Resend?fileid={fileId}");
            if (!response.IsSuccessStatusCode)
                _logger.Warn($"Failed to request resend of file from {baseAddress}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

            return new UploadResponse
            {
                IsSuccessful = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? String.Empty : $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}"
            };
        }

        public async Task<UploadResponse> UploadData(Uri baseAddress,FileChunk input)
        {
            _logger.Info($"Uploading data");
            _logger.Debug($"Host: {baseAddress}, Data: {input}");

            var stringContent = new StringContent(JsonConvert.SerializeObject(input), System.Text.Encoding.UTF8, "application/json");
            var client = HttpClientFactory.Get();
            var response = await client.PostAsync($"{baseAddress}/api/FileChunk", stringContent);
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
