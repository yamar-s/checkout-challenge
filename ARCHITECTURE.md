# Payment Gateway Architecture

## **High-Level Architecture**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Layer    │◄──►│   Core Domain   │◄──►│ Infrastructure  │
│                 │    │                 │    │                 │
│ • Controllers   │    │ • Orchestrator  │    │ • Repositories  │
│ • Middleware    │    │ • Entities      │    │ • Bank Service  │
│ • DI Container  │    │ • Events        │    │ • EF Core       │
│ • Error Handler │    │ • Validators    │    │ • Recovery Svc  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                 ┌─────────────────────────────┐
                 │      PostgreSQL Database    │
                 │ • Payments                  │
                 │ • PaymentTransactions       │
                 │ • TransactionEvents         │
                 └─────────────────────────────┘
```

## **Payment Orchestration Pattern**

### **Core Components**

1. **PaymentOrchestrator**: Coordinates the entire payment flow
2. **PaymentTransaction**: Tracks the current state of the operation
3. **TransactionEvent**: Records every step for audit and recovery
4. **TransactionRecoveryService**: Background service for automatic recovery

### **Flow Diagram**

```
[Client Request] 
        │
        ▼
[PaymentService] ──► [PaymentOrchestrator] 
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
[Create Transaction]  [Record Event]      [Validate Request]
        │                     │                     │
        ▼                     ▼                     ▼
[Bank Processing] ──► [Record Event] ──► [Error Handling]
        │                     │                     │
        ▼                     ▼                     ▼
[Local Storage]   ──► [Record Event] ──► [Compensation]
        │                     │                     │
        ▼                     ▼                     ▼
[Complete Transaction] ──► [Final Event] ──► [Return Response]
```

##  **Atomicity & Consistency Guarantees**

### **Transaction States**
```
Created → BankApproved → Saved → Completed
   │           │           │
   ▼           ▼           ▼
Failed    Refunding   RequiresManualReview
```

### **Compensation Logic**
1. **Bank Success + Local Failure**: Automatic refund initiated
2. **Refund Failure**: Escalated to manual review with full context
3. **Partial Failures**: Retry with exponential backoff
4. **Critical Failures**: Immediate notification and audit trail

### **Idempotency**
- Client Request ID tracking prevents duplicate processing
- State checks before each operation
- Safe retry mechanisms for all operations

---

##  **Event Sourcing Implementation**

### **Event Store Design**
```sql
TransactionEvents Table:
├── Id (UUID)               -- Unique event identifier
├── TransactionId (UUID)    -- Links to PaymentTransaction
├── EventType (enum)        -- Type of event occurred
├── OccurredAt (timestamp)  -- Precise timing
├── EventData (JSON)        -- Contextual information
├── ErrorDetails (text)     -- Error information if applicable
└── StepOrder (int)         -- Sequence within transaction
```

### **Event Types & Flow**
```
Success Flow:
1. TransactionStarted
2. BankProcessingStarted  
3. BankProcessingCompleted
4. PaymentDataSaveStarted
5. PaymentDataSaveCompleted
6. TransactionCompleted

Failure & Recovery Flow:
1. TransactionStarted
2. BankProcessingStarted
3. BankProcessingCompleted
4. PaymentDataSaveStarted
5. PaymentDataSaveFailed
6. CompensationStarted
7. RefundStarted
8. RefundCompleted (or RefundFailed → ManualReviewRequired)
```

### **Benefits**
- **Complete Audit Trail**: Every operation is recorded
- **Debugging**: Precise failure point identification
- **Compliance**: Immutable record for regulations
- **Recovery**: State reconstruction from events
- **Analytics**: Transaction flow analysis

##  **Background Recovery Service**

### **TransactionRecoveryService**
```csharp
// Runs every 5 minutes
// Identifies transactions requiring intervention:
// - Status: RequiresManualReview  
// - Status: Refunding (stuck > 30 minutes)
// - Automatic retry with exponential backoff
```

### **Recovery Scenarios**
1. **Stuck Refunds**: Retry refund operations
2. **Manual Review**: Alert operations team
3. **Network Timeouts**: Retry with circuit breaker
4. **Bank API Issues**: Escalate with full context

---

## **Data Architecture**

### **Entity Relationships**
```
Payment (1) ←─── ClientRequestId (unique) ──→ Idempotency
    │
    │ (business data)
    │
PaymentTransaction (1) ←─── State Management ──→ Orchestration
    │
    │ (1:N)
    │
TransactionEvent (*) ←─── Event Store ──→ Audit Trail
```

### **Database Indexes**
```sql
-- Performance optimizations
CREATE INDEX IX_Payments_ClientRequestId ON Payments(ClientRequestId);
CREATE INDEX IX_TransactionEvents_TransactionId_StepOrder 
  ON TransactionEvents(TransactionId, StepOrder);
CREATE INDEX IX_PaymentTransactions_Status 
  ON PaymentTransactions(Status);
```

##  **Error Handling & Monitoring**

### **Error Classification**
1. **Validation Errors**: Client-side issues (400)
2. **Business Errors**: Domain rule violations (422)
3. **External Service Errors**: Bank API failures (502)
4. **Infrastructure Errors**: Database issues (500)
5. **Timeout Errors**: Network/performance issues (504)

### **Monitoring Strategy**
- **Structured Logging**: Correlation IDs across all components
- **Event Metrics**: Success/failure rates by event type
- **Performance Tracking**: Transaction duration analysis
- **Alert Thresholds**: Failed transaction count limits
- **Health Checks**: Database and external service availability

### **Observability Stack**
```
Application Logs → ILogger → Structured Output
Event Store → Analytics → Business Intelligence  
Metrics → Performance Monitoring → Alerting
Health Checks → Service Status → Operational Dashboard
```

---

##  **Security & Compliance**

### **Data Protection**
- **Card Data**: PCI-compliant handling (last 4 digits only)
- **Event Store**: Immutable audit trail
- **Request IDs**: Client correlation without sensitive data
- **Error Logging**: Sanitized error information

### **Compliance Features**
- **Audit Trail**: Complete transaction history
- **Data Retention**: Configurable retention policies
- **Access Control**: Repository-level security (planned)
- **Encryption**: Database-level encryption (planned)

---

## **Scalability & Performance**

### **Current Optimizations**
- **Database Indexes**: Query optimization
- **Event Store**: Efficient time-series queries
- **Connection Pooling**: EF Core optimization
- **Async Operations**: Non-blocking I/O

### **Future Scalability**
- **Horizontal Scaling**: Multiple API instances
- **Database Sharding**: Transaction-based partitioning
- **Event Store Optimization**: Time-based partitioning
- **Caching Layer**: Redis for frequent queries
- **Message Queues**: Asynchronous event processing

---

##  **Testing Strategy**

### **Test Pyramid**
```
End-to-End Tests (API)
├── Happy path scenarios
├── Error handling flows  
├── Recovery mechanisms
└── Idempotency validation

Integration Tests (Components)
├── Repository operations
├── External service calls
├── Event store functionality
└── Background service behavior

Unit Tests (Business Logic)
├── Payment orchestration
├── Validation rules
├── Event generation
└── State transitions
```

### **Test Data Management**
- **Test Containers**: Isolated database instances
- **Factory Pattern**: Consistent test data creation
- **Mocking**: External service simulation
- **Snapshot Testing**: Event store validation

For detailed implementation, see source code and inline documentation.
