# Build React Frontend
FROM node:20 AS frontend-build
WORKDIR /app

# Copy package files first
COPY frontend/package*.json ./
COPY frontend/vite.config.js ./

# Clean install to fix npm bug
RUN rm -rf node_modules package-lock.json && \
    npm cache clean --force && \
    npm install --legacy-peer-deps --no-optional

# Copy source files
COPY frontend/index.html ./
COPY frontend/src/ ./src/

# Build
RUN npm run build

# Build .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy backend project file
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