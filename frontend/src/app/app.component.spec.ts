import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AppComponent } from './app.component';
import { GameApiService } from './game-api.service';
import {
  GameMode,
  GameState,
  MoveRequest,
  Scoreboard,
  VersionRequest
} from './models';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let apiService: jasmine.SpyObj<GameApiService>;

  const initialGame: GameState = {
    gameId: 'game-123',
    version: 0,
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

  const gameAfterMove: GameState = {
    ...initialGame,
    version: 1,
    board: ['X', '', '', '', '', '', '', '', ''],
    currentPlayer: 'O',
    moveHistory: [
      {
        moveNumber: 1,
        player: 'X',
        row: 0,
        column: 0,
        cellIndex: 0
      }
    ]
  };

  const wonGame: GameState = {
    ...gameAfterMove,
    version: 5,
    board: ['X', 'X', 'X', 'O', 'O', '', '', '', ''],
    currentPlayer: 'O',
    gameStatus: 'Won',
    winner: 'X',
    winningCells: [0, 1, 2],
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
        row: 1,
        column: 0,
        cellIndex: 3
      },
      {
        moveNumber: 3,
        player: 'X',
        row: 0,
        column: 1,
        cellIndex: 1
      },
      {
        moveNumber: 4,
        player: 'O',
        row: 1,
        column: 1,
        cellIndex: 4
      },
      {
        moveNumber: 5,
        player: 'X',
        row: 0,
        column: 2,
        cellIndex: 2
      }
    ],
    scoreboard: {
      xWins: 1,
      oWins: 0,
      draws: 0
    }
  };

  const drawGame: GameState = {
    ...initialGame,
    version: 9,
    board: [
      'X', 'O', 'X',
      'X', 'O', 'O',
      'O', 'X', 'X'
    ],
    currentPlayer: 'O',
    gameStatus: 'Draw',
    winner: null,
    winningCells: [],
    moveHistory: []
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj<GameApiService>(
      'GameApiService',
      [
        'createGame',
        'getGame',
        'submitMove',
        'undo',
        'resetGame',
        'getMoves',
        'getScoreboard',
        'resetScoreboard'
      ]
    );

    // AppComponent constructor automatically calls createGame().
    apiService.createGame.and.returnValue(of(initialGame));

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        {
          provide: GameApiService,
          useValue: apiService
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  // -------------------------------------------------------
  // Component creation
  // -------------------------------------------------------

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------
  // Initial game creation
  // -------------------------------------------------------

  it('should create a new TwoPlayer game when component is initialized', () => {
    expect(apiService.createGame).toHaveBeenCalledWith('TwoPlayer');
    expect(component.game()).toEqual(initialGame);
  });

  // -------------------------------------------------------
  // HTML rendering
  // -------------------------------------------------------

  it('should display the Tic Tac Toe heading', () => {
    const heading = fixture.nativeElement.querySelector('h1');

    expect(heading).toBeTruthy();
    expect(heading.textContent.trim()).toBe('Tic Tac Toe');
  });

  it('should render 9 board cells', () => {
    const cells = fixture.nativeElement.querySelectorAll('.cell');

    expect(cells.length).toBe(9);
  });

  it('should display the current player when game is in progress', () => {
    const turnCard = fixture.nativeElement.querySelector('.turn-card');

    expect(turnCard.textContent).toContain('Current turn');
    expect(turnCard.textContent).toContain('X');
  });

  it('should display the correct game status message', () => {
    const statusBanner =
      fixture.nativeElement.querySelector('.status-banner');

    expect(statusBanner.textContent)
      .toContain('Player X, select a cell.');
  });

  // -------------------------------------------------------
  // Game mode
  // -------------------------------------------------------

  it('should mark Two Player button as active initially', () => {
    const buttons =
      fixture.nativeElement.querySelectorAll('.mode-group button');

    expect(buttons[0].classList.contains('active')).toBeTrue();
    expect(buttons[1].classList.contains('active')).toBeFalse();
  });

  it('should change mode to Computer', () => {
    apiService.createGame.and.returnValue(
      of({
        ...initialGame,
        gameMode: 'Computer'
      })
    );

    component.setMode('Computer');
    fixture.detectChanges();

    expect(component.selectedMode()).toBe('Computer');
    expect(apiService.createGame).toHaveBeenCalledWith('Computer');
  });

  it('should start a new game when switching game mode', () => {
    component.setMode('Computer');

    expect(apiService.createGame).toHaveBeenCalledWith('Computer');
  });

  // -------------------------------------------------------
  // Cell label
  // -------------------------------------------------------

  it('should generate the correct cell label', () => {
    expect(component.cellLabel(0)).toBe('Row 1, Column 1');
    expect(component.cellLabel(4)).toBe('Row 2, Column 2');
    expect(component.cellLabel(8)).toBe('Row 3, Column 3');
  });

  it('should render correct aria labels for board cells', () => {
    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    expect(cells[0].getAttribute('aria-label'))
      .toBe('Row 1, Column 1');

    expect(cells[4].getAttribute('aria-label'))
      .toBe('Row 2, Column 2');

    expect(cells[8].getAttribute('aria-label'))
      .toBe('Row 3, Column 3');
  });

  // -------------------------------------------------------
  // Cell disabled state
  // -------------------------------------------------------

  it('should disable cells when there is no game', () => {
    component.game.set(null);
    fixture.detectChanges();

    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    cells.forEach((cell: HTMLButtonElement) => {
      expect(cell.disabled).toBeTrue();
    });
  });

  it('should disable a cell that is already occupied', () => {
    component.game.set(gameAfterMove);
    fixture.detectChanges();

    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    expect(cells[0].disabled).toBeTrue();
    expect(cells[1].disabled).toBeFalse();
  });

  it('should disable all cells when game is won', () => {
    component.game.set(wonGame);
    fixture.detectChanges();

    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    cells.forEach((cell: HTMLButtonElement) => {
      expect(cell.disabled).toBeTrue();
    });
  });

  it('should disable cells when it is computer turn', () => {
    const computerGame: GameState = {
      ...initialGame,
      gameMode: 'Computer',
      currentPlayer: 'O'
    };

    component.game.set(computerGame);
    fixture.detectChanges();

    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    cells.forEach((cell: HTMLButtonElement) => {
      expect(cell.disabled).toBeTrue();
    });
  });

  // -------------------------------------------------------
  // Playing a cell
  // -------------------------------------------------------

  it('should submit a move when an available cell is clicked', () => {
    apiService.submitMove.and.returnValue(of(gameAfterMove));

    component.playCell(0);

    const expectedRequest: MoveRequest = {
      gameId: 'game-123',
      player: 'X',
      row: 0,
      column: 0,
      expectedVersion: 0
    };

    expect(apiService.submitMove)
      .toHaveBeenCalledWith(expectedRequest);
  });

  it('should update the game after a successful move', () => {
    apiService.submitMove.and.returnValue(of(gameAfterMove));

    component.playCell(0);

    expect(component.game()).toEqual(gameAfterMove);
  });

  it('should not submit a move when the cell is already occupied', () => {
    component.game.set(gameAfterMove);

    apiService.submitMove.calls.reset();

    component.playCell(0);

    expect(apiService.submitMove).not.toHaveBeenCalled();
  });

  // -------------------------------------------------------
  // Undo
  // -------------------------------------------------------

  it('should allow undo when there are moves in progress game', () => {
    component.game.set(gameAfterMove);
    fixture.detectChanges();

    expect(component.canUndo()).toBeTrue();
  });

  it('should not allow undo when there are no moves', () => {
    component.game.set(initialGame);

    expect(component.canUndo()).toBeFalse();
  });

  it('should call undo API with expected version and game id', () => {
    component.game.set(gameAfterMove);

    apiService.undo.and.returnValue(of(initialGame));

    component.undo();

    expect(apiService.undo).toHaveBeenCalledWith(
      { expectedVersion: 1 },
      'game-123'
    );
  });

  it('should display undo notice after successful undo', () => {
    component.game.set(gameAfterMove);

    apiService.undo.and.returnValue(of(initialGame));

    component.undo();
    fixture.detectChanges();

    expect(component.noticeMessage())
      .toBe('Last move undone.');
  });

  // -------------------------------------------------------
  // Reset Game
  // -------------------------------------------------------

  it('should call resetGame API', () => {
    component.game.set(gameAfterMove);

    apiService.resetGame.and.returnValue(of(initialGame));

    component.resetGame();

    expect(apiService.resetGame).toHaveBeenCalledWith(
      { expectedVersion: 1 },
      'game-123'
    );
  });

  it('should display reset game notice after successful reset', () => {
    component.game.set(gameAfterMove);

    apiService.resetGame.and.returnValue(of(initialGame));

    component.resetGame();
    fixture.detectChanges();

    expect(component.noticeMessage())
      .toBe('New game started. Scoreboard was kept unchanged.');
  });

  // -------------------------------------------------------
  // Reset scoreboard
  // -------------------------------------------------------

  it('should call resetScoreboard API', () => {
    const resetScoreboard: Scoreboard = {
      xWins: 0,
      oWins: 0,
      draws: 0
    };

    apiService.resetScoreboard.and.returnValue(
      of(resetScoreboard)
    );

    component.resetScoreboard();

    expect(apiService.resetScoreboard).toHaveBeenCalled();
  });

  it('should update scoreboard after resetting scoreboard', () => {
    const resetScoreboard: Scoreboard = {
      xWins: 0,
      oWins: 0,
      draws: 0
    };

    const currentGame: GameState = {
      ...wonGame
    };

    component.game.set(currentGame);

    apiService.resetScoreboard.and.returnValue(
      of(resetScoreboard)
    );

    component.resetScoreboard();

    expect(component.game()?.scoreboard)
      .toEqual(resetScoreboard);
  });

  it('should display scoreboard values', () => {
    component.game.set(wonGame);
    fixture.detectChanges();

    const scoreGrid =
      fixture.nativeElement.querySelector('.score-grid');

    expect(scoreGrid.textContent).toContain('1');
    expect(scoreGrid.textContent).toContain('X wins');
    expect(scoreGrid.textContent).toContain('O wins');
    expect(scoreGrid.textContent).toContain('Draws');
  });

  // -------------------------------------------------------
  // Winning game
  // -------------------------------------------------------

  it('should display winner when game is won', () => {
    component.game.set(wonGame);
    fixture.detectChanges();

    const turnCard =
      fixture.nativeElement.querySelector('.turn-card');

    expect(turnCard.textContent).toContain('Winner');
    expect(turnCard.textContent).toContain('Player X');
  });

  it('should display winning game status', () => {
    component.game.set(wonGame);
    fixture.detectChanges();

    const statusBanner =
      fixture.nativeElement.querySelector('.status-banner');

    expect(statusBanner.textContent)
      .toContain('Player X wins');

    expect(statusBanner.classList.contains('won'))
      .toBeTrue();
  });

  it('should highlight winning cells', () => {
    component.game.set(wonGame);
    fixture.detectChanges();

    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    expect(cells[0].classList.contains('winning')).toBeTrue();
    expect(cells[1].classList.contains('winning')).toBeTrue();
    expect(cells[2].classList.contains('winning')).toBeTrue();

    expect(cells[3].classList.contains('winning')).toBeFalse();
  });

  // -------------------------------------------------------
  // Draw game
  // -------------------------------------------------------

  it('should display draw result', () => {
    component.game.set(drawGame);
    fixture.detectChanges();

    const turnCard =
      fixture.nativeElement.querySelector('.turn-card');

    expect(turnCard.textContent).toContain('Result');
    expect(turnCard.textContent).toContain('Draw');
  });

  it('should display draw status banner', () => {
    component.game.set(drawGame);
    fixture.detectChanges();

    const statusBanner =
      fixture.nativeElement.querySelector('.status-banner');

    expect(statusBanner.textContent)
      .toContain('The game is a draw.');

    expect(statusBanner.classList.contains('draw'))
      .toBeTrue();
  });

  // -------------------------------------------------------
  // Computer mode
  // -------------------------------------------------------

  it('should display computer thinking message', () => {
    const computerGame: GameState = {
      ...initialGame,
      gameMode: 'Computer',
      currentPlayer: 'O'
    };

    component.game.set(computerGame);
    fixture.detectChanges();

    const statusBanner =
      fixture.nativeElement.querySelector('.status-banner');

    expect(statusBanner.textContent)
      .toContain('Computer is choosing a move');
  });

  it('should display computer game metadata', () => {
    const computerGame: GameState = {
      ...initialGame,
      gameMode: 'Computer'
    };

    component.game.set(computerGame);
    fixture.detectChanges();

    const gameMeta =
      fixture.nativeElement.querySelector('.game-meta');

    expect(gameMeta.textContent)
      .toContain('Human: X · Computer: O');
  });

  it('should display Two Player game metadata', () => {
    component.game.set(initialGame);
    fixture.detectChanges();

    const gameMeta =
      fixture.nativeElement.querySelector('.game-meta');

    expect(gameMeta.textContent)
      .toContain('X vs O');
  });

  // -------------------------------------------------------
  // Move history
  // -------------------------------------------------------

  it('should display empty move history message when there are no moves', () => {
    component.game.set(initialGame);
    fixture.detectChanges();

    const emptyState =
      fixture.nativeElement.querySelector('.empty-state');

    expect(emptyState).toBeTruthy();
    expect(emptyState.textContent)
      .toContain('No moves yet.');
  });

  it('should display move history when moves exist', () => {
    component.game.set(gameAfterMove);
    fixture.detectChanges();

    const rows =
      fixture.nativeElement.querySelectorAll('tbody tr');

    expect(rows.length).toBe(1);

    expect(rows[0].textContent).toContain('1');
    expect(rows[0].textContent).toContain('X');
    expect(rows[0].textContent)
      .toContain('Row 1, Column 1');
  });

  it('should display the number of moves in move history heading', () => {
    component.game.set(gameAfterMove);
    fixture.detectChanges();

    const panelHeadings =
      fixture.nativeElement.querySelectorAll('.panel-heading');

    const moveHistoryHeading = panelHeadings[1];

    expect(moveHistoryHeading.textContent)
      .toContain('1 moves');
  });

  // -------------------------------------------------------
  // Error handling
  // -------------------------------------------------------

  it('should display an error message when API call fails', () => {
    const error = {
      status: 500,
      error: {
        message: 'Something went wrong'
      }
    };

    apiService.submitMove.and.returnValue(
      throwError(() => error)
    );

    component.playCell(0);
    fixture.detectChanges();

    const alert =
      fixture.nativeElement.querySelector('[role="alert"]');

    expect(alert).toBeTruthy();
    expect(component.errorMessage()).not.toBe('');
  });

  // -------------------------------------------------------
  // Busy state
  // -------------------------------------------------------

  it('should set busy to false after successful API request', () => {
    apiService.submitMove.and.returnValue(of(gameAfterMove));

    component.playCell(0);

    expect(component.busy()).toBeFalse();
  });

  // -------------------------------------------------------
  // Button interactions
  // -------------------------------------------------------

  it('should call resetGame when Reset Game button is clicked', () => {
    component.game.set(gameAfterMove);

    apiService.resetGame.and.returnValue(of(initialGame));

    fixture.detectChanges();

    const buttons =
      fixture.nativeElement.querySelectorAll('.action-group button');

    const resetGameButton = buttons[1] as HTMLButtonElement;

    resetGameButton.click();

    expect(apiService.resetGame).toHaveBeenCalled();
  });

  it('should call resetScoreboard when Reset Scoreboard button is clicked', () => {
    apiService.resetScoreboard.and.returnValue(
      of(initialGame.scoreboard)
    );

    fixture.detectChanges();

    const buttons =
      fixture.nativeElement.querySelectorAll('.action-group button');

    const resetScoreboardButton = buttons[2] as HTMLButtonElement;

    resetScoreboardButton.click();

    expect(apiService.resetScoreboard).toHaveBeenCalled();
  });

  it('should change to Computer mode when Computer button is clicked', () => {
    apiService.createGame.and.returnValue(
      of({
        ...initialGame,
        gameMode: 'Computer'
      })
    );

    fixture.detectChanges();

    const buttons =
      fixture.nativeElement.querySelectorAll('.mode-group button');

    const computerButton = buttons[1] as HTMLButtonElement;

    computerButton.click();

    expect(apiService.createGame)
      .toHaveBeenCalledWith('Computer');
  });

  // -------------------------------------------------------
  // Accessibility
  // -------------------------------------------------------

  it('should have a board with grid role', () => {
    const board =
      fixture.nativeElement.querySelector('.board');

    expect(board.getAttribute('role')).toBe('grid');
    expect(board.getAttribute('aria-label'))
      .toBe('Tic Tac Toe board');
  });

  it('should have gridcell role on each board cell', () => {
    const cells =
      fixture.nativeElement.querySelectorAll('.cell');

    cells.forEach((cell: HTMLElement) => {
      expect(cell.getAttribute('role'))
        .toBe('gridcell');
    });
  });
});