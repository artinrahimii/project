# Build from repository root:
# docker build -f src/Rendezvous.Service/Dockerfile -t whitep-rendezvous .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Rendezvous.Service/WhiteP.Rendezvous.Service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-self-contained \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build --chown=10001:10001 /app/publish .
USER 10001:10001
ENTRYPOINT ["dotnet", "WhiteP.Rendezvous.Service.dll"]
