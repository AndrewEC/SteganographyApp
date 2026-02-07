. ./Scripts/_Common.ps1

function Invoke-Install {
    Write-Divider "Installing tools"
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install tools. `"dotnet tool restore`" command failed with status [$LASTEXITCODE]."
        exit
    }

    Write-Divider "Restoring solution"
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to restore solution. `"dotnet restore`" command failed with status [$LASTEXITCODE]."
        exit
    }
}