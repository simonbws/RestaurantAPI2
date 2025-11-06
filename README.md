#  RestaurantAPI – Integration & Unit Tests

This project contains **unit and integration tests** for the `RestaurantAPI` application.  
The goal was to verify endpoint behavior, data validation, and dependency configuration in a controlled in-memory environment.

---

## Stack and Tools

- **.NET / xUnit** – test framework  
- **FluentAssertions** – expressive assertions  
- **Microsoft.AspNetCore.Mvc.Testing** – host for integration tests  
- **EF Core InMemory** – lightweight in-memory database  
- **Moq** – mocking external services  
- **Newtonsoft.Json** – JSON serialization  

---

## Scope of Testing

### Integration Tests
- **RestaurantController**
  - `GET /api/restaurant` – verifies correct and incorrect query parameters (`pageSize`, `pageNumber`)  
  - `POST /api/restaurant` – tests valid and invalid create requests  
  - `DELETE /api/restaurant/{id}` – checks deletion logic, ownership, and `NotFound` handling  
- **AccountController**
  - `POST /api/account/login` – mocks `IAccountService` and validates login flow  
  - `POST /api/account/register` – tests model validation (`BadRequest` for invalid data)

###  Unit Tests
- **RestaurantQueryValidatorTests** – checks correct/incorrect pagination and sorting parameters  
- **RegisterUserDtoValidatorTests** – validates unique email logic and password confirmation  

---

## Test Files Overview

 - RestaurantControllerTests.cs | Integration tests for main restaurant endpoints. Runs on `WebApplicationFactory` with `InMemory` DB. Checks status codes and CRUD operations. 
 - AccountControllerTests.cs | Tests authentication and registration endpoints with mocked `IAccountService`. |
 - StartupTests.cs | Verifies dependency injection – ensures all controllers are correctly registered in DI container. |
 - RestaurantQueryValidatorTests.cs | Unit tests for `RestaurantQuery` model validator using `FluentValidation.TestHelper`. |
 - RegisterUserDtoValidatorTests.cs | Tests `RegisterUserDtoValidator` to confirm email uniqueness and password validation. |
 - FakePolicyEvaluator.cs | Bypasses `[Authorize]` attributes. Injects fake user identity during tests to simplify authentication. |
 - FakeUserFilter.cs | Adds mock user context (`Admin`, `Id=1`) to test requests requiring authentication. |
 - HttpContentHelper.cs | Extension for serializing DTOs to JSON (`ToJsonHttpContent`) before sending HTTP requests. |

---

## How It Works

Each integration test runs against a **real API instance** started by `WebApplicationFactory<Startup>`.  
The API uses an **in-memory EF Core database**, so no external SQL Server is needed.  
Fake authentication (`FakePolicyEvaluator` + `FakeUserFilter`) allows testing of endpoints protected by `[Authorize]`.

---
### Author 
simonbws
### This project is a fork and extension of the RestaurantAPI project
Developed as part of the Udemy course by Jakub Kozera.
---
[View Certificate of Completion] https://www.udemy.com/certificate/UC-64ef54c6-3719-4bed-bf53-e172b54d6acc/
## Example Test

```csharp
[Fact]
public async Task CreateRestaurant_WithValidModel_ReturnsCreatedStatus()
{
    var model = new CreateRestaurantDto()
    {
        Name = "TestRestaurant",
        City = "Kraków",
        Street = "Długa 5"
    };

    var httpContent = model.ToJsonHttpContent();
    var response = await _client.PostAsync("/api/restaurant", httpContent);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    response.Headers.Location.Should().NotBeNull();
}


