FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Live Movies.csproj", "."]
RUN dotnet restore "Live Movies.csproj"
COPY . .
RUN dotnet build "Live Movies.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Live Movies.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY uploads/ ./uploads/
RUN mkdir -p /app/uploads/movies

# Install PostgreSQL client for health checks
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Live Movies.dll"]