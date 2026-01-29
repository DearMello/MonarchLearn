Project Overview
MonarchLearn is a robust, high-performance E-Learning Management System (LMS) developed using ASP.NET Core 9.0. The platform is architected to handle complex educational workflows, ranging from asynchronous lesson delivery to automated grading and certification. The primary goal of this project is to demonstrate a deep understanding of Clean Architecture, SOLID principles, and Enterprise Design Patterns in a real-world scenario. It provides a scalable backend solution for modern educational platforms where data integrity and user experience are paramount.

Architectural Design
The project follows the Onion Architecture (Clean Architecture) pattern to ensure a high degree of decoupling and maintainability.

Domain Layer: Contains core business entities such as Course, Module, LessonItem, Enrollment, and Quiz. This layer is independent of any other layer.

Application Layer: Houses the business logic, DTOs, AutoMapper profiles, and Service interfaces. It acts as a bridge between the API and the data persistence layer.

Infrastructure Layer: Implements data access logic using Entity Framework Core, migrations, and third-party service integrations like SMTP for email notifications.

API Layer: The entry point of the system, handling HTTP requests, middleware, and dependency injection container configuration.

Technical Features and Business Logic
Identity and Security
Integrated ASP.NET Core Identity for robust authentication and role-based authorization (Admin, Instructor, Student).

Mandatory Email Confirmation system to ensure platform security and valid user acquisition.

Advanced password hashing and JWT-ready infrastructure for future mobile integrations.

Course and Content Management
Hierarchical data structure: Course - Module - LessonItem.

Support for multiple lesson types: Video lessons (with duration tracking), Reading materials, and Interactive Quizzes.

Learning Paths: Logic-driven access control where students must complete previous lessons to unlock subsequent ones.

Advanced Progress Tracking
Video Watch-Time Validation: A lesson is only marked as Completed if the student watches at least 90 percent of the video duration.

Real-time Progress Calculation: Dynamic percentage calculation based on the total number of items in a course using high-precision arithmetic to avoid floating-point errors.

Comprehensive Quiz Engine
Cooldown Mechanism: Prevents spamming by enforcing a waiting period (e.g., 2 hours) after a failed attempt.

Success Metrics: Customizable passing scores per quiz, with detailed logs of every student's answers and time spent.

Gamification and Certification
Daily Streaks: Tracks user consistency using UTC-to-Local time conversion logic, specifically optimized for Azerbaijan Standard Time.

Automated Certificates: Upon 100 percent completion, the system triggers the ICertificateService to generate a personalized PDF certificate for the student.

Tech Stack
Framework: .NET 9.0 (C#)

Database: Microsoft SQL Server (SSMS)

ORM: Entity Framework Core

Design Patterns: Unit of Work, Repository Pattern, Dependency Injection.

Mapping and Validation: AutoMapper, FluentValidation.

Documentation: Swagger / OpenAPI for API testing.

Database Schema
The database is designed with high normalization to maintain data consistency.

Enrollments: This table acts as the core of the system, linking Users to Courses and tracking the ProgressPercent.

LessonProgress: Granular tracking of every student-lesson interaction.

UserSubscriptions: Manages time-limited access to course content based on selected plans.

Setup and Installation
Clone the Repository: Use the command git clone followed by the repository URL.

Database Configuration: Update the connection string in the appsettings.json file.

Migrations: Run the Update-Database command in the Package Manager Console.

SMTP Configuration: Configure the email settings and App Password for the notification service.

Run Application: Launch the project and explore the API via Swagger documentation.
