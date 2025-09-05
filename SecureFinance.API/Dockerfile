FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SecureFinance.API/SecureFinance.API.csproj", "SecureFinance.API/"]
COPY ["SecureFinance.Core/SecureFinance.Core.csproj", "SecureFinance.Core/"]
COPY ["SecureFinance.Infrastructure/SecureFinance.Infrastructure.csproj", "SecureFinance.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SecureFinance.API/SecureFinance.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/SecureFinance.API"
RUN dotnet build "SecureFinance.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecureFinance.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "SecureFinance.API.dll"]