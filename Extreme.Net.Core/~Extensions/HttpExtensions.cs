using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Net
{
    internal static class HttpExtensions
    {
        readonly static Dictionary<string, string> headerSeparators = new Dictionary<string, string>(){
                {"User-Agent", " "}
        };

        public static HttpResponseMessage ToHttpResponseMessage(this HttpResponse httpResponse)
        {
            var code     = (System.Net.HttpStatusCode)((int)httpResponse.StatusCode);

            var response = new HttpResponseMessage(code); 
            var headers  = httpResponse.EnumerateHeaders();

            while (headers.MoveNext())
            {
                var pair = headers.Current;
                response.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }

            return response;
        }

        public static HttpRequest ToHttpRequest(this HttpRequestMessage request)
        {
            var httpRequest = new HttpRequest();

            var headers = request.Headers.Union(request.Content != null 
                ? (IEnumerable<KeyValuePair<string, IEnumerable<string>>>)request.Content.Headers 
                : Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());

            //TODO: Copy cookies

            foreach(var keyValue in headers)
            {
                httpRequest.AddHeader(keyValue.Key, String.Join(GetHeaderSeparator(keyValue.Key), keyValue.Value));
            }

            return httpRequest;
        }

        private static string GetHeaderSeparator(string name)
        {
            if (headerSeparators.ContainsKey(name))
            {
                return headerSeparators[name];
            }

            return ",";
        }
    }
}
