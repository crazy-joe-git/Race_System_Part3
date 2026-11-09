# Race_System

## Project Overview
Race_System is a role-based RESTful sports event management platform designed specifically for South African road running events. The platform bridges the gap between event management and athlete participation by providing an intuitive system for race creation, category selection, participant enrolments, and official finish-time tracking. Built with strict role separation between **Organisers** and **Participants**, Race_System ensures secure, streamlined operations for every race day.

---

## User Roles & Permissions
Race_System implements strict Role-Based Access Control (RBAC) to tailor the experience and security for different users:

1. **Organisers:**
   - Create, update, and manage upcoming running events and marathons.
   - Set up race categories (e.g., 5K, 10K, 21K, 42K).
   - Oversee participant registrations, review rosters, and manage event logistics.
   - Log and verify official finish times and results.

2. **Participants:**
   - Browse upcoming races and view detailed event information.
   - Register and enrol in race categories securely.
   - Track personal entries, milestones, and review past race history.
   - Manage personal profile settings and view official results.

---

## Part 1: Architecture & System Design
In Part 1 of the project, we established the theoretical and structural foundation necessary to build a scalable web application:
- **Domain Requirement Analysis:** Identifying user roles, permissions, and core business workflows.
- **Database Modeling & Scripting:** Designing a normalized relational database schema supporting secure authentication, event structures, enrolments, and result logs.
- **REST API Endpoint Planning:** Structuring a clean API specification mapping routes, HTTP methods, and response codes.
- **Version Control & Repository Setup:** Initializing the GitHub repository with automated workflow checks.

---

## Part 2: Backend Implementation & Testing
In Part 2, the architectural blueprints were transformed into a fully functional ASP.NET Core MVC application integrated with Entity Framework Core and automated testing:
- **Secure Authentication:** ASP.NET Core Identity with role management for Organisers and Participants.
- **Entity Framework Core (In-Memory & Relational):** Robust ORM integration handling database operations and migrations.
- **Automated Unit Testing:** Functional xUnit test suite (`Race_System_2_.Tests`) verifying core business logic and data integrity.
- **CI/CD Pipeline:** Automated build and test verification via GitHub Actions.

---

## Part 3: Containerisation, Deployment & Cloud Integration
In Part 3, the application was production-ready through containerization, continuous integration automation, and cloud asset storage:
- **Docker Containerisation:** Multi-stage `Dockerfile` built on .NET 8.0 SDK and ASP.NET runtime images, ensuring consistent deployment environments across host systems.
- **CI/CD Automation:** GitHub Actions pipeline (`ci.yml`) automatically executes dependency restoration, build verification, and automated unit tests upon code pushes and pull requests.
- **Cloud Storage (Azure Blob Storage):** Integrated cloud blob storage for managing user profile assets, media uploads, and document artifacts securely.

---

## Setup and Run Instructions

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET and web development workload) or VS Code.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerized runs).

### Running the Application Locally via Visual Studio
1. **Clone the Repository:**
   ```bash
   git clone [https://github.com/crazy-joe-git/Race_System-2-.git](https://github.com/crazy-joe-git/Race_System-2-.git)
   cd Race_System-2-