Param(
    [Switch]$Debug
)

if ($Debug) {
    dotnet pack -c Debug
    Exit
}
dotnet pack