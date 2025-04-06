function Invoke-ListVulnerable {
    Write-Divider "Checking vulnerable dependencies"

    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "'dotnet restore' failed with status [$LASTEXITCODE]."
        exit
    }

    dotnet list package --vulnerable
    if ($LASTEXITCODE -ne 0) {
        Write-Host "'dotnet list package --vulnerable' failed with status [$LASTEXITCODE]."
        exit
    }
}