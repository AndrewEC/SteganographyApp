function Remove-Folder {
    param(
        [Parameter(Mandatory)]
        [string] $FolderPath
    )

    if (Test-Path $FolderPath -PathType Container) {
        Write-Host "Removing folder $FolderPath"
        Remove-Item $FolderPath -Recurse -Force | Out-Null
    }
}

function Remove-File {
    param(
        [Parameter(Mandatory)]
        [string] $ItemPath
    )

    if (Test-Path $ItemPath -PathType Leaf) {
        Write-Host "Removing item: $ItemPath"
        Remove-Item $ItemPath
    }
}

function Write-Divider {
    param(
        [Parameter(Mandatory)]
        [string] $Label
    )

    $UpperCaseLabel = $(Get-Culture).TextInfo.ToTitleCase($Label)

    Write-Host "`n---------- $UpperCaseLabel ----------`n"
}
