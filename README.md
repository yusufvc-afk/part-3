# RaceDay

## Project Overview

RaceDay is a full-stack web-based event management system designed for the South African road running, walking and cycling community.

The system is designed to help Event Organisers manage events, categories, participant enrolments and race results while allowing Participants to browse and enter events and track their performance.

## User Roles

### Organiser

An Organiser can:

- Create events
- Edit events
- Delete events
- Manage event categories
- View event enrolments
- Capture participant results

### Participant

A Participant can:

- Create an account
- Browse upcoming events
- Enter events
- Select event categories
- View their own enrolments
- Track their personal results

## Workflow	

<img width="1917" height="900" alt="image" src="https://github.com/user-attachments/assets/70d454ad-17a9-4643-b499-f0f61ab26d47" />

## Why the workflow is not working 

<img width="736" height="1600" alt="image" src="https://github.com/user-attachments/assets/cfe33085-3574-411c-8fab-a677c7c5a651" />


## Youtube link

https://youtu.be/G3NF1wZ2Otc

## Part 1

Part 1 focuses on planning and database design.

The following deliverables are included:

- Entity Relationship Diagram
- REST API Endpoint Plan
- SQL Server database script
- Database seed data
- GitHub Actions validation workflow

## Repository Structure

```text
RaceDay/
│
├── .github/
│   └── workflows/
│       └── part1-validation.yml
│
├── docs/
│   ├── RaceDay_ERD.png
│   ├── RaceDay_API_Endpoint_Plan.md
│   ├── RaceDay_Database.sql
│   ├── Database_Design.md
│   └── System_Roles.md
│
└── README.md

# RaceDay API

## PROG6212 Programming 2B: Portfolio of Evidence Part 2

RaceDay is an ASP.NET Core Web API developed to manage sporting events, participant registrations and race results.

## Technologies

- C# and ASP.NET Core Web API (.NET 8)
- Entity Framework Core Code First
- Microsoft SQL Server
- ASP.NET Core cookie authentication
- Swagger/OpenAPI
- xUnit automated tests
- GitHub Actions CI

## User Roles

**Organiser:** Creates and manages events, assigns categories, views enrolments and records race results.

**Participant:** Registers an account, manages a profile, enrols in events, cancels eligible enrolments and views personal results.

## Features

- User registration and password hashing
- Login and logout
- Authenticated user profiles
- Role-based authorization
- Event CRUD operations
- Event categories and entry fees
- Participant enrolments and cancellation
- Organiser enrolment viewing
- Race result recording and retrieval
- Validation and error handling
- Swagger API documentation

## Running the API

Open a terminal inside `RaceDay.Api`.

```powershell
dotnet restore
dotnet ef database update
dotnet run
```

The SQL Server connection must be configured in the application's connection settings before applying migrations.

Open Swagger at the HTTPS address shown in the terminal, followed by `/swagger`.

## Running Automated Tests

From the repository root:

```powershell
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```

## Database Design

The system contains six principal entities:

1. Users
2. Events
3. Categories
4. EventCategories
5. Enrolments
6. Results

See `docs/ERD.md` and `docs/database.sql` for the database design and original SQL documentation.

The Part 2 database is managed through Entity Framework Core migrations.

## CI/CD

GitHub Actions automatically restores, builds and tests the RaceDay project when changes are pushed to GitHub.

Workflow: `.github/workflows/dotnet.yml`

## Security and Limitations

Passwords are stored using secure password hashing, and protected API endpoints require authenticated users with appropriate roles.

Additional production hardening would include CSRF protection for cookie-authenticated writes, stronger concurrency controls for category capacity and further integration tests.

## RaceDay Part 2 - Screenshots and Evidence

The following screenshots demonstrate the implementation, testing and verification of the RaceDay Event Management API.

### 1. Swagger API Documentation

The Swagger interface displays the available RaceDay API endpoints.

![Swagger Overview 1](docs/screenshots/swagger-overview-1.png)

![Swagger Overview 2](docs/screenshots/swagger-overview-2.png)

![Swagger Overview 3](docs/screenshots/swagger-overview-3.png)

![Swagger Overview 4](docs/screenshots/swagger-overview-4.png)

### 2. User Registration

Demonstrates the successful registration of users through the authentication API.

![Registration 1](docs/screenshots/registration-success-1.png)

![Registration 2](docs/screenshots/registration-success-2.png)

![Registration Request 1](docs/screenshots/reg-p-1.png)

![Registration Request 2](docs/screenshots/reg-p-2.png)

### 3. User Login and Logout

Demonstrates user authentication and successful login and logout operations.

![Login 1](docs/screenshots/login-success-1.png)

![Login 2](docs/screenshots/login-success-2.png)

![Login Request 1](docs/screenshots/login-p-1.png)

![Login Request 2](docs/screenshots/login-p-2.png)

![Logout](docs/screenshots/logout.png)

### 4. Event Creation

Demonstrates the creation of a race event by an Organiser.

![Event Created 1](docs/screenshots/event-created-1.png)

![Event Created 2](docs/screenshots/event-created-2.png)

### 5. Category Creation and Assignment

Demonstrates the creation of race categories and their assignment to events.

![Category Creation 1](docs/screenshots/create-cat-1.png)

![Category Creation 2](docs/screenshots/create-cat-2.png)

![Category Assigned 1](docs/screenshots/category-assigned-1.png)

![Category Assigned 2](docs/screenshots/category-assigned-2.png)

### 6. Participant Enrolment

Demonstrates participants registering for an event.

![Participant Enrolment 1](docs/screenshots/enroll-p-1.png)

![Participant Enrolment 2](docs/screenshots/enroll-p-2.png)

### 7. Organiser Enrolment Management

Demonstrates an Organiser viewing event registrations.

![Organiser Enrolments 1](docs/screenshots/organiser-enrolments-1.png)

![Organiser Enrolments 2](docs/screenshots/organiser-enrolments-2.png)

![Event Registration 1](docs/screenshots/event-reg-1.png)

![Event Registration 2](docs/screenshots/event-reg-2.png)

### 8. Race Result Recording

Demonstrates the recording and verification of participant race results.

![Result Recorded 1](docs/screenshots/result-recorded-1.png)

![Result Recorded 2](docs/screenshots/result-recorded-2.png)

![Verify Results 1](docs/screenshots/Verify-results-1.png)

![Verify Results 2](docs/screenshots/Verify-results-2.png)

### 9. SQL Server Database

Shows the RaceDay SQL Server database tables created through Entity Framework Core.

![Database Tables](docs/screenshots/database-tables.png)

### 10. Automated Unit Tests

Shows the execution of automated tests for the RaceDay API.

![Unit Tests](docs/screenshots/unit-tests.png)

### 11. GitHub Actions CI/CD

Shows the successful automated build and testing workflow.

![CI/CD Success](docs/screenshots/ci-cd-success.png)

### 12. Git Commit History

Shows the Git commit history documenting the development of the RaceDay project.

![Git Commits](docs/screenshots/git-commits.png)


## YouTube Video

YouTube link: https://youtu.be/9DBmqlgvHZ4

## AI Assistance Disclosure

AI assistance was used to help with testing.
