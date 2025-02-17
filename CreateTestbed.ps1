. ./_Common.ps1

Write-Output "`n---------- Removing testbed directories ----------`n"
Remove-Folder ./testbed
Remove-Folder ./SteganographyApp/bin/release


Write-Output "`n---------- Publishing release build ----------`n"
dotnet publish -c release
if($LastExitCode -ne 0){
    Write-Output "publish failed with status: $LastExitCode"
    Exit
}

New-Item -Path ./testbed -ItemType Directory | Out-Null


Write-Output "`n---------- Copying publish output ----------`n"
Write-Output "Copying output from SteganographyApp publish"
Get-ChildItem -Path ./SteganographyApp/bin/release/netcoreapp8.0/publish | Copy-Item -Recurse -Destination ./testbed


Write-Output "`n---------- Copying test assets ----------`n"
Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination ./testbed