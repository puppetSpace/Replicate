using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Infrastructure.Services
{
    public class ProbeService
    {
		private readonly IHttpClientFactory _httpClientFactory;

		public ProbeService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

        public async Task<ProbeResult> Probe(string address)
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(20);
			try
			{
				var response = await client.GetAsync($"{address}/api/probe");
				return new ProbeResult(response.IsSuccessStatusCode, response.ReasonPhrase);
			}
			catch (Exception ex)
			{
				return new ProbeResult(false, ex.Message);
			}
		}
    }

	public struct ProbeResult
	{
		public ProbeResult(bool isSuccessful, string message)
		{
			IsSuccessful = isSuccessful;
			Message = message;
		}

		public bool IsSuccessful { get; }

		public string Message { get;}


	}
}
