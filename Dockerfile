# syntax=docker/dockerfile:1.3
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim as builder

# suppress data collection
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# caches restore result by copying csproj file separately
WORKDIR /source/app
COPY src/api.csproj .

# restore packages
RUN dotnet restore

# Copies the rest of the code
COPY src/. .

# build and publish
RUN dotnet publish \
    -c release \
    -o published \
    api.csproj

# runner
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim

# install curl
RUN apt update && apt install curl -y

# Create a non-root user
RUN addgroup --system app \
    && adduser --system --ingroup app app

WORKDIR /home/app/
COPY --from=builder /source/app/published .
RUN chown app:app -R /home/app

USER app

ENV PORT=8080
ENV ASPNETCORE_URLS="http://0.0.0.0:${PORT};http://0.0.0.0:9001"

EXPOSE $PORT

ENTRYPOINT [ "dotnet", "api.dll" ]
