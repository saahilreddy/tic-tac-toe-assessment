# Tic Tac Toe — Angular 19 + ASP.NET Core 10

## 1. Project Overview

This repository contains a full-stack Tic Tac Toe application developed as a technical assessment.

The solution consists of:

* **Angular 19** frontend using TypeScript.
* **ASP.NET Core 10 Web API** backend.
* REST APIs versioned under `/api/v1`.
* Backend-owned game and scoreboard state.
* Two game modes:

  * Two Player
  * Play Against Computer
* Move history and mode-specific undo.
* Win and draw detection.
* Winning-cell highlighting.
* Optimistic concurrency using an aggregate version.
* Clean Architecture with Domain, Application, Infrastructure, and API layers.
* MediatR-based commands and queries.
* FluentValidation for request validation.
* Centralized exception handling using `ProblemDetails`.
* Swagger/OpenAPI documentation.
* Health checks.
* Structured logging and OpenTelemetry instrumentation.
* Automated backend and frontend tests.

The application intentionally uses **in-memory persistence** because persistent database storage was not required for the assessment.

---

## 2. Tech Stack

### Frontend

| Technology                  | Version |
| --------------------------- | ------- |
| Angular                     | 19.2    |
| TypeScript                  | 5.7.2   |
| RxJS                        | 7.8.1   |
| Zone.js                     | 0.15    |
| Node.js                     | 22      |
| npm                         | 10+     |
| Jasmine Core                | 7.0.2   |
| Jasmine Type Definitions    | 6.0.0   |
| Karma                       | 6.4.4   |
| Karma Jasmine               | 5.1.0   |
| Karma Chrome Launcher       | 3.2.0   |
| Karma Jasmine HTML Reporter | 2.3.0   |

Frontend state is managed using **Angular Signals**, while HTTP communication is handled through Angular `HttpClient`.

### Backend

* .NET 10
* ASP.NET Core Web API
* C#
* MediatR
* FluentValidation
* Clean Architecture
* CQRS-style commands and queries
* Repository pattern
* In-memory repositories
* ASP.NET Core ProblemDetails
* API Versioning
* Swagger / OpenAPI
* Health Checks
* `ILogger<T>` structured logging
* OpenTelemetry
* xUnit-based automated tests

### Development Tools

* Git
* GitHub
* Visual Studio / Visual Studio Code
* Swagger UI
* Chrome / ChromeHeadless

---

## 3. Features Implemented

### Game Management

* Create a new Tic Tac Toe game.
* Support for a 3 × 3 board.
* Two Player mode.
* Play Against Computer mode.
* Maintain the current player.
* Maintain complete move history.
* Reset the current game.
* Backend remains the source of truth for game state.

### Game Rules

The backend domain layer validates:

* Valid board positions.
* Empty-cell requirement.
* Correct player turn.
* Moves after game completion.
* Row wins.
* Column wins.
* Diagonal wins.
* Draw conditions.
* Winning-cell identification.

### Computer Opponent

The computer player follows a deterministic priority strategy:

1. Win if a winning move is available.
2. Block the opponent's winning move.
3. Take the center.
4. Take a corner.
5. Take any remaining available cell.

### Undo

Two Player mode:

* Undo removes the most recent move.

Computer mode:

* Undo removes the human X move and corresponding computer O move together.

Undo is intentionally disabled after the game has completed.

### Scoreboard

The application maintains:

* X wins
* O wins
* Draws

Completed games update the scoreboard only once using game-ID-based idempotency.

Resetting a game does not modify the scoreboard.

### Concurrency

Game mutations use optimistic concurrency.

Each game maintains a `Version`.

The client sends an `expectedVersion` with mutations.

If another operation has already changed the game, the backend returns:

```text
409 Conflict
```

This prevents stale clients from overwriting newer game state.

### API and Infrastructure

* Versioned REST API.
* Swagger/OpenAPI.
* Centralized exception handling.
* RFC 7807-style ProblemDetails responses.
* Health endpoints.
* Structured JSON logging.
* OpenTelemetry tracing and metrics.
* CORS configuration for the Angular application.

