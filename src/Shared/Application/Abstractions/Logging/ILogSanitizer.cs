namespace Application.Abstractions.Logging;

public interface ILogSanitizer
{
    IngestLogRequest Sanitize(IngestLogRequest input);
}
