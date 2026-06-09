import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';
import { RecordList } from './record-list';

describe('RecordList', () => {
  let fixture: ComponentFixture<RecordList>;
  let records: jasmine.SpyObj<KnowledgeRecordService>;
  const record: KnowledgeRecord = {
    id: 'record-id',
    title: 'Registro de prueba',
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
        pageSize: 10,
        totalItems: 1,
        totalPages: 1
      })
    );
    records.delete.and.returnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [RecordList],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: KnowledgeRecordService, useValue: records }
      ]
    }).compileComponents();
  });

  it('should render records from the service', fakeAsync(() => {
    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Registro de prueba');
  }));

  it('should call list with page 1 and default page size on init', fakeAsync(() => {
    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(records.list).toHaveBeenCalledWith(1, 10, {});
  }));

  it('should not delete when confirmation is cancelled', fakeAsync(() => {
    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
    tick();

    spyOn(window, 'confirm').and.returnValue(false);
    fixture.componentInstance.confirmDelete(record);
    expect(records.delete).not.toHaveBeenCalled();
  }));

  it('should delete when confirmation is accepted', fakeAsync(() => {
    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
    tick();

    spyOn(window, 'confirm').and.returnValue(true);
    fixture.componentInstance.confirmDelete(record);
    expect(records.delete).toHaveBeenCalledOnceWith('record-id');
  }));

  it('should render search input and filter selects', fakeAsync(() => {
    fixture = TestBed.createComponent(RecordList);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Buscar');
    expect(compiled.textContent).toContain('Estado');
    expect(compiled.textContent).toContain('Tipo');
    expect(compiled.textContent).toContain('IA');
  }));
});
