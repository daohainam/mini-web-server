#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get upgrade -y
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY [".", "."]
RUN dotnet restore "./MiniWebServer/./MiniWebServer.csproj"
COPY . .
WORKDIR "/src/MiniWebServer"
RUN dotnet build "./MiniWebServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiniWebServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["MiniWebServer/Docker/mini-web-server-docker.json", "./mini-web-server.json"]
COPY ["MiniWebServer/Docker/log4net.xml", "./log4net.xml"]
COPY ["MiniWebServer/Docker/log4net-demoapp.xml", "./log4net-demoapp.xml"]
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "MiniWebServer.dll"]