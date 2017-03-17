// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Documentation.Utils
{
    public static class ApiContractHelper
    {
        public static List<ApiContractType> SplitComplexContract(ApiContractType contractType)
        {
            InitializeIfNotYet();

            return PrimitiveContractTypes.Where(ct => contractType.HasFlag(ct)).ToList();
        }

        private static void InitializeIfNotYet()
        {
            lock (Lock)
            {
                if (_initialized)
                    return;

                Initialize();
                _initialized = true;
            }
        }

        private static void Initialize() => PrimitiveContractTypes.AddRange(
            Enum.GetValues(typeof(ApiContractType))
                .Cast<ApiContractType>()
                .Where(IsPowerOfTwo));

        private static bool IsPowerOfTwo(ApiContractType type)
            => type > 0 && ((type & type - 1) == 0);

        private static readonly List<ApiContractType> PrimitiveContractTypes = new List<ApiContractType>();
        private static volatile bool _initialized;
        private static readonly object Lock = new object();
    }
}