---

## 4. How to Run the Backend Locally

### Prerequisites

Install:

* .NET 10 SDK
* Node.js 22
* npm 10+
* Git

Verify the .NET SDK:

```bash
dotnet --version
```

Verify Node.js:

```bash
node --version
```

Verify npm:

```bash
npm --version
```

### Restore and Build

From the repository root:

```bash
cd backend
dotnet restore
dotnet build
```

### Run Backend

Run the API using the HTTP launch profile:

```bash
dotnet run --project TicTacToe.Api --launch-profile http
```

The backend is available at:

```text
http://localhost:62167
```

### Swagger

Swagger UI:

```text
http://localhost:62167/swagger
```

Swagger can be used to inspect and manually execute all API endpoints.

### Health Checks

Liveness:

```text
http://localhost:62167/health/live
```

Readiness:

```text
http://localhost:62167/health/ready
```

---

## 5. How to Run the Frontend Locally

### Install Dependencies

From the repository root:

```bash
cd frontend
npm install
```

### Start Angular

```bash
npm start
```

The Angular application runs at:

```text
http://localhost:4200
```

### API Communication

During local development, Angular uses the configured proxy:

```text
frontend/proxy.conf.json
```

API requests are routed to:

```text
http://localhost:62167
```

The application communicates with the versioned API under:

```text
/api/v1
```

### Production Build

To create a production build:

```bash
npm run build
```

---

## 6. API Endpoint Summary

Base URL:

```text
http://localhost:62167/api/v1
```

### Create Game

```http
POST /games
```

Request:

```json
{
  "mode": "TwoPlayer"
}
```

Supported modes:

```text
TwoPlayer
Computer
```

Returns:

```text
201 Created
```

with the created game state.

---

### Get Game

```http
GET /games/{id}
```

Returns the current game.

Possible responses:

```text
200 OK
404 Not Found
```

---

### Get Game State

```http
GET /games/{id}/state
```

Returns the current state of the game.

---

### Get Move History

```http
GET /games/{id}/moves
```

Returns the moves currently recorded for the game.

---

### Submit Move

```http
POST /games/{id}/moves
```

Request:

```json
{
  "gameId": "GUID",
  "player": "X",
  "row": 0,
  "column": 0,
  "expectedVersion": 0
}
```

The backend validates:

* Route game ID matches body game ID.
* Game exists.
* Game is still in progress.
* Row and column are valid.
* Cell is empty.
* Player is the current player.
* Computer mode accepts human X moves.
* Expected version matches the current game version.

Possible responses:

