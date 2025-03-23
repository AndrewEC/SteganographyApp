. ./Scripts/_Common.ps1

function Invoke-BenchmarksScript {
    Write-Divider "Running Benchmarks"
    dotnet run --project ./SteganographyApp.Common.Benchmarks -c release
}