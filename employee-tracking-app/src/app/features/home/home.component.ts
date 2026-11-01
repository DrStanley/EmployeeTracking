import { Component, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  readonly roles = [
    {
      icon: 'person',
      title: 'Employees',
      items: ['Create an account as an employee', 'Share your entire screen to clock in', 'Clock in and take breaks', 'See this week’s hours', 'Request time off']
    },
    {
      icon: 'groups',
      title: 'Managers and admins',
      items: ['Admins assign manager access and add people', 'See who is working', 'Read screen reviews', 'Approve timesheets and PTO', 'Run payroll']
    }
  ];

  readonly features = [
    { icon: 'schedule', title: 'Clock and breaks', text: 'Clock in shares your entire screen. Snapshots are sent while you work, and clock-out is saved right away.' },
    { icon: 'monitor', title: 'Screen review', text: 'After clock-out, the session is reviewed in the background. Managers and admins read the result. Employees do not.' },
    { icon: 'apps', title: 'App hours', text: 'Compare time spent in EmpTrack and other apps with the hours you actually clocked.' },
    { icon: 'event_note', title: 'Shift schedule', text: 'See the week you are assigned, including which days are off.' },
    { icon: 'calendar_month', title: 'PTO calendar', text: 'Approved time off and company holidays on one month view.' },
    { icon: 'task_alt', title: 'Approvals', text: 'Managers review timesheets and PTO. Admins can see who is working right now.' },
    { icon: 'history', title: 'Audit history', text: 'Approvals, rejections, and policy changes stay on record.' }
  ];

  constructor() {
    inject(Title).setTitle('EmpTrack');
    const access = sessionStorage.getItem('accessToken');
    const refresh = localStorage.getItem('refreshToken');
    if (!access && !refresh) return;
    this.authStore.restoreSession();
    if (this.authStore.isAuthenticated() || refresh) {
      this.router.navigateByUrl('/dashboard');
    }
  }
}