```text
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

---

### Undo

```http
POST /games/{id}/undo
```

Request:

```json
{
  "expectedVersion": 2
}
```

Behavior:

* Two Player mode → removes one move.
* Computer mode → removes the X/O move pair.
* Completed games → undo is rejected/disabled by design.

---

### Reset Game

```http
POST /games/{id}/reset
```

Request:

```json
{
  "expectedVersion": 5
}
```

Creates a fresh game session using the same game mode.

The scoreboard is not modified.

---

### Get Scoreboard

```http
GET /scoreboard
```

Example response:

```json
{
  "xWins": 1,
  "oWins": 2,
  "draws": 1
}
```

---

### Reset Scoreboard

```http
POST /scoreboard/reset
```

Resets the session scoreboard.

---

### Status Codes

| Status | Meaning                                  |
| ------ | ---------------------------------------- |
| 200    | Successful read/mutation                 |
| 201    | Game created                             |
| 400    | Invalid input or game-rule violation     |
| 404    | Game does not exist                      |
| 409    | Optimistic concurrency conflict          |
| 500    | Unexpected server/infrastructure failure |

### Error Contract

Exceptional failures are returned using `application/problem+json`.

Example:

```json
{
  "type": "https://localhost/errors/concurrency-conflict",
  "title": "Concurrency conflict",
  "status": 409,
  "detail": "The game changed before your move could be saved. Refresh the game and try again.",
  "instance": "/api/v1/games/.../moves",
  "traceId": "..."
}
```

For the complete API contract, see:

```text
API.md
```

Swagger is also available locally at:

```text
http://localhost:62167/swagger
```

---

## 7. How to Run Tests

The solution contains tests at multiple levels.

### Backend Tests

From the backend directory:

```bash
cd backend
dotnet test
```

The backend tests cover:

### Domain Tests

* Game rules.
* Valid and invalid moves.
* Row wins.
* Column wins.
* Diagonal wins.
* Draw detection.
* Winning-cell detection.
* Move history.
* Undo state reconstruction.
* Computer move priority.

### Application Tests

* Command handlers.
* Query handlers.
* Scoreboard behavior.
* Scoreboard idempotency.
* Computer-mode undo.
* Optimistic concurrency.

### API Integration Tests

* Game creation.
* Invalid moves.
* ProblemDetails responses.
* API behavior and HTTP status codes.

### Frontend Tests

From the frontend directory:

```bash
cd frontend
npm test
```

This executes:

```bash
ng test --watch=false --browsers=ChromeHeadless
```

Frontend unit tests cover:

* Game state.
* Move handling.
* Turn changes.
* Win/draw state.
* Undo.
* Reset.
* Scoreboard.
* Computer mode.
* UI state updates.

### Test Strategy

```text
Tests
│
├── Backend
│   ├── Domain unit tests
│   ├── Application tests
│   └── API integration tests
│
└── Frontend
    └── Jasmine / Karma unit tests
```

The test suite is designed to validate business rules at the domain level, use-case behavior at the application level, HTTP behavior at the API level, and UI behavior at the Angular level.

---

## 8. AI Tools and Prompt Summary

AI-assisted development was used as a development aid during the implementation.

### Areas Where AI Assistance Was Used

AI assistance was used for:

* Initial project structure and architecture discussion.
* Clean Architecture organization.
* CQRS/MediatR structure.
* API contract design.
* Game-rule implementation ideas.
* Computer-player decision logic.
* Optimistic concurrency design.
* FluentValidation implementation.
* Exception handling and ProblemDetails.
* Unit and integration test scenarios.
* Angular component and service structure.
* Angular Signals usage.
* Code review and refactoring suggestions.
* README and API documentation preparation.

### Prompt Categories

Representative prompts included:

```text
Design a Clean Architecture structure for an ASP.NET Core Tic Tac Toe API.

How should game state and scoreboard state be separated from the API layer?

Design optimistic concurrency for a game aggregate using a version number.

What validation rules should be applied when submitting a Tic Tac Toe move?

How should undo behave differently in Two Player and Computer modes?

Create a deterministic computer move strategy with the priority:
win, block, center, corner, any available cell.

Suggest unit and integration test scenarios for the Tic Tac Toe domain and API.

How should Angular Signals be used for local game state while keeping the backend as the source of truth?

Review the implementation for separation of concerns and potential concurrency issues.
```

### AI Usage Approach

AI-generated suggestions were treated as development assistance rather than blindly copied implementation.

The generated recommendations were reviewed and adapted based on:

* Assessment requirements.
* Existing project architecture.
* Angular 19 conventions.
* ASP.NET Core practices.
* Maintainability.
* Testability.
* Separation of concerns.

Final implementation decisions remained aligned with the assessment requirements.

---

## 9. Design Decisions

### Clean Architecture

The backend is separated into:

```text
API
 ↓
Application
 ↓
