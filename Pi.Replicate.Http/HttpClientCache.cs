using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Http
{
    internal static class HttpClientCache
    {
        private static Dictionary<Uri, HttpClient> _clients = new Dictionary<Uri, HttpClient>();
        private static object _lockObject = new object();

        public static HttpClient GetHttpClient(Uri uri)
        {
            lock (_lockObject)
            {
                if (!_clients.ContainsKey(uri))
                {
                    _clients.Add(uri, CreateHttpClient(uri));
                }

                return _clients[uri];
            }
        }


        //todo best practices tells that only one instance of HttpClient should be used.
        //but I  don't know what todo with security yet, so it could be that the headers per baseadress differ due to securitytoken of something else
        private static HttpClient CreateHttpClient(Uri uri)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = uri;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    
    }
}
