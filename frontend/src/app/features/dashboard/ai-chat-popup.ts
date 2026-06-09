import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { KnowledgeRecordService } from '../records/knowledge-record.service';

interface ChatMessage {
  text: string;
  isUser: boolean;
  timestamp: Date;
}

@Component({
  selector: 'app-ai-chat-popup',
  imports: [
    DatePipe,
    FormsModule,
    MatBadgeModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
  ],
  template: `
    <button mat-fab class="chat-fab" (click)="toggleOpen()" aria-label="Chat IA">
      <mat-icon>smart_toy</mat-icon>
    </button>

    @if (isOpen()) {
      <div class="chat-popup">
        <div class="chat-header">
          <span>Asistente IA</span>
          <button mat-icon-button (click)="isOpen.set(false)">
            <mat-icon>close</mat-icon>
          </button>
        </div>

        <div class="chat-messages" #messagesContainer>
          @for (msg of messages(); track msg.timestamp) {
            <div class="message" [class.user]="msg.isUser" [class.bot]="!msg.isUser">
              <div class="bubble">{{ msg.text }}</div>
              <div class="time">{{ msg.timestamp | date: 'HH:mm' }}</div>
            </div>
          }

          @if (loading()) {
            <div class="message bot">
              <div class="bubble typing">Escribiendo...</div>
            </div>
          }
        </div>

        <div class="chat-input">
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <input
              matInput
              [(ngModel)]="question"
              placeholder="Escribe tu pregunta..."
              (keyup.enter)="send()"
              [disabled]="loading()"
            />
          </mat-form-field>
          <button
            mat-icon-button
            (click)="send()"
            [disabled]="loading() || !question.trim()"
          >
            <mat-icon>send</mat-icon>
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .chat-fab {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
    }
    .chat-popup {
      position: fixed;
      bottom: 88px;
      right: 24px;
      width: 360px;
      height: 520px;
      background: var(--mat-sys-surface);
      border-radius: 16px;
      box-shadow: 0 4px 24px rgba(0,0,0,0.2);
      display: flex;
      flex-direction: column;
      z-index: 1000;
      overflow: hidden;
    }
    .chat-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      font-weight: 500;
      background: var(--mat-sys-primary-container);
    }
    .chat-messages {
      flex: 1;
      overflow-y: auto;
      padding: 12px 16px;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .message { display: flex; flex-direction: column; max-width: 80%; }
    .message.user { align-self: flex-end; align-items: flex-end; }
    .message.bot { align-self: flex-start; }
    .bubble {
      padding: 8px 12px;
      border-radius: 12px;
      font-size: 14px;
      line-height: 1.4;
      white-space: pre-wrap;
      word-break: break-word;
    }
    .message.user .bubble {
      background: var(--mat-sys-primary);
      color: var(--mat-sys-on-primary);
      border-bottom-right-radius: 4px;
    }
    .message.bot .bubble {
      background: var(--mat-sys-surface-container-high);
      border-bottom-left-radius: 4px;
    }
    .typing { font-style: italic; opacity: 0.7; }
    .time { font-size: 11px; opacity: 0.6; margin-top: 2px; }
    .chat-input {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 8px 12px;
      border-top: 1px solid var(--mat-sys-outline-variant);
    }
    .chat-input mat-form-field { flex: 1; }
  `]
})
export class AiChatPopup {
  private readonly records = inject(KnowledgeRecordService);

  protected readonly isOpen = signal(false);
  protected readonly messages = signal<ChatMessage[]>([
    {
      text: 'Hola! Soy tu asistente de conocimiento. Hazme cualquier pregunta.',
      isUser: false,
      timestamp: new Date(),
    }
  ]);
  protected readonly loading = signal(false);
  protected question = '';

  protected toggleOpen(): void {
    this.isOpen.update((v) => !v);
  }

  protected send(): void {
    const q = this.question.trim();
    if (!q || this.loading()) return;

    this.messages.update((msgs) => [
      ...msgs,
      { text: q, isUser: true, timestamp: new Date() },
    ]);
    this.question = '';
    this.loading.set(true);

    this.records.chat(q).subscribe({
      next: (response) => {
        this.messages.update((msgs) => [
          ...msgs,
          {
            text: response.answer ?? 'No obtuve respuesta.',
            isUser: false,
            timestamp: new Date(),
          },
        ]);
        this.loading.set(false);
      },
      error: () => {
        this.messages.update((msgs) => [
          ...msgs,
          {
            text: 'Lo siento, no pude conectar con el servicio de IA.',
            isUser: false,
            timestamp: new Date(),
          },
        ]);
        this.loading.set(false);
      },
    });
  }
}
