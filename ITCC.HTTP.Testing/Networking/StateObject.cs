using System.Net.Sockets;
using System.Text;

namespace ITCC.HTTP.Testing.Networking
{
    public class StateObject
    {
        public Socket WorkSocket;
        public const int BufferSize = 256 * 1024;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Sb = new StringBuilder();
        public int TotalReceived = 0;
    }
}
