import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Observable, catchError, EMPTY, finalize, tap } from 'rxjs';
import { GameApiService } from './game-api.service';
import { GameMode, GameState, MoveRequest, Player } from './models';
import { getApiErrorMessage } from './error.interceptor';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  private readonly api = inject(GameApiService);

  readonly game = signal<GameState | null>(null);
  readonly selectedMode = signal<GameMode>('TwoPlayer');
  readonly busy = signal(false);
  readonly errorMessage = signal('');
  readonly noticeMessage = signal('');

  readonly canUndo = computed(() => {
    const game = this.game();
    return !!game && game.gameStatus === 'InProgress' && game.moveHistory.length > 0 && !this.busy();
  });

  readonly boardCells = Array.from({ length: 9 }, (_, index) => index);

  constructor() {
    this.startNewGame('TwoPlayer');
  }

  cellLabel(index: number): string {
    const row = Math.floor(index / 3) + 1;
    const column = (index % 3) + 1;
    return `Row ${row}, Column ${column}`;
  }

  isWinningCell(index: number): boolean {
    return this.game()?.winningCells.includes(index) ?? false;
  }

  isCellDisabled(index: number): boolean {
    const game = this.game();
    if (!game || this.busy() || game.gameStatus !== 'InProgress') {
      return true;
    }

    if (game.gameMode === 'Computer' && game.currentPlayer === 'O') {
      return true;
    }

    return game.board[index] !== '';
  }

  setMode(mode: GameMode): void {
    if (mode === this.selectedMode() && this.game()?.moveHistory.length === 0) {
      return;
    }

    this.selectedMode.set(mode);
    this.startNewGame(mode);
  }

  playCell(index: number): void {
    const game = this.game();
    if (!game || this.isCellDisabled(index)) {
      return;
    }

    const request: MoveRequest = {
      gameId: game.gameId,
      player: game.currentPlayer,
      row: Math.floor(index / 3),
      column: index % 3,
      expectedVersion: game.version
    };

    this.execute(
      this.api.submitMove(request),
      state => this.applyGame(state),
      false
    );
  }

  undo(): void {
    const game = this.game();
    if (!game || !this.canUndo()) {
      return;
    }

    this.execute(
      this.api.undo({ expectedVersion: game.version }, game.gameId),
      state => {
        this.applyGame(state);
        this.noticeMessage.set(game.gameMode === 'Computer'
          ? 'Last human/computer move pair undone.'
          : 'Last move undone.');
      },
      false
    );
  }

  resetGame(): void {
    const game = this.game();
    if (!game) {
      this.startNewGame(this.selectedMode());
      return;
    }

    this.execute(
      this.api.resetGame({ expectedVersion: game.version }, game.gameId),
      state => {
        this.applyGame(state);
        this.noticeMessage.set('New game started. Scoreboard was kept unchanged.');
      },
      false
    );
  }

  resetScoreboard(): void {
    this.execute(
      this.api.resetScoreboard().pipe(
        tap(scoreboard => {
          const current = this.game();
          if (current) {
            this.game.set({ ...current, scoreboard });
          }
        })
      ),
      () => this.noticeMessage.set('Scoreboard reset.'),
      false
    );
  }

  private startNewGame(mode: GameMode): void {
    this.selectedMode.set(mode);
    this.execute(
      this.api.createGame(mode),
      state => {
        this.applyGame(state);
        this.noticeMessage.set('');
      },
      true
    );
  }

  private applyGame(state: GameState): void {
    this.game.set(state);
    this.selectedMode.set(state.gameMode);
    this.errorMessage.set('');
  }

  private execute<T>(request$: Observable<T>, onSuccess: (value: T) => void, silent: boolean): void {
    this.busy.set(true);
    if (!silent) {
      this.errorMessage.set('');
    }

    request$.pipe(
      catchError(error => {
        this.errorMessage.set(getApiErrorMessage(error));
        return EMPTY;
      }),
      finalize(() => this.busy.set(false))
    ).subscribe({ next: onSuccess });
  }
}
