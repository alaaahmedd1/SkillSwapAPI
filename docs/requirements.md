# SkillSwap — Project Context & Requirements

## 1. Project Overview

### 1.1 Project Description

SkillSwap is a Peer-to-Peer Skill Exchange platform that enables users to teach and learn skills from each other without traditional courses or direct monetary payments.

The platform operates using a virtual time-based currency called **Time Credits**, where users exchange knowledge and skills on an hour-for-hour basis.

The core principle is:

**One hour of teaching earns one hour of learning credit.**

Users can share skills they already possess, discover other users who offer skills they want to learn, schedule exchange sessions, and earn or spend time credits through completed sessions.

### 1.2 Core Business Concept

The platform consists of the following core concepts:

* **Skill Exchange:** Users teach skills they know and learn skills they want to acquire.
* **Time Wallet:** Each user has a wallet containing virtual time credits.
* **Time-Based Economy:** Teaching for one hour earns one time credit, while learning for one hour consumes one time credit.
* **Smart Peer Matching:** The system identifies users with complementary teaching and learning interests.
* **Paid Top-Ups:** Users can purchase additional time credits through supported payment gateways.

### 1.3 Example Workflow

1. Ahmed offers C# as a skill he can teach.
2. Nesreen wants to learn C# and offers English in exchange.
3. The system identifies their complementary skills and may suggest them to each other.
4. They agree on a session and schedule a booking.
5. After the session is successfully completed, the system transfers the corresponding time credits between their wallets.
6. Each participant can submit a review and award a recognition badge.

Users who do not have time to teach can purchase additional time credits through available payment packages.

### 1.4 Project Objectives

* Enable peer-to-peer knowledge sharing.
* Provide a reliable time-credit economy.
* Facilitate skill discovery and complementary peer matching.
* Support scheduling, booking, and interactive online learning.
* Maintain transparent transaction histories and wallet balances.
* Build a reputation system based on reviews and badges.

---

## 2. Users and Core Domain Concepts

### 2.1 User Types

The platform supports the following user access scenarios:

* **Guest:** An unauthenticated visitor who can browse permitted public listings and profiles.
* **Registered User:** An authenticated user who can manage a profile, offer and request skills, book sessions, and use the time wallet.
* **Session Participant:** A registered user participating in an accepted skill exchange session.

A registered user can act as both a teacher and a learner. These are roles within an exchange, not separate account types.

### 2.2 Core Domain Concepts

| Concept        | Description                                               |
| -------------- | --------------------------------------------------------- |
| User           | An account that participates in the platform.             |
| User Profile   | Public and personal profile information and preferences.  |
| Skill          | A skill that users can offer or want to learn.            |
| Availability   | Weekly schedules and available session time slots.        |
| Proposal       | A request to arrange a skill exchange session.            |
| Booking        | A confirmed session scheduled between participants.       |
| Session        | An actual teaching and learning interaction.              |
| Time Wallet    | A user's virtual time-credit balance.                     |
| Transaction    | A recorded movement of time credits or purchased credits. |
| Review         | Feedback submitted after a completed session.             |
| Badge          | Recognition awarded to a user by another user.            |
| Top-Up Package | A purchasable package of virtual time credits.            |

---

# 3. Functional Requirements and Use Cases

## Module 1: User & Authentication (IAM)

**Purpose:** Manage account registration, authentication, identity verification, password recovery, and session security.

### UC-01: Register New Account

A new user can create an account using:

* Email and password.
* Social authentication through Google or Apple.

The system must validate registration data and prevent duplicate accounts according to the configured identity rules.

### UC-02: Authenticate & Login

A registered user can authenticate using supported login methods.

Upon successful authentication, the system issues the required authentication tokens, including JWT access and refresh tokens.

### UC-03: Request Password Reset

A user can request password recovery by submitting their registered email address.

The system sends a password-reset OTP or link through the configured email provider.

### UC-04: Verify OTP Code

A user submits the verification code received through email.

The system validates the code and determines whether it is valid for the requested verification operation.

