# Courier Module

Courier is the communication and notification module.

It tracks email messages and sends queued emails.

Courier stores email records in MongoDB.

It uses Redis for message brokering and queue work.

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

## 4. Template Management

Templates are stored in one MongoDB collection:

```text
templates
```

The main entity is:

```csharp
Template
```

For now, it has:
- `Type`
- `EmailTranslations`

Future types can add their own translation lists.

Template endpoints:

```text
GET    /api/v1/templates
GET    /api/v1/templates/{id}
POST   /api/v1/templates
PUT    /api/v1/templates/{id}
DELETE /api/v1/templates/{id}
```

Email translation endpoints:

```text
POST   /api/v1/templates/{id}/email-translations
PUT    /api/v1/templates/{id}/email-translations/{language}
DELETE /api/v1/templates/{id}/translations/{language}
```

Only one email translation is allowed per language.

---

## 5. Email Write Flow

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

## 6. Email Delivery Flow

This is the normal async email flow.

Another module publishes an email request to Redis.

Courier reads that request, creates an `Email` document in MongoDB, then a worker sends it later.

```text
Redis stream
  -> EmailRequestConsumer.ExecuteAsync(...)
    -> EmailRequestConsumer.ProcessEntryAsync(...)
      -> EmailOutboxService.QueueAsync(...)
        -> TemplateRepository.GetByKeyAsync(...)
        -> SimpleEmailTemplateRenderer.Render(...)
        -> Email.Create(...)
        -> EmailRepository.AddAsync(...)
        -> ICourierLogger.LogAuditAsync(...)

MongoDB pending email
  -> EmailDeliveryWorker.ExecuteAsync(...)
    -> EmailOutboxService.ProcessNextPendingAsync(...)
      -> EmailRepository.ClaimNextPendingAsync(...)
      -> IEmailSender.SendAsync(...)
        -> Email.MarkAsSent(...)
        -> EmailRepository.UpdateAsync(...)
        -> ICourierLogger.LogAuditAsync(...)
```

### Step 1: Read From Redis

`EmailRequestConsumer` is a background service.

It runs from:

```text
Courier.Infrastructure/BackgroundServices/EmailRequestConsumer.cs
```

Main method:

```csharp
ExecuteAsync(CancellationToken stoppingToken)
```

It reads messages from this Redis stream:

```csharp
CourierConst.Redis.EmailRequestsStream
```

Each Redis message must have the field:

```csharp
CourierConst.Redis.EventFieldName
```

That field contains JSON for:

```csharp
IntegrationEvent<EmailQueueRequest>
```

`ProcessEntryAsync(...)` deserializes the envelope.

Then it validates the event name and version.

If the JSON is invalid, Courier logs a system error and acknowledges the message.

This prevents the same bad message from blocking the stream.

### Step 2: Queue The Email

After Redis data is parsed, the consumer calls:

```csharp
EmailOutboxService.QueueAsync(...)
```

This method:
- Loads the template with `TemplateRepository.GetByKeyAsync(...)`.
- Verifies the template type is `TemplateType.Email`.
- Finds the requested language translation.
- Builds template values.
- Adds the automatic `today` value.
- Renders the subject.
- Renders the body.
- Creates the `Email` entity with `Email.Create(...)`.
- Saves the email with `EmailRepository.AddAsync(...)`.
- Writes an audit log with `ICourierLogger.LogAuditAsync(...)`.

The saved email starts with:

```csharp
EmailStatus.Pending
```

This means the email is stored but not sent yet.

### Step 3: Render The Template

Template rendering happens in:

```text
Courier.Application/Services/SimpleEmailTemplateRenderer.cs
```

Main method:

```csharp
Render(string template, IReadOnlyDictionary<string, string> values, bool htmlEncodeValues = false)
```

It replaces placeholders like:

```text
{{user.name}}
{{organization.name}}
{{today}}
```

If a placeholder value is missing, rendering fails.

The email is not queued.

For HTML templates, values are encoded before replacement.

This helps avoid injecting unsafe HTML from placeholder values.

### Step 4: Claim A Pending Email

`EmailDeliveryWorker` is another background service.

It runs from:

```text
Courier.Infrastructure/BackgroundServices/EmailDeliveryWorker.cs
```

Main method:

```csharp
ExecuteAsync(CancellationToken stoppingToken)
```

It calls:

```csharp
EmailOutboxService.ProcessNextPendingAsync(...)
```

That method calls:

```csharp
EmailRepository.ClaimNextPendingAsync(...)
```

The repository finds the oldest email where:

```csharp
Status == Pending
NextAttemptAt <= DateTime.UtcNow
```

Then it changes the status to:

```csharp
EmailStatus.Processing
```

This claim step helps avoid duplicate sending when more than one worker is running.

### Step 5: Send The Email

After the email is claimed, `EmailOutboxService` calls:

```csharp
IEmailSender.SendAsync(...)
```

`IEmailSender` is the vendor boundary.

Today Courier uses:

```text
Courier.Infrastructure/EmailSenders/NoopEmailSender.cs
```

`NoopEmailSender` does not send a real email.

It only returns success.

Later, this can be replaced with another sender.

Examples:
- SMTP sender.
- SendGrid sender.
- Amazon SES sender.
- Azure Communication Services sender.

Only the `IEmailSender` implementation should change.

The outbox flow should stay the same.

### Step 6: Mark Success Or Failure

If sending succeeds:

```csharp
Email.MarkAsSent()
```

The email becomes:

```csharp
EmailStatus.Sent
```

`SentAt` is set.

`NextAttemptAt` is cleared.

Courier updates MongoDB with:

```csharp
EmailRepository.UpdateAsync(...)
```

Then it writes an audit log with:

```csharp
ICourierLogger.LogAuditAsync(...)
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

`NextAttemptAt` is moved into the future.

If retry limit is reached, it becomes:

```csharp
EmailStatus.Failed
```

Courier writes:
- An audit log with `ICourierLogger.LogAuditAsync(...)`.
- A system log with `ICourierLogger.LogSystemAsync(...)`.

The retry limit comes from:

```csharp
CourierParam.EmailDelivery.MaxRetries
```

If that parameter cannot be loaded, Courier uses:

```csharp
CourierConst.Worker.DefaultMaxRetries
```

### Important Classes

| Class | Responsibility |
| :--- | :--- |
| `EmailRequestConsumer` | Reads email requests from Redis. |
| `EmailOutboxService` | Queues and processes pending emails. |
| `TemplateRepository` | Loads templates from MongoDB. |
| `SimpleEmailTemplateRenderer` | Replaces template placeholders. |
| `EmailRepository` | Saves, claims, and updates email documents. |
| `EmailDeliveryWorker` | Background worker that sends pending emails. |
| `IEmailSender` | Abstraction for the email vendor. |
| `NoopEmailSender` | Current fake sender implementation. |
| `ICourierLogger` | Publishes audit and system log events. |
| `Email` | Domain entity that owns status changes and retry behavior. |

---

## 6. Main Entity

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

## 7. Design Rules

- Keep email behavior in the `Email` entity.
- Use `EmailService` for orchestration.
- Use `EmailValidator` for request validation.
- Use `EmailRepository` for MongoDB access.
- Use `EmailOutboxService` for async queue and delivery work.
- Use `IEmailSender` as the email vendor boundary.
- Do not put vendor-specific email code inside services.
- Use DTOs for API responses.
- Use `EmailLiteDto` for lists.
- Use `EmailDto` for details.
- Do not expose heavy message bodies in list endpoints.
