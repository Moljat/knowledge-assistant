import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';
import { RecordList } from './record-list';

describe('RecordList', () => {
  let fixture: ComponentFixture<RecordList>;
  let records: jasmine.SpyObj<KnowledgeRecordService>;
  const record: KnowledgeRecord = {
    id: 'record-id',
    title: 'Registro eliminable',
    content: 'Contenido',
    source: 'Pruebas',
    type: 2,
    status: 1,
    aiStatus: 0,
    createdAtUtc: '2026-06-09T18:00:00Z',
    updatedAtUtc: '2026-06-09T18:00:00Z'
  };

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', [
      'delete',
      'list'
    ]);
    records.list.and.returnValue(
      of({
        items: [record],
        page: 1,
        pageSize: 20,
        totalItems: 1,
        totalPages: 1
      })
    );
    records.delete.and.returnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [RecordList],
      providers: [
        provideNoopAnimations(),
        { provide: KnowledgeRecordService, useValue: records }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
  });

  it('should render records from the service', () => {
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Registro eliminable');
  });

  it('should not delete when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);

    fixture.componentInstance.confirmDelete(record);

    expect(records.delete).not.toHaveBeenCalled();
  });

  it('should delete when confirmation is accepted', () => {
    spyOn(window, 'confirm').and.returnValue(true);

    fixture.componentInstance.confirmDelete(record);

    expect(records.delete).toHaveBeenCalledOnceWith('record-id');
  });
});
