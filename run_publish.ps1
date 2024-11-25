Param(
    [Switch]$Release
)

$Folder = "debug"
if ($Release) {
    $Folder = "release"
}


Write-Host "`n---------- Removing publish directories ----------`n"
function Remove-Folder {
	param([string] $FolderPath)

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


Write-Host "`n---------- Publishing $Folder build ----------`n"
dotnet publish -c $Folder
if($LastExitCode -ne 0){
    Write-Host "publish failed with status: $LastExitCode"
    Exit
}


Write-Host "`n---------- Copying publish output ----------`n"
function Copy-Folder {
    param(
        [string] $Project,
        [string] $Output,
        [string] $Rename
    )

    Write-Host "Copying output from $Project publish"
    Copy-Item ./$Project/bin/$Folder/net8.0/publish -Recurse -Destination $Output
    Get-ChildItem -Path ./$Project/bin/$Folder/net8.0/ | Where-Object Name -Like "*.dll" | Copy-Item -Force -Destination ./publish

    if (-Not($Rename -eq "")) {
        cd publish
        Rename-Item -Path publish -NewName $Rename
        cd ..
    }
}

Copy-Folder "SteganographyApp" "."