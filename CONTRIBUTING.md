# Contributing / Review Notes

## Development rules

Keep dependency direction intact:

```text
Domain <- Application <- Infrastructure
                       ^
                       |
                      API
```

The domain must not reference ASP.NET Core, MediatR, FluentValidation, or persistence implementations.

Use cases belong in Application commands/queries and should remain asynchronous.

Use `ILogger<T>` for diagnostics. Do not add `Console.WriteLine` application logging.

Use `OperationResult<T>` for expected game-rule failures. Reserve exceptions for exceptional failures and let the global exception handler translate them into ProblemDetails.

Do not expose domain entities directly from controller actions.

Any state mutation should participate in optimistic concurrency using the aggregate version.

## Validation

Distinguish:

1. Transport/input validation — FluentValidation.
2. Game/business rules — Domain `GameRules` and `Game`.

## Tests

### Tests

- Domain business rules should have unit-test coverage.
- Application use cases should have appropriate automated tests.
- REST API behavior should have integration-test coverage where applicable.
- Angular behavior should be covered with Jasmine/Karma tests where appropriate.

## Pull request checklist

```text
[ ] Domain dependency rules preserved
[ ] API DTOs remain separate from domain
[ ] Async APIs used
[ ] Concurrency version considered
[ ] Validation covered
[ ] Logging added where useful
[ ] Tests updated
[ ] README/API docs updated
[ ] `dotnet build` passes
[ ] `dotnet test` passes
[ ] `npm run build` passes
[ ] frontend tests pass
```
