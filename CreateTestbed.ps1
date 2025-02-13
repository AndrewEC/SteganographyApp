Write-Host "`n---------- Removing testbed directories ----------`n"
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

Remove-Folder ./testbed
Remove-Folder ./SteganographyApp/bin/release


Write-Host "`n---------- Publishing release build ----------`n"
dotnet publish -c release
if($LastExitCode -ne 0){
    Write-Host "publish failed with status: $LastExitCode"
    Exit
}

New-Item -Path ./testbed -ItemType Directory | Out-Null


Write-Host "`n---------- Copying publish output ----------`n"
Write-Host "Copying output from SteganographyApp publish"
Get-ChildItem -Path ./SteganographyApp/bin/release/netcoreapp8.0/publish | Copy-Item -Recurse -Destination ./testbed


Write-Host "`n---------- Copying test assets ----------`n"
Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination ./testbed