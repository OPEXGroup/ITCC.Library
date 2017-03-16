// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.Api.Documentation.Testing.Enums;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using static ITCC.HTTP.API.Enums.ApiContractType;

namespace ITCC.HTTP.Api.Documentation.Testing.Views
{
    [ApiView(ApiHttpMethod.Get)]
    public class FirstListEntryView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Entry index")]
        [ApiContract(PositiveNumber)]
        public int Index { get; set; }

        [ApiViewPropertyDescription("Entry type")]
        [ApiContract(EnumValue)]
        public FirstEnum Type { get; set; }
    }
}
