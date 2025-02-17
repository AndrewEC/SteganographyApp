function Remove-Folder {
    param([string] $FolderPath)

    if (Test-Path $FolderPath -PathType Container) {
        Write-Output "Removing folder $FolderPath"
        Remove-Item -Recurse -Force $FolderPath | Out-Null

        if (Test-Path $FolderPath -PathType Container) {
            throw "Could not delete folder $FolderPath"
        }
    }
}

function Remove-File {
    param([string] $ItemPath)

    if (Test-Path $ItemPath -PathType Leaf) {
        Write-Output "Removing item: $ItemPath"
        Remove-Item $ItemPath
    }
}

function Write-Divider {
    param([string] $Label)

    $UpperCaseLabel = $(Get-Culture).TextInfo.ToTitleCase($Label)

    Write-Output "`n---------- $UpperCaseLabel ----------`n"
}