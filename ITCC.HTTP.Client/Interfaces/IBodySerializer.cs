using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.Client.Interfaces
{
    public interface IBodySerializer
    {
        string ContentType { get; }

        string Serialize(object data);
    }
}
