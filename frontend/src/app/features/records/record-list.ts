import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Subject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';

@Component({
  selector: 'app-record-list',
  imports: [AsyncPipe, DatePipe, MatButtonModule, MatCardModule],
  templateUrl: './record-list.html',
  styleUrl: './record-list.scss'
})
export class RecordList {
  private readonly records = inject(KnowledgeRecordService);
  private readonly reload$ = new Subject<void>();

  readonly state$ = this.reload$.pipe(
    startWith(undefined),
    switchMap(() => this.records.list()),
    map((page) => ({
      error: null,
      loading: false,
      records: page.items,
      totalItems: page.totalItems
    })),
    catchError(() =>
      of({
        error: 'No se pudieron cargar los registros.',
        loading: false,
        records: [] as KnowledgeRecord[],
        totalItems: 0
      })
    ),
  );

  confirmDelete(record: KnowledgeRecord): void {
    const confirmed = window.confirm(`Eliminar "${record.title}"?`);

    if (!confirmed) {
      return;
    }

    this.records.delete(record.id).subscribe({
      next: () => this.reload(),
      error: () => window.alert('No se pudo eliminar el registro.')
    });
  }

  private reload(): void {
    this.reload$.next();
  }
}
