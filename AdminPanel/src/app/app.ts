import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FeedbackModalComponent } from './shared/components/feedback-modal/feedback-modal.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FeedbackModalComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('QrMenu.Frontend');
}
