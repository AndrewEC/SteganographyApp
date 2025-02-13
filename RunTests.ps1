Param(
    [Switch] $ReportOnFailure
)

$ProgressPreference = "SilentlyContinue"


Write-Host "`n---------- Cleaning out existing build artifacts ----------`n"
function Remove-Folder {
    param([string] $FolderPath)

    if (Test-Path $FolderPath -PathType Container) {
        Write-Host "Removing folder $FolderPath"
        Remove-Item -Recurse -Force $FolderPath | Out-Null

        if (Test-Path $FolderPath -PathType Container) {
            throw "Could not delete folder $FolderPath"
        }
    }
}

function Remove-File {
    param([string] $ItemPath)

    if (Test-Path $ItemPath -PathType Leaf) {
        Write-Host "Removing item: $ItemPath"
        Remove-Item $ItemPath
    }
}

Remove-Folder ./SteganographyApp.Common.Tests/bin
Remove-Folder ./SteganographyApp.Common.Tests/obj
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/bin
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/obj
Remove-Folder ./SteganographyApp.Common.Integration.Tests/bin
Remove-Folder ./SteganographyApp.Common.Integration.Tests/obj

Remove-File ./coverage.json
Remove-File ./coverage.opencover.xml


Write-Host "`n---------- Rebuilding Project ----------`n"
dotnet build SteganographyApp.sln --no-incremental
if ($LastExitCode -ne 0) {
    Write-Host "Build failed with status code: $LastExitCode"
    Exit
}

$ReportsFolder = "./reports"
Remove-Folder $ReportsFolder

Write-Host "Creating report directory"
New-Item -ItemType Directory -Path $ReportsFolder | Out-Null
if (-Not (Test-Path $ReportsFolder -PathType Container)) {
    Write-Host "Could not create $ReportsFolder directory"
    Exit
}


Write-Host "`n---------- Running unit tests ----------`n"

Write-Host "---------- Running SteganographyApp.Common.Tests tests ----------"
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
    Write-Host "'coverlet' SteganographyApp.Common.Tests command failed with status: $LastExitCode"
    if (-Not $reportOnFailure) {
        Exit
    }
}

Write-Host "---------- Running SteganographyApp.Common.Arguments.Tests tests ----------"
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
    Write-Host "'coverlet' SteganographyApp.Common.Arguments.Tests command failed with status: $LastExitCode"
    if (-Not $reportOnFailure) {
        Exit
    }
}


Write-Host "`n---------- Running SteganographyApp.Common.Integration.Tests tests ----------`n"
dotnet test ./SteganographyApp.Common.Integration.Tests
if ($LastExitCode -ne 0) {
    Write-Host "dotnet test ./SteganographyApp.Common.Integration.Tests command failed with status: $LastExitCode"
    Exit
}


Write-Host "`n---------- Generating coverage report ----------`n"
dotnet tool run reportgenerator "-reports:coverage.opencover.xml" "-targetDir:reports"
if ($LastExitCode -ne 0) {
    Write-Host "'reportgenerator' command failed with status: $LastExitCode"
    Exit
}