. ./Scripts/_Common.ps1

function Invoke-UnitTest {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectName,
        [Parameter(Mandatory)]
        [scriptblock]$RunCommand
    )

    Write-Divider "Running $ProjectName tests"

    $RunCommand.Invoke()

    $CoverletExitCode = $LASTEXITCODE

    $ReportDir = Join-Path ./reports/unit-tests $ProjectName

    if (-not (Test-Path $ReportDir -PathType Container)) {
        New-Item $ReportDir -ItemType Directory | Out-Null
    }

    dotnet tool run reportgenerator "-reports:coverage.opencover.xml" "-targetDir:$ReportDir"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "'reportgenerator' command failed with status: [$LASTEXITCODE]"
        exit
    }

    if ($CoverletExitCode -ne 0) {
        Write-Host "'coverlet' $ProjectName command failed with status: [$CoverletExitCode]"
        exit
    }

    Write-Host "Code coverage report available at [$ReportDir]."
}

function Invoke-TestScript {
    Write-Divider "Cleaning out existing build artifacts"

    Remove-Folder ./SteganographyApp.Common.Tests/bin
    Remove-Folder ./SteganographyApp.Common.Tests/obj
    Remove-Folder ./SteganographyApp.Common.Arguments.Tests/bin
    Remove-Folder ./SteganographyApp.Common.Arguments.Tests/obj
    Remove-Folder ./SteganographyApp.Common.Integration.Tests/bin
    Remove-Folder ./SteganographyApp.Common.Integration.Tests/obj
    Remove-Folder ./reports/unit-tests

    Remove-File ./coverage.json
    Remove-File ./coverage.opencover.xml


    Write-Divider "Rebuilding Project"

    dotnet build SteganographyApp.sln --no-incremental
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed with status code: [$LASTEXITCODE]"
        exit
    }


    Write-Divider "Running unit tests"

    Invoke-UnitTest "SteganographyApp.Common.Tests" {
        dotnet tool run coverlet `
            ./SteganographyApp.Common.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Tests.dll `
            --target "dotnet" `
            --targetargs "test ./SteganographyApp.Common.Tests --no-build" `
            --exclude-by-file "**/RootLogger.cs" `
            --exclude-by-file "**/Logger.cs" `
            --exclude-by-file "**/LazyLogger.cs" `
            --exclude-by-file "**/LoggerFactory.cs" `
            --exclude-by-file "**/BasicImageInfo.cs" `
            --exclude-by-file "**/ConsoleProxy.cs" `
            --exclude-by-file "**/FileProxy.cs" `
            --exclude-by-file "**/ImageProxy.cs" `
            --exclude-by-file "**/ReadWriteStream.cs" `
            --exclude-by-file "**/ServiceContainer.cs" `
            --threshold 75 `
            --threshold-type line `
            --threshold-type branch `
            --threshold-stat total `
            --format opencover
    }

    Invoke-UnitTest "SteganographyApp.Common.Arguments.Tests" {
        dotnet tool run coverlet `
            ./SteganographyApp.Common.Arguments.Tests/bin/Debug/netcoreapp8.0/SteganographyApp.Common.Arguments.Tests.dll `
            --target "dotnet" `
            --targetargs "test ./SteganographyApp.Common.Arguments.Tests --no-build" `
            --exclude-by-file "**/Help.cs" `
            --exclude-by-file "**/ArgumentsServiceContainer.cs" `
            --threshold 80 `
            --threshold-type line `
            --threshold-type branch `
            --threshold-stat total `
            --format opencover
    }


    Write-Divider "Running SteganographyApp.Common.Integration.Tests tests"
    dotnet test ./SteganographyApp.Common.Integration.Tests
    if ($LASTEXITCODE -ne 0) {
        Write-Host "dotnet test ./SteganographyApp.Common.Integration.Tests command failed with status: [$LASTEXITCODE]"
        exit
    }
}