Param(
    [Switch] $Release
)

$BuildType = "debug"
if ($Release) {
    $BuildType = "release"
}

Write-Host "`n---------- Removing old publish directories ----------`n"
function Remove-Folder {
    param([string] $FolderPath)

    if (Test-Path $FolderPath -PathType Container) {
        Write-Host "Removing folder $FolderPath"
        Remove-Item -Recurse -Force $FolderPath | Out-Null

        if (Test-Path $FolderPath -PathType Container) {
            throw "Could not delete folder $FolderPath"
        }
    }
}

Remove-Folder ./SteganographyApp/bin/$BuildType/netcoreapp8.0/publish
Remove-Folder ./SteganographyApp.Common/bin/$BuildType/netcoreapp8.0/publish
Remove-Folder ./SteganographyApp.Common.Arguments/bin/$BuildType/netcoreapp8.0/publish


Write-Host "`n---------- Publishing $BuildType build ----------`n"
dotnet publish -c $BuildType
if ($LastExitCode -ne 0) {
    Write-Host "publish failed with status: $LastExitCode"
    Exit
}