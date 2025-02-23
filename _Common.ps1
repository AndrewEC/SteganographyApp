function Remove-Folder {
    param([string] $FolderPath)

    if (Test-Path $FolderPath -PathType Container) {
        Write-Output "Removing folder $FolderPath"
        Remove-Item $FolderPath -Recurse -Force | Out-Null
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