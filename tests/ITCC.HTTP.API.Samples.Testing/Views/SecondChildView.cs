// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using ITCC.HTTP.API.Samples.Testing.Enums;

namespace ITCC.HTTP.API.Samples.Testing.Views
{
    [ApiView(ApiHttpMethod.Post)]
    public class SecondChildView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(ApiContractType.NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Some flags")]
        [ApiContract(ApiContractType.EnumValue)]
        public SecondEnum SomeEnum { get; set; }

        [ApiViewPropertyDescription("Creation date")]
        [ApiContract(ApiContractType.None)]
        public DateTime Created { get; set; }
    }
}
