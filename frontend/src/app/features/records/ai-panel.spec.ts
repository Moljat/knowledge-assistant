import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { AiPanel } from './ai-panel';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';

describe('AiPanel', () => {
  let records: jasmine.SpyObj<KnowledgeRecordService>;

  const record: KnowledgeRecord = {
    id: 'record-id',
    title: 'Registro',
    content: 'Contenido',
    source: null,
    type: 2,
    status: 1,
    aiStatus: 0,
    summary: null,
    category: null,
    recommendations: null,
    aiError: null,
    aiProcessedAtUtc: null,
    createdAtUtc: '2026-06-09T00:00:00Z',
    updatedAtUtc: '2026-06-09T00:00:00Z',
  };

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', [
      'requestAiSummary',
      'requestAiClassification',
      'requestAiRecommendations',
      'askQuestion',
    ]);
    records.requestAiSummary.and.returnValue(of({ ...record, aiStatus: 3, summary: 'Resumen' }));
    records.requestAiClassification.and.returnValue(of({ ...record, aiStatus: 3, category: 'Ventas' }));
    records.requestAiRecommendations.and.returnValue(
      of({ ...record, aiStatus: 3, recommendations: 'Revisar' }),
    );
    records.askQuestion.and.returnValue(of({ ...record, aiStatus: 3 }));

    await TestBed.configureTestingModule({
      imports: [AiPanel],
      providers: [
        provideNoopAnimations(),
        { provide: KnowledgeRecordService, useValue: records },
      ],
    }).compileComponents();
  });

  function createPanel(value: KnowledgeRecord = record) {
    const fixture = TestBed.createComponent(AiPanel);
    fixture.componentRef.setInput('record', value);
    fixture.detectChanges();
    return fixture;
  }

  it('runs all analysis actions and emits updated records', () => {
    const fixture = createPanel();
    const component = fixture.componentInstance as any;
    const emitted: KnowledgeRecord[] = [];
    fixture.componentInstance.updated.subscribe((value) => emitted.push(value));

    component.runSummary();
    component.runClassification();
    component.runRecommendations();

    expect(records.requestAiSummary).toHaveBeenCalledOnceWith('record-id');
    expect(records.requestAiClassification).toHaveBeenCalledOnceWith('record-id');
    expect(records.requestAiRecommendations).toHaveBeenCalledOnceWith('record-id');
    expect(emitted.length).toBe(3);
    expect(component.aiProcessing).toBeFalse();
  });

  it('asks a trimmed non-empty question and reports failures', () => {
    const fixture = createPanel();
    const component = fixture.componentInstance as any;
    component.question = ' ¿Qué sigue? ';

    component.ask();

    expect(records.askQuestion).toHaveBeenCalledOnceWith('record-id', ' ¿Qué sigue? ');
    records.requestAiSummary.and.returnValue(throwError(() => new Error('fail')));
    component.runSummary();
    expect(component.errorMessage).toContain('No se pudo completar');

    records.askQuestion.and.returnValue(throwError(() => new Error('fail')));
    component.ask();
    expect(component.errorMessage).toContain('No se pudo obtener respuesta');
  });

  it('disables actions while pending or processing', () => {
    const pending = createPanel({ ...record, aiStatus: 1 }).componentInstance as any;
    expect(pending.disabled()).toBeTrue();

    const processing = createPanel({ ...record, aiStatus: 2 }).componentInstance as any;
    expect(processing.disabled()).toBeTrue();

    const available = createPanel().componentInstance as any;
    expect(available.disabled()).toBeFalse();
    expect(available.statusLabel).toBe('No solicitado');
  });
});
