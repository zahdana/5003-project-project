# Dataset Sharing Platform

A full-stack web application for managing and sharing research datasets.  
The platform allows users to upload, tag, search, comment on, and download datasets with fine-grained permission control.


## 1. Project Structure

The repository is organized into three main parts:

├── backend/ # ASP.NET Core Web API
│ ├── DatasetSharingPlatform.Api.sln
│ ├── DatasetSharingPlatform.Api/
│ │ ├── Controllers/ # API controllers
│ │ ├── Data/ # DbContext, EF Core configuration
│ │ ├── DTOs/ # Data transfer objects
│ │ ├── Models/ # Entity classes
│ │ ├── Properties/
│ │ ├── appsettings.json # DB config
│ │ ├── Program.cs / Startup.cs
│ │ └── ...
│
├── frontend/ # Vue.js single-page application
│ ├── package.json
│ ├── vue.config.js
│ ├── src/
│ │ ├── views/ # Pages
│ │ ├── components/ # UI components
│ │ ├── router/ # Vue Router configuration
│ │ ├── services/ # axios.js etc.
│ │ └── store/
│ └── public/
│
└── database/
├── script.sql # Database schema
└── *.bak / *.mdf # Backup or actual DB file

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
