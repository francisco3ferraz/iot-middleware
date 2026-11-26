import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  async function renderWith(
    respond: (request: ReturnType<HttpTestingController['expectOne']>) => void,
  ) {
    const fixture = TestBed.createComponent(App);
    TestBed.tick(); // whenStable would wait on the pending request forever
    respond(http.expectOne('/api/health'));
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).textContent?.trim();
  }

  it('shows API, DB and broker ok when every check is healthy', async () => {
    const text = await renderWith((r) => r.flush({ checks: { db: 'Healthy', broker: 'Healthy' } }));
    expect(text).toBe('API ok, DB ok, broker ok');
  });

  it('shows a failing check as down', async () => {
    const text = await renderWith((r) =>
      r.flush({ checks: { db: 'Unhealthy', broker: 'Healthy' } }),
    );
    expect(text).toBe('API ok, DB down, broker ok');
  });

  it('shows API down when the request fails', async () => {
    const text = await renderWith((r) => r.flush('', { status: 502, statusText: 'Bad Gateway' }));
    expect(text).toBe('API down');
  });

  it('fails loudly when the response lacks a check', async () => {
    await expect(renderWith((r) => r.flush({ checks: { db: 'Healthy' } }))).rejects.toThrow(
      'Health response has no broker check',
    );
  });
});
