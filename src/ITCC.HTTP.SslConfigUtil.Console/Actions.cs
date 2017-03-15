// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    internal static class Actions
    {
        internal static UtilCommand BindCommand { get; } = new UtilCommand
        {
            Name = "bind",
            Description = "",
            Alias = new List<string> { "b", },
            ParameterSets = new List<ParameterSet>
            {

            }
        };

        class BindWithSubjectNameParamSet : ParameterSet
        {
            public override List<CommandParameter> Parameters { get; set; } = new List<CommandParameter>
            {
                
            };
            public override string Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}