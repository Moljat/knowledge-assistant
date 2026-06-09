import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { KnowledgeRecordService } from './knowledge-record.service';
import { RecordForm } from './record-form';

describe('RecordForm', () => {
  let records: jasmine.SpyObj<KnowledgeRecordService>;

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', [
      'create', 'getById', 'update'
    ]);
    records.create.and.returnValue(of({} as any));
    records.update.and.returnValue(of({} as any));
    records.getById.and.returnValue(of({
      id: 'test-id',
      title: 'Test',
      content: 'Content',
      source: null,
      type: 2,
      status: 1,
      aiStatus: 0,
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: '2026-01-01T00:00:00Z'
    }));

    await TestBed.configureTestingModule({
      imports: [RecordForm],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: KnowledgeRecordService, useValue: records }
      ]
    }).compileComponents();
  });

  it('should create the form component', () => {
    const fixture = TestBed.createComponent(RecordForm);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show submit button disabled when form is invalid', () => {
    const fixture = TestBed.createComponent(RecordForm);
    fixture.detectChanges();
    const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(btn.disabled).toBeTrue();
  });
});
