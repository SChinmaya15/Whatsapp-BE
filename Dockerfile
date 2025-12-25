# -------------------------
# Runtime
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# -------------------------
# Build
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /backend

# Copy project file
COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

COPY . .
RUN dotnet publish backend/backend.csproj -c Release -o /app/publish

# -------------------------
# Final
# -------------------------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "backend.dll"]
