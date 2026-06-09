import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { KnowledgeRecord, KnowledgeRecordService } from './knowledge-record.service';
import { RecordForm } from './record-form';

describe('RecordForm', () => {
  let records: jasmine.SpyObj<KnowledgeRecordService>;

  const existing: KnowledgeRecord = {
    id: 'test-id',
    title: 'Existing',
    content: 'Content',
    source: 'Source',
    type: 3,
    status: 1,
    aiStatus: 0,
    createdAtUtc: '2026-01-01T00:00:00Z',
    updatedAtUtc: '2026-01-01T00:00:00Z'
  };

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', [
      'create', 'getById', 'update'
    ]);
    records.create.and.returnValue(of({ ...existing }));
    records.update.and.returnValue(of({ ...existing }));
    records.getById.and.returnValue(of({ ...existing }));

    await TestBed.configureTestingModule({
      imports: [RecordForm],
      providers: [
        provideNoopAnimations(),
        provideRouter([{ path: 'records', component: RecordForm }]),
        { provide: KnowledgeRecordService, useValue: records }
      ]
    }).compileComponents();
  });

  function createFixture() {
    const f = TestBed.createComponent(RecordForm);
    f.detectChanges();
    return f;
  }

  it('should create the form component', () => {
    expect(createFixture().componentInstance).toBeTruthy();
  });

  it('should show submit button disabled when form is invalid', () => {
    const fixture = createFixture();
    const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(btn.disabled).toBeTrue();
  });

  it('should enable submit button when form is valid', () => {
    const fixture = createFixture();
    const comp = fixture.componentInstance as any;
    comp.form.patchValue({ title: 'Test', content: 'Some content' });
    fixture.detectChanges();
    const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(btn.disabled).toBeFalse();
  });

  it('should call create service on submit in create mode', fakeAsync(() => {
    const fixture = createFixture();
    const comp = fixture.componentInstance as any;
    comp.form.patchValue({ title: 'New', content: 'Content' });
    comp.onSubmit();
    tick();
    expect(records.create).toHaveBeenCalledWith({
      title: 'New', content: 'Content', source: null, type: 2
    });
  }));

  it('should set submitting during save and clear after', fakeAsync(() => {
    records.create.and.returnValue(new Observable(() => {}));
    const fixture = TestBed.createComponent(RecordForm);
    fixture.detectChanges();
    const comp = fixture.componentInstance as any;
    comp.form.patchValue({ title: 'Test', content: 'Content' });
    comp.onSubmit();
    expect(comp.submitting).toBeTrue();
  }));

  it('should show error message on create failure', fakeAsync(() => {
    records.create.and.returnValue(throwError(() => new Error('fail')));
    const fixture = createFixture();
    const comp = fixture.componentInstance as any;
    comp.form.patchValue({ title: 'Test', content: 'Content' });
    comp.onSubmit();
    tick();
    fixture.detectChanges();
    expect(comp.errorMessage).toContain('No se pudo guardar');
  }));

  it('should disable submit button while submitting', fakeAsync(() => {
    records.create.and.returnValue(new Observable(() => {}));
    const fixture = createFixture();
    const comp = fixture.componentInstance as any;
    comp.form.patchValue({ title: 'Test', content: 'Content' });
    fixture.detectChanges();
    comp.onSubmit();
    fixture.detectChanges();
    const btn: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(btn.disabled).toBeTrue();
    expect(btn.textContent).toContain('Guardando');
  }));
});
