Tic Tac Toe — Angular 19 + ASP.NET Core 10

1. Project Overview

Full-stack browser-based Tic Tac Toe implementation for the technical assessment.

Angular 19 + TypeScript frontend

ASP.NET Core 10 Web API backend

REST API under /api/v1

Backend-owned game state, move history, and scoreboard

Two Player and Play Against Computer modes

Mode-specific undo, win/draw detection, and winning-cell highlighting

Clean Architecture with Domain, Application, Infrastructure, and API layers

CQRS-style commands/queries with MediatR

FluentValidation, ProblemDetails, Swagger/OpenAPI, health checks

In-memory persistence

Automated backend and frontend tests

Detailed requirement mapping: REQUIREMENTS-TRACEABILITY.md

2. Tech Stack

Frontend

Technology

Version

Angular

19.2

TypeScript

5.7.2

RxJS

7.8.1

Zone.js

0.15

Node.js

22

npm

10+

Jasmine Core

5.6.x

@types/jasmine

5.1.x

Karma

6.4.4

Karma Jasmine

5.1.x

Karma Chrome Launcher

3.2.0

Karma Coverage

2.2.1

Angular Signals are used for reactive UI state and HttpClient for API communication.

Backend

.NET 10 / ASP.NET Core Web API

C#

Clean Architecture

CQRS-style commands and queries

MediatR

FluentValidation

Repository pattern with in-memory repositories

ProblemDetails

API Versioning

Swagger/OpenAPI

Health Checks

Structured logging with ILogger<T>

OpenTelemetry

xUnit

3. Features Implemented

Game

3 × 3 board

Two Player mode

Play Against Computer mode

Current-player tracking

Move history

Reset Game

Backend is the source of truth

Rules

Valid/invalid move validation

Row, column, and diagonal win detection

Draw detection

Winning-cell identification

No moves after game completion

Scoreboard update on completion

Computer

Computer is deterministic and follows the required priority:

Win if possible

Block X if necessary

Take center

Take a corner

Take any available cell

Undo

Two Player: remove the latest move

Computer mode: remove the human X move and corresponding computer O move

Undo disabled after completion

Scoreboard

Tracks:

X wins

O wins

Draws

Completed games update the scoreboard once using game-ID-based idempotency. Resetting a game does not reset the scoreboard.

Concurrency

Game mutations use optimistic concurrency through a Version/expectedVersion check. Stale mutations return 409 Conflict.

4. How to Run the Backend Locally

Prerequisites

.NET 10 SDK

Node.js 22

npm 10+

Git

Restore, build, and run

From the repository root:

cd backend
dotnet restore
dotnet build
dotnet run --project TicTacToe.Api --launch-profile http

Backend:

http://localhost:62167

Swagger:

http://localhost:62167/swagger

Health endpoints:

http://localhost:62167/health/live
http://localhost:62167/health/ready

5. How to Run the Frontend Locally

From the repository root:

cd frontend
npm install
npm start

Frontend:

http://localhost:4200

Local API requests use frontend/proxy.conf.json and are routed to:

http://localhost:62167

The frontend calls the versioned API under:

/api/v1

Production build:

npm run build

6. API Endpoint Summary

Base URL:

http://localhost:62167/api/v1

Method

Endpoint

Purpose

POST

/games

Create game

GET

/games/{id}

Get game

GET

/games/{id}/state

Get current state

GET

/games/{id}/moves

Get move history

POST

/games/{id}/moves

Submit move

POST

/games/{id}/undo

Undo

POST

/games/{id}/reset

Reset game

GET

/scoreboard

Get scoreboard

POST

/scoreboard/reset

Reset scoreboard

Move requests include the game ID, player, row/column, and expectedVersion.

The backend validates game existence, game status, coordinates, cell availability, turn ownership, computer-mode rules, and expected version.

Common responses:

200 OK

201 Created

400 Bad Request

404 Not Found

409 Conflict

500 Internal Server Error

Detailed API contract and examples: API.md

7. How to Run Tests

Backend

cd backend
dotnet test

For the CI-style Release run:

dotnet test --no-build --configuration Release

The repository targets .NET 10 and opts into Microsoft Testing Platform through global.json.

Coverage includes:

Valid and invalid moves

Turn switching

Row/column/diagonal wins

Draws

Reset

Two Player undo

Computer-mode undo

Scoreboard behavior and idempotency

Computer move selection

Move-after-completion handling

API/HTTP behavior and error responses

Frontend

cd frontend
npm test

This runs:

ng test --watch=false --browsers=ChromeHeadless

