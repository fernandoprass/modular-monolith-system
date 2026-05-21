# Core Modular System
This repository documents the architecture for a distributed system designed for scalability, security, and high availability. The ecosystem is composed of three interconnected systems designed to operate in an event-driven architecture.

## System Architecture Status
|**System**  | **Role** |**Technology Stack**|**Status**|
|--|--|--|--|
| **Shared** | Common utilities, models, and interfaces | .NET Standard | Ongoing |
| **Sentinel** | Centralized Logging & Auditing | .NET, MongoDB, Redis | Planned |
| **IAM** | Identity & Access Management | .NET, PostgreSQL, Redis | Ongoing |
| **Courier** | Communication & Notification Hub | .NET, MongoDB, Redis | Planned |

> **Current Focus:** Engineering efforts are currently dedicated **exclusively** to the **IAM module**. The specifications below for CommHub and Sentinel define the architectural blueprint for future expansion.

----------

## 1. Shared (common classes and interfaces)
_Common codebase for shared models, utilities, and interfaces across all systems._
- **Objective**: Promote code reuse and maintain consistency across modules.

## 2. Sentinel (Centralized Logging & Auditing)
_The system for distributed tracing, log aggregation, and long-term security auditing._
- **Objective**: Provide a single source of truth for system observability and compliance.
- **Technology**: .NET + MongoDB (leveraging TTL indexes for automatic log pruning).
- **Planned Features**:
    - **Distributed Tracing**: Integration with OpenTelemetry to track request flows across IAM and CommHub. 
    - **Automated Retention Policies**: Implementing MongoDB TTL indexes to manage storage costs based on compliance requirements.
    - **Full-Text Search**: High-performance indexing for rapid debugging and log retrieval.
    - **Anomaly Alerting**: Logic to detect patterns indicative of security threats (e.g., rapid failed logins, abnormal API access).
    - **Audit Trail**: Immutable storage of administrative actions for compliance (GDPR/SOC2).

## 3. IAM (Identity & Access Management)
_Core system for multi-tenant authentication, authorization, and user lifecycle management._
-   **Objective**: Centralize user identity and tenant management.
-   **Key Status**: Development is underway using Clean Architecture.

## 4. Courier  (Communication & Notification Hub)
_A centralized engine to manage all outbound and inbound communication flows._
- **Objective**: Decouple communication logic (email, push, SMS) from business services.
- **Technology**: .NET + MongoDB (Schema-less storage is ideal for varying message payloads).
- **Planned Features**:
    - **Multi-Channel Support**: Unified API for SMTP (Email), SMS (Twilio/AWS SNS), and Push Notifications (FCM/APNs).
    - **Templating Engine**: Server-side rendering for email/notification bodies (Handlebars/Razor).
    - **Delivery Tracking**: Webhook integration to track message delivery, open rates, and click-through metrics.
    - **Conversation Threading**: Ability to group messages by context/user for chat-like UI support.
    - **Message Queuing**: Priority queues for time-sensitive alerts vs. marketing batches.

----------

## Event-Driven Communication (Redis)
All systems communicate asynchronously via **Redis**. This decoupled architecture ensures that the IAM system can publish events (e.g., `UserCreated`, `PasswordResetRequested`) without direct knowledge of who consumes them.

1. **IAM** publishes an event (e.g., `UserRegistered`).
2. **Courier** consumes it to send a "Welcome Email".
3. **Sentinel** consumes it to create a permanent audit record of the new account.