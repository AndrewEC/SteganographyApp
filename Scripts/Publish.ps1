. ./Scripts/_Common.ps1

function Invoke-PublishScript {
    $BuildType = "release"

    Write-Divider "Removing old publish directories"

    Remove-Folder ./SteganographyApp/bin/$BuildType/netcoreapp8.0/publish
    Remove-Folder ./SteganographyApp.Common/bin/$BuildType/netcoreapp8.0/publish
    Remove-Folder ./SteganographyApp.Common.Arguments/bin/$BuildType/netcoreapp8.0/publish

    Write-Divider "Publishing $BuildType build"
    dotnet publish -c $BuildType
    if ($LASTEXITCODE -ne 0) {
        Write-Host "publish failed with status: $LASTEXITCODE"
        Exit
    }
}