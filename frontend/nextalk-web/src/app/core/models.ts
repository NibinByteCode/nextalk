export interface AuthResponse {
  accessToken: string;
  userId: string;
  userName: string;
  email: string;
}

export interface Conversation {
  id: string;
  name: string;
  kind: number;
  memberIds: string[];
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
}
