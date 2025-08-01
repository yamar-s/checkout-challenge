CREATE DATABASE payment;

CREATE TABLE "Payments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Status" VARCHAR(20) NOT NULL,
    "CardLastFour" VARCHAR(4) NOT NULL,
    "ExpiryMonth" INT NOT NULL,
    "ExpiryYear" INT NOT NULL,
    "Currency" VARCHAR(3) NOT NULL,
    "Amount" BIGINT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "AuthorizationCode" VARCHAR(100)
);

