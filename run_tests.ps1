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

Write-Host("`n----------Running unit tests----------`n")
coverlet ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp2.2/SteganographyApp.Common.Tests.dll --target "dotnet" --targetargs "test SteganographyApp.sln --no-build" --format opencover
if($LastExitCode -ne 0){
    Write-Host("'coverlet' command failed with status: $LastExitCode")
    Exit
}

Write-Host("`n---------- Generating coverage report ----------`n")
reportgenerator "-reports:coverage.opencover.xml" "-targetDir:reports"
if($LastExitCode -ne 0){
    Write-Host("'reportgenerator' command failed with status: $LastExitCode")
    Exit
}

Write-Host("`nOpening report")
./reports/index.htm
