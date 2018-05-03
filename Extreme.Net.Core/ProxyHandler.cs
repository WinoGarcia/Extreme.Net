using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Extreme.Net
{
    public class ProxyHandler : HttpClientHandler
    {
        private ProxyClient proxyClient;

        public ProxyHandler(ProxyClient proxyClient)
        {
            this.proxyClient = proxyClient;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            HttpResponse httpResponse;

            var httpRequest  = request.ToHttpRequest();
            httpRequest.Proxy = proxyClient;
            httpRequest.SslCertificateValidatorCallback += (sender, certificate, chain, errors) => true;
            cancellationToken.ThrowIfCancellationRequested();

            if (request.Method == System.Net.Http.HttpMethod.Get)
            {
                httpResponse = await GetAsync(httpRequest, request);
            }
            else if(request.Method == System.Net.Http.HttpMethod.Post)
            {
                httpResponse = await PostAsync(httpRequest, request);
            }
            else
            {
                httpResponse = await RawAsync(httpRequest, request);
            }

            cancellationToken.ThrowIfCancellationRequested();

            response = httpResponse.ToHttpResponseMessage();
            response.RequestMessage = request;

            var stream = httpResponse.ToMemoryStream();
            if (stream != null)
            {
                response.Content = new ProgressStreamContent(stream, CancellationToken.None);
            }

            return response;
        }

        private async Task<HttpResponse> GetAsync(HttpRequest request, HttpRequestMessage message)
        {
            return await request.GetAsync(message.RequestUri.ToString());
        }

        private async Task<HttpResponse> PostAsync(HttpRequest request, HttpRequestMessage message)
        {
            var content = await message.Content.ReadAsByteArrayAsync();
            return await request.PostAsync(message.RequestUri.ToString(), content);
        }

        private async Task<HttpResponse> RawAsync(HttpRequest request, HttpRequestMessage message)
        {
            var method = ConvertMethod(message.Method);
            return await request.RawAsync(method, message.RequestUri.ToString());
        }

        private HttpMethod ConvertMethod(System.Net.Http.HttpMethod netHttpMethod)
        {
            if (netHttpMethod == System.Net.Http.HttpMethod.Head)
                return HttpMethod.HEAD;

            if (netHttpMethod == System.Net.Http.HttpMethod.Delete)
                return HttpMethod.DELETE;
            
            if (netHttpMethod == System.Net.Http.HttpMethod.Put)
                return HttpMethod.PUT;

            throw new HttpException($"Method {netHttpMethod} not supported");
        }
    }
}
