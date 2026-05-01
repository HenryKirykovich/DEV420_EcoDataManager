-- ============================================================
-- EcoData Manager - Database Setup Script
-- Run this in SQL Server Management Studio or sqlcmd
-- against your (localdb)\MSSQLLocalDB instance
-- ============================================================

IF DB_ID('EcoManagementDB') IS NULL
    CREATE DATABASE EcoManagementDB;
GO

USE EcoManagementDB;
GO

-- Species
IF OBJECT_ID('dbo.Observations', 'U') IS NOT NULL DROP TABLE dbo.Observations;
IF OBJECT_ID('dbo.Species', 'U') IS NOT NULL DROP TABLE dbo.Species;
IF OBJECT_ID('dbo.Environment', 'U') IS NOT NULL DROP TABLE dbo.Environment;
GO

CREATE TABLE dbo.Species (
    SpeciesId   INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NULL,
    Category    NVARCHAR(50)  NULL,
    ConservationStatus NVARCHAR(50) NULL
);
GO

CREATE TABLE dbo.Observations (
    ObservationId INT IDENTITY(1,1) PRIMARY KEY,
    SpeciesId     INT  NOT NULL,
    Location      NVARCHAR(100) NULL,
    Date          DATE NULL,
    Quantity      INT  NULL,
    CONSTRAINT FK_Observations_Species FOREIGN KEY (SpeciesId) REFERENCES dbo.Species(SpeciesId)
);
GO

CREATE TABLE dbo.Environment (
    Location        NVARCHAR(100) NOT NULL,
    Date            DATE NOT NULL,
    Temperature     DECIMAL(10,2) NULL,
    Humidity        DECIMAL(10,2) NULL,
    AirQualityIndex INT NULL,
    CONSTRAINT PK_Environment PRIMARY KEY (Location, Date)
);
GO
