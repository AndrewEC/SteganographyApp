Param(
    [Switch]$Release
)

if ($Release) {
    dotnet pack -c release
    Exit
}
dotnet pack