The OTP length must be configurable as either 4 or 6 digits according to the final authentication configuration.

### UC-05: Reset Password

A user with successful password-reset verification can set a new password.

The system validates the new password and completes the reset process.

### UC-06: Guest Browsing

Unauthenticated visitors can explore permitted public listings and public user profiles.

Guest access must not expose private profile information, wallet data, personal transactions, or protected operations.

### UC-07: Logout & Token Revocation

An authenticated user can log out.

The system invalidates or revokes the applicable refresh token or active session so that the revoked session cannot continue obtaining new access tokens.

---

## Module 2: User Profile & Preferences

**Purpose:** Manage user information, skill inventory, weekly availability, and exchange preferences.

### UC-08: Get Own Profile

An authenticated user can retrieve their own profile, including:

* Display name and profile information.
* Skills offered and skills wanted.
* Relevant user statistics, including earned and spent hours.
* Earned badges and profile-related information.

Private profile information must only be accessible to the account owner or authorized operations.

### UC-09: Update Profile Information

A user can update their profile information, including:

* Bio.
* Display name.
* Professional or personal title.
* Country.
* Timezone.

The system validates submitted data and applies the user's updated profile information.

### UC-10: Manage Skill Inventory

A user can manage two skill categories:

1. **Skills Offered (Teaches):** Skills the user can teach to others.
2. **Skills Wanted (Wants to Learn):** Skills the user wants to acquire.

For each applicable skill entry, the user can add, update, or remove information such as skill level and required or offered credits.

The system maintains the association between users and their skills.

### UC-11: Set Weekly Swap Schedule

A user can define their recurring weekly availability.

Supported days:

* Sunday
* Monday
* Tuesday
* Wednesday
* Thursday
* Friday
* Saturday

Supported time periods:

* Morning
* Afternoon
* Evening

Availability must account for the user's configured timezone when determining actual dates and times.

### UC-12: Configure Exchange Rules

A user can configure exchange preferences, including:

* Instant Swaps.
* Online Only Sessions.
* Auto-match Starter Requests.

The system stores these preferences and applies them to relevant discovery, matching, and booking workflows.

---

## Module 3: Discovery, Matching & Search

**Purpose:** Help users discover other users, find complementary skills, and explore public profiles.

### UC-13: Discover Peers / Feed Listing

Users can browse active user listings through a paginated feed.

Supported filtering criteria include:

* Skill.
* Country.
* Gender, subject to applicable privacy and product rules.
* Online status.

The system supports pagination and filtering without exposing restricted user information.

### UC-14: Smart Peer Matching

The system identifies potential exchange partners based on complementary skill interests.

The primary matching condition is:

* User A offers a skill that User B wants to learn.
* User B offers a skill that User A wants to learn.

The matching functionality retrieves or recommends users who satisfy the configured matching criteria.

Matching may consider additional preferences such as availability, exchange rules, and relevant user preferences.

The initial matching functionality is algorithmic. AI-based matching can be introduced as an enhancement without changing the core business requirements.

### UC-15: View Peer Public Profile

A user or permitted guest can view another user's public profile.

The public profile may include:

* Bio and display information.
* Skills offered.
* Relevant availability.
* Rating and peer reviews.
* Received badges.

The system must distinguish public information from private account and wallet information.

---

## Module 4: Proposals & Session Booking

**Purpose:** Manage availability checks, exchange proposals, acceptance or rejection, and booking cancellation.

### UC-16: Fetch User Available Time Slots

A user can retrieve available dates and time slots for a target peer.

The system considers the target user's weekly availability and existing scheduled bookings when determining available slots.

### UC-17: Create & Send Proposal

An authenticated user can send a proposal to another user.

A proposal includes:

* Target participant.
* Skill to be taught.
* Skill to be learned.
* Proposed date.
* Start time.
* Session duration.

Supported durations:

* 30 minutes.
* 1 hour.
* 2 hours.

The system validates the proposal against applicable skill, availability, and booking rules.

