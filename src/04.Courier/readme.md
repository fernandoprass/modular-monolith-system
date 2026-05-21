# Courier Module

Courier is the communication and notification module.

It tracks email messages and prepares the module for future delivery workers.

Courier stores email records in MongoDB.

It uses Redis for future message brokering and queue work.

The module lives in:

```text
src/04.Courier
```

---

## 1. Purpose

Courier keeps communication history outside business modules.

Example:
- IAM needs to send a welcome email.
- IAM creates or publishes an email request.
- Courier stores the email record.
- A client can search email metadata later.
- A detail endpoint can load the full body when needed.

This keeps business modules decoupled from email storage and delivery details.

---

## 2. Project Structure

```text
src/04.Courier/
+-- Courier.API/
+-- Courier.Application/
+-- Courier.Domain/
+-- Courier.Infrastructure/
```

| Project | Responsibility |
| :--- | :--- |
| `Courier.API` | HTTP endpoints, startup, and request handling. |
| `Courier.Application` | Email services and validation. |
| `Courier.Domain` | Entities, DTOs, constants, mappers, and repository contracts. |
| `Courier.Infrastructure` | MongoDB context, repositories, and Redis client setup. |

---

## 3. Email Read Flow

```text
HTTP request
  -> EmailController
    -> EmailService
      -> IEmailRepository
        -> MongoDB
```

List endpoint:

```text
GET /api/v1/emails
```

It returns lightweight email metadata.

It does not return the email body.

Detail endpoint:

```text
GET /api/v1/emails/{id}
```

It returns the complete email log.

It includes the body.

---

## 4. Email Write Flow

```text
HTTP request
  -> EmailController
    -> EmailService
      -> Email.Create(...)
        -> IEmailRepository
          -> MongoDB
```

Create endpoint:

```text
POST /api/v1/emails
```

It stores a new email record with `Pending` status.

It returns the generated ID.

---

## 5. Main Entity

Courier owns:
- `Email`

Important fields:
- `OrganizationId`
- `UserId`
- `Module`
- `Feature`
- `TemplateKey`
- `Recipient`
- `Subject`
- `Body`
- `Status`
- `Timestamp`
- `SentAt`
- `NextAttemptAt`
- `RetryCount`
- `Attempts`

`Body` can be large.

Do not include it in list responses.

---

## 6. Design Rules

- Keep email behavior in the `Email` entity.
- Use `EmailService` for orchestration.
- Use `EmailValidator` for request validation.
- Use `EmailRepository` for MongoDB access.
- Use DTOs for API responses.
- Use `EmailLiteDto` for lists.
- Use `EmailDto` for details.
- Do not expose heavy message bodies in list endpoints.
