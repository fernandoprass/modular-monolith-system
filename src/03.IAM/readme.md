# IAM System (Identity & Access Management)

A robust, multi-tenant Identity and Access Management system built with **Clean Architecture** principles, focusing on security, scalability, and maintainability.

## Architectural Overview

The project is divided into four main layers to ensure separation of concerns and testability:

- **IAM.Domain**: The core layer. Contains Entities (User, Customer), Repository Interfaces, and Business Logic. It is independent of any external frameworks.
- **IAM.Application**: Orchestration layer. Contains **Contracts** (Interfaces & DTOs), Application Services, and Validators. 
- **IAM.Infrastructure**: Implementation of external concerns such as **PostgreSQL** persistence via Entity Framework Core and specialized security adapters.
- **IAM.API**: The entry point. Handles HTTP requests, middleware, and dependency injection configuration.

## Key IAM Features

A professional-grade IAM must handle more than just logins. Our roadmap includes:

- **Multi-tenant Isolation**: Secure data separation using a `slug` (unique tenant identifier) to route requests and isolate customer data.
- **Identity Provisioning**: Complete lifecycle management for Users and Customers (Tenants), from registration to deactivation.    
- **Authentication Flow**:  
    - Secure login using **Slug + Email + Password**.  
    - Password hashing using the **Argon2** algorithm.    
    - Account lockout mechanisms (`access_failed_count`) to prevent brute-force attacks.      
- **Authorization (RBAC/ABAC)**: Role-Based or Attribute-Based Access Control to manage granular permissions within each tenant.
- **Email Verification & Recovery**: Integrated workflows for `email_verified_at` tracking and secure password reset tokens.   
- **Audit Logging**: Comprehensive tracking of security events, including `last_login_at` and administrative changes.   
- **Notification Pattern**: A non-exception-based error handling strategy using `Result<T>` and `Message` objects to communicate business failures gracefully.
    
## Technical Stack

- **Language**: C# / .NET 8+  
- **Database**: PostgreSQL (using `snake_case` naming conventions)
- **ORM**: Entity Framework Core  
- **Validation**: Fluent-style instance-based validators (DI-ready) 
- **Security**: Argon2 for Credential Hashing
    
## Project Structure & Naming

We follow a strict naming convention to ensure semantic clarity:

- **Contracts**: Located in the Application layer, these folders group Interfaces, Request DTOs, and Response DTOs.
- **Result Pattern**: All services return a `Result` object, avoiding the overhead of "Control Flow by Exception."
- **Suffixes**: Clear semantic distinction using `...Error`, `...Warning`, and `...Info` for business message