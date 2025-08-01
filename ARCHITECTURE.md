## Architecture

### High-Level Architecture

```
+-------------------+        +---------------------+        +-------------------+
|   API (ASP.NET)   | <----> |   Core (Domain)     | <----> | Infrastructure    |
|                   |        |  - Entities         |        |  - EF Core        |
| - Controllers     |        |  - Interfaces       |        |  - Repositories   |
| - Dependency      |        |  - Business Logic   |        |  - Bank Service   |
|   Injection       |        +---------------------+        +-------------------+
+-------------------+
```

### Payment & Refund Flow

1. **Payment Processing**
   - API receives payment request.
   - Core validates and processes payment.
   - The external bank is called first. If authorized, the payment is saved locally.
   - If the local save fails after a successful bank authorization, the system attempts to refund the payment by calling the bank simulator's refund endpoint immediately (synchronously, best-effort).



```
[API] ---> [Core: PaymentService] ---> [Bank: ProcessPayment]
                                         |
                                         v
                                [DB: Save Payment]
                                         |
                                         v
                        [If Save Fails: Bank Refund (sync)]
```

### Observability

- Uses `ILogger<T>` for structured logs in all critical flows (payment, refund attempts).
- Logs include context (paymentId, etc.) for traceability.

### Testing

- Unit tests for all business logic, including refund scenarios.
- Integration tests for API endpoints and DB interactions.
- Helpers/factories for DRY, maintainable test code.

### Developer Experience

- `dev.sh` script for common dev tasks (test, run, Docker, etc.).
- Clear code and English documentation.
- Modular, extensible structure for easy onboarding.

---

For more details, see code comments and test helpers.
