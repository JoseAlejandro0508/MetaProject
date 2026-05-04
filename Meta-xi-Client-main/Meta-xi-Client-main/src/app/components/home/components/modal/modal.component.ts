import { Component, EventEmitter, Output,Input } from '@angular/core';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
})
export class ModalComponent {
  @Output() closeModal = new EventEmitter<boolean>();
  @Input() isOpen = false;
  close() {
    this.closeModal.emit(false);
  }
}
