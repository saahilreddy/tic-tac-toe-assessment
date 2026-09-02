import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateGameRequest, GameMode, GameState, MoveRequest, Scoreboard, VersionRequest } from './models';
import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  createGame(mode: GameMode): Observable<GameState> {
    const request: CreateGameRequest = { mode };
    return this.http.post<GameState>(`${this.baseUrl}/games`, request);
  }

  getGame(gameId: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/games/${gameId}`);
  }

  submitMove(request: MoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${request.gameId}/moves`, request);
  }

  undo(request: VersionRequest, gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/undo`, request);
  }

  resetGame(request: VersionRequest, gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/reset`, request);
  }

  getMoves(gameId: string): Observable<GameState['moveHistory']> {
    return this.http.get<GameState['moveHistory']>(`${this.baseUrl}/games/${gameId}/moves`);
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}
