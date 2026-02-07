param(
    [Parameter(Mandatory)]
    [ValidateSet(
        "AllTests",
        "Benchmarks",
        "Install",
        "Mutations",
        "Package",
        "Publish",
        "Testbed",
        "Tests",
        "Vulnerable"
    )]
    [string]$ScriptAction
)

Set-StrictMode -Version Latest

. ./Scripts/Invoke-Benchmarks.ps1
. ./Scripts/Invoke-Install.ps1
. ./Scripts/Invoke-Mutations.ps1
. ./Scripts/Invoke-Package.ps1
. ./Scripts/Invoke-Publish.ps1
. ./Scripts/Invoke-Testbed.ps1
. ./Scripts/Invoke-Tests.ps1
. ./Scripts/Invoke-Vulnerable.ps1

$ProgressPreference = "SilentlyContinue"
$global:ProgressPreference = "SilentlyContinue"
$ErrorActionPreference = "Stop"
$global:ErrorActionPreference = "Stop"

switch ($ScriptAction) {
    "AllTests" {
        Invoke-Install
        Invoke-TestScript
        Invoke-MutationsScript
        Invoke-ListVulnerable
    }
    "Benchmarks" { Invoke-BenchmarksScript }
    "Install" { Invoke-Install }
    "Mutations" { Invoke-MutationsScript }
    "Package" { Invoke-PackageScript }
    "Publish" { Invoke-PublishScript }
    "Testbed" { Invoke-CreateTestbedScript }
    "Tests" { Invoke-TestScript }
    "Vulnerable" { Invoke-ListVulnerable }
}
