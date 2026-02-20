namespace Infrastructure.Logging;

public interface ILogSanitizer
{
    IngestLogRequest Sanitize(IngestLogRequest input);
}
