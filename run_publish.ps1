Write-Host("`n---------- Removing publish directories ----------`n")

if(Test-Path ./publish){
    Write-Host("Removing publish_release output folder")
    Remove-Item -Recurse -Force ./publish | Out-Null
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

Write-Host("`n---------- Copying build output to publish directory ----------`n")
Write-Host("Copying output from SteganographyApp publish")
Copy-Item ./SteganographyApp/bin/release/netcoreapp5.0/publish -Recurse -Destination .
Get-ChildItem -Path ./SteganographyApp/bin/release/netcoreapp5.0/obfuscated | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish

Write-Host("Copying output from SteganographyApp.Calculator publish")
Copy-Item ./SteganographyApp.Calculator/bin/release/netcoreapp5.0/publish -Recurse -Destination ./publish
Get-ChildItem -Path ./SteganographyApp.Calculator/bin/release/netcoreapp5.0/obfuscated | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish/publish
cd publish
Rename-Item -Path publish -NewName Calculator
cd ..

Write-Host("Copying output from SteganographyApp.Converter publish")
Copy-Item ./SteganographyApp.Converter/bin/release/netcoreapp5.0/publish -Recurse -Destination ./publish
Get-ChildItem -Path ./SteganographyApp.Converter/bin/release/netcoreapp5.0/obfuscated | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish/publish
cd publish
Rename-Item -Path publish -NewName Converter -Force
cd ..