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

New domain rules should have domain unit tests.

New use-case behavior should have Application tests.

REST contract changes should have API integration tests.

User-visible flows should have Playwright coverage where practical.

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
