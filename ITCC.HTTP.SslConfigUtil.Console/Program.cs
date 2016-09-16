using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using ITCC.HTTP.SslConfigUtil.Core;
using ITCC.Logging.Core;
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

            //var inputParamsParseResult = ParseArgs(args);
            //if (!inputParamsParseResult.IsSucceed)
            //{
            //    System.Console.WriteLine($"Input params are incorrect: {inputParamsParseResult.FailReason}" +
            //                             "\n\nRun program with -help parameter to know how to use this util.\n");
            //    return -1;
            //}

            //if (inputParamsParseResult.ParamDictionary.ContainsKey("-help"))
            //{
            //    DisplayHelp();
            //    return 0;
            //}

            //if (inputParamsParseResult.ParamDictionary.ContainsKey("-verbose"))
            //    Logger.Level = LogLevel.Debug;

            //var bindResult = CertificateController.Bind(
            //    inputParamsParseResult.ParamDictionary["-assemblypath"],
            //    inputParamsParseResult.ParamDictionary["-cn"],
            //    inputParamsParseResult.ParamDictionary["-ip"],
            //    inputParamsParseResult.ParamDictionary["-port"],
            //    inputParamsParseResult.ParamDictionary.ContainsKey("-createCertificate")
            //    );

            //System.Console.WriteLine(bindResult);

            var res = Binder.Bind(@"C:\Users\black\Source\Repos\ITCC.Library\ITCC.HTTP\bin\Debug\ITCC.HTTP.dll", @"ololo.my.domain", @"127.0.0.1", @"50505", true);
            System.Console.WriteLine(res);
            System.Console.ReadLine();
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