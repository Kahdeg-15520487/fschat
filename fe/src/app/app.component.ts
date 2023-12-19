import { Component, OnInit, inject, SecurityContext } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '@auth0/auth0-angular';
import { of } from 'rxjs';
import { catchError, first } from 'rxjs/operators';

import { chatService } from './chat.service';
import { MessageObject } from './MessageObject';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  private title = 'chatting';
  public user: string = ''; // Initialize the 'user' property
  private userId: string = '';
  private room: string = '';
  private roomId: string = '';
  public messageText: string = '';
  public messageArray: Array<{ user: string, message: string, room: string }> = [];

  private chatService: chatService = inject(chatService);
  private auth: AuthService = inject(AuthService);
  private sanitizer: DomSanitizer = inject(DomSanitizer);

  constructor() { }

  private fetchUserInfo() {
    this.auth.user$.pipe(first(),
      catchError((_) => {
        return of(null);
      })).subscribe({
        next: user => {
          this.user = user?.name ?? '';
          this.userId = user?.sub ?? '';
        }
      });
  }

  ngOnInit() {
    this.fetchUserInfo();
    this.chatService.newUserJoined().subscribe(data => {
      this.room = data.room;
      console.log(data);
      this.messageArray.push(data);
    });
    this.chatService.userLeftRoom().subscribe(data => this.messageArray.push(data));
    this.chatService.newMessageReceived().subscribe(data => {
      if (this.sanitizer.sanitize(SecurityContext.HTML, data.message) !== data.message) {
        alert("xss attack detected! +1 point for you!");
        return;
      }
      console.log(data);
      this.messageArray.push(data);
    });
  }

  async join() {
    let message: MessageObject;
    message = { 
      user: this.user, 
      room: this.room, 
      userId: this.userId,
      messageId: '',
      message: '',
      roomId: ''
    };
    this.roomId = await this.chatService.joinRoom(message);
  }

  leave() {
    this.chatService.leaveRoom({ user: this.user, room: this.room });
  }

  sendMessage() {
    this.chatService.sendMessage({
      user: this.user, room: this.room, message: this.messageText,
      userId: this.userId,
      messageId: '',
      roomId: this.roomId
    });
    // Clear the input field after sending the message
    this.messageText = '';
  }

  // Function to handle key press event for joining
  handleJoinKeyPress(event: any) {
    if (event.key === 'Enter') {
      this.join();
    }
  }

  // Function to handle key press event for sending messages
  handleKeyPress(event: any) {
    if (event.key === 'Enter') {
      this.sendMessage();
    }
  }
}
