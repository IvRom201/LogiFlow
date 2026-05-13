import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, MatToolbarModule],
  template: `
    <mat-toolbar color="primary">
      <span>LogiFlow</span>
    </mat-toolbar>
    <router-outlet />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {}
