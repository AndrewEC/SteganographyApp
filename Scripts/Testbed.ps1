. ./Scripts/_Common.ps1

function Invoke-CreateTestbedScript {
    Write-Divider "Removing testbed directories"
    Remove-Folder ./testbed
    Remove-Folder ./SteganographyApp/bin/release


    Write-Divider "Publishing release build"
    dotnet publish -c release
    if ($LastExitCode -ne 0) {
        Write-Host "publish failed with status: [$LastExitCode]"
        Exit
    }

    New-Item ./testbed -ItemType Directory | Out-Null


    Write-Divider "Copying publish output"
    Write-Host "Copying output from SteganographyApp publish"
    Get-ChildItem ./SteganographyApp/bin/release/netcoreapp8.0/publish `
        | Copy-Item -Recurse -Destination ./testbed


    Write-Divider "Copying test assets"
    Get-ChildItem ./SteganographyApp.Common.Tests/TestAssets `
        | Where-Object Name -like "*.png" `
        | Copy-Item -Force -Destination ./testbed

    $AbsoluteTestbedPath = Join-Path $PSScriptRoot testbed
    Write-Host "Testbed directory has been created at [$AbsoluteTestbedPath]."
}
