// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Text;

namespace ITCC.HTTP.Client.Interfaces
{
    public interface IBodySerializer
    {
        Encoding Encoding { get; }

        string ContentType { get; }

        string Serialize(object data);
    }
}
