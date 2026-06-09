import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap, timer } from 'rxjs';
import { KnowledgeRecordService } from '../records/knowledge-record.service';

@Component({
  selector: 'app-dashboard',
  imports: [AsyncPipe, MatButtonModule, MatCardModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
  private readonly records = inject(KnowledgeRecordService);

  readonly stats$ = timer(0, 30_000).pipe(
    switchMap(() => this.records.getStats()),
    map((stats) => ({ error: null, stats })),
    startWith({ error: null, stats: null }),
    catchError(() => of({ error: 'No se pudieron cargar los indicadores.', stats: null })),
  );

  protected readonly capabilities = [
    {
      title: 'Gestión de registros',
      description: 'CRUD, búsqueda, filtros dinámicos y paginación.'
    },
    {
      title: 'Resumen inteligente',
      description: 'Síntesis de contenido empresarial mediante Mistral.'
    },
    {
      title: 'Clasificación',
      description: 'Etiquetas consistentes a partir del contenido almacenado.'
    },
    {
      title: 'Preguntas y recomendaciones',
      description: 'Respuestas contextualizadas y siguientes acciones sugeridas.'
    }
  ];
}
