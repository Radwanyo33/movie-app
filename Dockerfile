# Build React Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy frontend files
COPY frontend/package*.json ./
COPY frontend/vite.config.js ./
COPY frontend/index.html ./
COPY frontend/src/ ./src/

# Install dependencies and build
RUN npm ci
RUN npm run build

# Build .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy backend project file (handle space in filename)
COPY ["backend/Live Movies.csproj", "."]
RUN dotnet restore "Live Movies.csproj"

# Copy all backend files
COPY backend/ ./

# Build .NET
RUN dotnet build "Live Movies.csproj" -c Release -o /app/build
RUN dotnet publish "Live Movies.csproj" -c Release -o /app/publish

# Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/publish .
# Copy frontend build
COPY --from=frontend-build /app/dist ./wwwroot

# Create uploads directory
RUN mkdir -p /app/uploads/movies

# Install PostgreSQL client
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Set environment variables
ENV ASPNETCORE_URLS=http://*:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Live Movies.dll"]