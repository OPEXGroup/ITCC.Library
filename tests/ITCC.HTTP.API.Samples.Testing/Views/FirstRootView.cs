// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Samples.Testing.Views
{
    [ApiView(ApiHttpMethod.Get)]
    public class FirstRootView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(ApiContractType.NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Child elements")]
        [ApiContract(ApiContractType.NotNull | ApiContractType.CanBeEmpty | ApiContractType.ItemsNotNull)]
        public List<FirstListEntryView> Children { get; set; } = new List<FirstListEntryView>();

        [ApiContract(ApiContractType.NotNullNonEmptyGuidList)]
        public List<string> SomeGuids { get; set; } = new List<string>();

        [ApiViewCheck("All guids in list must be distinct")]
        // ReSharper disable once UnusedMember.Global
        public bool SomeGuidsMustBeDistinct() => SomeGuids.Count == SomeGuids.Distinct().Count();
    }
}
