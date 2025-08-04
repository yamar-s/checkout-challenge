#!/bin/bash

# Process Payment
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{
    "clientRequestId": "payment-1",
    "cardNumber": "1234567890123451",
    "expiryMonth": 12,
    "expiryYear": 2025,
    "currency": "USD",
    "amount": 100,
    "cvv": "123"
  }'

echo -e "\n---\n"

# Get Payment by Id (replace <PAYMENT_ID> with actual id)
curl -X GET http://localhost:5000/api/payments/<PAYMENT_ID>
