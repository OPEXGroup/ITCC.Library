using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using ITCC.HTTP.SslConfigUtil.Core;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Loggers;
using ITCC.Logging.Windows.Loggers;
using static ITCC.HTTP.SslConfigUtil.Console.ConsoleParams;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(), true);
            System.Console.OutputEncoding = Encoding.UTF8;

            return 0;
        }


        private static SecureString Password(string unsecurePassword)
        {
            if (string.IsNullOrEmpty(unsecurePassword))
                return null;

            var password = new SecureString();
            foreach (var c in unsecurePassword)
                password.AppendChar(c);
            return password;
        }

        private static ArgsParsingResult ParseArgs(string[] args)
        {
            if (args.Any(x => x.Equals("-help", StringComparison.InvariantCultureIgnoreCase)))
            {
                return new ArgsParsingResult
                {
                    IsSucceed = true,
                    ParamDictionary = new Dictionary<string, string> { { "-help", null } }
                };
            }

            var result = new Dictionary<string, string>();

            string currentParam = null;

            foreach (var item in args)
            {
                if (item[0] == '-')
                {
                    if (currentParam != null)
                    {
                        return new ArgsParsingResult
                        {
                            IsSucceed = false,
                            FailReason = $"No value passed for parameter '{currentParam}'."
                        };
                    }

                    var flagParam =
                        ParamList.Any(
                            x => x.IsFlag && x.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));

                    if (flagParam)
                    {
                        result.Add(item.ToLower(), null);
                        continue;
                    }

                    if (result.ContainsKey(item.ToLower()))
                    {
                        return new ArgsParsingResult
                        {
                            IsSucceed = false,
                            FailReason = $"Duplicate parameter '{item}'."
                        };
                    }

                    if (!ParamList.Any(x => x.IsFlag == false && x.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return new ArgsParsingResult
                        {
                            IsSucceed = false,
                            FailReason = $"Unknown parameter '{item}'."
                        };
                    }

                    currentParam = item;
                    continue;
                }

                if (currentParam == null)
                    return new ArgsParsingResult
                    {
                        IsSucceed = false,
                        FailReason = "Use \" ... \" to pass values with space symbols."
                    };

                result.Add(currentParam.ToLower(), item);
                currentParam = null;
            }

            if (currentParam != null)
            {
                if (ParamList.Any(x => x.IsFlag == false && x.Name.Equals(currentParam, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new ArgsParsingResult
                    {
                        IsSucceed = false,
                        FailReason = $"No value passed for parameter '{currentParam}'."
                    };
                }
            }

            foreach (var param in ParamList.Where(x => x.IsRequired))
            {
                if (!result.ContainsKey(param.Name.ToLower()))
                {
                    return new ArgsParsingResult
                    {
                        IsSucceed = false,
                        FailReason = $"Missing required param '{param.Name}'."
                    };
                }
            }

            return new ArgsParsingResult
            {
                IsSucceed = true,
                ParamDictionary = result
            };
        }
    }
}