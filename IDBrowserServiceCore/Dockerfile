#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
ENV Serilog__WriteTo__Async__Args__configure__0__Args__path=/var/log/IDBrowserServiceCore.log
RUN apt-get update \
    && apt-get install -y ffmpeg

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-amd64 AS build
WORKDIR /src
COPY ["IDBrowserServiceCore/IDBrowserServiceCore.csproj", "IDBrowserServiceCore/"]
RUN dotnet restore "IDBrowserServiceCore/IDBrowserServiceCore.csproj"
COPY . .
WORKDIR "/src/IDBrowserServiceCore"
RUN dotnet build "IDBrowserServiceCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IDBrowserServiceCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish/appsettings.json .
COPY --from=publish /app/publish/IDBrowserServiceCore.deps.json .
COPY --from=publish /app/publish/IDBrowserServiceCore.runtimeconfig.json .
COPY --from=publish /app/publish/*.dll ./
COPY --from=publish /app/publish/web.config .
COPY --from=publish /app/publish/runtimes/ runtimes/

RUN apt-get update && apt-get install -y curl

HEALTHCHECK --interval=1m --timeout=3s CMD curl --fail http://localhost:80 || exit 1 

ENTRYPOINT ["dotnet", "IDBrowserServiceCore.dll"]