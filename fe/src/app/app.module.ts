import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthModule } from '@auth0/auth0-angular';
import { AuthButtonComponent } from './components/loginbutton';

@NgModule({
  declarations: [
    AppComponent,
    AuthButtonComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    // Import the module into the application, with configuration
    AuthModule.forRoot({
      domain: 'dev-1sptgluc.auth0.com',
      
      clientId: 'oZYJkg2q2Ev6O8qJywyjM1r8bNSkVF59',
      authorizationParams: {
        redirect_uri: window.location.origin,
        audience: 'be-chat'
      }
    }),
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
