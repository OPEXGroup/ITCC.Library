﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.Logging.Windows.Enums
{
    internal enum EmailLoggerFlushReason
    {
        LoggerStarted,
        RegularPeriodical,
        ImportantMessage,
        QueueFull,
        ForceFlush
    }
}
