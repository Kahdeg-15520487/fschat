import { Injectable, inject } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Observable, Observer } from 'rxjs';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError, first } from 'rxjs/operators';
import { MessageObject } from './MessageObject';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class chatService {
  private accessToken: string = '';
  private auth: AuthService = inject(AuthService);
  private connection: signalR.HubConnection;
  private http: HttpClient = inject(HttpClient);
  private headers: { Authorization: string; } = { Authorization: '' };
  private hostAddress = environment.host;

  constructor() {
    this.fetchAccessToken();
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.hostAddress}/chatHub`
        , {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          accessTokenFactory: () => this.accessToken,
        }
      )
      .build();
  }

  private fetchAccessToken() {
    this.auth.getAccessTokenSilently().pipe(
      first(),
      catchError((error) => {
        console.error(error);
        return of(null);
      })
    ).subscribe((token) => {
      this.accessToken = token ?? '';
      this.headers = { 'Authorization': `Bearer ${this.accessToken}` };
    });
  }

  async joinRoom(data: MessageObject): Promise<{ roomId: string | null; pastMessages: Observable<{ messages: MessageObject[] }> | null; }> {
    try {
      await this.connection.start();
      const roomId = await this.connection.invoke<string>("JoinRoom", data);

      const messages = this.http.get<{ messages: MessageObject[] }>(`${this.hostAddress}/api/chat/messages?GroupId=` + roomId, { headers: this.headers });

      return { roomId: roomId, pastMessages: messages };
    } catch (err) {
      console.error(err);
      return { roomId: null, pastMessages: null };
    }
  }

  newUserJoined(): Observable<MessageObject> {
    return new Observable((observer: Observer<MessageObject>) => {
      this.connection.on('new user joined', (data: MessageObject) => {
        observer.next(data);
      });
    });
  }

  async leaveRoom(data: { user: string, room: string }) {
    try {
      await this.connection.invoke("LeaveRoom", data.room);
    } catch (err) {
      console.error(err);
    }
  }

  userLeftRoom(): Observable<MessageObject> {
    return new Observable((observer: Observer<MessageObject>) => {
      this.connection.on('left room', (data: MessageObject) => {
        observer.next(data);
      });
    });
  }

  async sendMessage(data: MessageObject) {
    try {
      await this.connection.invoke("SendMessage", data);
    } catch (err) {
      console.error(err);
    }
  }

  newMessageReceived(): Observable<MessageObject> {
    return new Observable((observer: Observer<MessageObject>) => {
      this.connection.on('new message', (data: MessageObject) => {
        observer.next(data);
      });
    });
  }

  getPdf(roomId: string): Observable<Blob> {
    return this.http.get(`${this.hostAddress}/api/chat/pdf?GroupId=` + roomId, { responseType: 'blob', headers: this.headers });
  }
}