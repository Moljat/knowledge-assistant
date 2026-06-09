import { Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule, Validators, FormBuilder } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { switchMap, tap } from 'rxjs';
import { KnowledgeRecordService } from './knowledge-record.service';

@Component({
  selector: 'app-record-form',
  imports: [
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './record-form.html',
  styleUrl: './record-form.scss',
})
export class RecordForm {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly records = inject(KnowledgeRecordService);

  protected readonly isEdit = this.route.snapshot.paramMap.has('id');
  private readonly recordId = this.route.snapshot.paramMap.get('id');

  protected readonly typeOptions = [
    { value: 1, label: 'Documento' },
    { value: 2, label: 'Nota' },
    { value: 3, label: 'Registro de negocio' },
  ];

  protected form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    content: ['', [Validators.required, Validators.maxLength(50000)]],
    source: ['', [Validators.maxLength(500)]],
    type: [2 as number, Validators.required],
  });

  constructor() {
    if (this.isEdit && this.recordId) {
      this.records.getById(this.recordId).subscribe((record) => {
        this.form.patchValue({
          title: record.title,
          content: record.content,
          source: record.source ?? '',
          type: record.type,
        });
      });
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    const { title, content, source, type } = this.form.getRawValue();
    const request = { title, content, source: source || null, type };

    const obs$ = this.isEdit && this.recordId
      ? this.records.update(this.recordId, request)
      : this.records.create(request);

    obs$.subscribe(() => this.router.navigate(['/records']));
  }
}
