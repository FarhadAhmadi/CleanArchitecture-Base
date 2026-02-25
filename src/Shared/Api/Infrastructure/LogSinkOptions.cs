namespace Web.Api.Infrastructure;

internal sealed class LogSinkOptions
{
    public const string SectionName = "LogSinks";

    public bool EnableConsole { get; init; } = true;
    public string Provider { get; init; } = "Seq";
    public string SeqServerUrl { get; init; } = "http://seq:5341";
    public string ElasticsearchNodeUrl { get; init; } = "http://elasticsearch:9200";
    public string ElasticsearchIndexFormat { get; init; } = "clean-architecture-logs-{0:yyyy.MM.dd}";
}
