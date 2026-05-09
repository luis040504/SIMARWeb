FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY src/*.csproj ./
RUN dotnet restore

COPY src/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8009
EXPOSE 8009

ENTRYPOINT ["dotnet", "API_Audit.dll"]
