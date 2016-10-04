using System;
using System.Text;

namespace ITCC.HTTP.Server.Service
{
    internal class PingResponse
    {
        public PingResponse(string input)
        {
            Time = DateTime.Now;
            Request = input;
        }

        public DateTime Time { get; set; }
        public string Request { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Time: {Time.ToString("s")}");
            builder.AppendLine($"Request:\n{Request}");
            return builder.ToString();
        }
    }
}