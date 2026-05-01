-- ecodata.sql
-- SQL script to create EcoData database and tables
-- Run this script in SQL Server (adjust file as needed)

IF DB_ID('EcoData') IS NULL
BEGIN
    CREATE DATABASE EcoData;
END
GO

USE EcoData;
GO

IF OBJECT_ID('dbo.Species', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Species (
        SpeciesId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NULL,
        ConservationStatus NVARCHAR(100) NULL
    );
END
GO

IF OBJECT_ID('dbo.Observations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Observations (
        ObservationId INT IDENTITY(1,1) PRIMARY KEY,
        SpeciesId INT NOT NULL,
        Location NVARCHAR(200) NOT NULL,
        Date DATE NOT NULL,
        Quantity INT NOT NULL,
        CONSTRAINT FK_Observations_Species FOREIGN KEY (SpeciesId) REFERENCES dbo.Species(SpeciesId)
    );
END
GO

IF OBJECT_ID('dbo.Environment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Environment (
        EnvId INT IDENTITY(1,1) PRIMARY KEY,
        Location NVARCHAR(200) NOT NULL,
        Date DATE NOT NULL,
        Temperature FLOAT NULL,
        Humidity FLOAT NULL,
        AirQualityIndex INT NULL
    );
END
GO

-- Optional: sample inserts can be added here or loaded from CSV
