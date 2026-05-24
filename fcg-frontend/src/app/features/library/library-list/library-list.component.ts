import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, map } from 'rxjs';
import { CatalogService } from '../../../core/services/catalog.service';
import { LibraryItem } from '../../../core/models/order.model';
import { Game } from '../../../core/models/game.model';

interface LibraryItemWithGame extends LibraryItem {
  gameTitle: string;
  priceCents: number;
  currency: string;
}

@Component({
  selector: 'app-library-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './library-list.component.html',
  styleUrl: './library-list.component.scss',
})
export class LibraryListComponent implements OnInit {
  items: LibraryItemWithGame[] = [];
  loading = true;
  error = '';

  constructor(private catalog: CatalogService) {}

  ngOnInit() {
    forkJoin({
      library: this.catalog.getLibrary(),
      games: this.catalog.getGames(),
    }).pipe(
      map(({ library, games }) => {
        const gameMap = new Map<string, Game>(games.map(g => [g.id, g]));
        return library.map(item => ({
          ...item,
          gameTitle: gameMap.get(item.gameId)?.title ?? 'Jogo não encontrado',
          priceCents: gameMap.get(item.gameId)?.priceCents ?? 0,
          currency: gameMap.get(item.gameId)?.currency ?? 'BRL',
        }));
      })
    ).subscribe({
      next: (items) => { this.items = items; this.loading = false; },
      error: () => { this.error = 'Erro ao carregar biblioteca.'; this.loading = false; },
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('pt-BR');
  }

  formatPrice(cents: number, currency: string): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: currency || 'BRL',
    }).format(cents / 100);
  }
}
