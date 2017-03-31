using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace serverapp
{
    public class StreamResponse
    {
        public Stream Stream { get; set; }
        public long SendTime { get; set; }
        public long ReadTime { get; set; }
    }

    public class HttpMessageSender
    {
        private static HttpClient _httpClient;
        public HttpMessageSender()
        {
            // Create _httpClient once only
            if (_httpClient != null)
            {
                return;
            }

            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseProxy = true
            })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public async Task<StreamResponse> SendGet(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var sendTime = 0L;
            var readTime = 0L;
            var sw = new Stopwatch();
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }
            sw.Start();
            var result = await _httpClient.SendAsync(req, cancellationToken);
            sw.Stop();
            sendTime = sw.ElapsedMilliseconds;
            sw.Restart();
            var resultStreamTask = result.Content.ReadAsStreamAsync();
            var resultStream = await resultStreamTask;
            sw.Stop();
            readTime = sw.ElapsedMilliseconds;
            return new StreamResponse { Stream = resultStream, SendTime = sendTime, ReadTime = readTime };
        }

        public async Task<StreamResponse> SendPost(Uri uri, string message, IDictionary<string, string> headers, string contentType, CancellationToken cancellationToken)
        {
            var sendTime = 0L;
            var readTime = 0L;
            var sw = new Stopwatch();
            var stringContent = new StringContent(message);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    stringContent.Headers.Add(header.Key, header.Value);
                }
            }
            sw.Start();
            var result = await _httpClient.PostAsync(uri, stringContent, cancellationToken);
            sw.Stop();
            sendTime = sw.ElapsedMilliseconds;
            sw.Restart();
            var resultStreamTask = result.Content.ReadAsStreamAsync();
            var resultStream = await resultStreamTask;
            sw.Stop();
            readTime = sw.ElapsedMilliseconds;
            return new StreamResponse { Stream = resultStream, SendTime = sendTime, ReadTime = readTime };
        }
    }
}