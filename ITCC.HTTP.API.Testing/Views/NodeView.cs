using System.Collections.Generic;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using ITCC.HTTP.API.Testing.Enums;
using static ITCC.HTTP.API.Enums.ApiContractType;

namespace ITCC.HTTP.API.Testing.Views
{
    [ApiView(ApiHttpMethod.Get)]
    public class NodeView
    {
        #region properties
        [ApiContract(NotNullNonWhitespaceString)]
        public string Name { get; set; }

        [ApiContract(PositiveNumber)]
        public int Count { get; set; }

        [ApiContract(StrictEnumValue)]
        public SimpleEnum SimpleEnum { get; set; }

        [ApiContract(EnumValue)]
        public FlagsEnum FlagsEnum { get; set; }

        [ApiContract(None)]
        public bool IsLeaf { get; set; }

        public string DoesNotHaveContract { get; set; }

        [ApiContract(CanBeNull | NotEmpty | ItemsNotNull)]
        public List<NodeView> Children { get; set; }
        #endregion

        #region checks

        public bool LeavesMustNotHaveChildren() => IsLeaf || Children != null;

        #endregion
    }
}
