# Courier Module

Courier is the communication module.

It stores email and notification records in MongoDB.

It uses Redis streams to receive user message requests from other modules.

The module lives in:

```text
back/src/04.Courier
```

---

## 1. Purpose

Courier keeps communication history outside business modules.

Example:
- IAM publishes a user message request.
- Courier reads the request.
- Courier loads the template.
- Courier creates an email, a notification, or both.
- The email worker sends pending emails later.
- The UI can read notification counts and notification details.

This keeps business modules decoupled from delivery and storage details.

---

## 2. Project Structure

```text
back/src/04.Courier/
+-- Courier.API/
+-- Courier.Application/
+-- Courier.Domain/
+-- Courier.Infrastructure/
```

| Project | Responsibility |
| :--- | :--- |
| `Courier.API` | HTTP endpoints, startup, and request handling. |
| `Courier.Application` | Message, email, notification, and template services. |
| `Courier.Domain` | Entities, DTOs, constants, mappers, and repository contracts. |
| `Courier.Infrastructure` | MongoDB context, repositories, Redis consumer, and email sender implementation. |

---

## 3. Templates

Templates are stored in MongoDB:

```text
templates
```

The main entity is:

```csharp
Template
```

Important fields:
- `Module`
- `Key`
- `IsAllowingOptOut`
- `Severity`
- `RetentionPolicy`
- `Translations`

Translations are grouped by language.

Each language can contain:
- `Email`
- `Notification`

If one channel is not used for a language, that channel is `null`.

Template endpoints:

```text
GET    /api/v1/templates
GET    /api/v1/templates/{id}
POST   /api/v1/templates
PUT    /api/v1/templates/{id}
DELETE /api/v1/templates/{id}
POST   /api/v1/templates/{id}/translations
PUT    /api/v1/templates/{id}/translations/{language}
DELETE /api/v1/templates/{id}/translations/{language}
```

Only one translation is allowed per language.

Languages use BCP 47 names, like `en` and `pt-BR`.

---

## 4. Message Request Flow

Other modules publish a `UserMessageEvent` to Redis.

Shared publishes the event to:

```csharp
SharedConst.Redis.CourierMessageRequestsStream
```

Courier consumes the same Redis stream through:

```csharp
CourierConst.Redis.MessageRequestsStream
```

Both constants point to:

```text
courier-message-requests
```

High-level flow:

```text
Source module
  -> IEventPublisher.PublishUserMessageEventAsync(...)
    -> Redis stream courier-message-requests
      -> CourierMessageRequestConsumer
        -> CourierMessageService.QueueAsync(...)
          -> TemplateRepository.GetByModuleAndKeyAsync(...)
          -> SimpleEmailTemplateRenderer.Render(...)
          -> EmailRepository.AddAsync(...) when email exists
          -> NotificationRepository.AddAsync(...) when notification exists
```

The Redis envelope uses:

```csharp
IntegrationEvent<UserMessageEvent>
```

Inside Courier, the consumer deserializes the payload into:

```csharp
CourierMessageRequest
```

---

## 5. Message Creation Rules

`CourierMessageService` loads the template by module and key.

Then it finds the requested language translation.

If the requested language is missing, it tries:

```csharp
SharedConst.System.DefaultLanguage
```

Then it creates channel records:
- If `translation.Email` exists, it creates an `Email`.
- If `translation.Notification` exists, it creates a `Notification`.
- If both exist, it creates both.
- If both are missing, the request fails.

`Recipient` is nullable on the message request.

It is required only when the selected template language has an email channel.

Template placeholders are rendered for:
- email subject
- email body
- notification title
- notification message
- notification action link

The renderer adds this automatic value:

```text
{{today}}
```

---

## 6. Email API

List endpoint:

```text
GET /api/v1/emails
```

Detail endpoint:

```text
GET /api/v1/emails/{id}
```

Create endpoint:

```text
POST /api/v1/emails
```

The list endpoint returns lightweight email metadata.

The detail endpoint returns the full email body.

The body can be large, so do not include it in list responses.

---

## 7. Notification API

List endpoint:

```text
GET /api/v1/notifications
```

Unread count endpoint:

```text
GET /api/v1/notifications/unread-count
```

Mark as read endpoint:

```text
PATCH /api/v1/notifications/{id}/read
```

Delete endpoint:

```text
DELETE /api/v1/notifications/{id}
```

Notifications are created by workers/services, not by a public POST endpoint.

---

## 8. Email Delivery Flow

Emails are created with:

```csharp
EmailStatus.Pending
```

The delivery worker sends pending emails later.

```text
MongoDB pending email
  -> EmailDeliveryWorker.ExecuteAsync(...)
    -> EmailOutboxService.ProcessNextPendingAsync(...)
      -> EmailRepository.ClaimNextPendingAsync(...)
      -> IEmailSender.SendAsync(...)
        -> Email.MarkAsSent(...)
        -> EmailRepository.UpdateAsync(...)
```

`EmailRepository.ClaimNextPendingAsync(...)` finds the oldest email where:

```csharp
Status == Pending
NextAttemptAt <= DateTime.UtcNow
```

Then it marks the email as:

```csharp
EmailStatus.Processing
```

This helps avoid duplicate sending when more than one worker is running.

Today Courier uses:

```text
Courier.Infrastructure/EmailSenders/NoopEmailSender.cs
```

`NoopEmailSender` does not send a real email.

It only returns success.

Later, replace only the `IEmailSender` implementation.

---

## 9. Retry Behavior

If sending succeeds:

```csharp
Email.MarkAsSent()
```

The email becomes:

```csharp
EmailStatus.Sent
```

If sending fails:

```csharp
Email.RecordFailure(...)
```

The email stores a `DeliveryAttempt`.

It increases `RetryCount`.

If retries remain, it goes back to:

```csharp
EmailStatus.Pending
```

If retry limit is reached, it becomes:

```csharp
EmailStatus.Failed
```

The retry limit comes from:

```csharp
CourierParam.EmailDelivery.MaxRetries
```

If that parameter cannot be loaded, Courier uses:

```csharp
CourierConst.Worker.DefaultMaxRetries
```

---

## 10. Important Classes

| Class | Responsibility |
| :--- | :--- |
| `CourierMessageRequestConsumer` | Reads user message requests from Redis. |
| `CourierMessageService` | Creates email and notification records from templates. |
| `EmailOutboxService` | Processes pending emails. |
| `TemplateRepository` | Loads templates from MongoDB. |
| `SimpleEmailTemplateRenderer` | Replaces template placeholders. |
| `EmailRepository` | Saves, claims, and updates email documents. |
| `NotificationRepository` | Saves, lists, counts, updates, and deletes notification documents. |
| `EmailDeliveryWorker` | Background worker that sends pending emails. |
| `IEmailSender` | Abstraction for the email vendor. |
| `NoopEmailSender` | Current fake sender implementation. |
| `ICourierLogger` | Publishes system log events. |
| `Email` | Domain entity that owns email status and retry behavior. |
| `Notification` | Domain entity that owns notification read status. |

---

## 11. Design Rules

- Keep email status behavior in the `Email` entity.
- Keep notification read behavior in the `Notification` entity.
- Use `CourierMessageService` to create email and notification records from templates.
- Use `EmailOutboxService` only for pending email delivery.
- Use `IEmailSender` as the email vendor boundary.
- Do not put vendor-specific email code inside services.
- Use DTOs for API responses.
- Use lightweight DTOs for list endpoints.
- Do not expose heavy message bodies in list endpoints.
