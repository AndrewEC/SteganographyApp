$CommonOutputFolder = "./SteganographyApp.Common.Tests/StrykerOutput"
$CommonProject = "./SteganographyApp.Common.Tests"
$ArgumentsOutputFolder = "./SteganographyApp.Common.Arguments.Tests/StrykerOutput"
$ArgumentsProject = "./SteganographyApp.Common.Arguments.Tests"

Write-Host "`n---------- Removing Previous Output Folders ----------`n"
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

Remove-Folder $CommonOutputFolder
Remove-Folder $ArgumentsOutputFolder


Write-Host("`n---------- Executing mutation tests ----------`n")
cd ./SteganographyApp.Common.Tests
dotnet tool run dotnet-stryker --config-file stryker-config.json
if($LastExitCode -ne 0){
    Write-Host("'stryker' failed with status: $LastExitCode")
    cd ..
    Exit
}
Write-Host "Report available at $CommonOutputFolder"