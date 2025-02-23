. ./_Common.ps1

Write-Divider "Removing testbed directories"
Remove-Folder ./testbed
Remove-Folder ./SteganographyApp/bin/release


Write-Divider "Publishing release build"
dotnet publish -c release
if($LastExitCode -ne 0){
    Write-Output "publish failed with status: $LastExitCode"
    Exit
}

New-Item ./testbed -ItemType Directory | Out-Null


Write-Divider "Copying publish output"
Write-Output "Copying output from SteganographyApp publish"
Get-ChildItem ./SteganographyApp/bin/release/netcoreapp8.0/publish `
    | Copy-Item -Recurse -Destination ./testbed


Write-Divider "Copying test assets"
Get-ChildItem ./SteganographyApp.Common.Tests/TestAssets `
    | Where-Object Name -like "*.png" `
    | Copy-Item -Force -Destination ./testbed