// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using static ITCC.HTTP.API.Enums.ApiContractType;

namespace ITCC.HTTP.Api.Documentation.Testing.Views
{
    [ApiView(ApiHttpMethod.Post)]
    public class SecondRootView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Child elements")]
        [ApiContract(NotNull | CanBeEmpty | ItemsNotNull)]
        public List<SecondChildView> Children { get; set; } = new List<SecondChildView>();
    }
}
