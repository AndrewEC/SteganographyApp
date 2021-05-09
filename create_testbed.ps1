Write-Host("`n---------- Removing testbed directories ----------`n")

if(Test-Path ./testbed){
    Write-Host("Removing testbed output folder")
    Remove-Item -Recurse -Force ./testbed | Out-Null
}

if(Test-Path ./SteganographyApp/bin/release){
    Write-Host("Removing SteganographyApp output folder")
    Remove-Item -Recurse -Force ./SteganographyApp/bin/release | Out-Null
}

if(Test-Path ./SteganographyApp.Calculator/bin/release){
    Write-Host("Removing SteganographyApp.Calculator output folder")
    Remove-Item -Recurse -Force ./SteganographyApp.Calculator/bin/release | Out-Null
}

if(Test-Path ./SteganographyApp.Converter/bin/release){
    Write-Host("Removing SteganographyApp.Converter output folder")
    Remove-Item -Recurse -Force ./SteganographyApp.Converter/bin/release | Out-Null
}

Write-Host("`n---------- Publishing release build ----------`n")
dotnet publish -c release
if($LastExitCode -ne 0){
    Write-Host("publish failed with status: $LastExitCode")
    Exit
}

New-Item -Path ./testbed -ItemType Directory | Out-Null

Write-Host("`n---------- Copying publish output ----------`n")
Write-Host("Copying output from SteganographyApp publish")
Get-ChildItem -Path ./SteganographyApp/bin/release/netcoreapp5.0/publish | Copy-Item -Recurse -Destination ./testbed

Write-Host("Copying output from SteganographyApp.Calculator publish")
Copy-Item ./SteganographyApp.Calculator/bin/release/netcoreapp5.0/publish -Recurse -Destination ./testbed
cd testbed
Rename-Item -Path publish -NewName Calculator
cd ..

Write-Host("Copying output from SteganographyApp.Converter publish")
Copy-Item ./SteganographyApp.Converter/bin/release/netcoreapp5.0/publish -Recurse -Destination ./testbed
cd testbed
Rename-Item -Path publish -NewName Converter -Force
cd ..

Write-Host("`n---------- Copying test assets ----------`n")
Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination ./testbed
Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination ./testbed/Calculator
Get-ChildItem -Path ./SteganographyApp.Common.Tests/TestAssets | Where-Object Name -Like "*.png" | Copy-Item -Force -Destination ./testbed/Converter