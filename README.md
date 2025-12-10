# Dataset Sharing Platform

A full-stack web application for managing and sharing research datasets.  
The platform allows users to upload, tag, search, comment on, and download datasets with fine-grained permission control.


## 1. Project Structure

The repository is organized into three main parts:

.
├── backend/                 # ASP.NET Core Web API
│   ├── DatasetSharingPlatform.Api.sln
│   ├── DatasetSharingPlatform.Api/
│   │   ├── Controllers/     # API controllers 
│   │   ├── Data/            # DbContext, EF Core configuration
│   │   ├── DTOs/            # Data transfer objects used by the API
│   │   ├── Models/          # Entity classes mapped to database tables
│   │   ├── Properties/
│   │   ├── appsettings.json # Configuration (including DB connection string)
│   │   └── Program.cs / Startup.cs
│   └── ...                  # Other backend support files
│
├── frontend/                # Vue.js single-page application 
│   ├── package.json
│   ├── vue.config.js
│   ├── src/
│   │   ├── views/           # Pages (Login, MainPage, SearchDataSet, DatasetDetail, etc.)
│   │   ├── components/      # Reusable UI components
│   │   ├── router/          # Vue Router configuration
│   │   ├── services/
│   │   │   └── axios.js     # Axios instance & API base URL
│   │   └── store/           # (If used) global state management
│   └── public/
│
└── database/
    ├── script.sql           # Database schema (tables, indexes, constraints)
    └── .bak/.mdf # Backup or attached database files

---
## 2. Dependencies
Backend (ASP.NET Core):

.NET 8 SDK

ASP.NET Core Web API

Entity Framework Core

SQL Server

JWT Authentication


Frontend (Vue.js):

Node.js + npm

Vue CLI

Axios

Vue Router


Database:

SQL Server 2022

SSMS 

## 3. How to Run the System
Step 1: Set up the Database

Open SSMS.

Create a new empty database.

Execute database/script.sql or import .bak/.mdf to create all tables and indexes.

In backend/appsettings.json, update:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DatasetSharingDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

Step 2: Run the Backend API
Option A: Using Visual Studio 2022

Open backend solution (.sln).

Set DatasetSharingPlatform.Api as startup project.

Click Run.


Option B: Using CLI
cd backend
dotnet restore
dotnet run

Step 3: Run the Frontend
cd frontend
npm install
npm run serve


Frontend will run at:

http://localhost:8080


Ensure Axios base URL in axios.js matches backend



Step 4. Login and Usage