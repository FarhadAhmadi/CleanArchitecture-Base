namespace ArchitectureTests.Modules.Common;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EndpointAccessCollection
{
    public const string Name = "EndpointAccessSerial";
}
