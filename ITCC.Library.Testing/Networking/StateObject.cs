using System.Net.Sockets;
using System.Text;

namespace ITCC.Library.Testing.Networking
{
    public class StateObject
    {
        public Socket WorkSocket;
        public const int BufferSize = 256;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Sb = new StringBuilder();
    }
}
