$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "TorrentHandler.csproj"
$outputDir = Join-Path $repoRoot "bin\Release"

$stagingDir = Join-Path $repoRoot "artifacts\release"
$templateConfig = Join-Path $repoRoot "config.template.json"
$zipPath = Join-Path $repoRoot "artifacts\TorrentHandler.zip"

dotnet publish $project -c Release -o $outputDir /p:PublishSingleFile=true /p:SelfContained=false /p:PublishReadyToRun=false

if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

Get-ChildItem -Path $outputDir -File | Copy-Item -Destination $stagingDir -Force

if (Test-Path $templateConfig)
{
	Copy-Item $templateConfig -Destination (Join-Path $stagingDir "config.json") -Force
}

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $stagingDir "*") -DestinationPath $zipPath
