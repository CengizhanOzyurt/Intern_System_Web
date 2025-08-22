# Yaz Bilgi Intern Task Management System

## Overview
This is a web-based task management application built with ASP.NET Core. The system allows users to register, login, and manage their tasks with start and end dates.

## Features
- User authentication (register and login)
- Task management (create, view, update, and delete tasks)
- Task scheduling with start and end dates
- RESTful API with Swagger documentation

## Technology Stack
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL
- **Frontend**: ASP.NET MVC Views with CSS
- **API Documentation**: Swagger/OpenAPI

## Project Structure
- **Controllers/**: Contains MVC controllers and API controllers
  - **Api/**: RESTful API controllers
  - **HomeController.cs**: Main controller for web pages
- **Models/**: Data models and database context
  - **UsersModel.cs**: User entity
  - **UsersTaskModel.cs**: Task entity
  - **InternDBcontext.cs**: Database context
- **Views/**: MVC views
  - **LoginPage/**: Login related views
  - **RegisterPage/**: Registration related views
  - **TodoPage/**: Task management views

## Prerequisites
- .NET 8.0 SDK
- PostgreSQL database

## Setup and Installation

1. **Clone the repository**
   ```
   git clone <repository-url>
   cd Yaz-bilgi-main
   ```

2. **Configure the database connection**
   - Update the connection string in `appsettings.json`

3. **Apply database migrations**
   ```
   dotnet ef database update
   ```

4. **Run the application**
   ```
   dotnet run
   ```

5. **Access the application**
   - Web Interface: https://localhost:5001 or http://localhost:5000
   - API Documentation: https://localhost:5001/swagger or http://localhost:5000/swagger

## Screenshots

### Login Screen
![Login Screen](/wwwroot/images/login.png)

### Registration Screen
![Registration Screen](/wwwroot/images/register.png)

### Todo Management Screen
![Todo Screen](/wwwroot/images/todo.png)
## API Endpoints

The application provides RESTful API endpoints for user and task management. You can explore these endpoints using the Swagger UI available at `/swagger`.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
