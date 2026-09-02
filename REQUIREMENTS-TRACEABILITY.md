# Requirements & Architecture Traceability

This document maps the supplied Tic Tac Toe assessment requirements and the architecture guidance to the implementation.

## Assessment requirements

| Requirement | Implementation |
|---|---|
| Angular + TypeScript | `frontend/` Angular 19 application |
| .NET Web API | `backend/TicTacToe.Api/` ASP.NET Core 10 |
| REST API | Versioned REST endpoints under `/api/v1` |
| In-memory storage acceptable | `TicTacToe.Infrastructure` in-memory repositories |
| Backend owns state | Game and scoreboard state only live in backend repositories |
| 3 × 3 board | Domain `Game` board with 9 cells |
| X/O turns | `Player`, `GameRules.ValidateMove` |
| Row/column/diagonal wins | `GameRules.FindWinner` |
| Winning cells highlighted | `winningCells` returned by API and styled by Angular |
| Draw | `GameRules.EvaluateOutcome` + completion handling |
| Reset game preserves scoreboard | `ResetGameCommandHandler` creates a fresh game; scoreboard is untouched |
| Move history | `Game.Moves`, `GET /games/{id}/moves`, UI history table |
| Two-player undo | removes one move |
| Computer-mode undo | removes X and O move pair |
| Undo disabled after completion | Option A from assessment |
| Session scoreboard | `IScoreboardRepository` + in-memory implementation |
| Score updated once | scoreboard repository uses completed game ID idempotency set |
| Basic computer opponent | `ComputerMoveSelector` |
| Computer priority | win, block, center, corner, any cell |
| Invalid move handling | FluentValidation + `GameRules` |
| Move after completion | domain rule rejects it |
| Frontend uses REST | `GameApiService` with `HttpClient` |
| UI state | Angular Signals |
| Backend source of truth | UI replaces its game signal with returned API state |
| Tests | Domain, Application, API integration, browser E2E |
| README | detailed run/config/design/test documentation |
| API documentation | `API.md` + Swagger |

## Architecture guidance

| Guidance | Implementation |
|---|---|
| Controller → Application/Service → Domain | Controllers dispatch MediatR requests; handlers call repositories/domain |
| Keep GameService from becoming a God class | No monolithic `GameService`; commands/queries + domain aggregate |
| Domain objects enforce business rules | `Game` + `GameRules` |
| Application coordinates | MediatR handlers |
| Lock not treated as distributed concurrency mechanism | No application-wide game lock; repository uses optimistic version compare/update |
| Async API | All controller/use-case repository calls are async |
| Avoid exceptions for normal validation | `OperationResult<T>` for expected game-rule outcomes; exceptions for exceptional conditions |
| Global exception handling | `GlobalExceptionHandler` with ProblemDetails |
| REST resource modeling | versioned resource-oriented endpoints and explicit state/moves resources |
| DTO separation | API request/response DTOs map to Application models/domain entities |
| FluentValidation | pipeline validation behavior |
| CQRS/MediatR where useful | commands and queries listed in README/API |
| Repository abstraction | `IGameRepository`, `IScoreboardRepository` |
| Real-time updates | deliberately not implemented; see SignalR decision |
| Optimistic concurrency | `Game.Version`, `ExpectedVersion`, repository compare/update |
| API versioning | `/api/v1`, Asp.Versioning |
| Structured logging | JSON console logging + MediatR logging behavior |
| Health checks | `/health/live`, `/health/ready` |
| Swagger/OpenAPI | Swashbuckle + Swagger UI |
| Automated tests | unit/application/integration/E2E layers |
| Cross-cutting infrastructure | logging, exception handling, authorization pipeline, health checks, OpenTelemetry |

## Intentional non-implementation

The supplied architectural notes explicitly say not to implement every production concern immediately. This repository keeps the assessment locally runnable by retaining an in-memory persistence implementation while providing the abstractions required for SQL/Cosmos/Redis later.

SignalR is also intentionally excluded because Angular Signals do not provide server push, but the assessment itself does not require server-push or multiple independent clients. REST responses are sufficient for this UI.
