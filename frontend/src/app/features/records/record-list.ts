import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import {
  BehaviorSubject,
  Subject,
  catchError,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import {
  KnowledgeRecord,
  KnowledgeRecordListFilters,
  KnowledgeRecordService,
} from './knowledge-record.service';

@Component({
  selector: 'app-record-list',
  imports: [
    AsyncPipe,
    DatePipe,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatSelectModule,
    MatTableModule,
    ReactiveFormsModule,
  ],
  templateUrl: './record-list.html',
  styleUrl: './record-list.scss',
})
export class RecordList {
  private readonly records = inject(KnowledgeRecordService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reload$ = new Subject<void>();

  protected readonly searchControl = new FormControl('');
  protected readonly statusControl = new FormControl<number | null>(null);
  protected readonly typeControl = new FormControl<number | null>(null);
  protected readonly aiStatusControl = new FormControl<number | null>(null);

  private readonly page$ = new BehaviorSubject<number>(1);
  private readonly pageSize$ = new BehaviorSubject<number>(10);

  protected readonly displayedColumns = ['title', 'status', 'type', 'updatedAt', 'actions'];

  protected readonly statusOptions = [
    { value: 1, label: 'Borrador' },
    { value: 2, label: 'Activo' },
    { value: 3, label: 'Archivado' },
  ];

  protected readonly typeOptions = [
    { value: 1, label: 'Documento' },
    { value: 2, label: 'Nota' },
    { value: 3, label: 'Registro de negocio' },
  ];

  protected readonly aiStatusOptions = [
    { value: 0, label: 'No solicitado' },
    { value: 1, label: 'Pendiente' },
    { value: 2, label: 'Procesando' },
    { value: 3, label: 'Completado' },
    { value: 4, label: 'Fallido' },
  ];

  private readonly filters$ = combineLatest([
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      startWith(''),
    ),
    this.statusControl.valueChanges.pipe(startWith(null)),
    this.typeControl.valueChanges.pipe(startWith(null)),
    this.aiStatusControl.valueChanges.pipe(startWith(null)),
  ]).pipe(
    map(([search, status, type, aiStatus]) => {
      const filters: KnowledgeRecordListFilters = {};
      if (search) filters.search = search;
      if (status !== null) filters.status = status;
      if (type !== null) filters.type = type;
      if (aiStatus !== null) filters.aiStatus = aiStatus;
      return filters;
    }),
  );

  readonly state$ = combineLatest([
    this.filters$,
    this.page$,
    this.pageSize$,
    this.reload$.pipe(startWith(undefined)),
  ]).pipe(
    switchMap(([filters, page, pageSize]) =>
      this.records.list(page, pageSize, filters).pipe(
        map((response) => ({
          error: null,
          records: response.items,
          totalItems: response.totalItems,
          page: response.page,
          pageSize: response.pageSize,
        })),
        catchError(() =>
          of({
            error: 'No se pudieron cargar los registros.',
            records: [] as KnowledgeRecord[],
            totalItems: 0,
            page: 1,
            pageSize: 10,
          }),
        ),
      ),
    ),
  );

  constructor() {
    this.searchControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.page$.next(1));
  }

  onPage(event: PageEvent): void {
    this.page$.next(event.pageIndex + 1);
    this.pageSize$.next(event.pageSize);
  }

  confirmDelete(record: KnowledgeRecord): void {
    const confirmed = window.confirm(`Eliminar "${record.title}"?`);

    if (!confirmed) return;

    this.records.delete(record.id).subscribe({
      next: () => this.reload$.next(),
      error: () => window.alert('No se pudo eliminar el registro.'),
    });
  }

  protected statusLabel(value: number): string {
    return this.statusOptions.find((o) => o.value === value)?.label ?? '';
  }

  protected typeLabel(value: number): string {
    return this.typeOptions.find((o) => o.value === value)?.label ?? '';
  }
}
