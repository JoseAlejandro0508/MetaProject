import { Component } from '@angular/core';

@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [],
  templateUrl: './activity.component.html',
  styleUrl: './activity.component.scss'
})
export class ActivityComponent {
  client(){
    window.open('https://wa.me/447346042117?text=%F0%9F%91%8BHola%20gerente%20Sabrina,%20vengo%20desde%20*%E2%99%A7Trump-Investing*,%20necesito%20me%20ayudes%20en%20un%20tema%20del%20cual%20tengo%20dudas', '_blank');
  }

  support(){
    window.open('https://t.me/TrumpInvesting', '_blank');
  }

  canal(){
    window.open('https://t.me/TrumpInvestin', '_blank');
  }
}