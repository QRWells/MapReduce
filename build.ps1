# check if dotnet sdk is installed
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet sdk is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
}

# publish the project
dotnet publish src/MapReduce -c Release -o output