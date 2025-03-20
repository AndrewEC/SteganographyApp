. ./_Common.ps1

Write-Divider "Packaging Libraries"

function Copy-NugetPackages {
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [string]$ProjectName
    )

    process {
        $PackageName = "$ProjectName.4.0.0.nupkg"
        $SourcePackagePath = Join-Path -Path $ProjectName `
            -ChildPath bin/release `
            -AdditionalChildPath $PackageName

        Write-Host "Copying package from [$SourcePackagePath]."

        if (-not (Test-Path $SourcePackagePath -PathType Leaf)) {
            Write-Host "Could not find package to copy."
            exit
        }

        Copy-Item $SourcePackagePath ./packages
    }
}

dotnet pack

Write-Divider "Copying Packages"

Remove-Folder ./packages
New-Item packages -ItemType Directory | Out-Null

@(
    "SteganographyApp.Common",
    "SteganographyApp.Common.Arguments"
) | Copy-NugetPackages

Write-Host ""
