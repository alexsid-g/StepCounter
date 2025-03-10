# Use official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project and restore dependencies
COPY TeamSteps.WebApi.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./
RUN dotnet publish "./TeamSteps.WebApi.csproj" -c Release -o out

# Use runtime-only image to keep the container small
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose API port
EXPOSE 5000
EXPOSE 5001

# Start the application
ENTRYPOINT ["dotnet", "TeamSteps.WebApi.dll"]