Frontend tests cover game state, move handling, turn changes, win/draw state, undo, reset, scoreboard, and computer-mode UI behavior.

8. AI Tools and Prompt Summary

AI-assisted development was used as a development aid.

Main areas

Requirement-to-architecture mapping

Clean Architecture and CQRS structure

API contract and validation design

Game rules and computer-move strategy

Optimistic concurrency

Test scenario generation/review

Angular Signals and service structure

Troubleshooting build/test issues

README/API documentation

Representative prompt types

Design a Clean Architecture structure for an ASP.NET Core Tic Tac Toe API.

Design backend-owned game state, move history, scoreboard, and undo behavior.

Design optimistic concurrency using a game version and expectedVersion.

Create the required deterministic computer strategy: win, block, center, corner, any.

Suggest unit and integration tests for the assessment scenarios.

Review Angular Signals usage while keeping the backend as the source of truth.

AI output was reviewed and adapted against the assignment, existing architecture, Angular/.NET conventions, testability, and maintainability. Final implementation decisions were manually reviewed.

9. Design Decisions

Clean Architecture

Dependency direction is inward:

API → Application → Domain
Infrastructure → Application / Domain abstractions

The Domain does not depend on ASP.NET Core, persistence, or Infrastructure implementations.

Domain-Owned Rules

Game and GameRules own game-specific rules such as move validation, turn handling, win/draw detection, winning cells, completion, and undo state reconstruction.

CQRS / MediatR

Commands handle state changes; queries handle reads. MediatR coordinates the application use cases.

Repository Abstraction

IGameRepository and IScoreboardRepository abstract persistence. The current implementation is in-memory and can be replaced without moving business rules into Infrastructure.

Backend as Source of Truth

The frontend renders the latest state returned by the backend rather than maintaining an independent authoritative game state.

Optimistic Concurrency

Mutations carry expectedVersion. A mismatch produces 409 Conflict instead of silently overwriting newer state.

Angular Signals

Signals provide reactive UI state for board, player, status, winner, moves, and scoreboard.

No SignalR

REST is sufficient for the assessment's single-browser interaction model; real-time server push was not required.

10. Clarifications and Assumptions

Storage

In-memory persistence is used, which is permitted by the assessment.

Game Session

Each game has a generated game ID and is owned by the backend for the lifetime of the running application.

Undo

Two Player: remove one move

Computer mode: remove the X/O pair

The implementation follows Clarification Option A from the assessment: Undo is disabled after a game is completed, so the completed scoreboard result remains final.

Scoreboard

The scoreboard is session-level. Reset Game does not reset it. A completed game contributes only once.

Computer

Human player = X; computer = O. The strategy is deterministic and rule-based, not AI/ML-driven.

API

The API is versioned under /api/v1 to provide a clear boundary for future changes.

11. Known Limitations

Game and scoreboard data are lost when the API restarts.

Multiple backend instances do not share the same in-memory state.

The solution is intended for local/single-process assessment use.

The computer opponent uses a basic deterministic strategy rather than Minimax or adaptive AI.

No authentication or user-specific game ownership.

No SignalR/WebSocket real-time multiplayer.

Frontend tests require a compatible Chrome/Chromium environment for ChromeHeadless.

12. Future Improvements

Replace in-memory repositories with SQL Server, PostgreSQL, or Cosmos DB.

Support distributed deployments with shared persistent state.

Add authentication and user-specific games/scoreboards.

Add SignalR for real-time multiplayer and live updates.

Add stronger computer AI and difficulty levels.

Persist completed games for replay and statistics.

Expand CI/CD with security, packaging, and deployment stages.

Centralize OpenTelemetry logs, metrics, traces, dashboards, and alerting.

Add further API contract testing and advanced error handling.

Project Structure

tic-tac-toe-assessment/
├── backend/
│   ├── TicTacToe.Domain/
│   ├── TicTacToe.Application/
│   ├── TicTacToe.Infrastructure/
│   ├── TicTacToe.Api/
│   ├── TicTacToe.Tests/
│   └── TicTacToe.sln
├── frontend/
│   ├── src/
│   ├── angular.json
│   ├── package.json
│   └── proxy.conf.json
├── .github/workflows/
├── global.json
├── API.md
├── REQUIREMENTS-TRACEABILITY.md
├── CONTRIBUTING.md
└── README.md

Supporting Documentation

API.md — detailed API contract

REQUIREMENTS-TRACEABILITY.md — assignment-to-implementation mapping

CONTRIBUTING.md — development and review guidelines