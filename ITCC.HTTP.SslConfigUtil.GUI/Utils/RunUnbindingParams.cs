namespace ITCC.HTTP.SslConfigUtil.GUI.Utils
{
    internal class RunUnbindingParams
    {
        public string ApplicationPath { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public bool Unsafe = false;
    }
}