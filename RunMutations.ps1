. ./_Common.ps1
$ProgressPreference = "SilentlyContinue"

$Projects = @(
    "./SteganographyApp.Common.Tests",
    "./SteganographyApp.Common.Arguments.Tests"
)

Write-Divider "Executing mutation tests"
function Invoke-Stryker {
    param(
        [string] $Project
    )

    $Output = Join-Path -Path $Project -ChildPath "StrykerOutput"

    Remove-Folder $Output

    Write-Divider "Running $Project mutation tests"

    Set-Location $Project
    try {
        dotnet tool run dotnet-stryker --config-file stryker-config.json
        if ($LastExitCode -ne 0) {
            Write-Output("'stryker' failed with status: $LastExitCode")
            Exit
        }
        Write-Output "Report available at $Output"
    } finally {
        Set-Location ..
    }
}

$Projects | ForEach-Object { Invoke-Stryker $_ }
