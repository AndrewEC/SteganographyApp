Param(
    [Switch]$OpenReport,
    [Switch]$ReportOnFailure
)


Write-Host "`n---------- Cleaning out existing build artifacts ----------`n"
$CommonBinFolder = "./SteganographyApp.Common.Tests/bin"
if(Test-Path $CommonBinFolder){
    Write-Host "Cleaning bin"
    Remove-Item -Recurse -Force $CommonBinFolder | Out-Null
}
$CommonObjFolder = "./SteganographyApp.Common.Tests/obj"
if(Test-Path $CommonObjFolder){
    Write-Host "Cleaning obj"
    Remove-Item -Recurse -Force $CommonObjFolder | Out-Null
}


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
dotnet tool run coverlet `
    ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp6.0/SteganographyApp.Common.Tests.dll `
    --target "dotnet" `
    --targetargs "test SteganographyApp.sln --no-build" `
    --exclude-by-file "**/SteganographyApp.Common/Arguments/Help.cs" `
    --exclude-by-file "**/SteganographyApp.Common/Injection/Logging/RootLogger.cs" `
    --format opencover `
    --threshold 80 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total

if($LastExitCode -ne 0){
    Write-Host "'coverlet' command failed with status: $LastExitCode"
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

if ($openReport) {
    Write-Host "Opening report"
    ./reports/index.htm
}
