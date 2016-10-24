using System;

namespace ITCC.WPF.Common
{
    public static class Delegates
    {
        public delegate void UiThreadRunner(Action action);
    }
}
