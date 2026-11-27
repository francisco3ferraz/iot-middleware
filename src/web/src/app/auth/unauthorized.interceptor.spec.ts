import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { firstValueFrom } from 'rxjs';
import { unauthorizedInterceptor } from './unauthorized.interceptor';

@Component({ template: '' })
class PageStub {}

describe('unauthorizedInterceptor', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([unauthorizedInterceptor])),
        provideHttpClientTesting(),
        provideRouter([
          { path: '', component: PageStub },
          { path: 'login', component: PageStub },
        ]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create('/');
  });

  afterEach(() => http.verify());

  async function respond401(url: string) {
    const response = firstValueFrom(TestBed.inject(HttpClient).get(url));
    http.expectOne(url).flush(null, { status: 401, statusText: 'Unauthorized' });
    await expect(response).rejects.toMatchObject({ status: 401 });
    await harness.fixture.whenStable();
  }

  it('sends the user to the login page when the API answers 401', async () => {
    await respond401('/api/health');

    expect(TestBed.inject(Router).url).toBe('/login');
  });

  it('leaves a refused login on the login page to show its own error', async () => {
    await respond401('/api/login');

    expect(TestBed.inject(Router).url).toBe('/');
  });
});
