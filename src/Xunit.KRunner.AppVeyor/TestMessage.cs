using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Xunit.KRunner.AppVeyor
{
    internal class TestMessage : HttpRequestMessage
    {
        public string TestName { get; set; }
        public string FileName { get; set; }
        public Outcome Outcome { get; set; }
        public TimeSpan Duration { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }

        public TestMessage(bool update = false)
            : base(update ? HttpMethod.Put :  HttpMethod.Post, "api/tests")
        {
            Content = new TestContent(this);
        }

        class TestContent : HttpContent
        {
            readonly TestMessage _message;
            Lazy<byte[]> _content;

            public TestContent(TestMessage message)
            {
                _message = message;
                _content = new Lazy<byte[]>(Generate);
                Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Headers.ContentEncoding.Add("utf-8");
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                var data = _content.Value;
                return stream.WriteAsync(data, 0, data.Length);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _content.Value.LongLength;
                return true;
            }

            private byte[] Generate()
            {
                var str = new JObject(
                    new JProperty("testName", _message.TestName),
                    new JProperty("testFramework", "xunit"),
                    new JProperty("fileName", _message.FileName),
                    new JProperty("outcome", _message.Outcome.ToString()),
                    new JProperty("durationMilliseconds", _message.Duration.TotalMilliseconds),
                    new JProperty("ErrorMessage", _message.ErrorMessage),
                    new JProperty("ErrorStackTrace", _message.ErrorStackTrace),
                    new JProperty("StdOut", _message.StdOut),
                    new JProperty("StdErr", _message.StdErr)
                ).ToString();
                return Encoding.UTF8.GetBytes(str);
            }
        }
    }
}
