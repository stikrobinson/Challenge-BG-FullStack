import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface Product {
  id: number;
  nombre: string;
}

export interface Supplier {
  id: number;
  nombre: string;
  direccion: string;
  ruc: string;
}

export interface Lot {
  idProducto: number;
  nombreProducto: string;
  idProveedor: number;
  nombreProveedor: string;
  precio: number;
  stock: number;
}

export interface ProductInput {
  nombre: string;
}

export interface SupplierInput {
  nombre: string;
  direccion: string;
  ruc: string;
}

export interface LotInput {
  idProducto: number;
  idProveedor: number;
  precio: number;
  stock: number;
}

export interface LotUpdate {
  precio: number;
  stock: number;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  constructor(private readonly http: HttpClient) {}

  getProducts() {
    return this.http.get<Product[]>('/api/productos');
  }

  getLots() {
    return this.http.get<Lot[]>('/api/lotes');
  }

  getSuppliers() {
    return this.http.get<Supplier[]>('/api/proveedores');
  }

  createProduct(product: ProductInput) {
    return this.http.post<Product>('/api/productos', product);
  }

  createSupplier(supplier: SupplierInput) {
    return this.http.post<Supplier>('/api/proveedores', supplier);
  }

  deleteSupplier(id: number) {
    return this.http.delete<void>(`/api/proveedores/${id}`);
  }

  updateProduct(id: number, product: ProductInput) {
    return this.http.put<void>(`/api/productos/${id}`, product);
  }

  deleteProduct(id: number) {
    return this.http.delete<void>(`/api/productos/${id}`);
  }

  createLot(lot: LotInput) {
    return this.http.post<Lot>(`/api/productos/${lot.idProducto}/lotes`, lot);
  }

  updateLot(productId: number, supplierId: number, lot: LotUpdate) {
    return this.http.put<void>(
      `/api/lotes/${productId}/${supplierId}`,
      lot,
    );
  }

  deleteLot(productId: number, supplierId: number) {
    return this.http.delete<void>(`/api/lotes/${productId}/${supplierId}`);
  }
}
