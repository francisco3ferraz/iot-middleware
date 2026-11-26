import { Component, computed } from '@angular/core';
import { httpResource } from '@angular/common/http';

interface HealthResponse {
  checks: Record<string, string>;
}

@Component({
  selector: 'app-root',
  template: `<p>{{ status() }}</p>`,
})
export class App {
  private readonly health = httpResource<HealthResponse>(() => '/api/health');

  protected readonly status = computed(() => {
    if (this.health.error()) return 'API down';
    const checks = this.health.value()?.checks;
    if (!checks) return 'Checking...';
    return `API ok, DB ${label(checks['db'])}, broker ${label(checks['broker'])}`;
  })
}

function label(status: string): string {
  return status === 'Healthy' ? 'ok' : 'down';
}
