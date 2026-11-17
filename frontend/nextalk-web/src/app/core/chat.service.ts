import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { ChatMessage, Conversation, UserSummary } from './models';

export type ChatNotification =
  | { type: 'conversation'; conversation: Conversation }
  | { type: 'message'; message: ChatMessage };

@Injectable({ providedIn: 'root' })
export class ChatService {
  private connection?: signalR.HubConnection;
  private activeConversationId: string | null = null;
  private readonly messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  private readonly notificationsSubject = new BehaviorSubject<ChatNotification | null>(null);
  readonly messages$ = this.messagesSubject.asObservable();
  readonly notifications$ = this.notificationsSubject.asObservable();

  constructor(private readonly auth: AuthService, private readonly http: HttpClient) {}

  async connect() {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, { accessTokenFactory: () => this.auth.token ?? '' })
      .withAutomaticReconnect().build();
    this.connection.on('ReceiveMessage', (message: ChatMessage) => {
      if (!this.messagesSubject.value.some(item => item.id === message.id))
        this.messagesSubject.next([...this.messagesSubject.value, message]);
    });
    this.connection.on('ConversationCreated', (conversation: Conversation) => {
      this.notificationsSubject.next({ type: 'conversation', conversation });
    });
    this.connection.on('MessageNotification', (message: ChatMessage) => {
      if (message.conversationId === this.activeConversationId &&
          !this.messagesSubject.value.some(item => item.id === message.id)) {
        this.messagesSubject.next([...this.messagesSubject.value, message]);
      }
      this.notificationsSubject.next({ type: 'message', message });
    });
    this.connection.onreconnected(() => {
      if (this.activeConversationId) void this.joinConversation(this.activeConversationId);
    });
    await this.connection.start();
  }

  async joinConversation(id: string) {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) throw new Error('SignalR is not connected.');
    this.activeConversationId = id;
    await this.connection.invoke('JoinConversation', id);
    const history = await firstValueFrom(this.http.get<ChatMessage[]>(`${environment.apiUrl}/chat/conversations/${id}/messages`));
    this.messagesSubject.next(history);
  }
  async sendMessage(content: string) {
    if (!this.connection || !this.activeConversationId) throw new Error('Select a conversation first.');
    await this.connection.invoke('SendMessage', this.activeConversationId, content);
  }
  async leaveConversation() {
    if (this.connection && this.activeConversationId) await this.connection.invoke('LeaveConversation', this.activeConversationId);
    this.activeConversationId = null;
    this.messagesSubject.next([]);
  }
  conversations() { return this.http.get<Conversation[]>(`${environment.apiUrl}/chat/conversations`); }
  publicRooms() { return this.http.get<Conversation[]>(`${environment.apiUrl}/chat/rooms`); }
  searchUsers(query: string) {
    return this.http.get<UserSummary[]>(`${environment.apiUrl}/chat/users/search`, { params: { query } });
  }
  createConversation(kind: number, name: string, memberIds: string[]) {
    return this.http.post<Conversation>(`${environment.apiUrl}/chat/conversations`, { kind, name, memberIds });
  }
  createRoom(name: string) { return this.http.post<Conversation>(`${environment.apiUrl}/chat/rooms`, { name }); }
}
