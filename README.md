# SkillSwap API

**A peer-to-peer skill-sharing platform powered by time-based exchanges.**

SkillSwap is a platform that enables people to teach what they know and learn what they need without relying on traditional monetary payments. Users exchange knowledge through structured learning sessions, earning time credits by teaching and spending them to learn new skills.

---

## Table of Contents

* [Overview](#overview)
* [How It Works](#how-it-works)
* [Core Features](#core-features)
* [System Architecture](#system-architecture)
* [System Modules](#system-modules)
* [Technology Stack](#technology-stack)
* [Database Overview](#database-overview)
* [Development Roadmap](#development-roadmap)
* [Getting Started](#getting-started)
* [Contributing](#contributing)

---

## Overview

Learning a new skill often requires money, while many people already have valuable knowledge they could share with others.

SkillSwap addresses this challenge through a time-based exchange system that connects people with complementary skills.

Instead of paying for lessons, users exchange their time and expertise:

* Teach a skill you know to earn time credits.
* Spend your earned credits to learn another skill.
* Connect with people whose teaching and learning goals complement yours.

**Core Principle: One hour of teaching earns one hour of learning credit.**

The platform aims to make knowledge sharing accessible while encouraging collaboration, trust, and continuous learning.

## How It Works

The platform follows a simple exchange cycle:

1. **Create a Profile:** Users register, describe their experience, and list the skills they can teach and want to learn.
2. **Discover Matches:** The platform helps users find people with complementary skills and compatible availability.
3. **Propose a Session:** Users send learning proposals specifying the skill, duration, and preferred time.
4. **Exchange Knowledge:** Both users participate in a scheduled learning session.
5. **Earn and Spend Credits:** After a session is completed and verified, the appropriate time credits are transferred between users.

### Example

A developer wants to improve their English communication skills and can teach C# programming.

Another user wants to learn C# and can teach English.

SkillSwap connects them, allowing both users to exchange knowledge without a traditional monetary payment.

---

## Core Features

| Feature                   | Description                                                               |
| ------------------------- | ------------------------------------------------------------------------- |
| Authentication & Profiles | Secure registration, login, profile management, and skill preferences.    |
| Skill Discovery           | Search, filter, and discover users based on their skills and preferences. |
| Smart Matching            | Identify potential learning partners based on complementary skills.       |
| Proposals & Booking       | Create, accept, decline, and manage learning session proposals.           |
| Interactive Sessions      | Support live learning, real-time communication, and session management.   |
| Time Wallet               | Track earned and spent credits with a complete transaction history.       |
| Reviews & Badges          | Build trust through session reviews, ratings, and achievements.           |

---

### Architecture Layers

The solution follows Clean Architecture while organizing business logic by
functional modules. Each module owns its related domain and application
components instead of grouping all entities and features into shared folders.


SkillSwap
│
├── Domain
│   │
│   ├── Users
│   │   ├── Entities
│   │   ├── Aggregates
│   │   ├── ValueObjects
│   │   └── Enums
│   │
│   ├── Skills
│   │   ├── Entities
│   │   ├── Aggregates
│   │   ├── ValueObjects
│   │   └── Enums
│   │
│   ├── Proposals
│   │   ├── Entities
│   │   ├── Aggregates
│   │   ├── ValueObjects
│   │   └── Enums
│   │
│   ├── Sessions
│   │   ├── Entities
│   │   ├── Aggregates
│   │   ├── ValueObjects
│   │   └── Enums
│   │
│   ├── Wallet
│   │   ├── Entities
│   │   ├── Aggregates
│   │   ├── ValueObjects
│   │   └── Enums
│   │
│   └── Reviews
│       ├── Entities
│       ├── Aggregates
│       ├── ValueObjects
│       └── Enums
│
├── Application
│   │
│   ├── Users
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── DTOs
│   │   └── Validators
│   │
│   ├── Skills
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── DTOs
│   │   └── Validators
│   │
│   ├── Proposals
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── DTOs
│   │   └── Validators
│   │
│   ├── Sessions
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── DTOs
│   │   └── Validators
│   │
│   ├── Wallet
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── DTOs
│   │   └── Validators
│   │
│   └── Reviews
│       ├── Commands
│       ├── Queries
│       ├── DTOs
│       └── Validators
│
├── Infrastructure
│   ├── Persistence
│   ├── Repositories
│   ├── ExternalServices
│   └── DependencyInjection
│
└── Presentation
    ├── Controllers
    ├── Middleware
    ├── Authentication
    └── Configuration

> This represents the intended logical architecture. The actual solution structure may evolve during implementation.

### Architectural Principles

* **Clean Architecture:** Keeps business rules independent of external frameworks and infrastructure.
* **CQRS:** Separates commands that modify application state from queries that retrieve data.
* **Repository & Unit of Work:** Provides abstractions for data access and coordinated persistence.
* **Dependency Injection:** Supports loosely coupled components and testability.
* **State Management:** Controls valid transitions between proposal and session statuses.

---

## System Modules

The backend is organized into seven functional modules.

### 1. User & Authentication (IAM)

Responsible for user identity, authentication, and account security.

* Registration and login.
* JWT access and refresh token management.
* Email verification and password recovery using OTP.
* Logout and token revocation.

### 2. User Profile & Preferences

Manages user information, skills, and learning preferences.

* Profile information, including bio, country, and timezone.
* Teaching and learning skill inventories.
* Weekly availability and scheduling preferences.
* Exchange preferences, such as online-only sessions.

### 3. Discovery, Matching & Search

Helps users discover suitable learning partners.

* Search and filter users by skills and preferences.
* Complementary skill matching.
* Public user profiles with skills, ratings, and badges.
* Discovery feeds and relevant user recommendations.

### 4. Proposals & Session Booking

Manages the process of arranging learning sessions.

* Retrieve available time slots.
* Create proposals with skills, dates, and durations.
* Accept or decline incoming proposals.
* Manage outgoing proposals and cancellations.

### 5. Live Interactive Sessions

Supports real-time learning experiences and session lifecycle management.

* Session room creation and access management.
* Video and audio integration through WebRTC providers.
* Real-time messaging and notifications.
* Interactive whiteboard support.
* Session completion and actual duration tracking.

### 6. Time Wallet & Transactions

Manages the platform's virtual time-credit economy.

* View wallet balances in hours and minutes.
* Track earned and spent time credits.
* Transfer credits after verified session completion.
* Optional credit purchases through supported payment providers.
* Payment webhook processing and PDF receipts.

### 7. Ratings, Reviews & Badges

Builds trust and encourages participation.

* Rate completed sessions.
* Submit written reviews.
* Award recognition badges.
* Display user ratings and achievements.

---

## Technology Stack

The following technologies represent the proposed backend stack and may be adjusted as development progresses.

| Category                | Technologies                            |
| ----------------------- | --------------------------------------- |
| Framework               | ASP.NET Core Web API, .NET 8 / .NET 9   |
| Language                | C#                                      |
| Architecture            | Clean Architecture                      |
| Database                | Microsoft SQL Server                    |
| ORM                     | Entity Framework Core                   |
| Application Patterns    | CQRS, MediatR, Repository, Unit of Work |
| Authentication          | ASP.NET Core Identity, JWT              |
| Validation              | FluentValidation                        |
| Object Mapping          | AutoMapper or Mapster                   |
| Real-time Communication | SignalR                                 |
| Video & Audio           | WebRTC, Agora or LiveKit                |
| Caching                 | Redis                                   |
| Payments                | Stripe or Paymob                        |
| Email                   | MailKit or SendGrid                     |
| PDF Generation          | QuestPDF                                |
| API Documentation       | Swagger / OpenAPI                       |

---

## Database Overview

The database is designed around the platform's primary business entities.

| Entity / Table | Responsibility                                                            |
| -------------- | ------------------------------------------------------------------------- |
| Users          | User accounts and profile information.                                    |
| Skills         | Available skills in the platform.                                         |
| UserSkills     | User-to-skill relationships, including teaching and learning preferences. |
| Availability   | Weekly user availability and scheduling.                                  |
| Proposals      | Learning requests, proposed schedules, and proposal statuses.             |
| Sessions       | Scheduled and completed learning sessions.                                |
| Wallets        | Current time-credit balances.                                             |
| Transactions   | Auditable records of earned, spent, and transferred credits.              |
| Reviews        | Session feedback and user ratings.                                        |
| Badges         | Achievements and recognition awarded to users.                            |

The final database schema and relationships will be refined during implementation.

---

## Development Roadmap

Development is planned in the following phases:

| Phase | Scope                                                               |
| ----- | ------------------------------------------------------------------- |
| 1     | Solution structure, database setup, and initial EF Core migrations. |
| 2     | Authentication, user profiles, and skill preferences.               |
| 3     | Discovery, search, and complementary skill matching.                |
| 4     | Proposals, scheduling, and booking workflows.                       |
| 5     | Time wallet, transactions, and payment integration.                 |
| 6     | Live sessions, reviews, and badges.                                 |

The roadmap may change as requirements are refined and modules are implemented.

---

## Getting Started

### Prerequisites

The following tools are expected to be required for local development:

* .NET SDK compatible with the project target framework.
* Microsoft SQL Server.
* Git.
* An IDE such as Visual Studio or Visual Studio Code.

### Setup

Once the solution structure and configuration are available, the setup process will include:

1. Clone the repository.
2. Configure the database connection string.
3. Apply the required Entity Framework Core migrations.
4. Restore dependencies and build the solution.
5. Run the Web API and access its Swagger documentation.

Detailed installation commands and environment configuration will be added as the implementation progresses.

---

## Contributing

SkillSwap is developed collaboratively.

Contributions should follow the team's agreed development workflow:

1. Create an issue or select an existing task.
2. Create a dedicated branch for the task.
3. Implement the changes and commit them with descriptive messages.
4. Push the branch and open a Pull Request against the main repository.
5. Review feedback, make any required changes, and merge after approval.

All contributors are encouraged to keep changes focused, follow the established architecture, and document significant implementation decisions.

---

**SkillSwap — Share your skills. Exchange your time. Grow together.**
