#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
ENV Urls=http://*:80
ENV Serilog__WriteTo__Async__Args__configure__0__Args__path=/var/log/IDBrowserServiceCore.log
RUN apt-get update \
    && apt-get install -y ffmpeg

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
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
ENTRYPOINT ["dotnet", "IDBrowserServiceCore.dll"]