. ./_Common.ps1

Param(
    [Switch] $Release
)

$BuildType = "debug"
if ($Release) {
    $BuildType = "release"
}

Write-Divider "Removing old publish directories"
Remove-Folder ./SteganographyApp/bin/$BuildType/netcoreapp8.0/publish
Remove-Folder ./SteganographyApp.Common/bin/$BuildType/netcoreapp8.0/publish
Remove-Folder ./SteganographyApp.Common.Arguments/bin/$BuildType/netcoreapp8.0/publish


Write-Divider "Publishing $BuildType build"
dotnet publish -c $BuildType
if ($LastExitCode -ne 0) {
    Write-Output "publish failed with status: $LastExitCode"
    Exit
}