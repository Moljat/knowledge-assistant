import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { KnowledgeRecordService } from '../records/knowledge-record.service';
import { AiChatPopup } from './ai-chat-popup';

describe('AiChatPopup', () => {
  let records: jasmine.SpyObj<KnowledgeRecordService>;

  beforeEach(async () => {
    records = jasmine.createSpyObj<KnowledgeRecordService>('KnowledgeRecordService', ['chat']);
    records.chat.and.returnValue(of({ answer: 'Respuesta del asistente' }));

    await TestBed.configureTestingModule({
      imports: [AiChatPopup],
      providers: [
        provideNoopAnimations(),
        { provide: KnowledgeRecordService, useValue: records },
      ],
    }).compileComponents();
  });

  it('toggles the popup and sends a trimmed question', () => {
    const fixture = TestBed.createComponent(AiChatPopup);
    const component = fixture.componentInstance as any;

    component.toggleOpen();
    expect(component.isOpen()).toBeTrue();

    component.question = '  ¿Qué registros hay?  ';
    component.send();

    expect(records.chat).toHaveBeenCalledOnceWith('¿Qué registros hay?');
    expect(component.question).toBe('');
    expect(component.loading()).toBeFalse();
    expect(component.messages().at(-1).text).toBe('Respuesta del asistente');
  });

  it('uses fallback text when the service has no answer', () => {
    records.chat.and.returnValue(of({ answer: null }));
    const component = TestBed.createComponent(AiChatPopup).componentInstance as any;
    component.question = 'Pregunta';

    component.send();

    expect(component.messages().at(-1).text).toBe('No obtuve respuesta.');
  });

  it('adds a recoverable message when chat fails', () => {
    records.chat.and.returnValue(throwError(() => new Error('fail')));
    const component = TestBed.createComponent(AiChatPopup).componentInstance as any;
    component.question = 'Pregunta';

    component.send();

    expect(component.loading()).toBeFalse();
    expect(component.messages().at(-1).text).toContain('no pude conectar');
  });

  it('does not send empty questions or duplicate requests while loading', () => {
    const component = TestBed.createComponent(AiChatPopup).componentInstance as any;
    component.question = '   ';
    component.send();
    component.question = 'Pregunta';
    component.loading.set(true);
    component.send();

    expect(records.chat).not.toHaveBeenCalled();
  });
});
