$CommonOutputFolder = "./SteganographyApp.Common.Tests/StrykerOutput"
$CommonProject = "./SteganographyApp.Common.Tests"
$ArgumentsOutputFolder = "./SteganographyApp.Common.Arguments.Tests/StrykerOutput"
$ArgumentsProject = "./SteganographyApp.Common.Arguments.Tests"

Write-Host "`n---------- Removing Previous Output Folders ----------`n"
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

Remove-Folder $CommonOutputFolder
Remove-Folder $ArgumentsOutputFolder


Write-Host "`n---------- Executing mutation tests ----------`n"
function Run-Mutations {
    param(
        [string] $Project,
        [string] $Output
    )
    Write-Host "---------- Running $Project mutation tests ----------"
    cd $Project
    dotnet tool run dotnet-stryker --config-file stryker-config.json
    if($LastExitCode -ne 0){
        Write-Host("'stryker' failed with status: $LastExitCode")
        cd ..
        Exit
    }
    Write-Host "Report available at $Output"
    cd ..
}

Run-Mutations $CommonProject $CommonOutputFolder
Run-Mutations $ArgumentsProject $ArgumentsOutputFolder