### UC-18: List User Proposals

A user can retrieve incoming and outgoing proposals.

Supported proposal status filters:

* Pending.
* Accepted.
* Declined.
* Cancelled.

The system returns only proposals accessible to the authenticated user.

### UC-19: Respond to Proposal (Accept / Reject)

The target user can accept or decline an incoming proposal.

The system validates that the proposal is in a state that permits the requested action.

Acceptance confirms the proposed booking subject to the applicable availability and booking rules.

### UC-20: Cancel Proposal / Booking

Either participant can cancel an eligible proposal or scheduled booking before the session starts.

The system validates the cancellation request against the proposal or booking's current state and applicable cancellation rules.

Cancellation must update the relevant status and prevent invalid subsequent transitions.

---

## Module 5: Live Interactive Session

**Purpose:** Provide online teaching sessions through real-time audio/video, chat, and collaborative tools.

### UC-21: Initialize Session Room

The system initializes a session room for an eligible booking.

It generates or retrieves the required room details and provider access tokens.

Potential providers include:

* Agora.
* LiveKit.
* Daily.co.

The selected provider must support the required authentication and session-access mechanisms.

### UC-22: Join Session

Both participants can join the scheduled session and connect to the supported audio/video stream.

The system validates participant identity and session access permissions.

### UC-23: Send In-Session Chat Messages

Participants can exchange real-time messages during an active session.

SignalR or the selected real-time communication mechanism can support chat delivery and related session events.

### UC-24: Interactive Whiteboard Collaboration

Participants can collaborate on a shared interactive whiteboard during teaching sessions.

The system supports real-time synchronization of whiteboard changes between authorized participants.

The whiteboard implementation and its storage or synchronization mechanism are determined during technical implementation.

### UC-25: End Session

An eligible participant can initiate session termination according to the session rules.

The system:

* Terminates or finalizes the active session.
* Tracks the actual elapsed session duration.
* Records the session completion information.
* Triggers the applicable time-credit transfer process.

Credit transfers must be processed through the Time Wallet module using the rules defined in this document.

Session completion and wallet updates must avoid duplicate credit transfers.

---

## Module 6: Time Wallet & Transactions

**Purpose:** Manage time-credit balances, transaction records, paid credit packages, and receipts.

### UC-26: Get Wallet Overview

An authenticated user can retrieve their wallet overview, including:

* Current available balance in hours and minutes.
* Total hours earned.
* Total hours spent.

Wallet values must reflect completed and successfully recorded transactions.

### UC-27: Get Transaction History

A user can retrieve their own paginated transaction history.

The history includes relevant information such as:

* Transaction type (Earned or Spent).
* Credit amount.
* Transaction date.
* Related session or purchase reference.
* Transaction status, where applicable.

The system must not allow users to access another user's private transaction history.

### UC-28: Purchase Time Credits (Top-Up)

A user can select an available time-credit package and initiate checkout.

Example packages:

* 2 hours.
* 5 hours.
* 12 hours.

The system creates the applicable purchase or payment record and initiates the configured payment provider's checkout flow.

The available packages, prices, and supported currencies are configurable business data.

### UC-29: Process Payment Webhook

The system receives payment gateway callbacks or webhooks from the configured payment provider.

Potential providers include Stripe and Paymob.

Upon verified payment success, the system grants the corresponding purchased time credits to the user's wallet.

Payment processing must validate the webhook's authenticity and prevent duplicate credit grants for the same successful payment.

A client-side checkout success message alone must not be treated as proof of payment.

### UC-30: Download Transaction PDF Receipt

A user can generate and download a PDF receipt for an eligible completed exchange or top-up purchase.

The receipt contains the relevant transaction details and reference information.

The system restricts access to receipts according to the transaction owner's permissions.

---

## Module 7: Rating, Reviews & Badges

**Purpose:** Maintain user reputation and peer recognition based on completed skill exchange sessions.

### UC-31: Submit Session Review

After an eligible session, a participant can submit a review for the other participant.

