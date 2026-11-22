import { TestBed } from '@angular/core/testing';
import { HttpTestingController, HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { vi } from 'vitest';
import { App } from './app';

describe('App', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    sessionStorage.removeItem('inventario-token');
    sessionStorage.removeItem('inventario-usuario');
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        FormsModule,
        HttpClientTestingModule,
        RouterModule.forRoot([]),
      ],
      declarations: [
        App
      ],
    })
      .compileComponents();
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    vi.restoreAllMocks();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Iniciar sesión');
  });

  it('switches to the registration form without showing promotional content', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const registerLink = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((button) => button.textContent?.includes('Regístrate'));
    registerLink?.click();
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.querySelector('h1')?.textContent).toContain('Crear una cuenta');
    expect(page.querySelector('aside')).toBeNull();
    expect(page.querySelector('form')).not.toBeNull();
    expect(page.querySelector('#register-first-name')).not.toBeNull();
    expect(page.querySelector('#register-last-name')).not.toBeNull();
  });

  it('shows the inventory after a successful login without reloading', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.credentials = { email: 'user@example.com', password: 'Password123!' };
    fixture.detectChanges();

    app.login();
    httpTesting.expectOne('/api/auth/login').flush({
      token: 'test-token',
      usuarioId: 1,
      email: 'user@example.com',
      nombre: 'Test',
      apellido: 'User',
    });

    httpTesting.expectOne('/api/productos').flush([]);
    httpTesting.expectOne('/api/lotes').flush([]);
    httpTesting.expectOne('/api/proveedores').flush([]);
    await fixture.whenStable();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.querySelector('.app-shell')).not.toBeNull();
    expect(page.querySelector('.sidebar')).toBeNull();
    expect(page.querySelector('.topbar')?.textContent).toContain('Test User');
    expect(page.querySelector('.logout-action')?.textContent).toContain('Cerrar sesión');
  });

  it('creates a supplier and refreshes the inventory', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.openNewSupplier();
    app.supplierForm = {
      nombre: 'Distribuidora Andina',
      direccion: 'Av. Central 123',
      ruc: '1234567890',
    };

    app.saveSupplier();

    const createRequest = httpTesting.expectOne('/api/proveedores');
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body).toEqual(app.supplierForm);
    createRequest.flush({
      id: 7,
      ...app.supplierForm,
    });

    httpTesting.expectOne('/api/productos').flush([]);
    httpTesting.expectOne('/api/lotes').flush([]);
    httpTesting.expectOne('/api/proveedores').flush([
      { id: 7, ...app.supplierForm },
    ]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(app.suppliers).toHaveLength(1);
    expect(app.activeModal).toBeNull();
    expect(app.successMessage).toContain('proveedor se agregó');
  });

  it('rejects supplier RUC values shorter than ten characters', () => {
    const app = TestBed.createComponent(App).componentInstance;
    app.supplierForm = {
      nombre: 'Distribuidora Andina',
      direccion: 'Av. Central 123',
      ruc: '123456789',
    };

    app.saveSupplier();

    expect(app.errorMessage).toContain('entre 10 y 13 caracteres');
    httpTesting.expectNone('/api/proveedores');
  });

  it('deletes a supplier through the general supplier endpoint', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    const supplier = {
      id: 7,
      nombre: 'Distribuidora Andina',
      direccion: 'Av. Central 123',
      ruc: '1234567890',
    };
    app.suppliers = [supplier];
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    app.deleteSupplier(supplier);

    const deleteRequest = httpTesting.expectOne('/api/proveedores/7');
    expect(deleteRequest.request.method).toBe('DELETE');
    deleteRequest.flush(null, { status: 204, statusText: 'No Content' });
    httpTesting.expectOne('/api/productos').flush([]);
    httpTesting.expectOne('/api/lotes').flush([]);
    httpTesting.expectOne('/api/proveedores').flush([]);
    await fixture.whenStable();

    expect(app.suppliers).toEqual([]);
    expect(app.successMessage).toContain('proveedor se eliminó');
  });

  it('filters products by supplier name', () => {
    const app = TestBed.createComponent(App).componentInstance;
    app.products = [
      { id: 1, nombre: 'Café' },
      { id: 2, nombre: 'Té' },
    ];
    app.lots = [
      {
        idProducto: 1,
        nombreProducto: 'Café',
        idProveedor: 3,
        nombreProveedor: 'Distribuidora Andina',
        precio: 12,
        stock: 8,
      },
    ];
    app.searchTerm = 'andina';

    expect(app.filteredProducts.map((product) => product.id)).toEqual([1]);
  });

  it('does not offer a supplier already assigned to the selected product', () => {
    const app = TestBed.createComponent(App).componentInstance;
    app.suppliers = [
      { id: 3, nombre: 'Distribuidora Andina', direccion: '', ruc: '' },
      { id: 4, nombre: 'Comercial Norte', direccion: '', ruc: '' },
    ];
    app.selectedProduct = { id: 1, nombre: 'Café' };
    app.lots = [
      {
        idProducto: 1,
        nombreProducto: 'Café',
        idProveedor: 3,
        nombreProveedor: 'Distribuidora Andina',
        precio: 12,
        stock: 8,
      },
    ];

    expect(app.availableSuppliers.map((supplier) => supplier.id)).toEqual([4]);
  });
});
