### Frontend

* Angular 19.2
* TypeScript 5.7
* RxJS 7.8.1
* Zone.js 0.15
* Node.js 22
* npm 10+
* Jasmine Core 7.0.2
* Jasmine Type Definitions 6.0.0
* Karma 6.4.4
* Karma Jasmine 5.1.0
* Karma Chrome Launcher 3.2.0
* Karma Jasmine HTML Reporter 2.3.0

The frontend unit-test stack uses Jasmine 7.0.2 with Karma.

---

## 21. Testing Strategy

Two backend/browser levels are represented:

```text
Tests
├── Domain unit tests
│   ├── GameRules
│   ├── winners
│   ├── draw
│   ├── undo state rebuild
│   └── computer move priority
│
├── Application tests
│   ├── command handlers
│   ├── scoreboard idempotency
│   ├── computer mode undo
│   └── optimistic concurrency
│
└── API integration tests
    ├── create game
    └── invalid move / ProblemDetails

Frontend
└── Jasmine / Karma unit tests
    ├── game state
    ├── move handling
    ├── undo
    ├── reset
    ├── scoreboard
    └── computer mode
```

Required assessment scenarios are covered by the domain, application, API integration, and frontend unit tests.

---

## 22. Local Setup

### Prerequisites

Install:

* .NET 10 SDK
* Node.js 22
* npm 10+
* Git

### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet test
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

Health:

```text
http://localhost:62167/health/live
http://localhost:62167/health/ready
```

### Frontend

```bash
cd frontend
npm install
npm start
```

Angular:

```text
http://localhost:4200
```

### Frontend unit tests

Run the Angular Jasmine/Karma test suite:

```bash
npm test
```

This executes:

```text
ng test --watch=false --browsers=ChromeHeadless
```

The relevant testing packages are:

```json
{
  "@types/jasmine": "^6.0.0",
  "jasmine-core": "^7.0.2",
  "karma": "^6.4.4",
  "karma-chrome-launcher": "^3.2.0",
  "karma-jasmine": "^5.1.0",
  "karma-jasmine-html-reporter": "^2.3.0"
}
```

---

## 24. Project Structure

```text
tic-tac-toe-assessment/
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
├── global.json
├── API.md
├── REQUIREMENTS-TRACEABILITY.md
├── CONTRIBUTING.md
└── README.md
```

---

## 29. Submission Checklist

Before submission:

```text
[ ] dotnet restore
[ ] dotnet build
[ ] dotnet test
[ ] npm install
[ ] npm run build
[ ] npm test
[ ] Verify Swagger at http://localhost:62167/swagger
[ ] Verify /health/live at http://localhost:62167/health/live
[ ] Verify /health/ready at http://localhost:62167/health/ready
[ ] Review API.md
[ ] Review REQUIREMENTS-TRACEABILITY.md
[ ] Review CONTRIBUTING.md
```