A review contains:

* Rating from 1 to 5 stars.
* Optional or required text feedback according to the final validation rules.
* Reference to the completed session and reviewed user.

The system validates review eligibility and prevents unauthorized or duplicate reviews according to the configured review policy.

### UC-32: Gift Badge to Peer

A user can award a recognition badge to another user.

Example badges:

* Best Tutor.
* Super Patient.
* Problem Solver.

The system records the badge, its recipient, the awarding user, and any applicable session reference.

The supported badge catalog and awarding rules are configurable.

### UC-33: Get User Reviews & Badges

A user or permitted guest can retrieve a user's public reputation information.

The response includes applicable reviews, ratings, and received badges.

The system applies pagination or filtering where required and respects public profile access rules.

---

# 4. Team Task Division

The following division defines the initial ownership of the modules and use cases among the four developers.

Module ownership does not change the shared project architecture or allow one module to bypass another module's business rules.

## Developer 1: Auth & User Profile Specialist

### Responsibilities

* Identity management and account security.
* Registration, authentication, password reset, and OTP verification.
* Access and refresh token lifecycle.
* User profile management.
* Skill inventory management.
* Weekly availability configuration.
* User exchange preferences.

### Assigned Use Cases

| Use Cases      | Scope                                                                         |
| -------------- | ----------------------------------------------------------------------------- |
| UC-01 to UC-07 | Registration, authentication, password recovery, guest access, and logout.    |
| UC-08 to UC-12 | Profile management, skill inventory, weekly availability, and exchange rules. |

### Primary Technologies

* ASP.NET Core Identity.
* JWT access and refresh tokens.
* FluentValidation.
* MailKit or SendGrid for email and OTP delivery.
* Entity Framework Core.

## Developer 2: Discovery, Matching & Reputation Specialist

### Responsibilities

* Peer discovery and search.
* Multi-criteria filtering and pagination.
* Smart peer matching.
* Public peer profile queries.
* Session reviews and ratings.
* Badge management and reputation queries.

### Assigned Use Cases

| Use Cases      | Scope                                               |
| -------------- | --------------------------------------------------- |
| UC-13          | Discover peers and filter feed listings.            |
| UC-14          | Smart peer matching.                                |
| UC-15          | Public peer profile.                                |
| UC-31 to UC-33 | Reviews, ratings, badges, and reputation retrieval. |

### Primary Technologies

* LINQ and Entity Framework Core query optimization.
* Database indexes for search performance.
* Redis for caching popular skills, categories, and active peer feeds.
* MediatR and CQRS for separating read queries from write operations.

## Developer 3: Booking, Proposals & Real-Time Session Specialist

### Responsibilities

* Availability and scheduling logic.
* Proposal and booking lifecycle.
* Session state management.
* Real-time notifications and chat.
* Video/audio provider integration.
* Collaborative whiteboard synchronization.

### Assigned Use Cases

| Use Cases      | Scope                                                           |
| -------------- | --------------------------------------------------------------- |
| UC-16          | Retrieve available time slots.                                  |
| UC-17 to UC-20 | Create, list, accept, reject, and cancel proposals or bookings. |
| UC-21 to UC-25 | Initialize, join, interact in, and end sessions.                |

### Primary Technologies

* SignalR for real-time notifications and in-session chat.
* Agora or LiveKit SDK for real-time communication and access-token generation.
* Stateless or Entity Framework Core for managing session and proposal workflows.

## Developer 4: Time Wallet, Transactions & Payment Specialist

### Responsibilities

* Time wallet balances.
* Credit ledger and transaction history.
* Credit transfers after completed sessions.
* Paid time-credit packages.
* Payment gateway integration and webhook processing.
* PDF receipt generation.

### Assigned Use Cases

| Use Cases      | Scope                                    |
| -------------- | ---------------------------------------- |
| UC-26 to UC-27 | Wallet overview and transaction history. |
| UC-28          | Purchase time-credit packages.           |
| UC-29          | Payment gateway webhook processing.      |
| UC-30          | Transaction PDF receipts.                |

