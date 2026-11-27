import { Component, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { form, FormField, FormRoot, required } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

export const loginUrl = '/api/login';
const missingField = 'Preencha o utilizador e a palavra-passe';

@Component({
  selector: 'app-login-page',
  imports: [FormRoot, FormField],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  protected readonly shaking = signal(false);
  protected readonly passwordShown = signal(false);
  protected readonly capsLock = signal(false);

  private readonly credentials = signal({ username: '', password: '' });
  protected readonly loginForm = form(
    this.credentials,
    (path) => {
      required(path.username, { message: missingField });
      required(path.password, { message: missingField });
    },
    {
      submission: {
        action: async () => {
          try {
            await firstValueFrom(this.http.post(loginUrl, this.credentials()));
          } catch (error) {
            if (!(error instanceof HttpErrorResponse && error.status === 401)) throw error;
            this.shaking.set(true);
            return { kind: 'credentials', message: 'Credenciais inválidas' };
          }
          await this.router.navigate(['/']);
          return undefined;
        },
        onInvalid: () => this.shaking.set(true),
      },
    },
  );

  // Stays quiet until a field is left or a submit is tried, which marks every field touched
  protected readonly error = computed(() =>
    this.loginForm().touched() ? (this.loginForm().errorSummary()[0]?.message ?? '') : '',
  );
}
