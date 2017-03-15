// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Threading.Tasks;

namespace ITCC.UI.Common
{
    public static class Delegates
    {
        public delegate Task AsyncUiThreadRunner(Action action);
    }
}
