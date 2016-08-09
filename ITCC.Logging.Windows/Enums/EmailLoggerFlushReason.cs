namespace ITCC.Logging.Enums
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
