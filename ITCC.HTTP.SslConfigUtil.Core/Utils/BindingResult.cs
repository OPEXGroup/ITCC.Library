using ITCC.HTTP.SslConfigUtil.Core.Enums;

namespace ITCC.HTTP.SslConfigUtil.Core.Utils
{
    public class BindingResult
    {
        public BindingStatus Status { get; set; }
        public string Reason { get; set; }
    }
}