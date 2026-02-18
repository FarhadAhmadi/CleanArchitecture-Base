param(
    [Parameter(Mandatory = $true)]
    [string]$Module,

    [Parameter(Mandatory = $true)]
    [string]$Feature,

    [ValidateSet("Command", "Query")]
    [string]$Kind = "Command"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-DirectoryIfMissing([string]$Path) {
    if (-not (Test-Path -Path $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$modulePascal = (Get-Culture).TextInfo.ToTitleCase($Module.ToLower()).Replace(" ", "")
$featurePascal = (Get-Culture).TextInfo.ToTitleCase($Feature.ToLower()).Replace(" ", "")

$applicationDir = Join-Path $projectRoot "src\Application\Modules\$modulePascal\$featurePascal"
$endpointDir = Join-Path $projectRoot "src\Web.Api\Endpoints\Modules\$modulePascal"
$testDir = Join-Path $projectRoot "tests\ArchitectureTests\Features\$modulePascal"

New-DirectoryIfMissing $applicationDir
New-DirectoryIfMissing $endpointDir
New-DirectoryIfMissing $testDir

$requestType = if ($Kind -eq "Command") { "ICommand" } else { "IQuery<object>" }
$handlerType = if ($Kind -eq "Command") { "ICommandHandler<$featurePascal$Kind>" } else { "IQueryHandler<$featurePascal$Kind, object>" }

$requestFile = Join-Path $applicationDir "$featurePascal$Kind.cs"
$handlerFile = Join-Path $applicationDir "$featurePascal${Kind}Handler.cs"
$validatorFile = Join-Path $applicationDir "$featurePascal${Kind}Validator.cs"
$mappingFile = Join-Path $applicationDir "$featurePascal`Mappings.cs"
$endpointFile = Join-Path $endpointDir "$featurePascal.cs"
$testFile = Join-Path $testDir "$featurePascal$Kind`Tests.cs"

@"
using Application.Abstractions.Messaging;

namespace Application.$modulePascal.$featurePascal;

public sealed record $featurePascal$Kind() : $requestType;
"@ | Set-Content -Path $requestFile

$handlerContent = if ($Kind -eq "Command") {
@"
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.$modulePascal.$featurePascal;

internal sealed class $featurePascal${Kind}Handler : $handlerType
{
    public async Task<Result> Handle($featurePascal$Kind request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success();
    }
}
"@
}
else {
@"
using Application.Abstractions.Messaging;

namespace Application.$modulePascal.$featurePascal;

internal sealed class $featurePascal${Kind}Handler : $handlerType
{
    public async Task<object> Handle($featurePascal$Kind request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return new { };
    }
}
"@
}

$handlerContent | Set-Content -Path $handlerFile

@"
using FluentValidation;

namespace Application.$modulePascal.$featurePascal;

internal sealed class $featurePascal${Kind}Validator : AbstractValidator<$featurePascal$Kind>
{
    public $featurePascal${Kind}Validator()
    {
    }
}
"@ | Set-Content -Path $validatorFile

@"
namespace Application.$modulePascal.$featurePascal;

internal static class $featurePascal`Mappings
{
}
"@ | Set-Content -Path $mappingFile

@"
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Modules.$modulePascal;

internal sealed class $featurePascal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(""/$($modulePascal.ToLower())/$($featurePascal.ToLower())"", () => Results.Ok());
    }
}
"@ | Set-Content -Path $endpointFile

@"
using Xunit;

namespace ArchitectureTests.Features.$modulePascal;

public sealed class $featurePascal${Kind}Tests
{
    [Fact]
    public void Placeholder()
    {
        Assert.True(true);
    }
}
"@ | Set-Content -Path $testFile

Write-Host "Feature scaffolding created:"
Write-Host " - $requestFile"
Write-Host " - $handlerFile"
Write-Host " - $validatorFile"
Write-Host " - $mappingFile"
Write-Host " - $endpointFile"
Write-Host " - $testFile"
