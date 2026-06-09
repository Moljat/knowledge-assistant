import { DatePipe } from '@angular/common';
import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';

@Component({
  selector: 'app-ai-panel',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <section class="ai-panel">
      <h2>Analisis IA</h2>

      <div class="ai-status">
        Estado: <strong>{{ statusLabel }}</strong>
      </div>

      @if (errorMessage) {
        <p class="text-error">{{ errorMessage }}</p>
      }

      @if (aiProcessing) {
        <div class="ai-loading">
          <mat-spinner diameter="20" />
          <span>Procesando...</span>
        </div>
      }

      <div class="ai-actions">
        <button mat-stroked-button (click)="runSummary()" [disabled]="disabled()">
          Resumir
        </button>
        <button mat-stroked-button (click)="runClassification()" [disabled]="disabled()">
          Clasificar
        </button>
        <button mat-stroked-button (click)="runRecommendations()" [disabled]="disabled()">
          Recomendar
        </button>
      </div>

      @if (record()?.summary; as summary) {
        <mat-form-field appearance="outline" class="ai-field">
          <mat-label>Resumen</mat-label>
          <textarea matInput readonly rows="3">{{ summary }}</textarea>
        </mat-form-field>
      }

      @if (record()?.category; as category) {
        <mat-form-field appearance="outline" class="ai-field">
          <mat-label>Categoria</mat-label>
          <input matInput readonly [value]="category" />
        </mat-form-field>
      }

      @if (record()?.recommendations; as recommendations) {
        <mat-form-field appearance="outline" class="ai-field">
          <mat-label>Recomendaciones</mat-label>
          <textarea matInput readonly rows="4">{{ recommendations }}</textarea>
        </mat-form-field>
      }

      <div class="ai-question">
        <mat-form-field appearance="outline" class="ai-field">
          <mat-label>Hacer una pregunta</mat-label>
          <input matInput [(ngModel)]="question" placeholder="Escribe tu pregunta..." [disabled]="disabled()" />
        </mat-form-field>
        <button mat-flat-button (click)="ask()" [disabled]="disabled() || !question.trim()">
          Preguntar
        </button>
      </div>

      @if (record()?.aiProcessedAtUtc; as date) {
        <p class="text-muted">Ultimo analisis: {{ date | date: 'short' }}</p>
      }
    </section>
  `,
  styles: [`
    .ai-panel { border-top: 1px solid var(--mat-sys-outline-variant); margin-top: 32px; padding-top: 24px; }
    .ai-panel h2 { margin: 0 0 16px; font-size: 20px; }
    .ai-status { margin-bottom: 12px; }
    .ai-loading { align-items: center; display: flex; gap: 8px; margin-bottom: 12px; }
    .ai-actions { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 16px; }
    .ai-field { width: 100%; }
    .ai-question { display: flex; gap: 8px; align-items: flex-start; }
    .ai-question mat-form-field { flex: 1; }
    .text-error { color: var(--mat-sys-error); margin: 8px 0; }
    .text-muted { color: var(--mat-sys-on-surface-variant); font-size: 12px; margin: 8px 0 0; }
  `]
})
export class AiPanel {
  private readonly records = inject(KnowledgeRecordService);

  readonly record = input<KnowledgeRecord | null>(null);
  readonly updated = output<KnowledgeRecord>();

  protected aiProcessing = false;
  protected errorMessage: string | null = null;
  protected question = '';

  protected get statusLabel(): string {
    const status = this.record()?.aiStatus;
    const labels: Record<number, string> = {
      0: 'No solicitado', 1: 'Pendiente', 2: 'Procesando',
      3: 'Completado', 4: 'Fallido'
    };
    return labels[status ?? 0] ?? 'Desconocido';
  }

  protected disabled(): boolean {
    return this.aiProcessing || this.record()?.aiStatus === 1 || this.record()?.aiStatus === 2;
  }

  private run(type: string, id: string): void {
    this.aiProcessing = true;
    this.errorMessage = null;

    const obs$ = type === 'summary' ? this.records.requestAiSummary(id)
      : type === 'classification' ? this.records.requestAiClassification(id)
      : this.records.requestAiRecommendations(id);

    obs$.subscribe({
      next: (record) => { this.aiProcessing = false; this.updated.emit(record); },
      error: () => {
        this.aiProcessing = false;
        this.errorMessage = 'No se pudo completar el analisis.';
      }
    });
  }

  protected runSummary(): void {
    const id = this.record()?.id;
    if (id) this.run('summary', id);
  }

  protected runClassification(): void {
    const id = this.record()?.id;
    if (id) this.run('classification', id);
  }

  protected runRecommendations(): void {
    const id = this.record()?.id;
    if (id) this.run('recommendations', id);
  }

  protected ask(): void {
    const id = this.record()?.id;
    if (!id || !this.question.trim()) return;

    this.aiProcessing = true;
    this.errorMessage = null;

    this.records.askQuestion(id, this.question).subscribe({
      next: (record) => { this.aiProcessing = false; this.updated.emit(record); },
      error: () => {
        this.aiProcessing = false;
        this.errorMessage = 'No se pudo obtener respuesta.';
      }
    });
  }
}
