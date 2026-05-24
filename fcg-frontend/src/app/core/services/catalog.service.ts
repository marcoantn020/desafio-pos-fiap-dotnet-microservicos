import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Game } from '../models/game.model';
import { LibraryItem, Order } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private base = environment.catalogApiUrl;

  constructor(private http: HttpClient) {}

  getGames(): Observable<Game[]> {
    return this.http.get<Game[]>(`${this.base}/games`);
  }

  placeOrder(gameId: string): Observable<{ id: string; status: string }> {
    return this.http.post<{ id: string; status: string }>(`${this.base}/orders`, { gameId });
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.base}/orders`);
  }

  getLibrary(): Observable<LibraryItem[]> {
    return this.http.get<LibraryItem[]>(`${this.base}/library`);
  }
}
