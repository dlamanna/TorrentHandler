$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "TorrentHandler.csproj"
$outputDir = Join-Path $repoRoot "bin\Debug"

dotnet publish $project -c Debug -o $outputDir /p:PublishSingleFile=true /p:SelfContained=false /p:PublishReadyToRun=false
