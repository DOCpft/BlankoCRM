export interface Client {
  id: string;
  name: string;
  phone?: string;
  network: string;
}

export interface Service {
  id: string;
  name: string;
  price: number;
  duration: number;
  description?: string;
}
