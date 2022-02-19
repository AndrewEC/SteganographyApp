Param(
    [Switch]$Release
)

$Folder = "debug"
$ChildFolder = ""
if ($Release) {
    $Folder = "release"
    $ChildFolder = "obfuscated"
}


Write-Host "`n---------- Removing publish directories ----------`n"
function Remove-Folder {
    [CmdletBinding()]
	param(
		[Parameter()]
		[string] $FolderPath
	)

    if (Test-Path $FolderPath) {
        Write-Host "Removing folder $FolderPath"
        Remove-Item -Recurse -Force $FolderPath | Out-Null

        if (Test-Path $FolderPath) {
            throw "The folder at path $FolderPath could not be deleted."
        }
    }
}

Remove-Folder ./publish
Remove-Folder ./SteganographyApp/bin/$Folder
Remove-Folder ./SteganographyApp.Calculator/bin/$Folder
Remove-Folder ./SteganographyApp.Converter/bin/$Folder


Write-Host "`n---------- Publishing $Folder build ----------`n"
dotnet publish -c $Folder
if($LastExitCode -ne 0){
    Write-Host "publish failed with status: $LastExitCode"
    Exit
}


Write-Host "`n---------- Copying publish output ----------`n"
function Copy-Folder {
    [CmdletBinding()]
    param(
        [Parameter()][string] $Project,
        [Parameter()][string] $Output,
        [Parameter()][string] $Rename
    )

    Write-Host "Copying output from $Project publish"
    Copy-Item ./$Project/bin/$Folder/netcoreapp5.0/publish -Recurse -Destination $Output
    Get-ChildItem -Path ./$Project/bin/$Folder/netcoreapp5.0/$ChildFolder | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish

    if (-Not($Rename -eq "")) {
        cd publish
        Rename-Item -Path publish -NewName $Rename
        cd ..
    }
}

Copy-Folder "SteganographyApp" "."
Copy-Folder "SteganographyApp.Calculator" "./publish" "Calculator"
Copy-Folder "SteganographyApp.Converter" "./publish" "Converter"