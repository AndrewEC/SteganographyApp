Param(
    [Switch]$ReportOnFailure
)


Write-Host "`n---------- Cleaning out existing build artifacts ----------`n"
function Remove-Folder {
    param([string] $FolderPath)

    if (Test-Path $FolderPath) {
        Write-Host "Removing folder $FolderPath"
        Remove-Item -Recurse -Force $FolderPath | Out-Null

        if (Test-Path $FolderPath) {
            throw "Could not delete folder $FolderPath"
        }
    }
}

Remove-Folder ./SteganographyApp.Common.Tests/bin
Remove-Folder ./SteganographyApp.Common.Tests/obj
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/bin
Remove-Folder ./SteganographyApp.Common.Arguments.Tests/obj
Remove-Item ./coverage.json
Remove-Item ./coverage.opencover.xml


Write-Host "`n---------- Rebuilding Project ----------`n"
dotnet build SteganographyApp.sln --no-incremental
if ($LastExitCode -ne 0) {
    Write-Host "Build failed with status code: $LastExitCode"
    Exit
}

$ReportsFolder = "./reports"
if(Test-Path $ReportsFolder){
    Write-Host "Removing old report directory and contents"
    Remove-Item -Recurse -Force $ReportsFolder | Out-Null
    if(Test-Path $ReportsFolder){
        Write-Host "Unable to delete $ReportsFolder directory."
        Exit
    }
}

Write-Host "Creating report directory"
New-Item -ItemType directory -Path $ReportsFolder | Out-Null
if(-Not (Test-Path $ReportsFolder)){
    Write-Host "Could not create $ReportsFolder directory"
    Exit
}


Write-Host "`n---------- Running unit tests ----------`n"

Write-Host "---------- Running SteganographyApp.Common.Tests tests ----------"
dotnet tool run coverlet `
    ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Tests.dll `
    --target "dotnet" `
    --targetargs "test ./SteganographyApp.Common.Tests --no-build" `
    --threshold 50 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total

if($LastExitCode -ne 0){
    Write-Host "'coverlet' SteganographyApp.Common.Tests command failed with status: $LastExitCode"
    if (-Not($reportOnFailure)) {
        Exit
    }
}

Write-Host "---------- Running SteganographyApp.Common.Arguments.Tests tests ----------"
dotnet tool run coverlet `
    ./SteganographyApp.Common.Arguments.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Arguments.Tests.dll `
    --target "dotnet" `
    --targetargs "test ./SteganographyApp.Common.Arguments.Tests --no-build" `
    --threshold 50 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total `
    --merge-with coverage.json `
    --format opencover

if($LastExitCode -ne 0){
    Write-Host "'coverlet' SteganographyApp.Common.Arguments.Tests command failed with status: $LastExitCode"
    if (-Not($reportOnFailure)) {
        Exit
    }
}


Write-Host "`n---------- Generating coverage report ----------`n"
dotnet tool run reportgenerator "-reports:coverage.opencover.xml" "-targetDir:reports"
if($LastExitCode -ne 0){
    Write-Host "'reportgenerator' command failed with status: $LastExitCode"
    Exit
}
