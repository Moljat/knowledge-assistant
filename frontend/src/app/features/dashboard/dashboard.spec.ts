import { ComponentFixture, TestBed, discardPeriodicTasks, fakeAsync, tick } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { Observable, of } from 'rxjs';
import { DashboardStats, KnowledgeRecordService } from '../records/knowledge-record.service';
import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  let fixture: ComponentFixture<Dashboard>;
  let records: jasmine.SpyObj<KnowledgeRecordService>;

  const stats: DashboardStats = {
    totalRecords: 10,
    byStatus: { Draft: 3, Active: 6, Archived: 1 },
    byType: { Document: 4, Note: 4, BusinessRecord: 2 },
    byAiStatus: { NotRequested: 7, Pending: 1, Processing: 0, Completed: 2, Failed: 0 }
  };

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', [
      'getStats'
    ]);
    records.getStats.and.returnValue(of(stats));

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: KnowledgeRecordService, useValue: records }
      ]
    }).compileComponents();
  });

  it('should present the four required AI and management capabilities', () => {
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    const cards = fixture.nativeElement.querySelectorAll('.capability-grid mat-card');
    expect(cards.length).toBe(4);
  });

  it('should display total records count', fakeAsync(() => {
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const values = compiled.querySelectorAll('.indicator-value');
    expect(values.length).toBe(4);
    expect(values[0].textContent).toContain('10');
    discardPeriodicTasks();
  }));

  it('should display active records count', fakeAsync(() => {
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labels = compiled.querySelectorAll('.indicator-label');
    expect(labels[1].textContent).toContain('Activos');
    discardPeriodicTasks();
  }));

  it('should fetch stats on init', fakeAsync(() => {
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(records.getStats).toHaveBeenCalled();
    discardPeriodicTasks();
  }));

  it('should show loading spinner before stats arrive', fakeAsync(() => {
    records.getStats.and.returnValue(new Observable(() => {}));
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();

    const spinner = fixture.nativeElement.querySelector('mat-spinner');
    expect(spinner).toBeTruthy();
    discardPeriodicTasks();
  }));

  it('should show error card when stats fail', fakeAsync(() => {
    records.getStats.and.returnValue(
      new Observable((sub) => { setTimeout(() => sub.error(new Error('fail'))); })
    );

    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No se pudieron cargar los indicadores');
    discardPeriodicTasks();
  }));
});
