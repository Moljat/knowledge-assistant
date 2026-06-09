import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface KnowledgeRecord {
  id: string;
  title: string;
  content: string;
  source: string | null;
  type: number;
  status: number;
  aiStatus: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedKnowledgeRecordResponse {
  items: KnowledgeRecord[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class KnowledgeRecordService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/records';

  list(page = 1, pageSize = 20): Observable<PagedKnowledgeRecordResponse> {
    return this.http.get<PagedKnowledgeRecordResponse>(this.baseUrl, {
      params: {
        page,
        pageSize
      }
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
