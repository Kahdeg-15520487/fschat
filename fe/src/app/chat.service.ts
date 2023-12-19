import { Injectable, inject } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Observable, Observer } from 'rxjs';
import { AuthService } from '@auth0/auth0-angular';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError, first } from 'rxjs/operators';
import { MessageObject } from './MessageObject';

@Injectable({
  providedIn: 'root'
})
export class chatService {
  private accessToken: string = '';
  private auth: AuthService = inject(AuthService);
  private connection: signalR.HubConnection;
  private http: HttpClient = inject(HttpClient);
  private headers: { Authorization: string; } = { Authorization: '' };

  constructor() {
    this.fetchAccessToken();
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5020/chatHub"
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

  async joinRoom(data: MessageObject): Promise<string> {
    try {
      await this.connection.start();
      data.roomId = await this.connection.invoke<string>("JoinRoom", data);

      this.http.get<{ username: string }>('http://localhost:5020/api/chat/username?UserId=' + data.userId, { headers: this.headers }).subscribe({
          next: apiResponse => {
            data.user = apiResponse.username;
          },
          error: err => {
            console.error('Error getting username:', err);
          }
        });

      this.http.get('http://localhost:5020/api/chat/messages?GroupId=' + data.roomId, { headers: this.headers }).subscribe({
        next: (messages) => {
          console.log('Chat messages:', messages);
        },
        error: (err) => {
          console.error(err);
        }
      });

      return data.roomId;
    } catch (err) {
      console.error(err);
      return '';
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
        this.http.get<{ userName: string }>('http://localhost:5020/api/chat/username?UserId=' + data.userId, { headers: this.headers }).subscribe({
          next: apiResponse => {
            data.user = apiResponse.userName;
            console.log(data);
            observer.next(data);
          },
          error: err => {
            console.error('Error getting username:', err);
          }
        });
      });
    });
  }
}