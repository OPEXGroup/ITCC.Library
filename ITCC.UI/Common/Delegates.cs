using System;

namespace ITCC.UI.Common
{
    public static class Delegates
    {
        public delegate void UiThreadRunner(Action action);
    }
}
