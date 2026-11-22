import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { AuthService, AuthUser } from './auth.service';
import {
  InventoryService,
  Lot,
  Product,
  Supplier,
} from './inventory.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.html',
})
export class App implements OnInit {
  currentUser: AuthUser | null = null;
  products: Product[] = [];
  lots: Lot[] = [];
  suppliers: Supplier[] = [];
  searchTerm = '';
  errorMessage = '';
  successMessage = '';
  loading = false;
  submitting = false;
  activeModal: 'product' | 'lot' | 'supplier' | null = null;
  editingProduct: Product | null = null;
  editingLot: Lot | null = null;
  selectedProduct: Product | null = null;

  authMode: 'login' | 'register' = 'login';
  credentials = { email: '', password: '' };
  registration = { nombre: '', apellido: '', email: '', password: '' };
  productForm = { nombre: '' };
  supplierForm = { nombre: '', direccion: '', ruc: '' };
  lotForm = { idProveedor: 0, precio: 0, stock: 0 };

  constructor(
    private readonly auth: AuthService,
    private readonly inventory: InventoryService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.currentUser = this.auth.user;
    if (this.auth.isAuthenticated) {
      this.loadInventory();
    }
  }

  get filteredProducts(): Product[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    if (!query) {
      return this.products;
    }

    return this.products.filter((product) => {
      const productMatches = product.nombre.toLocaleLowerCase().includes(query);
      const supplierMatches = this.lotsFor(product.id).some((lot) =>
        lot.nombreProveedor.toLocaleLowerCase().includes(query),
      );
      return productMatches || supplierMatches;
    });
  }

  get totalStock(): number {
    return this.lots.reduce((total, lot) => total + lot.stock, 0);
  }

  get totalSuppliers(): number {
    return new Set(this.lots.map((lot) => lot.idProveedor)).size;
  }

  get availableSuppliers(): Supplier[] {
    if (!this.selectedProduct) {
      return this.suppliers;
    }

    const assignedSupplierIds = new Set(
      this.lotsFor(this.selectedProduct.id).map((lot) => lot.idProveedor),
    );
    return this.suppliers.filter(
      (supplier) => !assignedSupplierIds.has(supplier.id),
    );
  }

