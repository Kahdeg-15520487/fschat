import { Injectable, inject } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Observable, Observer } from 'rxjs';
import { AuthService } from '@auth0/auth0-angular';
import { of } from 'rxjs';
import { catchError, first } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class chatService {
  private accessToken: string = '';
  private auth: AuthService = inject(AuthService);
  private connection: signalR.HubConnection;

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
    });
  }

  async joinRoom(data: { user: string, room: string }) {
    try {
      await this.connection.start();
      await this.connection.invoke("JoinRoom", data);
    } catch (err) {
      console.error(err);
    }
  }

  newUserJoined(): Observable<{ user: string, message: string, room: string }> {
    return new Observable((observer: Observer<{ user: string, message: string, room: string }>) => {
      this.connection.on('new user joined', (data: { user: string, message: string, room: string }) => {
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

  userLeftRoom(): Observable<{ user: string, message: string, room: string }> {
    return new Observable((observer: Observer<{ user: string, message: string, room: string }>) => {
      this.connection.on('left room', (data: { user: string, message: string, room: string }) => {
        observer.next(data);
      });
    });
  }

  async sendMessage(data: { user: string, room: string, message: string }) {
    try {
      await this.connection.invoke("SendMessage", data);
    } catch (err) {
      console.error(err);
    }
  }

  newMessageReceived(): Observable<{ user: string, message: string, room: string }> {
    return new Observable((observer: Observer<{ user: string, message: string, room: string }>) => {
      this.connection.on('new message', (data: { user: string, message: string, room: string }) => {
        observer.next(data);
      });
    });
  }
}