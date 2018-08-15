using Newtonsoft.Json;
using Pi.Replicate.Processors.Upload;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Http
{
    //todo mayne also make IUploadLink interface with only in
    public class HttpUploadLink : IUploadLink<FileChunk, object>
    {
        private static ILogger _logger = LoggerFactory.Get<HttpUploadLink>();

        public async Task<UploadResponse<object>> UploadData(Uri baseAddress,FileChunk input)
        {
            _logger.Info($"Uploading data to {baseAddress}");
            _logger.Debug($"Data: {input}");

            var stringContent = new StringContent(JsonConvert.SerializeObject(input), System.Text.Encoding.UTF8, "application/json");
            var client = HttpClientCache.GetHttpClient(baseAddress);
            var response = await client.PostAsync("/api/FileChunk", stringContent);
            if (!response.IsSuccessStatusCode)
                _logger.Warn($"Failed to upload data to {baseAddress}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

            return new UploadResponse<object>
            {
                IsSuccessful = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync()) : null,
                ErrorMessage = response.IsSuccessStatusCode ? String.Empty : $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}"
            };
        }
    }
}
