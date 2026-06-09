import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { KnowledgeRecordService } from './knowledge-record.service';

describe('KnowledgeRecordService', () => {
  let service: KnowledgeRecordService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(KnowledgeRecordService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should request a paginated record list', () => {
    service.list(2, 10, { search: 'ventas', status: 2 }).subscribe((response) => {
      expect(response.page).toBe(2);
      expect(response.pageSize).toBe(10);
    });

    const request = http.expectOne(
      '/api/v1/records?page=2&pageSize=10&search=ventas&status=2'
    );
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], page: 2, pageSize: 10, totalItems: 0, totalPages: 0 });
  });

  it('should get a record by id', () => {
    service.getById('test-id').subscribe((record) => {
      expect(record.title).toBe('Test Record');
    });

    const request = http.expectOne('/api/v1/records/test-id');
    expect(request.request.method).toBe('GET');
    request.flush({
      id: 'test-id',
      title: 'Test Record',
      content: 'Content',
      source: null,
      type: 1,
      status: 1,
      aiStatus: 0,
      summary: null,
      category: null,
      recommendations: null,
      aiError: null,
      aiProcessedAtUtc: null,
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: '2026-01-01T00:00:00Z'
    });
  });

  it('should create a record', () => {
    const request = { title: 'New', content: 'Content', source: null, type: 1 };
    service.create(request).subscribe((record) => {
      expect(record.id).toBe('new-id');
    });

    const req = http.expectOne('/api/v1/records');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({
      id: 'new-id',
      title: 'New',
      content: 'Content',
      source: null,
      type: 1,
      status: 1,
      aiStatus: 0,
      summary: null,
      category: null,
      recommendations: null,
      aiError: null,
      aiProcessedAtUtc: null,
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: '2026-01-01T00:00:00Z'
    });
  });

  it('should update a record', () => {
    const request = { title: 'Updated', content: 'Content', source: 'Source', type: 2 };
    service.update('test-id', request).subscribe((record) => {
      expect(record.title).toBe('Updated');
    });

    const req = http.expectOne('/api/v1/records/test-id');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({
      id: 'test-id',
      title: 'Updated',
      content: 'Content',
      source: 'Source',
      type: 2,
      status: 1,
      aiStatus: 0,
      summary: null,
      category: null,
      recommendations: null,
      aiError: null,
      aiProcessedAtUtc: null,
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: '2026-01-01T00:00:00Z'
    });
  });

  it('should delete a record by id', () => {
    service.delete('record-id').subscribe();

    const request = http.expectOne('/api/v1/records/record-id');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });

  it('should request dashboard stats', () => {
    service.getStats().subscribe((response) => {
      expect(response.totalRecords).toBe(10);
      expect(response.byStatus['Active']).toBe(6);
      expect(response.byType['Document']).toBe(4);
    });

    const request = http.expectOne('/api/v1/dashboard/stats');
    expect(request.request.method).toBe('GET');
    request.flush({
      totalRecords: 10,
      byStatus: { Draft: 3, Active: 6, Archived: 1 },
      byType: { Document: 4, Note: 4, BusinessRecord: 2 },
      byAiStatus: { NotRequested: 7, Pending: 1, Processing: 0, Completed: 2, Failed: 0 },
      totalRetries: 3
    });
  });
});
