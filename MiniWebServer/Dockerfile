FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app
RUN apt-get update && apt-get upgrade -y
# RUN apt-get install -y --force-yes nginx

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY [".", "."]
RUN dotnet restore "MiniWebServer/MiniWebServer.csproj"
COPY . .
WORKDIR "/src/MiniWebServer"
RUN dotnet build "MiniWebServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniWebServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["MiniWebServer/Docker/mini-web-server-docker.json", "./mini-web-server.json"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniWebServer.dll", "-d"]