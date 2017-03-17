// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Samples.Testing.Views
{
    [ApiView(ApiHttpMethod.Post)]
    public class SecondRootView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(ApiContractType.NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Child elements")]
        [ApiContract(ApiContractType.NotNull | ApiContractType.CanBeEmpty | ApiContractType.ItemsNotNull)]
        public List<SecondChildView> Children { get; set; } = new List<SecondChildView>();
    }
}
