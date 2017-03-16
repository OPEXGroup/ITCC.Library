// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.Api.Documentation.Testing.Enums;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using static ITCC.HTTP.API.Enums.ApiContractType;

namespace ITCC.HTTP.Api.Documentation.Testing.Views
{
    [ApiView(ApiHttpMethod.Post)]
    public class SecondChildView
    {
        [ApiViewPropertyDescription("Unique identifier")]
        [ApiContract(NotNullGuidString)]
        public string Guid { get; set; }

        [ApiViewPropertyDescription("Some flags")]
        [ApiContract(EnumValue)]
        public SecondEnum SomeEnum { get; set; }

        [ApiViewPropertyDescription("Creation date")]
        [ApiContract(None)]
        public DateTime Created { get; set; }
    }
}
