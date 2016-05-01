using System;

namespace ITCC.HTTP.Utils
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
    }
}