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

. ./Scripts/Benchmarks.ps1
. ./Scripts/Install.ps1
. ./Scripts/Mutations.ps1
. ./Scripts/Package.ps1
. ./Scripts/Publish.ps1
. ./Scripts/Testbed.ps1
. ./Scripts/Tests.ps1
. ./Scripts/Vulnerable.ps1

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
