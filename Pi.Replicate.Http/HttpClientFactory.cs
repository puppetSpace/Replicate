using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Http
{
    internal static class HttpClientFactory
    {
        //private static Dictionary<Uri, HttpClient> _clients = new Dictionary<Uri, HttpClient>();
        //private static object _lockObject = new object();
        private static HttpClient _httpClient = CreateHttpClient();

        public static HttpClient Get()
        {
            return _httpClient;
        }


        //todo best practices tells that only one instance of HttpClient should be used.
        //but I  don't know what todo with security yet, so it could be that the headers per baseadress differ due to securitytoken of something else
        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    
    }
}
