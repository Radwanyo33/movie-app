# Build React Frontend
FROM node:20 AS frontend-build
WORKDIR /app

COPY frontend/package*.json ./
COPY frontend/vite.config.js ./
COPY frontend/index.html ./
COPY frontend/src/ ./src/

RUN npm install
RUN npm run build

# Build .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

COPY ["backend/Live Movies.csproj", "."]
RUN dotnet restore "Live Movies.csproj"
COPY backend/ ./
RUN dotnet build "Live Movies.csproj" -c Release -o /app/build
RUN dotnet publish "Live Movies.csproj" -c Release -o /app/publish

# Final Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/dist ./wwwroot

RUN mkdir -p /app/uploads/movies
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Only set if not already set by Railway
ENV ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://*:8080}
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}

EXPOSE 8080

ENTRYPOINT ["dotnet", "Live Movies.dll"]