// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Text;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    public static class ConsoleParams
    {
        #region ParamList
        internal static readonly List<CommandParameter> ParamList = new List<CommandParameter>
        {
            new CommandParameter("-ip")
            {
                IsFlag = false,
                IsRequired = true,
                Description = @"Target interface ip address. Use 0.0.0.0 for listening on all interfaces."
            },
            new CommandParameter("-port")
            {
                IsFlag = false,
                IsRequired = true,
                Description = @"Port using for listen on"
            },
            new CommandParameter("-assemblyPath")
            {
                IsFlag = false,
                IsRequired = true,
                Description = @"Full path to server assebmly."
            },
            new CommandParameter("-cn")
            {
                IsFlag = false,
                IsRequired = true,
                Description = @"FQDN of service. Matching SSL certificate will be obtained from Localhost certificateStore. If certificate doesn't exist or is invalid - binding will be aborted. Use '-createCertificate' param to create self-signed certificate and use it."
            },
            new CommandParameter("-certPath")
            {
                IsFlag = false,
                IsRequired = false,
                Description = @"Full path to certificate file."
            },
            new CommandParameter("-createCertificate")
            {
                IsFlag = true,
                Description = @"Creates self-signed certificate with specified CN."
            },
            new CommandParameter("-verbose")
            {
                IsFlag = true,
                Description = @"Verbose output."
            },
            new CommandParameter("-help")
            {
                IsFlag = true,
                Description = @""
            },
        };
        #endregion

        public static void DisplayHelp()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("ITCC SSL Certificate Configuration Util v.1.0.0\n\n");
            stringBuilder.AppendLine("This util helps to configure HttpListener-based web servers (incl. ITCC.HTTP.Server) for using HTTPS.\n");
            stringBuilder.AppendLine("PARAMETER LIST");
            stringBuilder.AppendLine($"{"Parameter",-20}\t{"Using",-10}\tDescription");
            stringBuilder.AppendLine($"{"---------",-20}\t{"-----",-10}\t-----------");
            foreach (var parameter in ParamList)
            {
                string paramType;
                if (parameter.IsFlag)
                    paramType = "[Flag]";
                else
                    paramType = parameter.IsRequired ? "[Required]" : "[Optional]";

                stringBuilder.AppendLine($"{parameter.Name,-20}\t{paramType,-10}\t{parameter.Description}");
            }
            stringBuilder.AppendLine();
            System.Console.WriteLine(stringBuilder);
        }
    }
}