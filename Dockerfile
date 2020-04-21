ARG RUNTIME=3.1-buster-slim

# Build stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
ARG PUBLISH_RID=linux-x64
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out -r ${PUBLISH_RID} --no-self-contained

# Final stage
FROM mcr.microsoft.com/dotnet/core/runtime:${RUNTIME}
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "PrometheusSolarExporter.dll"]