Domain
```

Infrastructure provides implementations required by the application.

The domain does not depend on ASP.NET Core, MediatR, FluentValidation, or persistence implementations.

This keeps business rules independent from infrastructure and HTTP concerns.

### Domain-Owned Business Rules

The `Game` aggregate and `GameRules` are responsible for game-specific business rules.

Examples:

* Valid moves.
* Turn validation.
* Win detection.
* Draw detection.
* Winning cells.
* Game completion.
* Undo state reconstruction.

This avoids putting core game logic inside controllers.

### CQRS / MediatR

Commands and queries separate state-changing operations from reads.

Examples include:

```text
CreateGame
MakeMove
UndoMove
ResetGame
GetGame
GetGameState
GetMoves
GetScoreboard
ResetScoreboard
```

MediatR handlers coordinate the relevant application use cases.

### Repository Abstraction

The application uses:

```text
IGameRepository
IScoreboardRepository
```

The current implementation is in-memory.

This allows the persistence implementation to be replaced later without changing the domain rules or API contracts.

### Backend as the Source of Truth

The Angular application does not independently determine the authoritative game state.

After a mutation, the frontend uses the state returned by the backend.

This prevents client-side state from becoming inconsistent with the server.

### Optimistic Concurrency

A game contains a version value.

Mutating requests provide an `expectedVersion`.

The repository compares the expected version with the current version before applying an update.

A mismatch results in a conflict instead of silently overwriting newer state.

### ProblemDetails

Expected game-rule failures are handled as application/domain results where appropriate.

Unexpected exceptions are handled centrally through the global exception handler and converted into `ProblemDetails`.

This provides a consistent error contract to clients.

### Angular Signals

Angular Signals are used for local UI state because the game screen primarily needs reactive state for:

* Board.
* Current player.
* Game status.
* Winner.
* Move history.
* Scoreboard.

The backend remains responsible for authoritative state.

### No SignalR

SignalR was intentionally not implemented.

The assessment does not require independent clients to receive server-pushed game updates. REST responses are sufficient for the current UI.

Angular Signals provide client-side reactivity but are not a replacement for server-side push technology.

---

## 10. Clarifications and Assumptions

The following assumptions were made where the assessment requirements did not prescribe an exact implementation.

### Storage

In-memory storage is used.

No SQL, Cosmos DB, Redis, or other persistent database is required for the assessment.

### Game Session

Game state is associated with a generated game ID.

The backend owns the game state for the lifetime of the running application.

### Two Player Undo

Undo removes the most recent move.

### Computer Mode Undo

Undo removes the human X move and the corresponding computer O move together so that the user returns to the previous meaningful decision point.

### Undo After Completion

Undo is disabled after a game has completed.

This follows the selected assessment interpretation.

### Scoreboard

The scoreboard is maintained for the running application session.

Resetting a game does not reset the scoreboard.

### Scoreboard Idempotency

A completed game contributes to the scoreboard only once, using the game ID to prevent duplicate score updates.

### Computer Strategy

The computer opponent is intentionally deterministic and rule-based rather than AI/ML-driven.

Its priority is:

```text
Win
 ↓
Block
 ↓
Center
 ↓
Corner
 ↓
Any available cell
```

### API Versioning

The API is exposed under:

```text
/api/v1
```

This provides a clear version boundary for future API changes.

### Real-Time Communication

Real-time server push was not required by the assessment and is therefore intentionally excluded.

---

## 11. Known Limitations

### In-Memory Persistence

Game and scoreboard data are stored in memory.

Therefore:

* Restarting the API loses all games.
* Restarting the API resets the scoreboard.
* Multiple backend instances would not share the same game state.

### Single Application Instance

The current implementation is intended for local/single-instance execution.

A distributed deployment would require shared persistent state and a distributed concurrency strategy.

### Basic Computer Opponent

The computer opponent uses deterministic rule-based logic.

It does not implement:

* Minimax.
* Machine learning.
* Difficulty levels.
* Adaptive gameplay.

### No Authentication

The assessment does not require user authentication or authorization.

The API therefore does not currently associate games with authenticated users.

### No Real-Time Multiplayer

The application uses REST APIs.

There is no SignalR/WebSocket-based server push for independently connected clients.

### No Persistent Scoreboard

The scoreboard exists only for the lifetime of the application process.

### Browser Test Environment

Frontend tests use ChromeHeadless through Jasmine/Karma and therefore require a compatible Chrome/Chromium installation in the development or CI environment.

---

## 12. Future Improvements

If this application were extended beyond the assessment, the following improvements could be considered.

### Persistent Storage

Replace the in-memory repositories with:

* SQL Server + Entity Framework Core.
* Azure Cosmos DB.
* PostgreSQL.

The existing repository abstractions allow persistence to be introduced without moving game rules into the infrastructure layer.

### Distributed Deployment

For multiple API instances:

```text
Angular
   ↓
