using Pi.Replicate.Shared;
using Serilog;
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

        public async Task<ProbeResult<TE>> ProbeGet<TE>(string address)
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(20);
			try
			{
				var response = await client.GetAsync($"{address}");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					var returnValue = System.Text.Json.JsonSerializer.Deserialize<TE>(content);
					return new ProbeResult<TE>(isSuccessful: true, response.ReasonPhrase, returnValue);
				}
				return new ProbeResult<TE>(isSuccessful:false,response.ReasonPhrase,default);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to probe");
				return new ProbeResult<TE>(isSuccessful: false, ex.Message,default);
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


	public struct ProbeResult<TE>
	{
		public ProbeResult(bool isSuccessful, string message, TE responseData)
		{
			IsSuccessful = isSuccessful;
			Message = message;
			ResponseData = responseData;
		}

		public bool IsSuccessful { get; }

		public string Message { get; }

		public TE ResponseData { get;}

		public static ProbeResult<TE> EmptyResult => new ProbeResult<TE>(false, null,default);

		public override bool Equals(object obj)
		{
			return obj is ProbeResult<TE> pr
			? pr.IsSuccessful == IsSuccessful && string.Equals(pr.Message, Message) && object.Equals(pr.ResponseData, ResponseData)
			: false;
		}

		public override int GetHashCode()
		{
			return (IsSuccessful.GetHashCode() + Message?.GetHashCode() ?? 0 + ResponseData?.GetHashCode()??0) / 2;
		}

		public static bool operator ==(ProbeResult<TE> left, ProbeResult<TE> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ProbeResult<TE> left, ProbeResult<TE> right)
		{
			return !(left == right);
		}
	}
}
