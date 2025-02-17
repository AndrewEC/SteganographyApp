. ./_Common.ps1
$ProgressPreference = "SilentlyContinue"

Write-Divider "Cleaning out existing build artifacts"
Remove-Folder ./SteganographyApp.Common.Tests/bin
Remove-Folder ./SteganographyApp.Common.Tests/obj
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/bin
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/obj
Remove-Folder ./SteganographyApp.Common.Integration.Tests/bin
Remove-Folder ./SteganographyApp.Common.Integration.Tests/obj

Remove-File ./coverage.json
Remove-File ./coverage.opencover.xml


Write-Divider "Rebuilding Project"
dotnet build SteganographyApp.sln --no-incremental
if ($LastExitCode -ne 0) {
    Write-Output "Build failed with status code: $LastExitCode"
    Exit
}

$ReportsFolder = "./reports"
Remove-Folder $ReportsFolder

Write-Output "Creating report directory"
New-Item -ItemType Directory -Path $ReportsFolder | Out-Null
if (-Not (Test-Path $ReportsFolder -PathType Container)) {
    Write-Output "Could not create $ReportsFolder directory"
    Exit
}


Write-Divider "Running unit tests"

Write-Divider "Running SteganographyApp.Common.Tests tests"
dotnet tool run coverlet `
    ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Tests.dll `
    --target "dotnet" `
    --targetargs "test ./SteganographyApp.Common.Tests --no-build" `
    --exclude-by-file "**/RootLogger.cs" `
    --exclude-by-file "**/Logger.cs" `
    --exclude-by-file "**/BasicImageInfo.cs" `
    --exclude-by-file "**/ConsoleProxy.cs" `
    --exclude-by-file "**/FileProxy.cs" `
    --exclude-by-file "**/ImageProxy.cs" `
    --exclude-by-file "**/ReadWriteStream.cs" `
    --threshold 60 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total

if ($LastExitCode -ne 0) {
    Write-Output "'coverlet' SteganographyApp.Common.Tests command failed with status: $LastExitCode"
}

Write-Divider "Running SteganographyApp.Common.Arguments.Tests tests"
dotnet tool run coverlet `
    ./SteganographyApp.Common.Arguments.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Arguments.Tests.dll `
    --target "dotnet" `
    --targetargs "test ./SteganographyApp.Common.Arguments.Tests --no-build" `
    --exclude-by-file "**/Help.cs" `
    --threshold 60 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total `
    --merge-with coverage.json `
    --format opencover

if ($LastExitCode -ne 0) {
    Write-Output "'coverlet' SteganographyApp.Common.Arguments.Tests command failed with status: $LastExitCode"
}


Write-Divider "Running SteganographyApp.Common.Integration.Tests tests"
dotnet test ./SteganographyApp.Common.Integration.Tests
if ($LastExitCode -ne 0) {
    Write-Output "dotnet test ./SteganographyApp.Common.Integration.Tests command failed with status: $LastExitCode"
    Exit
}


Write-Divider "Generating coverage report"
dotnet tool run reportgenerator "-reports:coverage.opencover.xml" "-targetDir:reports"
if ($LastExitCode -ne 0) {
    Write-Output "'reportgenerator' command failed with status: $LastExitCode"
    Exit
}
