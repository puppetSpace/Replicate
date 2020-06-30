using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public class WebhookTester
    {
		private readonly IHttpClientFactory _httpClientFactory;

		public WebhookTester(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<WebhookResult> Test<TE>(string address,TE data)
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(20);
			try
			{
				var response = await client.PostAsync($"{address}",data,throwErrorOnResponseNok:true);
				return new WebhookResult(response.IsSuccessStatusCode, response.ReasonPhrase);
			}
			catch (Exception ex)
			{
				return new WebhookResult(false, ex.Message);
			}
		}
	}

	public class WebhookResult
	{
		public WebhookResult(bool isSuccessful, string message)
		{
			IsSuccessful = isSuccessful;
			Message = message;
		}

		public bool IsSuccessful { get; }

		public string Message { get;}

	}
}
