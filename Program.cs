using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverapp
{
    public enum Method
    {
        GET,
        POST,
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var url = args[0];
            var postGet = args[1].ToLower() == "get" ? Method.GET : Method.POST;
            string body = null;
            var runs = int.Parse(args[2]);
            Dictionary<string, string> headers = null;
            if (!string.IsNullOrEmpty(args[3]))
            {
                headers = new Dictionary<string, string>();
                var split = args[3].Split(',');
                foreach (var header in split)
                {
                    var t = header.Trim();
                    var st = header.Split(':');
                    headers.Add(st[0].Trim(), st[1].Trim());
                }
            }
            if (postGet == Method.POST)
            {
                body = string.IsNullOrEmpty(args[4]) ? args[4] : null;
            }
            var p = new Program();
            p.Run(url, postGet, runs, headers, body).Wait();
        }

        public async Task Run(string url, Method method, int runs, Dictionary<string, string> headers = null, string body = null)
        {
            var headersS = headers == null ? "null" : string.Join(",", headers.Select(i => i.Key + ":" + i.Value));
            var bodyS = body == null ? "None" : body;
            Console.Out.WriteLine($"Sending {runs} {method.ToString()} requests to {url}.\nHeaders = {headersS}\nBody = {bodyS}");
            var time = 0L;
            var max = 0L;
            var min = long.MaxValue;
            var timeRead = 0L;
            var timeSend = 0L;
            var maxRead = 0L;
            var minRead = long.MaxValue;
            var maxSend = 0L;
            var minSend = long.MaxValue;
            for (var i = 0; i < runs; i++)
            {
                var cts = new System.Threading.CancellationTokenSource();
                var sender = new HttpMessageSender();
                var sw = Stopwatch.StartNew();
                var response = method == Method.GET ? await sender.SendGet(new Uri(url), headers, cts.Token) : await sender.SendPost(new Uri(url), body, headers, "application/json", cts.Token);
                sw.Stop();

                if (i != 0)
                {
                    time += sw.ElapsedMilliseconds;
                    timeRead += response.ReadTime;
                    timeSend += response.SendTime;
                    if (max < sw.ElapsedMilliseconds) max = sw.ElapsedMilliseconds;
                    if (min > sw.ElapsedMilliseconds) min = sw.ElapsedMilliseconds;
                    if (maxRead < response.ReadTime) maxRead = response.ReadTime;
                    if (minRead > response.ReadTime) minRead = response.ReadTime;
                    if (maxSend < response.SendTime) maxSend = response.SendTime;
                    if (minSend > response.SendTime) minSend = response.SendTime;
                    if (i % 10 == 0)
                    {
                        var sbi = new StringBuilder();
                        sbi.AppendLine($"#Requests={i} out of {runs} completed");
                        sbi.AppendLine($"Average responsetime={time / (i - 1)}ms\tAverage send time={timeSend / (i - 1)}ms\tAverage read time={timeRead / (i - 1)}");
                        Console.Out.WriteLine(sbi.ToString());
                    }
                }
                else
                {
                    Console.Out.WriteLine("First Request Details");
                    Console.Out.WriteLine($"Response Time = {sw.ElapsedMilliseconds}ms\tSend time={response.SendTime}ms\tRead time={response.ReadTime}ms");
                }
            }
            var sb = new StringBuilder();
            sb.AppendLine($"#Requests={runs}");
            sb.AppendLine($"Average responsetime={time / (runs - 1)}ms\tAverage send time={timeSend / (runs - 1)}ms\tAverage read time={timeRead / (runs - 1)}");
            sb.AppendLine($"Max response time = {max}ms\tMin response time ={min}ms");
            sb.AppendLine($"Max send time={maxSend}ms\tMin send time={minSend}ms");
            sb.AppendLine($"Max read time={maxRead}ms\tMin read time={minRead}ms");
            Console.Out.Write(sb.ToString());
        }
    }
}
