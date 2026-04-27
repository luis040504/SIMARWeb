FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/API_WasteCatalog.csproj ./
RUN dotnet restore
COPY src/ ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8010
ENTRYPOINT ["dotnet", "API_WasteCatalog.dll"]
