export interface Order {
  id: string;
  userId: string;
  gameId: string;
  priceCents: number;
  currency: string;
  status: 'Pending' | 'Confirmed' | 'Cancelled';
  placedAtUtc: string;
}

export interface LibraryItem {
  id: string;
  userId: string;
  gameId: string;
  acquiredAtUtc: string;
}
