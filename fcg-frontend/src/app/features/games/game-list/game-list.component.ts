import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CatalogService } from '../../../core/services/catalog.service';
import { Game } from '../../../core/models/game.model';

@Component({
  selector: 'app-game-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game-list.component.html',
  styleUrl: './game-list.component.scss',
})
export class GameListComponent implements OnInit {
  games: Game[] = [];
  loading = true;
  error = '';
  buyingId: string | null = null;
  successMessage = '';

  constructor(private catalog: CatalogService, private router: Router) {}

  ngOnInit() {
    this.catalog.getGames().subscribe({
      next: (games) => { this.games = games; this.loading = false; },
      error: () => { this.error = 'Erro ao carregar jogos.'; this.loading = false; },
    });
  }

  buy(game: Game) {
    if (this.buyingId) return;
    this.buyingId = game.id;
    this.successMessage = '';
    this.catalog.placeOrder(game.id).subscribe({
      next: () => {
        this.successMessage = `Pedido de "${game.title}" realizado! Aguarde a confirmação.`;
        this.buyingId = null;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao realizar pedido.';
        this.buyingId = null;
      },
    });
  }

  formatPrice(cents: number, currency: string): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: currency || 'BRL',
    }).format(cents / 100);
  }

  goToOrders() {
    this.router.navigate(['/orders']);
  }
}
