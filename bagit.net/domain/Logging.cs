using Microsoft.Extensions.Logging;

namespace bagit.net.domain
{
    public static class Logging
    {
        public static void LogEvent(MessageRecord messageRecord, bool quiet, ILogger logger)
        {

            switch (messageRecord.GetLevel())
            {
                case MessageLevel.INFO:
                    if (!quiet) logger.LogInformation(messageRecord.GetMessage());
                    break;
                case MessageLevel.ERROR:
                    logger.LogError(messageRecord.GetMessage());
                    break;
                case MessageLevel.WARNING:
                    logger.LogWarning(messageRecord.GetMessage());
                    break;
                default:
                    throw new InvalidDataException("Unknown message level");
            }
        }

        public static void LogEvents(IEnumerable<MessageRecord> records, bool quiet, ILogger logger)
        {
            foreach (var messageRecord in records)
            {
                LogEvent(messageRecord, quiet, logger);
            }
        }
    }
}
