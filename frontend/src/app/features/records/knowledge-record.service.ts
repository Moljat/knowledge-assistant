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
  summary: string | null;
  category: string | null;
  recommendations: string | null;
  aiError: string | null;
  aiProcessedAtUtc: string | null;
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

export interface KnowledgeRecordListFilters {
  search?: string;
  category?: string;
  status?: number;
  type?: number;
  aiStatus?: number;
}

export interface CreateKnowledgeRecordRequest {
  title: string;
  content: string;
  source: string | null;
  type: number;
}

export interface UpdateKnowledgeRecordRequest {
  title: string;
  content: string;
  source: string | null;
  type: number;
}

export interface DashboardStats {
  totalRecords: number;
  byStatus: Record<string, number>;
  byType: Record<string, number>;
  byAiStatus: Record<string, number>;
}

@Injectable({ providedIn: 'root' })
export class KnowledgeRecordService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/records';
  private readonly dashboardUrl = '/api/v1/dashboard';

  list(
    page = 1,
    pageSize = 20,
    filters: KnowledgeRecordListFilters = {}
  ): Observable<PagedKnowledgeRecordResponse> {
    return this.http.get<PagedKnowledgeRecordResponse>(this.baseUrl, {
      params: this.buildListParams(page, pageSize, filters)
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getById(id: string): Observable<KnowledgeRecord> {
    return this.http.get<KnowledgeRecord>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateKnowledgeRecordRequest): Observable<KnowledgeRecord> {
    return this.http.post<KnowledgeRecord>(this.baseUrl, request);
  }

  update(id: string, request: UpdateKnowledgeRecordRequest): Observable<KnowledgeRecord> {
    return this.http.put<KnowledgeRecord>(`${this.baseUrl}/${id}`, request);
  }

  requestAiSummary(id: string): Observable<KnowledgeRecord> {
    return this.http.post<KnowledgeRecord>(`${this.baseUrl}/${id}/ai/summary`, {});
  }

  requestAiClassification(id: string): Observable<KnowledgeRecord> {
    return this.http.post<KnowledgeRecord>(`${this.baseUrl}/${id}/ai/classification`, {});
  }

  requestAiRecommendations(id: string): Observable<KnowledgeRecord> {
    return this.http.post<KnowledgeRecord>(`${this.baseUrl}/${id}/ai/recommendations`, {});
  }

  askQuestion(id: string, question: string): Observable<KnowledgeRecord> {
    return this.http.post<KnowledgeRecord>(`${this.baseUrl}/${id}/ai/questions`, { question });
  }

  chat(question: string): Observable<{ answer: string | null }> {
    return this.http.post<{ answer: string | null }>(`/api/v1/ai/chat`, { question });
  }

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.dashboardUrl}/stats`);
  }

  private buildListParams(
    page: number,
    pageSize: number,
    filters: KnowledgeRecordListFilters
  ): Record<string, string | number> {
    const params: Record<string, string | number> = {
        page,
        pageSize
    };

    if (filters.search) {
      params['search'] = filters.search;
    }

    if (filters.category) {
      params['category'] = filters.category;
    }

    if (filters.status !== undefined) {
      params['status'] = filters.status;
    }

    if (filters.type !== undefined) {
      params['type'] = filters.type;
    }

    if (filters.aiStatus !== undefined) {
      params['aiStatus'] = filters.aiStatus;
    }

    return params;
  }
}
