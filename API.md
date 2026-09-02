# API Contract — Tic Tac Toe v1

Base URL:

```text
http://localhost:5000/api/v1
```

Swagger UI:

```text
http://localhost:5000/swagger
```

## Resources

### Create game

`POST /games`

```json
{
  "mode": "TwoPlayer"
}
```

Modes:

- `TwoPlayer`
- `Computer`

Returns `201 Created` and a `GameStateResponse`.

### Get game

`GET /games/{id}`

Returns `200 OK` or `404 Not Found`.

### Get game state

`GET /games/{id}/state`

Equivalent explicit state representation of the game resource.

### Get move history

`GET /games/{id}/moves`

Returns the current move history.

### Submit move

`POST /games/{id}/moves`

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

- route/body game ID match
- game exists
- game is in progress
- row and column are valid
- cell is empty
- player is the current player
- Computer Mode accepts only human X moves
- expected version is current

Responses:

- `200 OK`
- `400 Bad Request`
- `404 Not Found`
- `409 Conflict`

### Undo

`POST /games/{id}/undo`

```json
{
  "expectedVersion": 2
}
```

Two Player Mode removes one move.

Computer Mode removes the human X move and computer O move together.

Undo is disabled after completion by design.

### Reset game

`POST /games/{id}/reset`

```json
{
  "expectedVersion": 5
}
```

Creates a fresh game session using the same game mode. Scoreboard is not changed.

### Scoreboard

`GET /scoreboard`

```json
{
  "xWins": 1,
  "oWins": 2,
  "draws": 1
}
```

### Reset scoreboard

`POST /scoreboard/reset`

Returns the reset scoreboard.

## Game state response

```json
{
  "gameId": "GUID",
  "version": 4,
  "board": ["X", "O", "", "", "X", "", "", "", ""],
  "currentPlayer": "O",
  "gameMode": "TwoPlayer",
  "gameStatus": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [
    {
      "moveNumber": 1,
      "player": "X",
      "row": 0,
      "column": 0,
      "cellIndex": 0
    }
  ],
  "scoreboard": {
    "xWins": 0,
    "oWins": 0,
    "draws": 0
  }
}
```

## Status codes

| Status | Meaning |
|---|---|
| 200 | Successful read/mutation |
| 201 | Game created |
| 400 | Invalid input or game rule violation |
| 404 | Game does not exist |
| 409 | Optimistic concurrency conflict |
| 500 | Unexpected server/domain infrastructure failure |

## Error contract

Unexpected and exceptional failures use `application/problem+json`.

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
