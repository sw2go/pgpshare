using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest.Data
{
    public class RawBuffer : IRawBuffer
    {
        public bool HasData { get; private set; }
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string QueryString { get; private set; }
        public string Protocol { get; private set; }
        public IHeaderDictionary Headers { get; private set; }
        public string Body { get; private set; }

        public RawBuffer()
        {
        }

        public RawBuffer(HttpRequest request, string body)
        {
            Set(request, body);
        }

        public void Set(HttpRequest request, string body)
        {
            HasData = request != null;

            if (HasData)
            {
                Method = request.Method;
                Path = request.Path;
                QueryString = request.QueryString.HasValue ? request.QueryString.Value : "";
                Protocol = request.Protocol;
                Headers = request.Headers;
                Body = body;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{Method} {Path}{QueryString} {Protocol}");

            foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in Headers)
            {
                builder.AppendLine($"{header.Key}: {string.Join(" ", header.Value)}");
            }
            builder.AppendLine();

            builder.Append(Body);

            return builder.ToString();
        }
    }

    public interface IRawBuffer
    {
        public bool HasData { get; }
        public void Set(HttpRequest request, string body);
        public string Method { get; }
        public string Path { get; }
        public string QueryString { get; }
        public string Protocol { get; }
        IHeaderDictionary Headers { get; }
        public string Body { get; }

        public string ToString();
    }
}
