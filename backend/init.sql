-- Database initialization script
-- This runs when PostgreSQL container starts for the first time

-- Create the database (already created by POSTGRES_DB env var, but just in case)
CREATE DATABASE IF NOT EXISTS movie_database;

-- Connect to the database
\c movie_database;

-- Enable UUID extension if you use GUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create tables (optional - your EF Core migrations will handle this)
-- This is just for reference - EF Core will create tables from migrations

-- Note: Your application will run migrations automatically
-- This file is mainly for any custom SQL you want to run on database creation