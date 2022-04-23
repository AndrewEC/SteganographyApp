Write-Host "`n---------- Removing testbed directories ----------`n"
function Remove-Folder {
    [CmdletBinding()]
    param(
        [Parameter()][string] $FolderPath
    )

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
Remove-Folder ./SteganographyApp.Calculator/bin/release
Remove-Folder ./SteganographyApp.Converter/bin/release


Write-Host "`n---------- Publishing release build ----------`n"
dotnet publish -c release
if($LastExitCode -ne 0){
    Write-Host "publish failed with status: $LastExitCode"
    Exit
}

New-Item -Path ./testbed -ItemType Directory | Out-Null


Write-Host "`n---------- Copying publish output ----------`n"
Write-Host "Copying output from SteganographyApp publish"
Get-ChildItem -Path ./SteganographyApp/bin/release/netcoreapp6.0/publish | Copy-Item -Recurse -Destination ./testbed

Write-Host "Copying output from SteganographyApp.Calculator publish"
Copy-Item ./SteganographyApp.Calculator/bin/release/netcoreapp6.0/publish -Recurse -Destination ./testbed
cd testbed
Rename-Item -Path publish -NewName Calculator
cd ..

Write-Host "Copying output from SteganographyApp.Converter publish"
Copy-Item ./SteganographyApp.Converter/bin/release/netcoreapp6.0/publish -Recurse -Destination ./testbed
cd testbed
Rename-Item -Path publish -NewName Converter -Force
cd ..


Write-Host "`n---------- Copying test assets ----------`n"
function Copy-Assets {
    [CmdletBinding()]
    param(
        [Parameter()][string] $Destination
    )

    Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination $Destination
}

Copy-Assets ./testbed
Copy-Assets ./testbed/Calculator
Copy-Assets ./testbed/Converter