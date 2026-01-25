# Clinic Appointment System

A comprehensive healthcare management platform built with ASP.NET Core MVC that streamlines clinic operations through efficient appointment scheduling, role-based access control, and integrated patient management.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Database Setup](#database-setup)
- [How to Run](#how-to-run)
- [User Roles](#user-roles)
- [Project Structure](#project-structure)
- [Contributors](#contributors)

## 🏥 Overview

The Clinic Appointment System is a digital healthcare platform that addresses common challenges in medical facilities including:
- Long patient waiting times due to inefficient scheduling
- Appointment conflicts and overbooking issues
- Excessive administrative workload for healthcare providers
- Limited patient access to medical records and appointment history

This system provides a seamless experience for patients, doctors, and administrators through role-based interfaces and comprehensive data management.

## ✨ Features

### Patient Portal
- Secure registration and login
- Browse doctors by specialization
- Real-time appointment booking with availability checking
- View appointment history and status
- Access medical records and prescriptions
- Update personal and insurance information

### Doctor Dashboard
- Manage availability and schedules
- View daily appointments with patient details
- Access comprehensive patient medical histories
- Create and manage prescriptions
- Integration with laboratory test orders

### Administrator Panel
- User management across all roles
- Doctor assignment to departments
- System analytics and reporting
- Department and service configuration
- Billing oversight and monitoring

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core MVC 8.0
- **ORM:** Entity Framework Core 8.0
- **Database:** SQLite
- **Frontend:** Bootstrap 5, Razor Pages
- **Authentication:** ASP.NET Core Identity with BCrypt password hashing
- **Development Environment:** Visual Studio 2022
- **Version Control:** Git & GitHub

## 📦 Prerequisites

Before running this project, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or Visual Studio Code
- Git (for cloning the repository)

## 🚀 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/armends198/ClinicAppointmentSystem.git
   cd ClinicAppointmentSystem
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

   The project uses the following NuGet packages:
   - `BCrypt.Net-Next` (Version 4.0.3) - Password hashing
   - `Microsoft.EntityFrameworkCore` (Version 8.0.0) - ORM framework
   - `Microsoft.EntityFrameworkCore.Sqlite` (Version 8.0.0) - SQLite database provider
   - `Microsoft.EntityFrameworkCore.Tools` (Version 8.0.0) - Migration tools

## 💾 Database Setup

The project uses Entity Framework Core Code-First approach with SQLite database.

1. **Apply database migrations**
   
   Open Package Manager Console in Visual Studio (Tools > NuGet Package Manager > Package Manager Console) and run:
   ```
   Update-Database
   ```

   Or using .NET CLI:
   ```bash
   dotnet ef database update
   ```

2. **Database location**
   
   The SQLite database file will be created in the project directory as `clinic.db` (or as specified in your connection string in `appsettings.json`).

## ▶️ How to Run

### Using Visual Studio 2022

1. Open the solution file (`.sln`) in Visual Studio 2022
2. Press `F5` or click the "Run" button (IIS Express or project name)
3. The application will launch in your default browser

### Using .NET CLI

1. Navigate to the project directory
2. Run the following command:
   ```bash
   dotnet run
   ```
3. Open your browser and navigate to the URL shown in the console (typically `https://localhost:5001` or `http://localhost:5000`)

### Using Visual Studio Code

1. Open the project folder in VS Code
2. Open the integrated terminal (Ctrl + `)
3. Run:
   ```bash
   dotnet run
   ```
4. Navigate to the localhost URL in your browser

## 👥 User Roles

The system implements three distinct user roles:

| Role | Access Level | Key Functions |
|------|--------------|---------------|
| **Patient** | User | Book appointments, view medical records, manage profile |
| **Doctor** | Provider | Manage schedule, view patients, create prescriptions |
| **Administrator** | Admin | System configuration, user management, analytics |

## 📁 Project Structure

```
ClinicAppointmentSystem/
├── Controllers/         # MVC Controllers for handling requests
├── Models/             # Entity models and view models
├── Views/              # Razor views for UI
├── Data/               # DbContext and database configuration
├── Migrations/         # Entity Framework migrations
├── wwwroot/            # Static files (CSS, JS, images)
├── appsettings.json    # Application configuration
└── Program.cs          # Application entry point
```

## 👨‍💻 Contributors

This project was developed by students from Southeast European University, Tetovo, North Macedonia:

- **Endrit Abduramani** - Computer Science
- **Armend Sejfullov** - Computer Science  
- **Amar Ademi** - Computer Science

**Course:** Programming with .NET

## 📝 License

This project is developed for educational purposes as part of the Programming with .NET course.

## 🔮 Future Enhancements

- Automated email/SMS notifications
- Online payment integration
- Native mobile applications (iOS/Android)
- Telemedicine and video consultation support
- Advanced analytics and reporting dashboards
- Integration with existing EHR systems
- Calendar synchronization (Google, Outlook, Apple)

## 📧 Contact

For questions or contributions, please open an issue on GitHub or contact the repository maintainer.

---

**Repository:** [https://github.com/armends198/ClinicAppointmentSystem](https://github.com/armends198/ClinicAppointmentSystem)
