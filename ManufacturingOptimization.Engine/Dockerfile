# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ManufacturingOptimization.sln", "./"]
COPY ["ManufacturingOptimization.Engine/ManufacturingOptimization.Engine.csproj", "ManufacturingOptimization.Engine/"]
COPY ["ManufacturingOptimization.Common/ManufacturingOptimization.Common.csproj", "ManufacturingOptimization.Common/"]

# Restore dependencies
RUN dotnet restore "ManufacturingOptimization.Engine/ManufacturingOptimization.Engine.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/ManufacturingOptimization.Engine"
RUN dotnet build "ManufacturingOptimization.Engine.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingOptimization.Engine.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Create Data directory and set as volume for database persistence
RUN mkdir -p /app/Data
VOLUME ["/app/Data"]

ENTRYPOINT ["dotnet", "ManufacturingOptimization.Engine.dll"]
