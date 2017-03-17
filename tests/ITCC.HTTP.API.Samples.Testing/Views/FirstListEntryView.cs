// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using ITCC.HTTP.API.Samples.Testing.Enums;

namespace ITCC.HTTP.API.Samples.Testing.Views
{
    [ApiView(ApiHttpMethod.Get)]
    public class FirstListEntryView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(ApiContractType.NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Entry index")]
        [ApiContract(ApiContractType.PositiveNumber)]
        public int Index { get; set; }

        [ApiViewPropertyDescription("Entry type")]
        [ApiContract(ApiContractType.EnumValue)]
        public FirstEnum Type { get; set; }
    }
}
