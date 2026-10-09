# SkillSwap API

**A peer-to-peer skill-sharing platform powered by time-based exchanges.**

SkillSwap is a platform that enables people to teach what they know and learn what they need without relying on traditional monetary payments. Users exchange knowledge through structured learning sessions, earning time credits by teaching and spending them to learn new skills.

---
## Links
You can see our workflow through these links:
* [PRD](https://drive.google.com/drive/folders/1lj3Y2GVxgSYBydHbzTyX7M8r9gD9BZ7t?usp=sharing)
* [Jira](https://coinershot.atlassian.net/jira/software/projects/SSS/pages?atlOrigin=eyJpIjoiNTkzOGI4ZTJjYjg3NDI4Mzg2MzI0ZDVkOTM5MDFkMzgiLCJwIjoiaiJ9)

---
## Table of Contents

* [Overview](#overview)
* [How It Works](#how-it-works)
* [Core Features](#core-features)
* [System Architecture](#system-architecture)
* [Project Structure](#project-structure)
* [System Modules](#system-modules)
* [Technology Stack](#technology-stack)
* [Database Overview](#database-overview)
* [Development Roadmap](#development-roadmap)
* [Getting Started](#getting-started)
* [Contributing](#contributing)

---

# Overview

Learning a new skill often requires money, while many people already have valuable knowledge they could share with others.

SkillSwap addresses this challenge through a time-based exchange system that connects people with complementary skills.

Instead of paying for lessons, users exchange their time and expertise:

* Teach a skill you know to earn time credits.
* Spend earned credits to learn another skill.
* Connect with people whose teaching and learning goals complement yours.

**Core Principle: One hour of teaching earns one hour of learning credit.**

The platform aims to make knowledge sharing accessible while encouraging collaboration, trust, and continuous learning.

---

# How It Works

The platform follows a simple exchange cycle:

1. **Create a Profile**

   * Register an account.
   * Complete a profile.
   * Add skills the user can teach.
   * Add skills the user wants to learn.
   * Configure availability and exchange preferences.

2. **Discover Matches**

   * Search for users by skills.
   * Filter users by preferences and availability.
   * Discover complementary skill relationships.

3. **Propose a Session**

   * Select a skill.
   * Select a learning partner.
   * Choose duration.
   * Select a suitable time slot.
   * Send a learning proposal.

4. **Book and Attend**

   * The recipient accepts or declines the proposal.
   * An accepted proposal becomes a scheduled session.
   * Users attend the session through the supported communication provider.

5. **Complete the Session**

   * The session is completed and verified.
   * Actual session duration is recorded.

6. **Exchange Time Credits**

   * The learner spends time credits.
   * The teacher receives the corresponding time credits.
   * The transaction is recorded in the wallet ledger.

---

# Core Features

| Feature                   | Description                                                                          |
| ------------------------- | ------------------------------------------------------------------------------------ |
| Authentication & Profiles | Registration, login, account security, profile management, and preferences.          |
| Skill Management          | Users can define skills they teach and skills they want to learn.                    |
| Skill Discovery           | Search and discover users based on skills and preferences.                           |
| Smart Matching            | Identify users with complementary teaching and learning goals.                       |
| Proposals & Booking       | Create, accept, decline, cancel, and manage learning proposals.                      |
| Live Sessions             | Session rooms, real-time communication, messaging, and session lifecycle management. |
| Time Wallet               | Track earned and spent time credits with an auditable transaction history.           |
| Reviews & Ratings         | Review completed sessions and rate learning partners.                                |
| Badges                    | Award achievements based on defined platform rules.                                  |

---

# System Architecture

SkillSwap follows **Clean Architecture combined with Modular Domain Organization**.

The solution is divided into four main projects:

```text
SkillSwap
│
├── SkillSwap.Domain
├── SkillSwap.Application
├── SkillSwap.Infrastructure
└── SkillSwap.Presentation
```

Each project has a specific responsibility.

```text
                 ┌─────────────────────────────┐
                 │       Presentation          │
                 │                             │
                 │ Controllers                 │
                 │ Middleware                  │
                 │ Authentication              │
                 │ API Configuration            │
                 └──────────────┬──────────────┘
                                │
                                ▼
                 ┌─────────────────────────────┐
                 │       Application           │
                 │                             │
                 │ Users                       │
                 │ Skills                      │
                 │ Discovery                   │
                 │ Proposals                   │
                 │ Sessions                    │
                 │ Wallet                      │
                 │ Reviews                     │
                 │                             │
                 │ Commands / Queries / DTOs   │
                 │ Validators / Handlers       │
                 └──────────────┬──────────────┘
                                │
                                ▼
                 ┌─────────────────────────────┐
                 │          Domain             │
                 │                             │
                 │ Users                       │
                 │ Skills                      │
                 │ Discovery                   │
                 │ Proposals                   │
                 │ Sessions                    │
                 │ Wallet                      │
                 │ Reviews                     │
                 │                             │
                 │ Entities / Aggregates       │
                 │ Value Objects / Enums       │
                 │ Domain Rules / Events       │
                 └─────────────────────────────┘
                                ▲
                                │
                                │
                 ┌──────────────┴──────────────┐
                 │       Infrastructure       │
                 │                             │
                 │ Persistence                 │
                 │ Repository Implementations  │
                 │ Identity                    │
                 │ JWT                         │
                 │ Redis                       │
                 │ Email                       │
                 │ Payments                    │
                 │ Video Providers             │
                 │ External Services            │
                 └─────────────────────────────┘
```

### Dependency Direction

The dependency direction follows Clean Architecture:

```text
Presentation
     │
     ▼
Application
     │
     ▼
Domain

Infrastructure ───────► Application / Domain
```

The **Domain layer does not depend on Infrastructure, EF Core, ASP.NET Core, or external services**.

The Application layer contains use cases and business orchestration but does not contain infrastructure implementations.

Infrastructure implements the abstractions required by Application and Domain.

Presentation is responsible for exposing the application through HTTP/API endpoints.

---

# Project Structure

The important architectural rule is:

> **Organize business code by module first, then by architectural responsibility.**

Do not create one global folder containing all entities, repositories, commands, queries, and DTOs.

---

## Domain

Each business module owns its own domain model.

```text
SkillSwap.Domain
│
├── Users
│   ├── Entities
│   │   ├── User.cs
│   │   └── UserProfile.cs
│   │
│   ├── Aggregates
│   │   └── UserAggregate.cs
│   │
│   ├── ValueObjects
│   │   ├── Email.cs
│   │   ├── TimeZone.cs
│   │   └── UserName.cs
│   │
│   ├── Enums
│   │   └── UserStatus.cs
│   │
│   ├── Events
│   │   └── UserRegisteredDomainEvent.cs
│   │
│   └── Rules
│
├── Skills
│   ├── Entities
│   │   ├── Skill.cs
│   │   └── UserSkill.cs
│   │
│   ├── Aggregates
│   │   └── SkillAggregate.cs
│   │
│   ├── ValueObjects
│   │
│   ├── Enums
│   │
│   ├── Events
│   │
│   └── Rules
│
├── Discovery
│   ├── Entities
│   ├── Aggregates
│   ├── ValueObjects
│   ├── Enums
│   ├── Events
│   └── Rules
│
├── Proposals
│   ├── Entities
│   ├── Aggregates
│   ├── ValueObjects
│   ├── Enums
│   ├── Events
│   └── Rules
│
├── Sessions
│   ├── Entities
│   ├── Aggregates
│   ├── ValueObjects
│   ├── Enums
│   ├── Events
│   └── Rules
│
├── Wallet
│   ├── Entities
│   │   ├── Wallet.cs
│   │   └── Transaction.cs
│   │
│   ├── Aggregates
│   │   └── WalletAggregate.cs
│   │
│   ├── ValueObjects
│   │   ├── TimeCredit.cs
│   │   └── Money.cs
│   │
│   ├── Enums
│   │   └── TransactionType.cs
│   │
│   ├── Events
│   │   └── CreditsTransferredDomainEvent.cs
│   │
│   └── Rules
│
└── Reviews
    ├── Entities
    │   └── Review.cs
    │
    ├── Aggregates
    │   └── ReviewAggregate.cs
    │
    ├── ValueObjects
    │
    ├── Enums
    │
    ├── Events
    │
    └── Rules
```

### Domain Responsibilities

The Domain layer contains:

* Entities
* Aggregate Roots
* Value Objects
* Enums
* Domain Events
* Business Rules
* Domain-specific exceptions

It should **not** contain:

* EF Core
* DbContext
* HTTP
* Controllers
* JWT implementation
* Redis
* Stripe/Paymob
* Email providers
* SignalR
* External APIs

---

# Application

The Application layer follows the same module boundaries used by Domain.

```text
SkillSwap.Application
│
├── Users
│   ├── Commands
│   │   ├── Register
│   │   ├── Login
│   │   ├── Logout
│   │   ├── RefreshToken
│   │   ├── VerifyEmail
│   │   ├── ForgotPassword
│   │   └── ResetPassword
│   │
│   ├── Queries
│   │   ├── GetUserProfile
│   │   └── GetCurrentUser
│   │
│   ├── DTOs
│   │
│   ├── Validators
│   │
│   └── Mappings
│
├── Skills
│   ├── Commands
│   │   ├── AddSkill
│   │   ├── RemoveSkill
│   │   └── UpdateUserSkills
│   │
│   ├── Queries
│   │   ├── GetSkills
│   │   └── GetUserSkills
│   │
│   ├── DTOs
│   ├── Validators
│   └── Mappings
│
├── Discovery
│   ├── Commands
│   ├── Queries
│   │   ├── SearchUsers
│   │   ├── FindMatches
│   │   └── GetRecommendations
│   │
│   ├── DTOs
│   ├── Validators
│   └── Mappings
│
├── Proposals
│   ├── Commands
│   │   ├── CreateProposal
│   │   ├── AcceptProposal
│   │   ├── DeclineProposal
│   │   └── CancelProposal
│   │
│   ├── Queries
│   │   ├── GetProposal
│   │   └── GetUserProposals
│   │
│   ├── DTOs
│   ├── Validators
│   └── Mappings
│
├── Sessions
│   ├── Commands
│   │   ├── CreateSession
│   │   ├── StartSession
│   │   ├── CompleteSession
│   │   └── CancelSession
│   │
│   ├── Queries
│   │   ├── GetSession
│   │   ├── GetUserSessions
│   │   └── GetSessionRoom
│   │
│   ├── DTOs
│   ├── Validators
│   └── Mappings
│
├── Wallet
│   ├── Commands
│   │   ├── TransferCredits
│   │   ├── PurchaseCredits
│   │   └── ProcessPaymentWebhook
│   │
│   ├── Queries
│   │   ├── GetWallet
│   │   └── GetTransactions
│   │
│   ├── DTOs
│   ├── Validators
│   └── Mappings
│
└── Reviews
    ├── Commands
    │   ├── CreateReview
    │   └── AwardBadge
    │
    ├── Queries
    │   ├── GetReviews
    │   └── GetUserBadges
    │
    ├── DTOs
    ├── Validators
    └── Mappings
```

The Application layer contains:

* Commands
* Queries
* Command/Query Handlers
* DTOs
* Validators
* Application interfaces
* Use-case orchestration
* Authorization policies where appropriate

---

# Infrastructure

Infrastructure contains implementations of external concerns.

It should **not become a second Domain layer**.

```text
SkillSwap.Infrastructure
│
├── Persistence
│   ├── SkillSwapDbContext.cs
│   │
│   ├── Configurations
│   │   ├── Users
│   │   ├── Skills
│   │   ├── Discovery
│   │   ├── Proposals
│   │   ├── Sessions
│   │   ├── Wallet
│   │   └── Reviews
│   │
│   ├── Migrations
│   │
│   └── Seed
│
├── Repositories
│   ├── Users
│   ├── Skills
│   ├── Proposals
│   ├── Sessions
│   ├── Wallet
│   └── Reviews
│
├── Identity
│   ├── ApplicationUser.cs
│   ├── IdentityService.cs
│   └── IdentityConfiguration.cs
│
├── Authentication
│   ├── JwtTokenService.cs
│   ├── RefreshTokenService.cs
│   └── AuthenticationConfiguration.cs
│
├── Caching
│   └── Redis
│
├── Email
│   ├── MailKitEmailService.cs
│   └── EmailTemplates
│
├── Payments
│   ├── Stripe
│   └── Paymob
│
├── Video
│   ├── Agora
│   └── LiveKit
│
├── Realtime
│   └── SignalR
│
├── Pdf
│   └── QuestPdfService.cs
│
└── DependencyInjection
    └── InfrastructureServiceRegistration.cs
```

Infrastructure is responsible for:

* EF Core
* SQL Server
* Repository implementations
* Identity
* JWT implementation
* Redis
* Email providers
* Payment providers
* Video providers
* PDF generation
* External APIs
* Infrastructure-specific services

---

# Presentation

The Presentation layer exposes the application to clients.

```text
SkillSwap.Presentation
│
├── Controllers
│   ├── Users
│   ├── Skills
│   ├── Discovery
│   ├── Proposals
│   ├── Sessions
│   ├── Wallet
│   └── Reviews
│
├── Middleware
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
│
├── Authentication
│   └── CurrentUserService.cs
│
├── Hubs
│   └── SessionHub.cs
│
├── Filters
│
├── Configuration
│   ├── SwaggerConfiguration.cs
│   └── AuthenticationConfiguration.cs
│
└── Program.cs
```

Controllers should remain thin.

They should:

1. Receive HTTP requests.
2. Validate basic request requirements.
3. Dispatch Commands/Queries.
4. Return HTTP responses.

Business rules should not be implemented inside controllers.

---

# Module Boundaries

The most important rule in the architecture is **module ownership**.

For example, a proposal-related feature should not look like this:

```text
Domain/
    Entities/
        Proposal.cs

Application/
    Commands/
        CreateProposalCommand.cs
```

Instead, it should remain inside the Proposal module:

```text
Domain/
    Proposals/
        Entities/
            Proposal.cs

Application/
    Proposals/
        Commands/
            CreateProposal/
                CreateProposalCommand.cs
                CreateProposalHandler.cs
```

The same rule applies to every module.

This keeps related business logic together and prevents the solution from becoming a collection of global technical folders.

---

# System Modules

## 1. Users

Responsible for identity, authentication, account security, and user profiles.

### Responsibilities

* Registration
* Login
* Logout
* JWT access tokens
* Refresh tokens
* Email verification
* Password recovery
* OTP
* User profile
* Bio
* Country
* Timezone
* Account status
* User preferences

---

## 2. Skills

Responsible for skills and user skill inventories.

### Responsibilities

* Create/manage platform skills
* Assign skills to users
* Define teaching skills
* Define learning skills
* Skill proficiency
* Skill categories

---

## 3. Discovery

Responsible for finding suitable learning partners.

### Responsibilities

* Search users
* Filter by skills
* Filter by availability
* Filter by preferences
* Complementary skill matching
* Recommendations
* Public user profiles
* Ratings and badges summary

Discovery should consume information from other modules through defined application contracts rather than directly manipulating another module's domain objects.

---

## 4. Proposals

Responsible for arranging learning sessions.

### Responsibilities

* Create proposal
* Select learning partner
* Select skill
* Select duration
* Select preferred time
* Accept proposal
* Decline proposal
* Cancel proposal
* Proposal status transitions

Example lifecycle:

```text
Pending
   │
   ├──► Accepted
   │       │
   │       ▼
   │    Scheduled
   │       │
   │       ▼
   │    Completed
   │
   ├──► Declined
   │
   └──► Cancelled
```

Invalid state transitions must be rejected by the domain/application rules.

---

## 5. Sessions

Responsible for the actual learning session.

### Responsibilities

* Create session after accepted proposal
* Schedule session
* Start session
* Session room access
* Real-time communication
* Session messaging
* Track start/end time
* Track actual duration
* Complete session
* Cancel session

The Session module is responsible for the session lifecycle, while the Wallet module is responsible for the financial/time-credit consequences of a completed session.

---

## 6. Wallet

Responsible for the time-credit economy.

### Responsibilities

* Wallet balance
* Earn credits
* Spend credits
* Transfer credits
* Transaction history
* Payment integration
* Payment webhooks
* Credit purchases
* Receipts

The wallet should maintain an **auditable transaction ledger** rather than relying only on a mutable balance.

Example:

```text
Wallet
   │
   ├── Current Balance
   │
   └── Transactions
        ├── Earned
        ├── Spent
        ├── Transfer
        └── Purchased
```

---

## 7. Reviews

Responsible for feedback and achievements.

### Responsibilities

* Rate completed sessions
* Create written reviews
* Calculate/display ratings
* Award badges
* Retrieve user achievements

Reviews should only be created for valid completed sessions.

---

# Cross-Module Communication

Modules should not directly modify another module's entities.

For example:

```text
Sessions
   │
   │ SessionCompleted
   ▼
Wallet
   │
   │ Transfer Credits
   ▼
Transactions
```

A completed session can trigger an application/domain event or another defined application-level integration mechanism.

The important rule is:

```text
Module A
   │
   ▼
Contract / Command / Event
   │
   ▼
Module B
```

and not:

```text
Module A
   │
   ▼
Directly modifies Module B entities
```

This keeps module boundaries clear and reduces coupling.

---

# Technology Stack

| Category                | Technology                                       |
| ----------------------- | ------------------------------------------------ |
| Framework               | ASP.NET Core Web API                             |
| Runtime                 | .NET 8 / .NET 9                                  |
| Language                | C#                                               |
| Architecture            | Clean Architecture + Modular Domain Organization |
| Database                | Microsoft SQL Server                             |
| ORM                     | Entity Framework Core                            |
| Application Patterns    | CQRS, MediatR                                    |
| Persistence             | Repository + Unit of Work where justified        |
| Authentication          | ASP.NET Core Identity + JWT                      |
| Validation              | FluentValidation                                 |
| Mapping                 | Mapster or AutoMapper                            |
| Real-time Communication | SignalR                                          |
| Video & Audio           | WebRTC / Agora / LiveKit                         |
| Caching                 | Redis                                            |
| Payments                | Stripe / Paymob                                  |
| Email                   | MailKit / SendGrid                               |
| PDF Generation          | QuestPDF                                         |
| API Documentation       | Swagger / OpenAPI                                |

---

# Database Overview

The database is designed around the platform's primary business concepts.

| Entity / Table | Module    | Responsibility                            |
| -------------- | --------- | ----------------------------------------- |
| Users          | Users     | User accounts and profile information     |
| RefreshTokens  | Users     | Refresh token lifecycle                   |
| Skills         | Skills    | Available platform skills                 |
| UserSkills     | Skills    | Teaching and learning skill relationships |
| Availability   | Users     | Weekly availability                       |
| Proposals      | Proposals | Learning requests and proposal lifecycle  |
| Sessions       | Sessions  | Scheduled and completed sessions          |
| Wallets        | Wallet    | Current time-credit balances              |
| Transactions   | Wallet    | Auditable credit transactions             |
| Reviews        | Reviews   | Session feedback and ratings              |
| Badges         | Reviews   | User achievements                         |

The final schema and relationships will be refined during implementation.

---

# Development Roadmap

| Phase | Scope                                                                                 |
| ----- | ------------------------------------------------------------------------------------- |
| 1     | Solution structure, module boundaries, database setup, and initial EF Core migrations |
| 2     | Users, authentication, profiles, and skill management                                 |
| 3     | Discovery, search, and complementary skill matching                                   |
| 4     | Proposals, scheduling, and booking workflows                                          |
| 5     | Wallet, transactions, and payment integration                                         |
| 6     | Live sessions, reviews, ratings, and badges                                           |

The roadmap may change as requirements are refined.

---

# Getting Started

## Prerequisites

* .NET SDK compatible with the project target framework
* Microsoft SQL Server
* Git
* Visual Studio or Visual Studio Code

## Setup

1. Clone the repository.
2. Configure the database connection string.
3. Configure required external services.
4. Restore dependencies.
5. Apply EF Core migrations.
6. Build the solution.
7. Run the Web API.
8. Open Swagger/OpenAPI documentation.

Detailed environment variables and installation commands will be added as implementation progresses.

---

# Contributing

SkillSwap is developed collaboratively.

Contributors should follow the team's development workflow:

1. Create or select an issue.
2. Create a dedicated branch.
3. Implement the requested module/feature.
4. Keep changes within the appropriate module boundary.
5. Create focused commits.
6. Push the branch.
7. Open a Pull Request against the main branch.
8. Address review feedback.
9. Merge after approval.

Changes should preserve:

* Clean Architecture dependency rules.
* Module boundaries.
* Domain encapsulation.
* Thin controllers.
* Separation between commands and queries.
* Testability.
* Clear and descriptive commits.

---

# Architectural Rules

The following rules are mandatory for the project.

### Rule 1 — Domain Independence

```text
Domain
  ❌ EF Core
  ❌ ASP.NET Core
  ❌ Infrastructure
  ❌ External APIs
```

The Domain must remain framework-independent.

### Rule 2 — Module Ownership

Every business concept belongs to one module.

```text
Users       → User-related business rules
Skills      → Skill-related business rules
Discovery   → Matching and discovery
Proposals   → Proposal lifecycle
Sessions    → Session lifecycle
Wallet      → Time credits and transactions
Reviews     → Reviews and badges
```

### Rule 3 — No Global Business Folders

Avoid structures such as:

```text
Domain/
├── Entities/
├── Services/
├── ValueObjects/
└── Enums/
```

Prefer:

```text
Domain/
├── Users/
├── Skills/
├── Discovery/
├── Proposals/
├── Sessions/
├── Wallet/
└── Reviews/
```

### Rule 4 — Application Follows Domain Modules

The Application layer must mirror the module boundaries:

```text
Application/
├── Users/
├── Skills/
├── Discovery/
├── Proposals/
├── Sessions/
├── Wallet/
└── Reviews/
```

### Rule 5 — Controllers Stay Thin

Controllers should not contain business logic.

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Command / Query
     │
     ▼
Handler
     │
     ▼
Domain
```

### Rule 6 — Infrastructure Implements Contracts

Infrastructure provides implementations for abstractions required by the application.

```text
Application
    │
    ├── IRepository
    ├── IEmailService
    ├── IPaymentService
    └── IVideoService
             ▲
             │
Infrastructure
    ├── EfRepository
    ├── MailKitEmailService
    ├── StripePaymentService
    └── LiveKitVideoService
```

### Rule 7 — Protect Aggregate Boundaries

Aggregates should control modifications to their internal state.

Other modules should not directly modify another module's aggregate.

### Rule 8 — Database Configuration Follows Modules

EF Core configurations should also be organized by module:

```text
Persistence/
└── Configurations/
    ├── Users/
    ├── Skills/
    ├── Discovery/
    ├── Proposals/
    ├── Sessions/
    ├── Wallet/
    └── Reviews/
```

---

# Final Architecture

The complete solution should conceptually look like this:

```text
SkillSwap
│
├── SkillSwap.Domain
│   │
│   ├── Users
│   ├── Skills
│   ├── Discovery
│   ├── Proposals
│   ├── Sessions
│   ├── Wallet
│   └── Reviews
│
├── SkillSwap.Application
│   │
│   ├── Users
│   ├── Skills
│   ├── Discovery
│   ├── Proposals
│   ├── Sessions
│   ├── Wallet
│   └── Reviews
│
├── SkillSwap.Infrastructure
│   │
│   ├── Persistence
│   │   ├── Configurations
│   │   │   ├── Users
│   │   │   ├── Skills
│   │   │   ├── Discovery
│   │   │   ├── Proposals
│   │   │   ├── Sessions
│   │   │   ├── Wallet
│   │   │   └── Reviews
│   │   │
│   │   └── Migrations
│   │
│   ├── Repositories
│   ├── Identity
│   ├── Authentication
│   ├── Caching
│   ├── Email
│   ├── Payments
│   ├── Video
│   ├── Realtime
│   ├── Pdf
│   └── DependencyInjection
│
└── SkillSwap.Presentation
    │
    ├── Controllers
    │   ├── Users
    │   ├── Skills
    │   ├── Discovery
    │   ├── Proposals
    │   ├── Sessions
    │   ├── Wallet
    │   └── Reviews
    │
    ├── Middleware
    ├── Authentication
    ├── Hubs
    ├── Filters
    ├── Configuration
    └── Program.cs
```

**SkillSwap — Share your skills. Exchange your time. Grow together.**
