import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { GameApiService } from './game-api.service';
import {
  GameMode,
  GameState,
  MoveRequest,
  Scoreboard,
  VersionRequest
} from './models';
import { environment } from '../environments/environment';

describe('GameApiService', () => {
  let service: GameApiService;
  let httpMock: HttpTestingController;

  const baseUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        GameApiService
      ]
    });

    service = TestBed.inject(GameApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('createGame', () => {
    it('should create a game with the selected mode', () => {
      const mode: GameMode = 'TwoPlayer';

      const expectedGame: GameState = {
        gameId: 'game-123',
        version: 1,
        board: ['', '', '', '', '', '', '', '', ''],
        currentPlayer: 'X',
        gameMode: 'TwoPlayer',
        gameStatus: 'InProgress',
        winner: null,
        winningCells: [],
        moveHistory: [],
        scoreboard: {
          xWins: 0,
          oWins: 0,
          draws: 0
        }
      };

      service.createGame(mode).subscribe(game => {
        expect(game).toEqual(expectedGame);
      });

      const req = httpMock.expectOne(`${baseUrl}/games`);

      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        mode: 'TwoPlayer'
      });

      req.flush(expectedGame);
    });
  });

  describe('getGame', () => {
    it('should get a game by id', () => {
      const gameId = 'game-123';

      const expectedGame: GameState = {
        gameId,
        version: 2,
        board: [
          'X', 'O', '',
          '', 'X', '',
          '', '', ''
        ],
        currentPlayer: 'O',
        gameMode: 'TwoPlayer',
        gameStatus: 'InProgress',
        winner: null,
        winningCells: [],
        moveHistory: [
          {
            moveNumber: 1,
            player: 'X',
            row: 0,
            column: 0,
            cellIndex: 0
          },
          {
            moveNumber: 2,
            player: 'O',
            row: 0,
            column: 1,
            cellIndex: 1
          }
        ],
        scoreboard: {
          xWins: 0,
          oWins: 0,
          draws: 0
        }
      };

      service.getGame(gameId).subscribe(game => {
        expect(game).toEqual(expectedGame);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/games/${gameId}`
      );

      expect(req.request.method).toBe('GET');

      req.flush(expectedGame);
    });
  });

  describe('submitMove', () => {
    it('should submit a move and return the updated game state', () => {
      const moveRequest: MoveRequest = {
        gameId: 'game-123',
        player: 'X',
        row: 1,
        column: 1,
        expectedVersion: 2
      };

      const expectedGame: GameState = {
        gameId: 'game-123',
        version: 3,
        board: [
          'X', 'O', '',
          '', 'X', '',
          '', '', ''
        ],
        currentPlayer: 'O',
        gameMode: 'TwoPlayer',
        gameStatus: 'InProgress',
        winner: null,
        winningCells: [],
        moveHistory: [
          {
            moveNumber: 1,
            player: 'X',
            row: 0,
            column: 0,
            cellIndex: 0
          },
          {
            moveNumber: 2,
            player: 'O',
            row: 0,
            column: 1,
            cellIndex: 1
          },
          {
            moveNumber: 3,
            player: 'X',
            row: 1,
            column: 1,
            cellIndex: 4
          }
        ],
        scoreboard: {
          xWins: 0,
          oWins: 0,
          draws: 0
        }
      };

      service.submitMove(moveRequest).subscribe(game => {
        expect(game).toEqual(expectedGame);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/games/${moveRequest.gameId}/moves`
      );

      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(moveRequest);

      req.flush(expectedGame);
    });
  });

  describe('undo', () => {
    it('should undo the game using the expected version', () => {
      const gameId = 'game-123';

      const versionRequest: VersionRequest = {
        expectedVersion: 3
      };

      const expectedGame: GameState = {
        gameId,
        version: 2,
        board: [
          'X', 'O', '',
          '', '', '',
          '', '', ''
        ],
        currentPlayer: 'X',
        gameMode: 'TwoPlayer',
        gameStatus: 'InProgress',
        winner: null,
        winningCells: [],
        moveHistory: [
          {
            moveNumber: 1,
            player: 'X',
            row: 0,
            column: 0,
            cellIndex: 0
          },
          {
            moveNumber: 2,
            player: 'O',
            row: 0,
            column: 1,
            cellIndex: 1
          }
        ],
        scoreboard: {
          xWins: 0,
          oWins: 0,
          draws: 0
        }
      };

      service.undo(versionRequest, gameId).subscribe(game => {
        expect(game).toEqual(expectedGame);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/games/${gameId}/undo`
      );

      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(versionRequest);

      req.flush(expectedGame);
    });
  });

  describe('resetGame', () => {
    it('should reset the game', () => {
      const gameId = 'game-123';

      const versionRequest: VersionRequest = {
        expectedVersion: 4
      };

      const expectedGame: GameState = {
        gameId,
        version: 5,
        board: ['', '', '', '', '', '', '', '', ''],
        currentPlayer: 'X',
        gameMode: 'TwoPlayer',
        gameStatus: 'InProgress',
        winner: null,
        winningCells: [],
        moveHistory: [],
        scoreboard: {
          xWins: 0,
          oWins: 0,
          draws: 0
        }
      };

      service.resetGame(versionRequest, gameId).subscribe(game => {
        expect(game).toEqual(expectedGame);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/games/${gameId}/reset`
      );

      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(versionRequest);

      req.flush(expectedGame);
    });
  });

  describe('getMoves', () => {
    it('should get the move history for a game', () => {
      const gameId = 'game-123';

      const expectedMoves: GameState['moveHistory'] = [
        {
          moveNumber: 1,
          player: 'X',
          row: 0,
          column: 0,
          cellIndex: 0
        },
        {
          moveNumber: 2,
          player: 'O',
          row: 1,
          column: 1,
          cellIndex: 4
        }
      ];

      service.getMoves(gameId).subscribe(moves => {
        expect(moves).toEqual(expectedMoves);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/games/${gameId}/moves`
      );

      expect(req.request.method).toBe('GET');

      req.flush(expectedMoves);
    });
  });

  describe('getScoreboard', () => {
    it('should get the scoreboard', () => {
      const expectedScoreboard: Scoreboard = {
        xWins: 5,
        oWins: 3,
        draws: 2
      };

      service.getScoreboard().subscribe(scoreboard => {
        expect(scoreboard).toEqual(expectedScoreboard);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/scoreboard`
      );

      expect(req.request.method).toBe('GET');

      req.flush(expectedScoreboard);
    });
  });

  describe('resetScoreboard', () => {
    it('should reset the scoreboard', () => {
      const expectedScoreboard: Scoreboard = {
        xWins: 0,
        oWins: 0,
        draws: 0
      };

      service.resetScoreboard().subscribe(scoreboard => {
        expect(scoreboard).toEqual(expectedScoreboard);
      });

      const req = httpMock.expectOne(
        `${baseUrl}/scoreboard/reset`
      );

      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});

      req.flush(expectedScoreboard);
    });
  });
});