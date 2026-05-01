# =============================================
# Multi-stage Dockerfile para .NET 8
# =============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj primero para cache de dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar el resto y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Seguridad: correr como usuario no-root
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 5071

ENV ASPNETCORE_URLS=http://+:5071
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Meta-xi.dll"]