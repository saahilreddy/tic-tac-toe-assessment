export type Player = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'Computer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveHistoryItem {
  moveNumber: number;
  player: Player;
  row: number;
  column: number;
  cellIndex: number;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  version: number;
  board: string[];
  currentPlayer: Player;
  gameMode: GameMode;
  gameStatus: GameStatus;
  winner: Player | null;
  winningCells: number[];
  moveHistory: MoveHistoryItem[];
  scoreboard: Scoreboard;
}

export interface CreateGameRequest {
  mode: GameMode;
}

export interface MoveRequest {
  gameId: string;
  player: Player;
  row: number;
  column: number;
  expectedVersion: number;
}

export interface VersionRequest {
  expectedVersion: number;
}
