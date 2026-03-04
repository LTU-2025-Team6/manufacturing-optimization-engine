# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ManufacturingOptimization.sln", "./"]
COPY ["ManufacturingOptimization.ProviderSimulator/ManufacturingOptimization.ProviderSimulator.csproj", "ManufacturingOptimization.ProviderSimulator/"]
COPY ["ManufacturingOptimization.Common/ManufacturingOptimization.Common.csproj", "ManufacturingOptimization.Common/"]

# Restore dependencies
RUN dotnet restore "ManufacturingOptimization.ProviderSimulator/ManufacturingOptimization.ProviderSimulator.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/ManufacturingOptimization.ProviderSimulator"
RUN dotnet build "ManufacturingOptimization.ProviderSimulator.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingOptimization.ProviderSimulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ManufacturingOptimization.ProviderSimulator.dll"]
