import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { ChatService } from '../../core/chat.service';
import { ChatMessage, Conversation } from '../../core/models';

@Component({
  selector: 'app-chat', standalone: true, imports: [CommonModule, FormsModule],
  template: `<div class="chat-shell"><aside class="sidebar"><div class="brand"><span class="brand-mark">N</span><strong>NexTalk</strong></div>
    <div class="profile"><span class="avatar">{{ auth.user?.userName?.charAt(0)?.toUpperCase() }}</span><div><b>{{ auth.user?.userName }}</b><small>Online</small></div></div>
    <button class="side-action" (click)="createPrivate()">＋ New private chat</button><button class="side-action" (click)="createGroup()">＋ New group</button><button class="side-action" (click)="createRoom()">＋ Public room</button>
    <h3>My conversations</h3><button class="conversation" *ngFor="let c of conversations" [class.active]="selected?.id === c.id" (click)="open(c)"><span class="conversation-icon">{{ c.kind === 2 ? '◇' : '◯' }}</span>{{ c.name }}</button>
    <h3>Public rooms</h3><button class="conversation" *ngFor="let r of rooms" [class.active]="selected?.id === r.id" (click)="open(r)"><span class="conversation-icon">#</span>{{ r.name }}</button>
    <button class="logout" (click)="logout()">Log out</button></aside>
    <main class="conversation-view"><div class="toast" *ngIf="toast">{{ toast }}</div><header><div><small>CONVERSATION</small><h2>{{ selected?.name || 'Choose a conversation' }}</h2></div><button (click)="load()">↻</button></header>
      <section class="messages"><div class="empty" *ngIf="!selected">Select a conversation to start chatting.</div><article *ngFor="let m of messages" [class.mine]="m.senderId === auth.user?.userId"><strong>{{ m.senderName }}</strong><p>{{ m.content }}</p><small>{{ m.sentAt | date:'shortTime' }}</small></article></section>
      <form (ngSubmit)="send()"><input [(ngModel)]="text" name="message" [disabled]="!selected" maxlength="4000" placeholder="Write a message..."><button class="primary" [disabled]="!selected || !text.trim()">Send</button></form><p class="error">{{ error }}</p>
    </main></div>`
})
export class ChatComponent implements OnInit, OnDestroy {
  conversations: Conversation[] = []; rooms: Conversation[] = []; selected: Conversation | null = null; messages: ChatMessage[] = []; text = ''; error = ''; toast = ''; private sub?: Subscription; private notificationSub?: Subscription; private toastTimer?: ReturnType<typeof setTimeout>;
  constructor(
    public readonly auth: AuthService,
    private readonly chat: ChatService,
    private readonly router: Router,
    private readonly changeDetector: ChangeDetectorRef
  ) {}
  async ngOnInit() {
    if (!this.auth.token) { await this.router.navigateByUrl('/login'); return; }
    this.sub = this.chat.messages$.subscribe(messages => {
      this.messages = messages;
      this.changeDetector.detectChanges();
    });
    this.notificationSub = this.chat.notifications$.subscribe(notification => {
      if (!notification) return;
      if (notification.type === 'conversation') {
        if (!this.conversations.some(item => item.id === notification.conversation.id))
          this.conversations = [notification.conversation, ...this.conversations];
        this.load();
        this.showToast(`${notification.conversation.name} is now available`);
      } else if (this.selected?.id !== notification.message.conversationId) {
        this.load();
        this.showToast(`${notification.message.senderName}: ${notification.message.content}`);
      }
      this.changeDetector.detectChanges();
    });
    try { await this.chat.connect(); this.load(); } catch { this.error = 'Could not connect to chat server.'; }
  }
  ngOnDestroy() { this.sub?.unsubscribe(); this.notificationSub?.unsubscribe(); if (this.toastTimer) clearTimeout(this.toastTimer); void this.chat.leaveConversation(); }
  load() {
    this.chat.conversations().subscribe({ next: x => this.conversations = x, error: () => this.error = 'Could not load conversations.' });
    this.chat.publicRooms().subscribe({ next: x => this.rooms = x, error: () => this.error = 'Could not load rooms.' });
  }
  async open(conversation: Conversation) { try { if (this.selected) await this.chat.leaveConversation(); this.selected = conversation; await this.chat.joinConversation(conversation.id); } catch { this.error = 'Unable to open conversation.'; } }
  send() {
    const value = this.text.trim();
    if (!value) return;
    this.chat.sendMessage(value)
      .then(() => { this.text = ''; this.load(); })
      .catch(() => this.error = 'Message could not be sent.');
  }
  createPrivate() {
    const query = prompt("Enter the other user's username or email");
    if (!query) return;
    this.chat.searchUsers(query).subscribe({
      next: users => {
        if (!users.length) { this.error = 'No user found with that username or email.'; return; }
        const user = users.length === 1 ? users[0] : users.find(x => x.userName.toLowerCase() === query.toLowerCase()) ?? users[0];
        this.chat.createConversation(1, '', [user.id]).subscribe({
          next: c => { this.load(); void this.open(c); },
          error: err => this.error = err.error?.message ?? 'Could not create private chat.'
        });
      },
      error: err => this.error = err.error?.message ?? 'Could not search for that user.'
    });
  }
  createGroup() { const name = prompt('Group name'); const ids = prompt('Other member IDs, comma separated'); if (name && ids) this.chat.createConversation(2, name, ids.split(',').map(x => x.trim()).filter(Boolean)).subscribe({ next: c => { this.load(); void this.open(c); }, error: err => this.error = err.error?.message ?? 'Could not create group.' }); }
  createRoom() { const name = prompt('Public room name'); if (name) this.chat.createRoom(name).subscribe({ next: c => { this.load(); void this.open(c); }, error: err => this.error = err.error?.message ?? 'Could not create room.' }); }
  logout() { this.auth.logout(); void this.router.navigateByUrl('/login'); }
  private showToast(message: string) {
    this.toast = message;
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => this.toast = '', 5000);
    if (typeof Notification !== 'undefined' && Notification.permission === 'granted')
      new Notification('NexTalk', { body: message });
  }
}
