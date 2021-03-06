Write-Host("`n---------- Removed Previous Output Folders ----------`n")
if(Test-Path ./SteganographyApp.Common.Tests/StrykerOutput){
    Write-Host("Removing old output folder and contents")
    Remove-Item -Recurse -Force ./SteganographyApp.Common.Tests/StrykerOutput | Out-Null
}
if(Test-Path ./SteganographyApp.Common.Tests/StrykerOutput){
    Write-Host("Could not delete existing StrykerOutput folder")
    Exit
}

cd ./SteganographyApp.Common.Tests

Write-Host("`n---------- Executing mutation tests ----------`n")
dotnet tool run dotnet-stryker --config-file-path stryker-config.json
if($LastExitCode -ne 0){
    Write-Host("'stryker' failed with status: $LastExitCode")
    cd ..
    Exit
}

Write-Host("`n---------- Opening report ----------`n")
cd ..
Get-ChildItem -Directory ./SteganographyApp.Common.Tests/StrykerOutput | % {
    $path = $_.FullName
    Start-Process "$path\reports\mutation-report.html"
}
