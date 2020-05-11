using Pi.Replicate.Shared;
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

        public async Task<ProbeResult> ProbeGet(string address)
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(20);
			try
			{
				var response = await client.GetAsync($"{address}");
				return new ProbeResult(response.IsSuccessStatusCode, response.ReasonPhrase);
			}
			catch (Exception ex)
			{
				return new ProbeResult(false, ex.Message);
			}
		}

		public async Task<ProbeResult> ProbePost<TE>(string address,TE data)
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(20);
			try
			{
				var response = await client.PostAsync($"{address}",data,throwErrorOnResponseNok:true);
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


		public static ProbeResult EmptyResult => new ProbeResult(false, null);

		public override bool Equals(object obj)
		{

			return obj is ProbeResult pr
			? pr.IsSuccessful == IsSuccessful && string.Equals(pr.Message, Message)
			: false;
		}

		public override int GetHashCode()
		{
			return (IsSuccessful.GetHashCode() + Message?.GetHashCode() ?? 0) / 2;
		}

		public static bool operator ==(ProbeResult left, ProbeResult right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ProbeResult left, ProbeResult right)
		{
			return !(left == right);
		}
	}
}
