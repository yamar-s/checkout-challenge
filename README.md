# Payment Gateway

A robust, modular, and extensible payment gateway developed following Clean Architecture principles. Integrates with PostgreSQL and includes comprehensive automated testing. The focus throughout the development was on clarity, maintainability, and minimal complexity, as required for an offline take-home exercise.

---

## Key Decisions
- The solution is simple and maintainable, avoiding over-engineering and unnecessary abstractions.
- The API and domain are focused strictly on the functional requirements of the challenge.
- The payment flow is synchronous: the external bank is called first, and only on success is the payment saved locally.
- If the local save fails after a successful bank authorization, the system attempts to refund the payment by calling the bank simulator's refund endpoint. This automatic reversal helps keep the external bank and the local database consistent, providing a best-effort approach to data integrity while maintaining a simple and focused solution for the challenge.
- Automated tests cover repository, service, controller, and error scenarios.

---

## Application Layers

- **Core**: Contains all business rules, entities, and interfaces. This layer is completely independent of external dependencies. Domain models, validation logic, and service contracts are defined here.
- **Infrastructure**: Implements the interfaces defined in Core. This includes the repository (using Entity Framework Core with PostgreSQL) and external service integrations (such as the bank service). All data access and third-party integrations are isolated here.
- **API**: The presentation layer, built with ASP.NET Core. Exposes REST endpoints, handles dependency injection, and orchestrates the application flow by calling into the Core and Infrastructure layers.
- **Tests**: Contains all unit and integration tests. xUnit and Moq are used to ensure that the business logic, data access, and API endpoints behave as expected. Tests are written against interfaces, not concrete implementations, to maximize flexibility and maintainability.

---


## Bank Simulator

A bank simulator is provided and used for integration tests. The simulator responds as follows:
- If any required field is missing, it returns 400 Bad Request with an error message.
- If all fields are present:
  - Card number ending with odd digit (1, 3, 5, 7, 9): returns 200 Ok, authorized, with a random authorization_code.
  - Card number ending with even digit (2, 4, 6, 8): returns 200 Ok, unauthorized.
  - Card number ending with zero (0): returns 503 Service Unavailable.

### Starting the Simulator
To start the simulator, use:
```sh
docker compose up
```

### Calling the Simulator
- Endpoint: `POST http://localhost:8080/payments`
- Example request body:
```json
{
  "card_number": "2222405343248877",
  "expiry_date": "04/2025",
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```
- Example response:
```json
{
  "authorized": true,
  "authorization_code": "0bb07405-6d44-4b50-a14f-7ae0beff13ad"
}
```

---

## Project Structure

```
src/
  PaymentGateway.Core/           # Business rules, entities, interfaces
  PaymentGateway.Infrastructure/ # Concrete implementations (repos, services, EF Core)
  PaymentGateway.Api/            # REST API (ASP.NET Core)
tests/
  ...                            # Unit and integration tests
docker/
  imposters/                     # Service simulators (e.g., Mountebank)
```

## Technologies
- .NET 9
- ASP.NET Core
- Entity Framework Core + PostgreSQL
- xUnit, Moq
- Mountebank (for external service integration tests)

---

## How to Run

1. **Build**
   ```sh
   ./dev.sh run
   ```
2. **Tests**
   ```sh
   ./dev.sh test
   ```
3. **Run API**
   ```sh
   ./dev.sh run
   ```
4. **Migrations**
   ```sh
   dotnet ef database update --project src/PaymentGateway.Infrastructure
   ```
5. **Mountebank (bank simulator)**
   ```sh
   mb --config docker/imposters/bank_simulator.ejs
   ```

---

## API Testing

You can test the API using the provided Postman collection (`PaymentGateway.postman_collection.json`) or via CLI with the script `api_curl_examples.sh`:

### Using Postman
- Import `PaymentGateway.postman_collection.json` into Postman.
- Use the "Process Payment" and "Get Payment by Id" requests to interact with the API.

### Using curl (CLI)
- Run the script:
  ```sh
  bash api_curl_examples.sh
  ```
- Or use the individual curl commands inside the script for custom testing.

---


