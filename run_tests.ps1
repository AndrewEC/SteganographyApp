Write-Host("`n---------- Cleaning out existing build artifacts ----------`n")
if(Test-Path ./SteganographyApp.Common.Tests/bin){
    Write-Host("Cleaning bin")
    Remove-Item -Recurse -Force ./SteganographyApp.Common.Tests/bin | Out-Null
}
if(Test-Path ./SteganographyApp.Common.Tests/obj){
    Write-Host("Cleaning obj")
    Remove-Item -Recurse -Force ./SteganographyApp.Common.Tests/obj | Out-Null
}

Write-Host("`n---------- Rebuilding Project ----------`n")
dotnet build SteganographyApp.sln --no-incremental

if(Test-Path ./reports){
    Write-Host("Removing old report directory and contents")
    Remove-Item -Recurse -Force ./reports | Out-Null
}
if(Test-Path ./reports){
    Write-Host("Unable to delete ./reports directory.")
    Exit
}

Write-Host("Creating report directory")
New-Item -ItemType directory -Path ./reports | Out-Null
if(-Not (Test-Path ./reports)){
    Write-Host("Could not create ./reports directory")
    Exit
}

Write-Host("`n---------- Running unit tests ----------`n")
dotnet tool run coverlet `
    ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp5.0/SteganographyApp.Common.Tests.dll `
    --target "dotnet" `
    --targetargs "test SteganographyApp.sln --no-build" `
    --format opencover `
    --exclude "[*]SteganographyApp.Common.Providers.*" `
    --threshold 85 `
    --threshold-type line `
    --threshold-type branch `
    --threshold-stat total
if($LastExitCode -ne 0){
    Write-Host("'coverlet' command failed with status: $LastExitCode")
    Exit
}

Write-Host("`n---------- Generating coverage report ----------`n")
dotnet tool run reportgenerator "-reports:coverage.opencover.xml" "-targetDir:reports"
if($LastExitCode -ne 0){
    Write-Host("'reportgenerator' command failed with status: $LastExitCode")
    Exit
}

Write-Host("`n---------- Opening report ----------`n")
./reports/index.htm
