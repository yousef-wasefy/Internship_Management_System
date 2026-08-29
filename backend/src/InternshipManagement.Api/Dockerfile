# Multi-stage build: the SDK image (large, has the compiler) only exists to produce a
# published output; the final image is the much smaller ASP.NET *runtime* image, which
# has no compiler at all - it just runs the already-built DLL. Build context is this
# project's own folder (see docker-compose.yml), so nothing from the sibling Tests
# project or the rest of the repo ever gets copied in.

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restoring before copying the rest of the source means Docker can reuse this layer on
# a rebuild as long as the .csproj itself hasn't changed, even if source files have.
COPY InternshipManagement.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Explicit rather than relying on the base image's own default, so it's obvious from
# reading this file alone which port the container listens on.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InternshipManagement.Api.dll"]
