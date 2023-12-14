import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Observable, Observer } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class chatService {

  constructor() { }

  private connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5020/chatHub"
      , {
        skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c',
      }
    )
    .build();

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