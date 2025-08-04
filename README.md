# Payment Gateway

A robust payment gateway implementing Clean Architecture with distributed transaction coordination to ensure atomicity and consistency.

---

## Layered Architecture

### Core (Domain)
Contains business rules and main entities:

- **Entities**: `Payment`, `PaymentTransaction`, `TransactionEvent`
- **Orchestrator**: `PaymentOrchestrator` - coordinates the entire payment flow
- **Services**: `PaymentService`, `PaymentValidator`
- **Interfaces**: Contracts for all external dependencies

```
src/PaymentGateway.Core/
├── Entities/           # Domain models
├── Services/           # Business logic
└── Interfaces/         # Contracts
```

### Infrastructure
Implements Core interfaces and manages data and external services:

- **Repositories**: Data access (`PaymentRepository`, `TransactionRepository`, `TransactionEventRepository`)
- **External Services**: `BankService` for bank communication
- **Background Services**: `TransactionRecoveryService` for automatic recovery
- **Persistence**: Entity Framework Core with PostgreSQL

```
src/PaymentGateway.Infrastructure/
├── Repositories/       # Data access
├── Services/           # External and background services
└── Data/              # Database context
```

### API (Presentation)
Application entry layer:

- **Controllers**: REST endpoints
- **Middleware**: Error handling
- **Configuration**: Dependency injection

```
src/PaymentGateway.Api/
├── Controllers/        # REST endpoints
├── Middleware/         # Interceptors
└── Program.cs         # Application configuration
```

---


## Database

### 3 Main Tables
```sql
Payments              -- Payment data (final result)
PaymentTransactions   -- Coordination state (where we are in the process)  
TransactionEvents     -- Detailed events (complete audit)
```

### Relationships
```
PaymentTransaction (1) ──── TransactionEvent (*)
         │
         └── Payment (final result if successful)
```

---

## How to Run

### Quick Start
```bash
# Start all services
./dev.sh run

# Run tests
./dev.sh test
```

### Database Configuration
```bash
# Apply migrations
dotnet ef database update --project src/PaymentGateway.Infrastructure
```

### Testing the API
```bash
# Process payment
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 10000,
    "currency": "GBP", 
    "cardNumber": "2222405343248877",
    "expiryMonth": 4,
    "expiryYear": 2025,
    "cvv": "123",
    "clientRequestId": "unique-req-123"
  }'
```

---

## Additional Documentation

- [`ARCHITECTURE.md`](./ARCHITECTURE.md) - Detailed system architecture

---

