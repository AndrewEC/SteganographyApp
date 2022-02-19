Write-Host "`n---------- Removed Previous Output Folders ----------`n"
$StrykerOutputFolder = "./SteganographyApp.Common.Tests/StrykerOutput"
if(Test-Path $StrykerOutputFolder){
    Write-Host "Removing old output folder and contents"
    Remove-Item -Recurse -Force $StrykerOutputFolder | Out-Null
}
if(Test-Path $StrykerOutputFolder){
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
Get-ChildItem -Directory $StrykerOutputFolder | % {
    $path = $_.FullName
    Start-Process "$path\reports\mutation-report.html"
}
