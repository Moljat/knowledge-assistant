import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatCardModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
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
