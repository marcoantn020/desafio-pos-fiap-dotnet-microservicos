import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, map } from 'rxjs';
import { CatalogService } from '../../../core/services/catalog.service';
import { Order } from '../../../core/models/order.model';
import { Game } from '../../../core/models/game.model';

interface OrderWithGame extends Order {
  gameTitle: string;
}

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss',
})
export class OrderListComponent implements OnInit {
  orders: OrderWithGame[] = [];
  loading = true;
  error = '';

  constructor(private catalog: CatalogService) {}

  ngOnInit() {
    forkJoin({
      orders: this.catalog.getOrders(),
      games: this.catalog.getGames(),
    }).pipe(
      map(({ orders, games }) => {
        const gameMap = new Map<string, Game>(games.map(g => [g.id, g]));
        return orders.map(o => ({
          ...o,
          gameTitle: gameMap.get(o.gameId)?.title ?? 'Jogo não encontrado',
        }));
      })
    ).subscribe({
      next: (orders) => { this.orders = orders; this.loading = false; },
      error: () => { this.error = 'Erro ao carregar pedidos.'; this.loading = false; },
    });
  }

  formatPrice(cents: number, currency: string): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: currency || 'BRL',
    }).format(cents / 100);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString('pt-BR');
  }
}
