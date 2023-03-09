# check if dotnet sdk is installed
if ! command -v dotnet &> /dev/null
then
    echo "dotnet sdk could not be found"
    exit
fi

# publish the project
dotnet publish src/MapReduce -c Release -o output