using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class HttpJsonExtensions
    {
        public static async Task<HttpResponseMessage> PostAsync<TE>(this HttpClient client,string url,TE data,bool throwErrorOnResponseNok=false)
        {
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(data);
            var stringContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, stringContent);
			if (!response.IsSuccessStatusCode && throwErrorOnResponseNok)
			{
				var httpStatusCode = response.StatusCode;
				var reasonPhrase = response.ReasonPhrase;
				response.Dispose();
				throw new InvalidOperationException($"Failed to post data. Reason: {httpStatusCode}: {reasonPhrase}");
			}
            return response;
        }

        public static async Task<TOut> PostAsync<TIn,TOut>(this HttpClient client, string url, TIn data)
        {
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(data);
            var stringContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, stringContent);

			if (response.IsSuccessStatusCode)
				return System.Text.Json.JsonSerializer.Deserialize<TOut>(await response.Content.ReadAsStringAsync());
			else
			{
				var httpStatusCode = response.StatusCode;
				var reasonPhrase = response.ReasonPhrase;
				response.Dispose();
				throw new InvalidOperationException($"Failed to post data. Reason: {httpStatusCode}: {reasonPhrase}");
			}
        }
    }
}
