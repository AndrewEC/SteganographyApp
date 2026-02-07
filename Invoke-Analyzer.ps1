if (-not (Get-Command "Invoke-ScriptAnalyzer" -ErrorAction SilentlyContinue)) {
    Write-Host "Invoke-ScriptAnalyzer command could not be found."
    Write-Host "Attempting to install PSScriptAnalyzer module."
    Write-Host "Running Install-Module -Name PSScriptAnalyzer -Force"
    Install-Module -Name PSScriptAnalyzer -Force
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install PSScriptAnalyzer. Install exited with status [$LASTEXITCODE]."
        exit
    }
} else {
    Write-Host "PSScriptAnalyzer module found."
}

Write-Host "Running PSScriptAnalyzer..."

pwsh -c Invoke-ScriptAnalyzer `
    -EnableExit `
    -Path C:\Stuff\Programming\NetFramework\SteganographyApp `
    -Recurse

if ($LASTEXITCODE -eq 0) {
    Write-Host "PSScriptAnalyzer finished without reporting any errors."
}
