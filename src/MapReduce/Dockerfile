﻿FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MapReduce/MapReduce.csproj", "MapReduce/"]
RUN dotnet restore "MapReduce/MapReduce.csproj"
COPY . .
WORKDIR "/src/MapReduce"
RUN dotnet build "MapReduce.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapReduce.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapReduce.dll"]
