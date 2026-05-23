import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class TelegramService {

  private botToken = '7792867781:AAEU4xnwAEpo6rrmcoORpvRR42VLIR0Knlw';
  private chatId = '-5271009438'; 
  private telegramApiUrl = `https://api.telegram.org/bot${this.botToken}`;

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  sendPhoto(photo: File, caption: string): void {
    const formData = new FormData();
    formData.append('chat_id', this.chatId);
    formData.append('photo', photo);
    formData.append('caption', caption);

    this.http.post(`${this.telegramApiUrl}/sendPhoto`, formData).subscribe({
      next: (response) => {
        console.log('Photo sent successfully:', response);
        this.notificationService.correct('Foto enviada correctamente.');
      },
      error: (err) => {
        console.error('Error sending photo:', err);
        this.notificationService.errorMessage(
          'Error al enviar la foto. Inténtalo nuevamente.'
        );
      },
    });
  }

  sendMessage$(message: string): Observable<any> {
    const payload = {
      chat_id: this.chatId,
      text: message,
    };

    return this.http.post(`${this.telegramApiUrl}/sendMessage`, payload);
  }

  sendMessage(message: string): void {
    this.sendMessage$(message).subscribe({
      next: (response) => {
        console.log('Message sent successfully:', response);
        this.notificationService.correct('Mensaje enviado correctamente.');
      },
      error: (err) => {
        console.error('Error sending message:', err);
        this.notificationService.errorMessage(
          'Error al enviar el mensaje. Inténtalo nuevamente.'
        );
      },
    });
  }
}
