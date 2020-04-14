using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pi.Replicate.Application
{
    public class HttpHelper
    {
        private readonly HttpClient _client;
        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<TOut> Post<TIn, TOut>(string endpoint, TIn data)
        {
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(data);
            var stringContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("", stringContent);
            if (response.IsSuccessStatusCode)
                return System.Text.Json.JsonSerializer.Deserialize<TOut>(await response.Content.ReadAsStringAsync());
            else
                throw new InvalidOperationException($"Failed to post data. Reason: {response.StatusCode}: {response.ReasonPhrase}");

        }

        public async Task Post<TIn>(string endpoint, TIn data)
        {
            await Post<TIn,object>(endpoint,data);
        }
    }
}