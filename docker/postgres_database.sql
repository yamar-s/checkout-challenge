CREATE TABLE "Payments" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "Amount" bigint NOT NULL,
    "AuthorizationCode" text NULL,
    "CardLastFour" text NOT NULL,
    "ClientRequestId" text NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT NOW(),
    "Currency" text NOT NULL,
    "ExpiryMonth" integer NOT NULL,
    "ExpiryYear" integer NOT NULL,
    "Status" integer NOT NULL,
    
    CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
);

-- Create unique index on ClientRequestId (for idempotency)
CREATE UNIQUE INDEX "IX_Payments_ClientRequestId" 
ON "Payments" ("ClientRequestId") 
WHERE "ClientRequestId" IS NOT NULL;

-- =====================================================
-- Table: Transactions
-- Stores transaction orchestration state and metadata
-- =====================================================
CREATE TABLE "Transactions" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "Amount" bigint NOT NULL,
    "BankAuthCode" text NULL,
    "ClientRequestId" text NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT NOW(),
    "Currency" text NOT NULL,
    "LastError" text NULL,
    "LastUpdated" timestamptz NOT NULL DEFAULT NOW(),
    "PaymentId" uuid NOT NULL,
    "RetryAttempts" integer NOT NULL DEFAULT 0,
    "Status" integer NOT NULL,
    
    CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id")
);

-- =====================================================
-- Table: TransactionEvents
-- Stores event log for transaction orchestration
-- =====================================================
CREATE TABLE "TransactionEvents" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" timestamptz NOT NULL DEFAULT NOW(),
    "Description" text NOT NULL,
    "EventData" text NULL,
    "EventType" text NOT NULL,
    "TransactionId" uuid NOT NULL,
    
    CONSTRAINT "PK_TransactionEvents" PRIMARY KEY ("Id")
);

-- =====================================================
-- Foreign Key Constraints
-- =====================================================

-- TransactionEvents -> Transactions (CASCADE DELETE)
ALTER TABLE "TransactionEvents" 
ADD CONSTRAINT "FK_TransactionEvents_Transactions_TransactionId" 
FOREIGN KEY ("TransactionId") 
REFERENCES "Transactions" ("Id") 
ON DELETE CASCADE;

-- =====================================================
-- Indexes for Performance
-- =====================================================

-- Index on TransactionId for faster event queries
CREATE INDEX "IX_TransactionEvents_TransactionId" 
ON "TransactionEvents" ("TransactionId");

-- Index for finding transactions by status (used by recovery service)
CREATE INDEX "IX_Transactions_Status" 
ON "Transactions" ("Status");

-- Index for finding transactions by ClientRequestId (idempotency checks)
CREATE INDEX "IX_Transactions_ClientRequestId" 
ON "Transactions" ("ClientRequestId") 
WHERE "ClientRequestId" IS NOT NULL;

-- Index for finding recent transactions
CREATE INDEX "IX_Transactions_CreatedAt" 
ON "Transactions" ("CreatedAt");

-- Index for event queries by type and transaction
CREATE INDEX "IX_TransactionEvents_TransactionId_EventType" 
ON "TransactionEvents" ("TransactionId", "EventType");
