import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { appConfig } from './app.config';

describe('App', () => {
  it('shows the login page when the dashboard opens without a session', async () => {
    TestBed.configureTestingModule({
      providers: [...appConfig.providers, provideHttpClientTesting()],
    });
    const http = TestBed.inject(HttpTestingController);
    const harness = await RouterTestingHarness.create('/');

    http.expectOne('/api/health').flush(null, { status: 401, statusText: 'Unauthorized' });
    await harness.fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/login');
    expect(harness.routeNativeElement!.querySelector('h1')!.textContent).toBe('Monitor de clima');
  });
});
