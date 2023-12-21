import { Component, OnInit, inject, SecurityContext } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '@auth0/auth0-angular';
import { Router, ActivatedRoute } from '@angular/router';
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
  public roomId: string = '';
  public messageText: string = '';
  public messageArray: Array<{ user: string, message: string, room: string }> = [];

  private chatService: chatService = inject(chatService);
  private auth: AuthService = inject(AuthService);
  private sanitizer: DomSanitizer = inject(DomSanitizer);
  private route: ActivatedRoute = inject(ActivatedRoute);
  private router: Router = inject(Router);

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
    this.route.fragment.subscribe(fragment => {
      this.roomId = fragment ?? '';
    });
    this.fetchUserInfo();
    this.chatService.newUserJoined().subscribe(data => {
      this.room = data.room;
      this.messageArray.push(data);
    });
    this.chatService.userLeftRoom().subscribe(data => this.messageArray.push(data));
    this.chatService.newMessageReceived().subscribe(data => this.messageArray.push(data));
  }

  async join() {
    let message: MessageObject;
    message = {
      user: this.user,
      room: this.room,
      userId: this.userId,
      messageId: '',
      message: '',
      roomId: this.roomId
    };
    const response = await this.chatService.joinRoom(message);
    this.roomId = response.roomId ?? '';
    this.router.navigate([], { fragment: this.roomId });

    response.pastMessages?.pipe(first()).subscribe((messages) => {
      messages.messages.forEach(m => {
        this.messageArray.push({ user: m.user, message: m.message, room: m.room });
      });
    });
  }

  leave() {
    this.chatService.leaveRoom({ user: this.user, room: this.room });
  }

  sendMessage() {
    if (this.sanitizer.sanitize(SecurityContext.HTML, this.messageText) !== this.messageText) {
      alert("xss attack detected! +1 point for you!");
      return;
    }
    this.chatService.sendMessage({
      user: this.user, room: this.room, message: this.messageText,
      userId: this.userId,
      messageId: '',
      roomId: this.roomId
    });
    // Clear the input field after sending the message
    this.messageText = '';
  }

  exportToPDF() {
    this.chatService.getPdf(this.roomId)
      .subscribe((response: Blob) => {
        const fileURL = URL.createObjectURL(response);
        window.open(fileURL);
      });
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
