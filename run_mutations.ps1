if(Test-Path ./SteganographyApp.Common.Tests/StrykerOutput){
    Write-Host("Removing old output folder and contents")
    Remove-Item -Recurse -Force ./SteganographyApp.Common.Tests/StrykerOutput | Out-Null
}

cd ./SteganographyApp.Common.Tests

Write-Host("Executing mutation tests")
dotnet stryker --reporters "['html']"
if($LastExitCode -ne 0){
    Write-Host("'stryker' failed with status: $LastExitCode")
    cd ..
    Exit
}

Write-Host("Opening report")
cd ..
Get-ChildItem -Directory ./SteganographyApp.Common.Tests/StrykerOutput | % {
    $path = $_.FullName
    Start-Process "$path\reports\mutation-report.html"
}