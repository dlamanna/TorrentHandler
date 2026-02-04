$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "TorrentHandler.csproj"
$outputDir = Join-Path $repoRoot "bin\Release"

$artifactDir = Join-Path $repoRoot "artifacts"
$exePath = Join-Path $outputDir "TorrentHandler.exe"
$artifactExe = Join-Path $artifactDir "TorrentHandler.exe"

dotnet publish $project -c Release -o $outputDir /p:PublishSingleFile=true /p:SelfContained=false /p:PublishReadyToRun=false

New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null

if (-not (Test-Path $exePath))
{
	throw "Missing published executable: $exePath"
}

Copy-Item $exePath -Destination $artifactExe -Force
