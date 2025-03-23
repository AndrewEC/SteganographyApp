. ./Scripts/_Common.ps1

function Copy-MutationReport {
    param(
        [Parameter(Mandatory)]
        [string]$Project
    )

    $ReportRoot = Join-Path $Project StrykerOutput

    Write-Host "Copying Stryker reports from [$ReportRoot]."

    if (-not (Test-Path $ReportRoot -PathType Container)) {
        Write-Host "Could not find Stryker reports directory."
        return
    }

    # Folder structure is:
    # StrykerOutput/<timestamped_folder/reports/mutation-report.html
    $StrykerReport = $ReportRoot | Get-ChildItem | Select-Object -First 1
    if ($null -eq $StrykerReport) {
        Write-Host "No reports found in Styker output directory."
        return
    }

    $StrykerHtmlReport = $StrykerReport | Get-ChildItem `
        | Select-Object -First 1 `
        | Get-ChildItem `
        | Select-Object -First 1

    $OutputPath = Join-Path reports/mutation-tests $Project
    New-Item $OutputPath -ItemType Directory | Out-Null
    Copy-Item $StrykerHtmlReport $OutputPath

    Write-Host "---"
    Write-Host "Report is now available at [$OutputPath]."
    Write-Host "---"
}

function Invoke-Stryker {
    param(
        [Parameter(Mandatory)]
        [string] $Project
    )

    $Output = Join-Path -Path $Project -ChildPath "StrykerOutput"

    Write-Divider "Running $Project mutation tests"

    Remove-Folder $Output

    Set-Location $Project
    try {
        dotnet tool run dotnet-stryker --config-file stryker-config.json
        if ($LastExitCode -ne 0) {
            Write-Host("'stryker' failed with status: $LastExitCode")
            exit
        }
        Write-Host "Report available at $Output"
    } finally {
        Set-Location ..
    }

    Copy-MutationReport $Project
}

function Invoke-MutationsScript {
    $Projects = @(
        "SteganographyApp.Common.Tests",
        "SteganographyApp.Common.Arguments.Tests"
    )

    Write-Divider "Executing mutation tests"

    Remove-Folder ./reports/mutation-tests

    $Projects | ForEach-Object { Invoke-Stryker $_ }
}
