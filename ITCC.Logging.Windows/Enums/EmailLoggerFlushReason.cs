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
