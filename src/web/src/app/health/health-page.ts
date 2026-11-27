import { Component, computed } from '@angular/core';
import { httpResource } from '@angular/common/http';

interface HealthResponse {
  checks: Record<string, string>;
}

@Component({
  selector: 'app-health-page',
  template: `<p>{{ status() }}</p>`,
})
export class HealthPage {
  private readonly health = httpResource<HealthResponse>(() => '/api/health');

  protected readonly status = computed(() => {
    if (this.health.isLoading()) return 'Checking...';
    if (this.health.error()) return 'API down';
    const checks = this.health.value()!.checks;
    return `API ok, DB ${label(checks, 'db')}, broker ${label(checks, 'broker')}`;
  });
}

function label(checks: Record<string, string>, name: string): string {
  const status = checks[name];
  if (status === undefined) throw new Error(`Health response has no ${name} check`);
  return status === 'Healthy' ? 'ok' : 'down';
}
