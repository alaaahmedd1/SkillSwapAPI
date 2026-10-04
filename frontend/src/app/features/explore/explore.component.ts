import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SkillCatalogItemDto } from '../../core/models/api.models';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-explore',
  imports: [AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <h2 class="page-title">Explore <span>Skills</span></h2>
      <p class="page-sub">Browse the catalog and tap a skill to find people offering it.</p>

      @if (error()) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="load()">Try again</button>
        </div>
      } @else if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else {
        <div class="catalog">
          @for (category of catalog(); track category.id) {
            <article class="cat-card card">
              <div class="cat-head">
                <h3>{{ category.name }}</h3>
                <span class="count">{{ category.skills.length }} skills</span>
              </div>
              @if (category.description) { <p class="cat-desc">{{ category.description }}</p> }
              <div class="cat-skills">
                @for (skill of category.skills; track skill.id) {
                  <button class="chip" (click)="pickSkill(skill.name)">{{ skill.name }}</button>
                }
              </div>
            </article>
          }
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 2px; }
    .page-title span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }
    .page-sub { font-size: 13px; color: var(--text-secondary); margin: 0 0 16px; }
    .catalog { display: flex; flex-direction: column; gap: 16px; }
    .cat-card { padding: 18px; transition: box-shadow 0.2s ease, transform 0.2s ease; }
    .cat-card:hover { box-shadow: 0 10px 32px rgba(31, 31, 61, 0.11); transform: translateY(-2px); }
    .cat-head { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
    .cat-head h3 { font-size: 15.5px; font-weight: 600; margin: 0; }
    .count { font-size: 11.5px; font-weight: 600; color: var(--primary); background: var(--primary-light); padding: 4px 10px; border-radius: 999px; white-space: nowrap; }
    .cat-desc { font-size: 12.5px; color: var(--text-secondary); margin: 8px 0 0; }
    .cat-skills { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 14px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    @media (min-width: 768px) {
      .page-title { font-size: 26px; }
      .catalog {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
        gap: 20px;
      }
    }
  `,
})
export class ExploreComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  catalog = signal<SkillCatalogItemDto[]>([]);
  loading = signal(true);
  error = signal('');

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const result = await firstValueFrom(
        this.http.get<SkillCatalogItemDto[]>(`${environment.apiUrl}/api/v1/skills`)
      );
      this.catalog.set(result);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Could not load the skill catalog.');
    } finally {
      this.loading.set(false);
    }
  }

  pickSkill(name: string): void {
    void this.router.navigate(['/home'], { queryParams: { skill: name } });
  }
}
