## Architecture & Outbox/Refund Flow

### High-Level Architecture

```
+-------------------+        +---------------------+        +-------------------+
|   API (ASP.NET)   | <----> |   Core (Domain)     | <----> | Infrastructure    |
|                   |        |  - Entities         |        |  - EF Core        |
| - Controllers     |        |  - Interfaces       |        |  - Repositories   |
| - Dependency      |        |  - Business Logic   |        |  - Bank Service   |
|   Injection       |        +---------------------+        |  - Outbox Worker  |
+-------------------+                                        +-------------------+
```

### Transactional Outbox & Refund Mechanism

1. **Payment Processing**
   - API receives payment request.
   - Core validates and processes payment.
   - If bank authorizes but DB save fails, an OutboxEvent of type `Refund` is created.

2. **Outbox Event**
   - OutboxEvent is stored in the DB (atomic with payment if possible).
   - Contains all info needed for a refund (paymentId, amount, currency, authorizationCode).

3. **Refund Processor (Background Worker)**
   - Periodically scans for unprocessed `Refund` events.
   - Calls the bank's refund endpoint using the event payload.
   - Marks event as processed or logs error for retry.

```
[API] ---> [Core: PaymentService] ---> [DB: Payment, OutboxEvent]
                                         |
                                         v
                              [Background: RefundProcessor]
                                         |
                                         v
                                [Bank: Refund Endpoint]
```

### Observability

- Uses `ILogger<T>` for structured logs in all critical flows (payment, refund, outbox processing).
- Logs include context (paymentId, eventId, etc.) for traceability.

### Testing

- Unit tests for all business logic, including outbox/refund scenarios.
- Integration tests for API endpoints and DB interactions.
- Helpers/factories for DRY, maintainable test code.

### Developer Experience

- `dev.sh` script for common dev tasks (test, run, Docker, etc.).
- Clear code comments and English documentation.
- Modular, extensible structure for easy onboarding.

---

For more details, see code comments and test helpers. For advanced observability or CI/CD, see the optional TODOs in the project summary.