  login(): void {
    if (!this.credentials.email.trim() || !this.credentials.password) {
      this.errorMessage = 'Ingresa tu correo y contraseña para continuar.';
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.auth.login(this.credentials).subscribe({
      next: (user) => {
        this.currentUser = {
          usuarioId: user.usuarioId,
          email: user.email,
          nombre: user.nombre,
          apellido: user.apellido,
        };
        this.credentials.password = '';
        this.submitting = false;
        this.changeDetector.markForCheck();
        this.loadInventory();
      },
      error: (error: unknown) => {
        this.submitting = false;
        this.handleError(error, 'No se pudo iniciar sesión. Inténtalo de nuevo.');
        this.changeDetector.markForCheck();
      },
    });
  }

  register(): void {
    const nombre = this.registration.nombre.trim();
    const apellido = this.registration.apellido.trim();
    const email = this.registration.email.trim();
    const password = this.registration.password;

    if (!nombre || !apellido || !email || !password) {
      this.errorMessage = 'Completa todos los campos para crear tu cuenta.';
      return;
    }
    if (nombre.length > 100 || apellido.length > 100) {
      this.errorMessage = 'El nombre y el apellido no pueden exceder los 100 caracteres.';
      return;
    }
    if (
      password.length < 8 ||
      !/[A-Z]/.test(password) ||
      !/[a-z]/.test(password) ||
      !/\d/.test(password) ||
      !/[^a-zA-Z0-9]/.test(password)
    ) {
      this.errorMessage =
        'La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un símbolo.';
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.auth.register({ nombre, apellido, email, password }).subscribe({
      next: () => {
        this.submitting = false;
        this.credentials = { email, password: '' };
        this.registration = { nombre: '', apellido: '', email: '', password: '' };
        this.authMode = 'login';
        this.successMessage =
          'Tu cuenta se creó correctamente. Ya puedes iniciar sesión.';
        this.changeDetector.markForCheck();
      },
      error: (error: unknown) => {
        this.submitting = false;
        this.handleError(error, 'No se pudo crear la cuenta. Inténtalo de nuevo.');
        this.changeDetector.markForCheck();
      },
    });
  }

  setAuthMode(mode: 'login' | 'register'): void {
    this.authMode = mode;
    this.errorMessage = '';
    this.successMessage = '';
    if (mode === 'register' && this.credentials.email) {
      this.registration.email = this.credentials.email;
    }
  }

  logout(): void {
    this.auth.logout();
    this.currentUser = null;
    this.products = [];
    this.lots = [];
    this.suppliers = [];
    this.searchTerm = '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  loadInventory(): void {
    this.loading = true;
    this.errorMessage = '';
    forkJoin({
      products: this.inventory.getProducts(),
      lots: this.inventory.getLots(),
      suppliers: this.inventory.getSuppliers(),
    }).subscribe({
      next: ({ products, lots, suppliers }) => {
        this.products = products;
        this.lots = lots;
        this.suppliers = suppliers;
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: (error: unknown) => {
        this.loading = false;
        this.handleError(error, 'No se pudo cargar el inventario.');
        this.changeDetector.markForCheck();
      },
    });
  }

  lotsFor(productId: number): Lot[] {
    return this.lots.filter((lot) => lot.idProducto === productId);
  }

  openNewProduct(): void {
    this.editingProduct = null;
    this.productForm = { nombre: '' };
    this.openModal('product');
  }

  openNewSupplier(): void {
    this.supplierForm = { nombre: '', direccion: '', ruc: '' };
    this.openModal('supplier');
  }

  openEditProduct(product: Product): void {
    this.editingProduct = product;
    this.productForm = { nombre: product.nombre };
    this.openModal('product');
  }

  openNewLot(product: Product): void {
    this.selectedProduct = product;
    this.editingLot = null;
    this.lotForm = { idProveedor: 0, precio: 0, stock: 0 };
    this.openModal('lot');
  }

  openEditLot(product: Product, lot: Lot): void {
    this.selectedProduct = product;
    this.editingLot = lot;
    this.lotForm = {
      idProveedor: lot.idProveedor,
      precio: lot.precio,
      stock: lot.stock,
    };
    this.openModal('lot');
  }

  closeModal(): void {
    this.activeModal = null;
    this.editingProduct = null;
    this.editingLot = null;
    this.selectedProduct = null;
  }

  saveProduct(): void {
    const nombre = this.productForm.nombre.trim();
    if (!nombre) {
      this.errorMessage = 'El nombre del producto es obligatorio.';
      return;
    }
    if (nombre.length > 100) {
      this.errorMessage = 'El nombre no puede exceder los 100 caracteres.';
      return;
    }

    const request = this.editingProduct
      ? this.inventory.updateProduct(this.editingProduct.id, { nombre })
      : this.inventory.createProduct({ nombre });
    const message = this.editingProduct
      ? 'El producto se actualizó correctamente.'
      : 'El producto se agregó al inventario.';
    this.runMutation(request, message);
  }

  saveSupplier(): void {
    const nombre = this.supplierForm.nombre.trim();
    const direccion = this.supplierForm.direccion.trim();
    const ruc = this.supplierForm.ruc.trim();

    if (!nombre || !direccion || !ruc) {
      this.errorMessage = 'Completa todos los campos del proveedor.';
      return;
    }
    if (nombre.length > 150) {
      this.errorMessage = 'El nombre del proveedor no puede exceder los 150 caracteres.';
      return;
    }
    if (direccion.length > 250) {
      this.errorMessage = 'La dirección no puede exceder los 250 caracteres.';
      return;
    }
    if (ruc.length < 10 || ruc.length > 13) {
      this.errorMessage = 'El RUC debe tener entre 10 y 13 caracteres.';
      return;
    }

    this.runMutation(
      this.inventory.createSupplier({ nombre, direccion, ruc }),
      'El proveedor se agregó correctamente.',
    );
  }

  saveLot(): void {
    if (!this.selectedProduct) {
      this.errorMessage = 'Selecciona un producto para guardar el lote.';
      return;
    }
    if (!this.editingLot && !this.lotForm.idProveedor) {
      this.errorMessage = 'Selecciona un proveedor para este lote.';
      return;
    }
    if (
      !this.editingLot &&
      !this.availableSuppliers.some(
        (supplier) => supplier.id === Number(this.lotForm.idProveedor),
      )
    ) {
      this.errorMessage = 'Este producto ya tiene un lote para el proveedor seleccionado.';
      return;
    }
    if (
      !Number.isInteger(Number(this.lotForm.precio)) ||
      Number(this.lotForm.precio) < 0 ||
      !Number.isInteger(Number(this.lotForm.stock)) ||
      Number(this.lotForm.stock) < 0
    ) {
      this.errorMessage = 'El precio y el stock deben ser números enteros iguales o mayores a cero.';
      return;
    }

    const precio = Number(this.lotForm.precio);
    const stock = Number(this.lotForm.stock);
    const request = this.editingLot
      ? this.inventory.updateLot(
          this.selectedProduct.id,
          this.editingLot.idProveedor,
          { precio, stock },
        )
      : this.inventory.createLot({
          idProducto: this.selectedProduct.id,
          idProveedor: Number(this.lotForm.idProveedor),
          precio,
          stock,
        });
    const message = this.editingLot
      ? 'El precio y el stock del lote se actualizaron.'
      : 'El lote del proveedor se agregó correctamente.';
    this.runMutation(request, message);
  }

  deleteProduct(product: Product): void {
    if (
      !window.confirm(
        `¿Eliminar "${product.nombre}"? Esta acción no se puede deshacer.`,
      )
    ) {
      return;
    }
    this.runMutation(
      this.inventory.deleteProduct(product.id),
      'El producto se eliminó correctamente.',
    );
  }

  deleteLot(lot: Lot): void {
    if (
      !window.confirm(
        `¿Eliminar el lote de ${lot.nombreProveedor} para "${lot.nombreProducto}"?`,
      )
    ) {
      return;
    }
    this.runMutation(
      this.inventory.deleteLot(lot.idProducto, lot.idProveedor),
      'El lote se eliminó correctamente.',
    );
  }

  deleteSupplier(supplier: Supplier): void {
    if (
      !window.confirm(
        `¿Eliminar al proveedor "${supplier.nombre}"? Si tiene lotes asociados, todos los lotes de ese proveedor se van a eliminar.`,
      )
    ) {
      return;
    }

    this.runMutation(
      this.inventory.deleteSupplier(supplier.id),
      'El proveedor se eliminó correctamente.',
    );
  }

  clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  private openModal(modal: 'product' | 'lot' | 'supplier'): void {
    this.clearMessages();
    this.activeModal = modal;
  }

  private runMutation(request: Observable<unknown>, successMessage: string): void {
    this.submitting = true;
    this.clearMessages();
    request.subscribe({
      next: () => {
        this.submitting = false;
        this.closeModal();
        this.successMessage = successMessage;
        this.changeDetector.markForCheck();
        this.loadInventory();
      },
      error: (error: unknown) => {
        this.submitting = false;
        this.handleError(error, 'No se pudo guardar el cambio.');
        this.changeDetector.markForCheck();
      },
    });
  }

  private handleError(error: unknown, fallback: string): void {
    console.error('[Inventario] Error en la operación:', error);
    this.successMessage = '';

    if (error instanceof HttpErrorResponse) {
      if (error.status === 401 && this.auth.isAuthenticated) {
        this.logout();
        this.closeModal();
        this.errorMessage = 'Tu sesión expiró. Inicia sesión nuevamente.';
        return;
      }

      const serverMessage = this.serverMessage(error.error);
      if (serverMessage) {
        this.errorMessage = serverMessage;
        return;
      }

      if (error.status === 0) {
        this.errorMessage = 'No hay conexión con el servidor. Verifica que la API esté disponible.';
        return;
      }
      if (error.status === 401) {
        this.errorMessage = 'El correo o la contraseña no son correctos.';
        return;
      }
      if (error.status === 423) {
        this.errorMessage = 'La cuenta está bloqueada temporalmente por varios intentos fallidos.';
        return;
      }
      if (error.status === 409) {
        this.errorMessage = 'Ya existe un registro con esos datos o está relacionado con otros registros.';
        return;
      }
      if (error.status === 400) {
        this.errorMessage = 'Revisa los datos ingresados y vuelve a intentarlo.';
        return;
      }
    }

    this.errorMessage = fallback;
  }

  private serverMessage(body: unknown): string | null {
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    if (!this.isRecord(body)) {
      return null;
    }

    const registrationErrors = body['errores'];
    const details = Array.isArray(registrationErrors)
      ? registrationErrors.filter(
          (value): value is string => typeof value === 'string',
        )
      : [];
    for (const key of ['mensaje', 'message', 'detail', 'title']) {
      const value = body[key];
      if (typeof value === 'string' && value.trim()) {
        return details.length ? `${value} ${details.join(' ')}` : value;
      }
    }
    if (details.length) {
      return details.join(' ');
    }

    const errors = body['errors'];
    if (this.isRecord(errors)) {
      const messages = Object.values(errors)
        .flatMap((value) => (Array.isArray(value) ? value : [value]))
        .filter((value): value is string => typeof value === 'string');
      return messages.length ? messages.join(' ') : null;
    }
    return null;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }
}
