import {
  Component,
  HostListener,
  inject,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { environment } from '../../environments/environment';
import { CommonModule, NgClass } from '@angular/common';
import { NgForOf, NgIf } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';

interface Task {
  id: number;
  friends: number;
  time: number;
  prize: number;
  completed: boolean;
  currentRefs: number;
  restTime: number;
}


@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [NgForOf, NgIf],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.scss',
})
export class TasksComponent {
  http = inject(HttpClient);
  Math = Math;
  taskLevels: Task[] = [];
  notif: boolean = false;
  notifMsg: string = '';
  notifStat: string = '';
  notifIcon: string = '';
  error: boolean = false;
  activeTask: Task | null = null;
  private timerId: any;
  ngOnInit() {
    this.initTasks();
    this.updateActiveTask();
  }
  updateActiveTask() {
    this.timerId = setInterval(() => {
      if (this.activeTask) {
        if (this.activeTask.restTime > 0) {
          this.activeTask.restTime--;
        }else{
          this.initTasks();

        }
      }

    }, 1000);

  }
  ngOnDestroy() {
    if (this.timerId) clearInterval(this.timerId);
  }
  async initTasks() {
    const url = `${environment.apiUrl}/Tasks/GetTasks`;
    try {
      const response = await firstValueFrom(this.http.post<Task[]>(url, { Username: localStorage.getItem('username') }));
      this.taskLevels = response;
    } catch (error) {
      console.error('Error:', error);
      this.showNotification('Error al cargar tareas', 'error');
    }
     console.log(this.taskLevels);
    this.taskLevels.forEach((task) => {
     
      if (task.restTime > 0 && !task.completed) {
        this.activeTask = task;
        console.log(this.activeTask);
      }
    });
  }
  showNotification(msg: string, type: string = 'success') {

    this.notifMsg = msg;

    if (type === 'error') {
      this.error = true;
      this.notifStat = 'Denegado';
      this.notifIcon = '✕';
    } else {
      this.error = false;
      this.notifStat = 'Éxito';
      this.notifIcon = '✔';
    }
    this.notif = true;
    this.notifMsg = msg;
    setTimeout(() => (this.notif = false), 3500);
  }
  async activateTask(taskId: number) {
    if (this.activeTask !== null) {
      this.showNotification('Ya tienes una tarea activa', 'error');
      return;
    }
    const url = `${environment.apiUrl}/Tasks/ActivateTask`;
    try {
      const response = await firstValueFrom(this.http.post(url, { Username: localStorage.getItem('username'), taskId: taskId }));
      
      console.log(response);
      this.showNotification('Tarea activada correctamente');
    } catch (error) {
      console.error(error);
      this.showNotification('Error al activar la tarea', 'error');
    }
    this.initTasks();
  }
}