### Primary Technologies

* Stripe.net or Paymob SDK.
* QuestPDF or DinkToPdf for PDF generation.
* Entity Framework Core database transactions and `IDbContextTransaction` for applicable atomic wallet operations.

---

# 5. Shared Technology Stack

The following technologies define the project's shared implementation stack. The architectural structure and project dependencies are defined separately in `README.md`.

| Category                | Technology                                                  |
| ----------------------- | ----------------------------------------------------------- |
| Framework               | ASP.NET Core Web API, .NET 9                                |
| Language                | C#                                                          |
| Database                | Microsoft SQL Server                                        |
| ORM                     | Entity Framework Core                                       |
| Architecture            | Clean Architecture, following the structure in README.md    |
| Application Pattern     | CQRS using MediatR                                          |
| Object Mapping          | AutoMapper or Mapster, subject to the project configuration |
| Validation              | FluentValidation                                            |
| Authentication          | ASP.NET Core Identity and JWT                               |
| API Documentation       | Swagger / OpenAPI                                           |
| Real-Time Communication | SignalR                                                     |
| Caching                 | Redis, where applicable                                     |
| Payments                | Stripe or Paymob                                            |
| Video/Audio             | Agora, LiveKit, or another approved provider                |
| PDF Generation          | QuestPDF or another approved library                        |

All developers must follow the same framework version, shared conventions, and architectural boundaries defined for the project.

Technology choices that have not yet been finalized must be confirmed before introducing provider-specific implementations.

---

# 6. Cross-Module Business Rules

The following rules apply across the relevant modules and use cases.

## 6.1 Time Credit Economy

* One hour of eligible teaching earns one time credit.
* One hour of eligible learning consumes one time credit.
* Thirty minutes corresponds to half an hour of time credit.
* Time-credit calculations must support fractional hours and minute-based session durations.
* The wallet balance must be derived from recorded credit movements and remain consistent with the transaction history.
* A wallet must not be credited multiple times for the same completed session or successful payment.

The exact rounding, partial-session, and dispute policies must be explicitly defined before implementing those scenarios.

## 6.2 Booking and Session Integrity

* Only eligible participants can access or modify their proposals, bookings, and sessions.
* Booking acceptance must account for availability and conflicting scheduled sessions.
* Proposal and booking operations must validate the current state before applying a transition.
* Session completion must be linked to the correct booking and participants.
* Wallet credit transfers must be associated with the corresponding completed session.

## 6.3 Security and Authorization

* Protected operations require valid authentication.
* Users can modify only their own private profile, wallet, and applicable records.
* Public profile access must not expose sensitive account information.
* Payment webhooks must be authenticated or cryptographically verified using the provider's supported mechanism.
* Session room access tokens must be issued only to authorized participants.

## 6.4 Data Consistency

* Financial and time-credit operations must preserve transaction consistency.
* Database transactions must be used where multiple related database changes must succeed or fail together.
* Payment and session completion processing must be designed to tolerate retries without duplicate credit transfers.
* Changes in one module must respect the business rules and ownership of related modules.

---

# 7. Scope and Implementation Notes

This document describes the product concept, functional requirements, use cases, module ownership, and shared technology requirements.

It does not prescribe the complete technical architecture or the internal project and folder structure.

The architecture, layer responsibilities, project references, module organization, and dependency rules are defined in `README.md`.

The following details remain subject to explicit product or technical decisions before implementation:

* Final video/audio provider.
* Final payment gateway and supported currencies.
* Time-credit rounding and partial-session rules.
* Booking cancellation deadlines and refund or credit-return policies.
* Review eligibility and duplicate-review policy.
* Exact matching weights and ranking criteria.
* Final OTP expiration, retry, and lockout policies.
* Final pricing and available top-up packages.
* Dispute handling and session no-show policies.

Until these decisions are confirmed, developers and AI agents must not invent business rules or treat optional examples in this document as finalized requirements.