Load Balancer
   ↓
API Instance 1
API Instance 2
API Instance 3
   ↓
Shared Database / Distributed Cache
```

Game state and concurrency would need to be handled using shared storage.

### Authentication and Authorization

Introduce:

* Microsoft Entra ID / OAuth 2.0 / OpenID Connect.
* User-specific games.
* User-specific scoreboards.
* Authorization policies.

### SignalR

SignalR could be introduced for:

* Real-time multiplayer.
* Server-pushed game updates.
* Multiple clients viewing the same game.
* Live scoreboard updates.

### Improved Computer AI

Introduce:

* Minimax.
* Alpha-beta pruning.
* Difficulty levels.
* Randomized moves at lower difficulty.

### Persistent Game History

Store completed games and moves for:

* Game replay.
* Historical statistics.
* User game history.
* Analytics.

### CI/CD

Introduce an automated pipeline that performs:

```text
Build
 ↓
Unit Tests
 ↓
Integration Tests
 ↓
Frontend Tests
 ↓
Security/Dependency Checks
 ↓
Package
 ↓
Deploy
```

### Observability

Extend OpenTelemetry with centralized:

* Logs.
* Metrics.
* Distributed traces.
* Application dashboards.
* Alerting.

### API Enhancements

Potential future improvements include:

* More granular API error codes.
* Pagination for historical games.
* API authentication.
* Rate limiting.
* API documentation examples.
* Contract testing.

---

# Project Structure

```text
tic-tac-toe-assessment/
│
├── backend/
│   ├── TicTacToe.Domain/
│   ├── TicTacToe.Application/
│   ├── TicTacToe.Infrastructure/
│   ├── TicTacToe.Api/
│   ├── TicTacToe.Tests/
│   └── TicTacToe.sln
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   └── environments/
│   ├── angular.json
│   ├── package.json
│   └── proxy.conf.json
│
├── .github/
│   └── workflows/
│
├── global.json
├── API.md
├── REQUIREMENTS-TRACEABILITY.md
├── CONTRIBUTING.md
└── README.md
```

---

# Quick Start

## Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project TicTacToe.Api --launch-profile http
```

Backend:

```text
http://localhost:62167
```

Swagger:

```text
http://localhost:62167/swagger
```

## Frontend

Open another terminal:

```bash
cd frontend
npm install
npm start
```

Frontend:

```text
http://localhost:4200
```

## Tests

Backend:

```bash
cd backend
dotnet test
```

Frontend:

```bash
cd frontend
npm test
```

Production frontend build:

```bash
npm run build
```

---

# Submission Checklist

Before submitting the repository:

```text
[ ] dotnet restore
[ ] dotnet build
[ ] dotnet test

[ ] npm install
[ ] npm run build
[ ] npm test

[ ] Verify backend starts successfully
[ ] Verify frontend starts successfully
[ ] Verify Swagger at http://localhost:62167/swagger
[ ] Verify /health/live
[ ] Verify /health/ready

[ ] Review API.md
[ ] Review REQUIREMENTS-TRACEABILITY.md
[ ] Review CONTRIBUTING.md

[ ] Confirm no secrets or credentials are committed
[ ] Confirm node_modules/, bin/, obj/, dist/ and generated files are ignored
[ ] Confirm README reflects the final implementation
```

---

# Repository Documentation

* `README.md` — Project overview, setup, testing, design and implementation notes.
* `API.md` — Detailed API contract and endpoint documentation.
* `REQUIREMENTS-TRACEABILITY.md` — Mapping between assessment requirements and implementation.
* `CONTRIBUTING.md` — Development and code-review guidelines.
