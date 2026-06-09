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
    service.list(2, 10).subscribe((response) => {
      expect(response.page).toBe(2);
      expect(response.pageSize).toBe(10);
    });

    const request = http.expectOne('/api/v1/records?page=2&pageSize=10');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], page: 2, pageSize: 10, totalItems: 0, totalPages: 0 });
  });

  it('should delete a record by id', () => {
    service.delete('record-id').subscribe();

    const request = http.expectOne('/api/v1/records/record-id');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
