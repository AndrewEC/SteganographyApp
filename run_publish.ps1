Param(
    [Switch]$release
)

$folder = "debug"
$childFolder = ""
if ($release) {
    $folder = "release"
    $childFolder = "obfuscated"
}

Write-Host("`n---------- Removing publish directories ----------`n")

if(Test-Path ./publish){
    Write-Host("Removing publish output folder")
    Remove-Item -Recurse -Force ./publish | Out-Null
}

if(Test-Path ./SteganographyApp/bin/$folder){
    Write-Host("Removing SteganographyApp output folder")
    Remove-Item -Recurse -Force ./SteganographyApp/bin/$folder | Out-Null
}

if(Test-Path ./SteganographyApp.Calculator/bin/$folder){
    Write-Host("Removing SteganographyApp.Calculator output folder")
    Remove-Item -Recurse -Force ./SteganographyApp.Calculator/bin/$folder | Out-Null
}

if(Test-Path ./SteganographyApp.Converter/bin/$folder){
    Write-Host("Removing SteganographyApp.Converter output folder")
    Remove-Item -Recurse -Force ./SteganographyApp.Converter/bin/$folder | Out-Null
}

Write-Host("`n---------- Publishing $folder build ----------`n")
dotnet publish -c $folder
if($LastExitCode -ne 0){
    Write-Host("publish failed with status: $LastExitCode")
    Exit
}

Write-Host("`n---------- Copying publish output ----------`n")
Write-Host("Copying output from SteganographyApp publish")
Copy-Item ./SteganographyApp/bin/$folder/netcoreapp5.0/publish -Recurse -Destination .
Get-ChildItem -Path ./SteganographyApp/bin/$folder/netcoreapp5.0/$childFolder | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish

Write-Host("Copying output from SteganographyApp.Calculator publish")
Copy-Item ./SteganographyApp.Calculator/bin/$folder/netcoreapp5.0/publish -Recurse -Destination ./publish
Get-ChildItem -Path ./SteganographyApp.Calculator/bin/$folder/netcoreapp5.0/$childFolder | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish/publish
cd publish
Rename-Item -Path publish -NewName Calculator
cd ..

Write-Host("Copying output from SteganographyApp.Converter publish")
Copy-Item ./SteganographyApp.Converter/bin/$folder/netcoreapp5.0/publish -Recurse -Destination ./publish
Get-ChildItem -Path ./SteganographyApp.Converter/bin/$folder/netcoreapp5.0/$childFolder | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish/publish
cd publish
Rename-Item -Path publish -NewName Converter -Force
cd ..