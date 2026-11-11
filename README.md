# CoachBoard

CoachBoard is a web application for managing sports training sessions, players and attendance. I developed this project using C#, ASP.NET Core MVC and Microsoft SQL Server.

Trainers can manage players and training sessions, while players can register their attendance and view their personal training history.

## Features

- User registration and login
- Role-based access for Trainers and Players
- Create, edit and delete player accounts
- Manage training sessions
- Register attendance (Present, Absent, Excused)
- Calculate attendance percentages
- Filter and sort training sessions
- View personal attendance history
- Prevent duplicate hall reservations

## Technologies

- **Backend:** C#, ASP.NET Core MVC
- **Frontend:** Razor Views, HTML, CSS, Bootstrap
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Database Management:** Code First, Migrations
- **Tools:** Visual Studio, Git, GitHub

## Technical Implementation

- MVC architecture with Models, Views and Controllers
- ViewModels for transferring data between controllers and views
- LINQ for database queries and filtering
- Cookie-based authentication
- Password hashing using PBKDF2
- Role-based authorization
- Server-side validation
- Anti-Forgery Tokens for form security
- Unique database index to prevent duplicate attendance records


**Project:** CoachBoard  
**Type:** Personal Project  
**Status:** In Development
