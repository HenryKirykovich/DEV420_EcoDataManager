# EcoData Manager

Windows Forms application built with **C# (.NET Framework 4.8)** for managing ecological observation data. Connects to a local SQL Server database, supports CSV/XML/JSON import & export, date-range filtering, bulk insert, and basic analytics.

---

## Requirements

- Windows 10/11
- Visual Studio 2019+ (or MSBuild)
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) — installed automatically with Visual Studio

---

## Database Setup

### Step 1 — Create the database and tables

Open **SQL Server Management Studio (SSMS)** or any SQL client, connect to:
```
Server: (localdb)\MSSQLLocalDB
Authentication: Windows Authentication
```

Open `setup.sql` (located in the project root) and execute it.  
This will create the `EcoManagementDB` database with three tables:

| Table | Primary Key | Description |
|---|---|---|
| `Species` | `SpeciesId` (identity) | Species catalogue |
| `Observations` | `ObservationId` (identity) | Wildlife observations, FK → Species |
| `Environment` | `(Location, Date)` | Environmental measurements |

### Step 2 — Load sample data

Use the application's **Import** button to load the included CSV files in this order:

1. `Species.csv` → save to table **Species**
2. `Observation.csv` → save to table **Observations**
3. `Environment.csv` → save to table **Environment**

> When prompted **"Clear all existing data before inserting?"** — choose **Yes** on first load.

---

## Running the Application

1. Open `EcoData Manager.sln` in Visual Studio
2. Build → `Ctrl+Shift+B`
3. Run → `F5`

Or build from command line:
```
msbuild "EcoData Manager.sln" /p:Configuration=Debug
```

---

## Connection String

Located in `DataAccess.cs`:
```csharp
return @"Server=(localdb)\MSSQLLocalDB;Database=EcoManagementDB;Trusted_Connection=True;";
```
Change this if you use a different SQL Server instance.

---

## Features

- Load and view any table with optional date-range filter
- Import data from **CSV**, **XML**, **JSON**
- Export current view to **CSV**, **XML**, **JSON**
- Bulk insert with automatic identity column detection
- Analytics: species observation totals + environment averages

---

## Project Structure

```
EcoData Manager/
├── DataAccess.cs        # Database layer (connection, query, bulk insert)
├── ImportExport.cs      # CSV / XML / JSON read & write
├── Form1.cs             # Main UI logic
├── Models/              # EnvironmentRecord, Observation, Species
├── setup.sql            # Database creation script
├── Species.csv          # Sample data
├── Observation.csv      # Sample data
└── Environment.csv      # Sample data
```
