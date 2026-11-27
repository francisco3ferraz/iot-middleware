import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { LoginPage } from './login-page';

@Component({ template: '' })
class HomeStub {}

describe('LoginPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  let page: HTMLElement;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: '', component: HomeStub },
          { path: 'login', component: LoginPage },
        ]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create('/login');
    page = harness.routeNativeElement!;
  });

  afterEach(() => http.verify());

  function fill(username: string, password: string) {
    for (const [id, value] of [
      ['username', username],
      ['password', password],
    ]) {
      const input = page.querySelector<HTMLInputElement>(`#${id}`)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    }
  }

  function submit() {
    page.querySelector<HTMLButtonElement>('.submit')!.click();
    TestBed.tick();
  }

  // whenStable doesn't wait for the form's submit action, so let it finish first
  async function settle() {
    await new Promise((resolve) => setTimeout(resolve));
    await harness.fixture.whenStable();
  }

  const form = () => page.querySelector('form')!;
  const errorText = () => page.querySelector('.error-wrap p')!.textContent?.trim();

  it('posts the typed username and password', () => {
    fill('admin', 'segredo');
    submit();

    const request = http.expectOne('/api/login');
    expect(request.request.body).toEqual({ username: 'admin', password: 'segredo' });
    request.flush(null);
  });

  it('shows Credenciais inválidas and shakes when the login is refused', async () => {
    fill('admin', 'wrong');
    submit();
    http
      .expectOne('/api/login')
      .flush({ detail: 'credenciais inválidas' }, { status: 401, statusText: 'Unauthorized' });
    await settle();

    expect(errorText()).toBe('Credenciais inválidas');
    expect(form().classList.contains('invalid')).toBe(true);
    expect(form().classList.contains('shake')).toBe(true);
  });

  it('asks for both fields and sends nothing when one is empty', async () => {
    fill('admin', '');
    submit();
    await settle();

    http.expectNone('/api/login');
    expect(errorText()).toBe('Preencha o utilizador e a palavra-passe');
    expect(form().classList.contains('invalid')).toBe(true);
  });

  it('goes to the health page after a good login', async () => {
    fill('admin', 'segredo');
    submit();
    http.expectOne('/api/login').flush(null);
    await settle();

    expect(TestBed.inject(Router).url).toBe('/');
  });
});
