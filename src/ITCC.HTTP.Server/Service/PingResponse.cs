// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Text;

namespace ITCC.HTTP.Server.Service
{
    public class PingResponse
    {
        public PingResponse(string input)
        {
            Time = DateTime.Now;
            Request = input;
        }

        public PingResponse()
        {
            Time = DateTime.Now;
            Request = string.Empty;
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