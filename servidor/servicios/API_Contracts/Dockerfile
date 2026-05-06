FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8006

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ContractsService/ContractsService.csproj", "ContractsService/"]
RUN dotnet restore "ContractsService/ContractsService.csproj"

COPY . .
WORKDIR "/src/ContractsService"
RUN dotnet build "ContractsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContractsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContractsService.dll"]