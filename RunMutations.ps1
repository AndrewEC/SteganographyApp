. ./_Common.ps1
$ProgressPreference = "SilentlyContinue"

$CommonOutputFolder = "./SteganographyApp.Common.Tests/StrykerOutput"
$CommonProject = "./SteganographyApp.Common.Tests"
$ArgumentsOutputFolder = "./SteganographyApp.Common.Arguments.Tests/StrykerOutput"
$ArgumentsProject = "./SteganographyApp.Common.Arguments.Tests"

Write-Divider "Removing Previous Output Folders"
Remove-Folder $CommonOutputFolder
Remove-Folder $ArgumentsOutputFolder


Write-Divider "Executing mutation tests"
function Run-Mutations {
    param(
        [string] $Project,
        [string] $Output
    )
    Write-Divider "Running $Project mutation tests"
    Set-Location $Project
    dotnet tool run dotnet-stryker --config-file stryker-config.json
    if($LastExitCode -ne 0){
        Write-Output("'stryker' failed with status: $LastExitCode")
        Set-Location ..
        Exit
    }
    Write-Output "Report available at $Output"
    Set-Location ..
}

Run-Mutations $CommonProject $CommonOutputFolder
Run-Mutations $ArgumentsProject $ArgumentsOutputFolder
